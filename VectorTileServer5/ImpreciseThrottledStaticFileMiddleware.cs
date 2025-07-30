
namespace VectorTileServer5
{


    public class ImpreciseThrottledStaticFileMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate m_next;
        private readonly Microsoft.Extensions.FileProviders.IFileProvider m_fileProvider;
        private readonly string m_requestPathPrefix;
        private readonly int m_bytesPerSecond;
        private readonly System.TimeSpan m_delayPerChunk;

        const int bufferSize = 16 * 1024; // 16 KB chunks

        public ImpreciseThrottledStaticFileMiddleware(
            Microsoft.AspNetCore.Http.RequestDelegate next,
            string rootDirectory,
            string requestPathPrefix,
            int bytesPerSecond
        )
        {
            this.m_next = next;
            this.m_fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(rootDirectory);
            this.m_requestPathPrefix = requestPathPrefix.TrimEnd('/');
            this.m_bytesPerSecond = bytesPerSecond; // Use bytesPerSecond directly
            this.m_delayPerChunk = System.TimeSpan.FromSeconds((double)bufferSize / this.m_bytesPerSecond);
        }

        public async System.Threading.Tasks.Task InvokeAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            Microsoft.AspNetCore.Http.PathString path = context.Request.Path;
            if (!path.StartsWithSegments(this.m_requestPathPrefix, out Microsoft.AspNetCore.Http.PathString subpath))
            {
                await this.m_next(context);
                return;
            }

            Microsoft.Extensions.FileProviders.IFileInfo file = this.m_fileProvider.GetFileInfo(subpath);
            if (!file.Exists)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return;
            }

            context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            context.Response.ContentType = GetMimeType(file.Name);
            context.Response.ContentLength = file.Length;
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";

            // No pre-calculated computation delay, rely on Stopwatch for each chunk
            System.TimeSpan desiredChunkProcessingTime = this.m_delayPerChunk;

            using System.IO.Stream stream = file.CreateReadStream();
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await context.Response.Body.WriteAsync(buffer, 0, bytesRead);
                    await context.Response.Body.FlushAsync();

                    sw.Stop(); // Stop to get elapsed time for this chunk's processing
                    System.TimeSpan actualElapsed = sw.Elapsed;

                    System.TimeSpan timeToDelay = desiredChunkProcessingTime - actualElapsed;

                    if (timeToDelay > System.TimeSpan.Zero)
                    {
                        await System.Threading.Tasks.Task.Delay(timeToDelay);
                    }

                    sw.Restart(); // Start stopwatch again for the next chunk
                }
            }
            catch (System.OperationCanceledException)
            {
                // Client disconnected, or request cancelled
                // You might log this, but no further action needed typically
            }
            catch (System.Exception ex)
            {
                // Log other exceptions during stream copying
                // context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // Consider if you want to send an error response or just let it drop
                System.Console.WriteLine($"Error during throttled download: {ex.Message}");
            }
            finally
            {
                sw.Stop(); // Ensure stopwatch is stopped when done
            }
        }

        private static string GetMimeType(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            return ext switch
            {
                ".pdf" => "application/pdf",
                ".rar" => "application/x-rar-compressed",
                ".zip" => "application/zip",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                // Add more common types or consider a more robust solution for production
                _ => "application/octet-stream"
            };
        }
    }
}