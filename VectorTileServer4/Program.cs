
namespace VectorTileServer4
{

    using Microsoft.AspNetCore.Builder; // use* 
    using Microsoft.Extensions.DependencyInjection; // Add* 


    public class Program
    {


        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // Add services to the container.
            // builder.Services.AddRazorPages();
            // string path = System.IO.Path.Combine(builder.Environment.WebRootPath, "switzerland.mbtiles");
            string path = System.IO.Path.Combine(builder.Environment.WebRootPath, "liechtenstein.mbtiles");
            path = @"D:\stefan.steiger\Documents\Visual Studio 2022\gitlab\VectorTileServer\VectorTileServer\App_Data\COR_switzerland.mbtiles";

            libWebAppBasics.Database.IConnectionFactory factory =
                new libWebAppBasics.Database.ConnectionFactory(
                  string.Format("Data Source={0};Version=3;", path)
                , typeof(SQLite.FullyManaged.SqliteClientFactory)
            );

            builder.Services.AddSingleton(factory);


            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.IsDevelopment(app.Environment))
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();


            DefaultFilesOptions dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add("index.htm");
            dfo.DefaultFileNames.Add("index.html");

            app.UseDefaultFiles(dfo);

            app.UseStaticFiles();

            app.UseRouting();

            // app.UseAuthorization();

            // app.MapRazorPages();

            app.MapGet("tiles/{x:int}/{y:int}/{z:int}", TileHandler.GetTileAsync);
            app.MapGet("fonts/{fontstack}/{range}", TileHandler.GetFontAsync);


            app.MapGet("/styles/bright/v3.json", TileHandler.DynamicPathAdjustedJsonHandlerAsync);
            app.MapGet("/styles/bright/style.json", TileHandler.DynamicPathAdjustedJsonHandlerAsync);

            await app.RunAsync();

            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
