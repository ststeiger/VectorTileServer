
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


using VectorTileServer.Models;


namespace VectorTileServer.Controllers
{


    public class HomeController : Controller
    {

        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment m_env;
        
        

        public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.m_env = env;
        }



        [HttpGet, Route("fonts/{fontstack}/{range}")]
        public FileStreamResult GetFont(string fontstack, string range)
        {
            string basePath = System.IO.Path.Combine(this.m_env.WebRootPath, "fonts");
            string fontDir = System.IO.Path.Combine(basePath, "KlokanTech " + fontstack);
            string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");

            if (!System.IO.File.Exists(fontFile))
            {
                Response.StatusCode = 404;
                return null;
            }


            Response.StatusCode = 200;

            Response.Headers["Date"] = "Wed, 13 Feb 2019 12:00:03 GMT";
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Cache-Control"] = "no-transform, public, max-age=86400";
            Response.Headers["Last-Modified"] = "Thu, 07 Feb 2019 09:43:32 GMT";


            return new FileStreamResult(System.IO.File.OpenRead(fontFile), "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };
        }


        private static string s_tileSQL = @"
CREATE VIEW tiles AS 
SELECT 
        map.zoom_level as zoom_level 
    ,map.tile_column as tile_column 
    ,map.tile_row as tile_row 
    ,images.tile_data as tile_data 
FROM map 
JOIN images ON map.tile_id = images.tile_id 
; ";


        protected System.IO.Stream GetTileStream(int x, int y, int z)
        {
            string webRoot = this.m_env.WebRootPath;
            string path = System.IO.Path.Combine(webRoot, @"2017-07-03_france_monaco.mbtiles");

            VectorTileRenderer.Sources.MbTilesSource source = 
                new VectorTileRenderer.Sources.MbTilesSource(path);
            
            return source.GetRawTile(x, y, z);
        } // End Function GetTileStream 


        // https://blogs.msdn.microsoft.com/webdev/2013/10/17/attribute-routing-in-asp-net-mvc-5/
        // https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.2
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
        [HttpGet, Route("{x:int}/{y:int}/{z:int}")]
        public FileStreamResult GetTile(int x, int y, int z)
        {

#if true
            Wgs84Coordinates wgs84 = Wgs84Coordinates.FromTile(x, y, z);
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(wgs84.Latitude, wgs84.Longitude, wgs84.ZoomLevel);
            System.IO.Stream stream = GetTileStream(ta.X, ta.Y, z);
#else
            System.IO.Stream stream = GetTileStream(x, y, z);
#endif


            if (stream == null)
                return null;

            // accept-ranges: bytes
            // access-control-allow-origin: *
            // cache-control: public, max-age=86400, no-transform
            // content-encoding: gzip
            // content-length: 38832
            // date: Sat, 09 Feb 2019 11:06:10 GMT
            // expect-ct: max-age=604800, report-uri="https://report-uri.cloudflare.com/cdn-cgi/beacon/expect-ct"
            // last-modified: Thu, 07 Feb 2019 11:44:10 GMT
            // status: 200
            // vary: Accept-Encoding

            Response.StatusCode = 200;
            Response.Headers["accept-ranges"] = "bytes";
            Response.Headers["access-control-allow-origin"] = "*";
            Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
            Response.Headers["content-encoding"] = "gzip";
            Response.Headers["content-length"] = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Response.Headers["date"] = "gzip";
            Response.Headers["expect-ct"] = "max-age=604800";
            Response.Headers["vary"] = "Accept-Encoding";
            

            return new FileStreamResult(stream, "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };

        } // End Function GetTile 


    } // End Class HomeController 


} // End Namespace VectorTileServer.Controllers 
