
namespace VectorTileServer.Controllers
{


    public class TilesController
        : Microsoft.AspNetCore.Mvc.Controller
    {

        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment m_env;


        public TilesController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.m_env = env;
        } // End Constructor 


        protected System.IO.Stream GetTileStream(int x, int y, int z)
        {
            string webRoot = this.m_env.WebRootPath;
            string path = System.IO.Path.Combine(webRoot, @"2017-07-03_france_monaco.mbtiles");

            // VectorTileRenderer.Sources.ITileSource source = new VectorTileRenderer.Sources.MbTilesSource(path);
            // return source.GetTile(x,y,z).Result;

            SimplisticTileSource source = new SimplisticTileSource(path);
            return source.GetRawTile(x, y, z);
        } // End Function GetTileStream 
        

        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        } // End Function FromTmsY 


        // https://blogs.msdn.microsoft.com/webdev/2013/10/17/attribute-routing-in-asp-net-mvc-5/
        // https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.2
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("tiles/{x:int}/{y:int}/{z:int}")]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTile(int x, int y, int z)
        {
            y = FromTmsY(y, z);
            System.IO.Stream stream = GetTileStream(x, y, z);
            if (stream == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            
            Response.StatusCode = 200;
            Response.Headers["accept-ranges"] = "bytes";
            Response.Headers["access-control-allow-origin"] = "*";
            Response.Headers["cache-control"] = "public, max-age=86400, no-transform";


            // http://www.binaryintellect.net/articles/1b60420c-39ca-4f01-9813-0951e553e146.aspx
            // https://weblog.west-wind.com/posts/2007/Jun/29/HttpWebRequest-and-GZip-Http-Responses
            string acceptencoding = this.HttpContext.Request.Headers["Accept-Encoding"];
            if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("gzip"))
                Response.Headers["content-encoding"] = "gzip";
            else
            {
                stream = StreamHelper.Uncompress<System.IO.Compression.GZipStream>(stream);

                if (acceptencoding.Contains("br"))
                {
                    Response.Headers["content-encoding"] = "br";
                    stream = StreamHelper.Compress<System.IO.Compression.BrotliStream>(stream);
                }
                else if (acceptencoding.Contains("deflate"))
                {
                    Response.Headers["content-encoding"] = "deflate";
                    stream = StreamHelper.Compress<System.IO.Compression.DeflateStream>(stream);
                }
            }
                

            Response.Headers["content-length"] = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            // Response.Headers["date"] = "Sat, 09 Feb 2019 11:06:10 GMT";
            // Response.Headers["expect-ct"] = "max-age=604800, report-uri="https://report-uri.cloudflare.com/cdn-cgi/beacon/expect-ct";
            // Response.Headers["last-modified"] = "Thu, 07 Feb 2019 11:44:10 GMT";
            Response.Headers["vary"] = "Accept-Encoding";


            return new Microsoft.AspNetCore.Mvc.FileStreamResult(stream, "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };

        } // End Function GetTile 


    } // End Class TilesController : Controller 


} // End Namespace VectorTileServer.Controllers 
