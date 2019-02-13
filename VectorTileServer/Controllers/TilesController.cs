
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
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("tiles/{x:int}/{y:int}/{z:int}")]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTile(int x, int y, int z)
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


            return new Microsoft.AspNetCore.Mvc.FileStreamResult(stream, "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };

        } // End Function GetTile 


    } // End Class TilesController : Controller 


} // End Namespace VectorTileServer.Controllers 
