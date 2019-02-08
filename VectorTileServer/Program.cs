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
            string path = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot\2017-07-03_france_monaco.mbtiles";

            VectorTileRenderer.Sources.MbTilesSource source = 
                new VectorTileRenderer.Sources.MbTilesSource(path);

            //     L.tileLayer("{server}/{style}/{z}/{x}/{y}{scalex}.png",
            // https://maps.wikimedia.org/osm-intl/18/136472/95588.png?lang=en
            // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#VB.Net


            int x = 136472;
            int y = 95588;
            int zoom = 14;

            double latitude = 43.735076;
            double longitude = 7.421051;
            TileCoords tile = CalcTileXY(latitude, longitude, zoom);


            // SELECT * FROM tiles WHERE tile_column = 8529 and tile_row = 5974 and zoom_level = 14

            string constr = string.Format("Data Source={0};Version=3;", path);
            string sql = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", tile.X, tile.Y, zoom) ;
            System.Console.WriteLine(sql, constr);


            byte[] result = null;

            using (System.IO.Stream strm = source.GetRawTile(tile.X, tile.Y, zoom))
            {
                try
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        strm.CopyTo(ms);
                        result = ms.ToArray();
                    }
                    System.Console.WriteLine(result);
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    // System.Console.ReadKey();
                }

            }

            // CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
