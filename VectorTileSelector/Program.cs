
namespace VectorTileSelector
{


    class Program
    {

        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            // SizeParser.Test();

            await TestAgility.Test();
            // await TestAngle.Test();

            await System.Console.Out.WriteLineAsync("Finished !");
            return 0;
        }

    }


}
