
namespace osmprocessor
{
    internal class Program
    {


        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
          
            // OsmConvertMerger.Test();

            // OsmBoundingBoxExtractor.Test();
            // OsmPbfMerger.Test();
            // OsmPbfExtractor.Test();

            await PlanetilerRunner.Test();

            await System.Console.Out.WriteLineAsync(" --- Press any key to continue --- ");
            return 0;
        }


    }


}
