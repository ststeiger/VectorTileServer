
namespace VectorTileServer5
{


    public class ThrottledStaticFileMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate m_next;
        private readonly Microsoft.Extensions.FileProviders.IFileProvider m_fileProvider;
        private readonly string m_requestPathPrefix;
        private readonly int m_bytesPerSecond;

        private readonly System.TimeSpan m_delayPerChunk;


        const int bufferSize = 16 * 1024; // 16 KB chunks

        public ThrottledStaticFileMiddleware(
            Microsoft.AspNetCore.Http.RequestDelegate next,
            string rootDirectory,
            string requestPathPrefix,
            int bytesPerSecond
        )
        {
            int correctionalOverhead =(int) (bytesPerSecond*0.03);

            if (correctionalOverhead >= bytesPerSecond)
                correctionalOverhead = 0;

            this.m_next = next;
            this.m_fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(rootDirectory);
            this.m_requestPathPrefix = requestPathPrefix.TrimEnd('/');
            this.m_bytesPerSecond = bytesPerSecond - correctionalOverhead;

            this.m_delayPerChunk = System.TimeSpan.FromSeconds((double)bufferSize / this.m_bytesPerSecond);
        }



        // Using SemaphoreSlim for async locking
        private static System.TimeSpan? s_cachedOverhead = null;
        private static readonly System.Threading.SemaphoreSlim s_lockReplacementSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        public static async System.Threading.Tasks.Task<System.TimeSpan> GetComputationDelay(System.TimeSpan delayPerChunk)
        {
            if (s_cachedOverhead.HasValue)
                return s_cachedOverhead.Value;

            await s_lockReplacementSemaphore.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (s_cachedOverhead.HasValue)
                    return s_cachedOverhead.Value;

                // Calculate overhead once and cache it
                const int numMeasurements = 10;
                System.TimeSpan totalOverhead = System.TimeSpan.Zero;

                for (int i = 0; i < numMeasurements; i++)
                {
                    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                    System.Diagnostics.Stopwatch processTime = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        System.TimeSpan ts = delayPerChunk - processTime.Elapsed;
                        processTime.Restart();

                        if (ts > System.TimeSpan.Zero)
                            await System.Threading.Tasks.Task.Delay(1);
                    }
                    finally
                    {
                        sw.Stop();
                    }
                    processTime.Stop();

                    System.TimeSpan measurement = sw.Elapsed - System.TimeSpan.FromMilliseconds(1);
                    totalOverhead += measurement;
                } // Next i 

                s_cachedOverhead = System.TimeSpan.FromTicks(totalOverhead.Ticks / numMeasurements);
                return s_cachedOverhead.Value;
            }
            finally
            {
                s_lockReplacementSemaphore.Release();
            }
        } // End Task GetComputationDelay 


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

            System.TimeSpan delayPerChunk = this.m_delayPerChunk - await GetComputationDelay(this.m_delayPerChunk);


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

                    System.TimeSpan ts = delayPerChunk - sw.Elapsed;

                    if (ts > System.TimeSpan.Zero)
                        await System.Threading.Tasks.Task.Delay(ts);

                    sw.Restart();
                } // Whend 
            }
            finally
            {
                sw.Stop();
            }

        } // End Task InvokeAsync 


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

                _ => "application/octet-stream"
            };
        } // End FUnction GetMimeType 


    } // End Class ThrottledStaticFileMiddleware 


} // End Namespace 
