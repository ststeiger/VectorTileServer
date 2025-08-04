
namespace SpriteGenerator
{


    internal class Program
    {


        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            try
            {
                // SvgRasterizer.Test();

                int hpadding = 1;
                int vpadding = 1;
                SpriteBuilder builder = new SpriteBuilder(hpadding, vpadding);



                string basePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "data");
                basePath = System.IO.Path.GetFullPath(basePath);


                string targetBasePath = System.IO.Path.Combine(basePath, "..", "..", "VectorTileServer", "wwwroot", "styles", "bright");
                targetBasePath = System.IO.Path.GetFullPath(targetBasePath);


                // string inputFolder = System.IO.Path.Combine(basePath, "rasterized");
                string inputFolder = System.IO.Path.Combine(basePath, "extracted_icons");
                string outputFile = @"sprite";
                bool cropTransparency = false;
                bool roundUpPower2 = false;
                int pixelRatio = 1;

                builder.BuildSheet(inputFolder, outputFile, cropTransparency, roundUpPower2, pixelRatio);

                string moveFromJson1 = System.IO.Path.Combine(inputFolder, outputFile + ".json");
                string moveFromPng1 = System.IO.Path.Combine(inputFolder, outputFile + ".png");

                string moveToJson1 = System.IO.Path.Combine(targetBasePath, outputFile + ".json");
                string moveToPng1 = System.IO.Path.Combine(targetBasePath, outputFile + ".png");





                // inputFolder = System.IO.Path.Combine(basePath, "rasterized2");
                inputFolder = System.IO.Path.Combine(basePath, "extracted_icons2");
                outputFile = "sprite@2x";
                pixelRatio = 2;

                builder.BuildSheet(inputFolder, outputFile, cropTransparency, roundUpPower2, pixelRatio);


                string moveFromJson2 = System.IO.Path.Combine(inputFolder, outputFile + ".json");
                string moveFromPng2 = System.IO.Path.Combine(inputFolder, outputFile + ".png");

                string moveToJson2 = System.IO.Path.Combine(targetBasePath, outputFile + ".json");
                string moveToPng2 = System.IO.Path.Combine(targetBasePath, outputFile + ".png");

                System.IO.File.Copy(moveFromJson1, moveToJson1, true);
                System.IO.File.Copy(moveFromPng1, moveToPng1, true);

                System.IO.File.Copy(moveFromJson2, moveToJson2, true);
                System.IO.File.Copy(moveFromPng2, moveToPng2, true);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            }


            await System.Console.Out.WriteLineAsync(" --- Press any key to continue --- ");
            return 0;
        }


    } // End Class Program 


}
