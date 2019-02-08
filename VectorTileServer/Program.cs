using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;



using System.Windows;

namespace VectorTileServer
{

    public class TileCoords
    {
        public int X;
        public int Y;
    }

    public class Program
    {


        private static TileCoords CalcTileXY(double lat, double lon, long zoom)
        {
            TileCoords CalcTileXY = new TileCoords();

            CalcTileXY.X = System.Convert.ToInt32(Math.Floor((lon + 180) / 360.0 * Math.Pow(2, zoom)));
            CalcTileXY.Y = System.Convert.ToInt32(Math.Floor((1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, zoom)));

            return CalcTileXY;
        }



        public static void Main(string[] args)
        {

            VectorTileRenderer.Sources.MbTilesSource source = 
                new VectorTileRenderer.Sources.MbTilesSource(@"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot\2017-07-03_france_monaco.mbtiles");

            //     L.tileLayer("{server}/{style}/{z}/{x}/{y}{scalex}.png",
            // https://maps.wikimedia.org/osm-intl/18/136472/95588.png?lang=en

            int x = 136472;
            int y = 95588;
            int zoom = 14;

            double latitude = 43.735076;
            double longitude = 7.421051;
            TileCoords tile = CalcTileXY(latitude, longitude, zoom);



            byte[] result = null;

            using (System.IO.Stream strm = source.GetRawTile(tile.X, tile.Y, zoom))
            {

                using (var ms = new System.IO.MemoryStream())
                {
                    strm.CopyTo(ms);
                    result = ms.ToArray();
                }
                System.Console.WriteLine(result);   
            }

            // CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
