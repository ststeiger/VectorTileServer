
namespace osmprocessor
{


    internal class Program
    {


        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {

            // OsmConvertMerger.Test();

            // OsmBoundingBoxExtractor.Test();
            // OsmPbfMerger.Test();

            // https://boundingbox.klokantech.com/
            // https://planet.openstreetmap.org/
            // https://download.geofabrik.de/
            // https://download.openstreetmap.fr/
            // OsmPbfExtractor.Test();


            await PlanetilerRunner.Test();

            await System.Console.Out.WriteLineAsync(" --- Press any key to continue --- ");
            return 0;
        } // End Task Main 


    } // End Class Program


} // End Namespace 
