
namespace WikiDataImporter
{


    internal class Program
    {


        public static async System.Threading.Tasks.Task TestAsync()
        {
            // A ZIP file always starts with the bytes 50 4B 03 04 in hexadecimal, which is "PK" in ASCII.
            // A bzip2 file always starts with the bytes 42 5A 68 in hexadecimal, which is "BZh" in ASCII.
            // hexdump -C -n <n> <filename>
            // hexdump -C -n 32 my_binary_file
            DirectImporter.Test();
            await CompressedImporter.Test();
        } // End Task TestAsync 


        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            await TestAsync();

            await System.Console.Out.WriteLineAsync(" --- Press any key to continue --- ");
            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
