
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



        private const string s_tileSQL = @"
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
        
        
        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        }
        
        
        // https://blogs.msdn.microsoft.com/webdev/2013/10/17/attribute-routing-in-asp-net-mvc-5/
        // https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.2
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("tiles/{x:int}/{y:int}/{z:int}")]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTile(int x, int y, int z)
        {
            y = FromTmsY(y, z);
            // https://github.com/klokantech/tileserver-php/blob/master/tileserver.php
            // https://github.com/apdevelop/map-tile-service-asp-net-core/tree/master/Src/MapTileService
            
            // SELECT * FROM metadata
            // SELECT min(zoom_level) AS min, max(zoom_level) AS max FROM tiles
            // SELECT hex(substr(tile_data,1,2)) AS magic FROM tiles LIMIT 1
            // SELECT min(tile_column) AS w, max(tile_column) AS e, min(tile_row) AS s,
            // max(tile_row) AS n FROM tiles WHERE zoom_level='.$metadata['maxzoom']);
            // w = -180 + 360 * ($resultdata[0]['w'] / pow(2, $metadata['maxzoom']));
            // e = -180 + 360 * ((1 + $resultdata[0]['e']) / pow(2, $metadata['maxzoom']));
            // n = $this->row2lat($resultdata[0]['n'], $metadata['maxzoom']);
            // s = $this->row2lat($resultdata[0]['s'] - 1, $metadata['maxzoom']);
            // metadata['bounds'] = implode(',', array($w, $s, $e, $n));

#if false
            Wgs84Coordinates wgs84 = Wgs84Coordinates.FromTile(x, y, z);
            // https://epsg.io/900913
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(wgs84.Latitude, wgs84.Longitude, wgs84.ZoomLevel);
            System.IO.Stream stream = GetTileStream(ta.X, ta.Y, z);
#else
            System.IO.Stream stream = GetTileStream(x, y, z);
#endif

            if (stream == null)
                return null;

            Response.StatusCode = 200;
            Response.Headers["accept-ranges"] = "bytes";
            Response.Headers["access-control-allow-origin"] = "*";
            Response.Headers["cache-control"] = "public, max-age=86400, no-transform";


            // http://www.binaryintellect.net/articles/1b60420c-39ca-4f01-9813-0951e553e146.aspx
            // https://weblog.west-wind.com/posts/2007/Jun/29/HttpWebRequest-and-GZip-Http-Responses
            string acceptencoding = this.HttpContext.Request.Headers["Accept-Encoding"];
            if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("gzip"))
                Response.Headers["content-encoding"] = "gzip";
            else if (acceptencoding.Contains("deflate"))
            {

                Response.Headers["content-encoding"] = "deflate";
                stream = StreamHelper.Compress<System.IO.Compression.DeflateStream>(stream);
            }
            else
                stream = StreamHelper.Uncompress<System.IO.Compression.GZipStream>(stream);
            
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
