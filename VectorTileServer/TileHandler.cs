
namespace VectorTileServer
{


    public sealed class NotFoundWithCacheControl
        : Microsoft.AspNetCore.Http.IResult
    {

        public static readonly NotFoundWithCacheControl Instance = new NotFoundWithCacheControl();


        public System.Threading.Tasks.Task ExecuteAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
            return System.Threading.Tasks.Task.CompletedTask;
        } // End Task ExecuteAsync 


    } // End Class NotFoundWithCacheControl 


    public sealed class ProtobufFileResult
        : Microsoft.AspNetCore.Http.IResult
    {
        private readonly string m_filePath;

        public ProtobufFileResult(string filePath)
        {
            this.m_filePath = filePath;
        } // End Constructor 

        public async System.Threading.Tasks.Task ExecuteAsync(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "application/x-protobuf";

            // Optional static headers
            httpContext.Response.Headers["Date"] = "Wed, 13 Feb 2019 12:00:03 GMT";
            httpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            httpContext.Response.Headers["Cache-Control"] = "no-transform, public, max-age=86400";
            httpContext.Response.Headers["Last-Modified"] = "Thu, 07 Feb 2019 09:43:32 GMT";

            using (System.IO.Stream stream = System.IO.File.OpenRead(this.m_filePath))
            {
                await stream.CopyToAsync(httpContext.Response.Body);
            } // End Using stream 

        } // End Task ExecuteAsync 


    } // End Class ProtobufFileResult 


    public class TileHandler
    {

        // app.MapGet("fonts/{fontstack}/{range}",
        public static Microsoft.AspNetCore.Http.IResult GetFont(
            string fontstack,
            string range,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env
        )
        {
            string basePath = System.IO.Path.Combine(env.WebRootPath, "fonts");
            string fontDir = System.IO.Path.Combine(basePath, "KlokanTech " + fontstack);
            string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");

            if (!System.IO.File.Exists(fontFile))
            {
                return Microsoft.AspNetCore.Http.Results.NotFound();
            }

            return new ProtobufFileResult(fontFile);
        } // End Function GetFont 


        public static async System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>
            DynamicPathAdjustedJsonHandlerAsync(
            Microsoft.AspNetCore.Http.HttpContext context,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env
        )
        {
            string fn = System.IO.Path.GetFileName(context.Request.Path);

            string server = context.Request.Scheme + System.Uri.SchemeDelimiter + context.Request.Host.Host;

            if (context.Request.Host.Port.HasValue && context.Request.Host.Port.Value != 80 && context.Request.Host.Port.Value != 443)
            {
                server += ":" + System.Convert.ToString(context.Request.Host.Port.Value, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (context.Request.PathBase.HasValue && context.Request.PathBase.Value != null)
            {
                if (!server.EndsWith("/") && !context.Request.PathBase.Value.StartsWith('/'))
                    server += "/";

                server += context.Request.PathBase.Value;
            }

            if (!server.EndsWith("/"))
                server += "/";

            string? templatePath = fn switch
            {
                "style.json" => System.IO.Path.Combine(env.WebRootPath, "styles", "bright", "style_TEMPLATE.json"),
                "v3.json" => System.IO.Path.Combine(env.WebRootPath, "styles", "bright", "v3_TEMPLATE.json"),
                _ => null
            };

            if (templatePath is not null && System.IO.File.Exists(templatePath))
            {
                string content = await System.IO.File.ReadAllTextAsync(templatePath, System.Text.Encoding.UTF8);
                content = content.Replace("{@server}", server);
                return Microsoft.AspNetCore.Http.Results.Content(content, "application/json", System.Text.Encoding.UTF8);
            }

            string errorJson = "{ \"error\": \"404\", \"message\": \"Not Found\", \"code\": 404 }";
            return Microsoft.AspNetCore.Http.Results.Json(
                System.Text.Json.JsonDocument.Parse(errorJson).RootElement,
                statusCode: 404
            );
        }






        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        } // End Function FromTmsY 

        // app.MapGet("tiles/{x:int}/{y:int}/{z:int}",


        public static async System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult> GetTileAsync(
            Microsoft.AspNetCore.Http.HttpContext httpContext,
            int x, int y, int z,
            libWebAppBasics.Database.IConnectionFactory factory
        )
        {
            // fix/workaround: bug in sqlite fully managed/ or rather the framework
            // bug: pcache1_c.cs, pcache_c.cs 


            try
            {
                y = FromTmsY(y, z);
                long fused64 = ((long)x << 32) | (long)y;

                await using System.Data.Common.DbConnection conn = factory.Connection;
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();


                await using System.Data.Common.DbCommand cmd = conn.CreateCommand();
                // PRAGMA mmap_size = 0; 
                // PRAGMA cache_size = -1; 
                cmd.CommandText = @$"SELECT tile_data FROM tiles WHERE tile_column = {x} AND tile_row = {y} AND zoom_level = {z};";


                await using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
                if (!await reader.ReadAsync())
                    // return Microsoft.AspNetCore.Http.Results.NotFound();
                    return NotFoundWithCacheControl.Instance;


                System.IO.Stream stream = reader.GetStream(reader.GetOrdinal("tile_data"));
                if (stream == null)
                {
                    // return Microsoft.AspNetCore.Http.Results.NotFound();
                    return NotFoundWithCacheControl.Instance;
                } // End if (stream == null) 


                string? acceptencoding = httpContext.Request.Headers["Accept-Encoding"];

                httpContext.Response.ContentType = "application/x-protobuf";
                httpContext.Response.Headers["accept-ranges"] = "bytes";
                httpContext.Response.Headers["access-control-allow-origin"] = "*";
                httpContext.Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
                httpContext.Response.Headers["vary"] = "Accept-Encoding";

                System.IO.Stream responseStream;
                string? encoding;

                if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("gzip"))
                {
                    encoding = "gzip";
                    responseStream = stream;
                }
                else
                {
                    using System.IO.Stream rawStream = StreamHelper.Uncompress<System.IO.Compression.GZipStream>(stream);


                    if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("br"))
                    {
                        encoding = "br";
                        responseStream = StreamHelper.Compress<System.IO.Compression.BrotliStream>(rawStream);
                    }
                    else if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("deflate"))
                    {
                        encoding = "deflate";
                        responseStream = StreamHelper.Compress<System.IO.Compression.DeflateStream>(rawStream);
                    }
                    else
                    {
                        encoding = null;
                        responseStream = rawStream;
                    }
                }

                if (!string.IsNullOrEmpty(encoding))
                    httpContext.Response.Headers["content-encoding"] = encoding;

                // Optional: set content-length if the stream supports seeking
                if (responseStream.CanSeek)
                    httpContext.Response.Headers["content-length"] = responseStream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

                return Microsoft.AspNetCore.Http.Results.Stream(responseStream, "application/x-protobuf");
            } // End Try 
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                return Microsoft.AspNetCore.Http.Results.Problem("Could not load tile from Mbtiles");
            } // End Catch 
        } // End Task GetTileAsync 


    } // End Class TileHandler 


} // End Namespace 
