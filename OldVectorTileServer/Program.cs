
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
            CreateWebHostBuilder(args).Build().Run();
        } // End Sub Main 
        
        
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder 
            CreateWebHostBuilder(string[] args)
        {
            return Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        } // End Function CreateWebHostBuilder 
        
        
    } // End Class Program 
    
    
} // End Namespace VectorTileServer 
