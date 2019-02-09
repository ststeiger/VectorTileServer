using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VectorTileServer.Models;

namespace VectorTileServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            
            
            
            
            
            return View();
        }
        
        
        
        
        
/*
CREATE VIEW tiles AS   
SELECT 
     map.zoom_level as zoom_level 
    ,map.tile_column as tile_column 
    ,map.tile_row as tile_row 
    ,images.tile_data as tile_data 
FROM map 
JOIN images ON map.tile_id = images.tile_id
*/


        public static void Test()
        {
            string basePath = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot";
            basePath = @"/root/github/VectorTileServer/VectorTileServer/wwwroot";
            string path = System.IO.Path.Combine( basePath, @"2017-07-03_france_monaco.mbtiles");
            
            VectorTileRenderer.Sources.MbTilesSource source = 
                new VectorTileRenderer.Sources.MbTilesSource(path);
            
            // L.tileLayer("{server}/{style}/{z}/{x}/{y}{scalex}.png",
            // https://maps.wikimedia.org/osm-intl/18/136472/95588.png?lang=en
            // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#VB.Net
            
            int x = 136472;
            int y = 95588;
            int zoom = 14;

            double latitude = 43.735076;
            double longitude = 7.421051;
            TileCoords tile = TileCoords.FromWgs84(latitude, longitude, zoom);
            
            Wgs84Coordinates wgs84 = Wgs84Coordinates.FromTile(tile);
            System.Console.WriteLine(wgs84);
            System.Console.WriteLine(tile);
            
            
            // function long2tile(lon,zoom) { return (Math.floor((lon+180)/360*Math.pow(2,zoom))); }
            // function lat2tile(lat,zoom)  { return (Math.floor((1-Math.log(Math.tan(lat*Math.PI/180) + 1/Math.cos(lat*Math.PI/180))/Math.PI)/2 *Math.pow(2,zoom))); }
            // console.log({ x: lat2tile(43.735076,14), y: long2tile(7.421051,14) } );
            // {x: 5974, y: 8529}
            
            /*
            function tile2long(x,z) {
                return (x/Math.pow(2,z)*360-180);
            }
            function tile2lat(y,z) {
                var n=Math.PI-2*Math.PI*y/Math.pow(2,z);
                return (180/Math.PI*Math.atan(0.5*(Math.exp(n)-Math.exp(-n))));
            }
            */

            
            
            // SELECT * FROM tiles WHERE tile_column = 8529 and tile_row = 5974 and zoom_level = 14
            string constr = string.Format("Data Source={0};Version=3;", path);
            string sql = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", tile.X, tile.Y, zoom) ;
            System.Console.WriteLine(sql, constr);
            
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(latitude, longitude, zoom);
            System.Console.WriteLine(ta);
            // using (System.IO.Stream strm = source.GetRawTile(tile.X, tile.Y, zoom))
            // using (System.IO.Stream strm = source.GetRawTile(ta.X, ta.Y, zoom))
            
            byte[] result = null;
            
            using (System.IO.Stream strm = source.GetRawTile(x, y, z))
            {
                
                try
                {
                    
#if STREAM_HAS_NO_LENGTH
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        strm.CopyTo(ms);
                        result = ms.ToArray();
                    } // End Using ms 
#else 
                    result = new byte[strm.Length];
                    strm.Read(result, 0, result.Length);
#endif
                    System.Console.WriteLine(result);
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    // System.Console.ReadKey();
                } // End Catch 
                
            } // End Using strm 

        }


        public static System.IO.Stream GetTileStream(int x, int y, int z)
        {
            string basePath = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot";
            basePath = @"/root/github/VectorTileServer/VectorTileServer/wwwroot";
            string path = System.IO.Path.Combine( basePath, @"2017-07-03_france_monaco.mbtiles");
            
            VectorTileRenderer.Sources.MbTilesSource source = 
                new VectorTileRenderer.Sources.MbTilesSource(path);
            
            return source.GetRawTile(x, y, z);
        } // End Function GetTileStream 
        
        
        [HttpGet]
        public FileStreamResult GetTest()
        {
            int zoom = 14;
            double latitude = 43.735076;
            double longitude = 7.421051;
            
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(latitude, longitude, zoom);

            
            int x = ta.X;
            int y = ta.Y;
            int z = zoom;
            
            
            // System.IO.Stream stream = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes("Hello World"));
            System.IO.Stream stream = GetTileStream(x, y, z);
            
            
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
            Response.Headers["accept-ranges"] = "bytes";
            Response.Headers["access-control-allow-origin"] = "*";
            Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
            Response.Headers["content-encoding"] = "gzip";
            Response.Headers["content-length"] = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Response.Headers["date"] = "gzip";
            Response.Headers["expect-ct"] = "max-age=604800";
            Response.Headers["vary"] = "Accept-Encoding";
            Response.StatusCode = 200;
            
            
            return new FileStreamResult(stream, "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                FileDownloadName = "test.txt"
            };
        }
        

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
