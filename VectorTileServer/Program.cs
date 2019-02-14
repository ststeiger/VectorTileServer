
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace VectorTileServer
{
    
    
    public class Program
    {
        
        
        public static void Main(string[] args)
        {


            string inputfile = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\LZF.cs";
            string outputfile = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\LZF.brotli";
            string decompressed = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\Decompressed.txt";

            StreamHelper.Compress<System.IO.Compression.DeflateStream>(inputfile, outputfile);
            StreamHelper.Uncompress<System.IO.Compression.DeflateStream>(outputfile, decompressed);

            // StreamHelper.Compress<System.IO.Compression.GZipStream>(inputfile, outputfile);
            // StreamHelper.Uncompress<System.IO.Compression.GZipStream>(outputfile, decompressed);

            // StreamHelper.Compress<System.IO.Compression.BrotliStream>(inputfile, outputfile);
            // StreamHelper.Uncompress<System.IO.Compression.BrotliStream>(outputfile, decompressed);

            // CreateWebHostBuilder(args).Build().Run();
        } // End Sub Main 


        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder 
            CreateWebHostBuilder(string[] args)
        {
            return Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        } // End Function CreateWebHostBuilder 
        
        
    } // End Class Program 
    
    
} // End Namespace VectorTileServer 
