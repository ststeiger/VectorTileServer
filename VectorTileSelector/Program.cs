
namespace VectorTileSelector
{


    class Program
    {

        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            // SizeParser.Test();
            // await GeofabrikDownloader.FetchAndDownloadAsync(Db.KmlDirectory, Db.Connection);
            // await SizeUpdater.UpdateSizeAsync();

            SimplePolygonParser.Test();




            // MollweideArea.Test();
            // await MapTilerSizeCheck.UpdateTileSize(Db.TileSizeDirectory, Db.Connection);


            // await TestAgility.Test();
            // await TestAngle.Test();

            await System.Console.Out.WriteLineAsync("Finished !");
            return 0;
         } // End Task Main 


    } // End Class Program 


} // End Namespace 
