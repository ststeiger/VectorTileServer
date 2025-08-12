
namespace VectorTileServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;


    public class Program
    {


        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder =
                Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

#if AOT
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });
#endif 

            builder.Services.AddSingleton(builder.Environment);

            string path = @"D:\stefan.steiger\Documents\Visual Studio 2022\gitlab\VectorTileServer\VectorTileServer\App_Data\COR_switzerland.mbtiles";
            
            path = @"D:\Programme\LessPortableApps\osm\planetiler\downloaded\dach.mbtiles";


            // To use it:
            string downloadsPath = DownloadsPath.GetDownloadsFolderPath();
            path = System.IO.Path.Combine(downloadsPath, "taiwan.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "benelux.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "mexico.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "central-america.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "iceland.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "ionian_sea.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "turkish_aegean_sea.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "iran.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "moldova.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "montenegro.mbtiles");




            path = System.IO.Path.Combine(downloadsPath, "europe.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "dach.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "switzerland.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "swiss_li.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "swiss_extract.mbtiles");
            path = System.IO.Path.Combine(downloadsPath, "baltics.mbtiles");

            
            


            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "zurich.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "zug.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "aargau.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "solothurn.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "bern.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "vaud.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "geneva.mbtiles");


            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "bahamas.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "maldives.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "azores.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "cyprus.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "malta.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "faroe.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "isle-of-man.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "guernsey-jersey.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "gibraltar.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "singapore.mbtiles");


            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "ticino.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "vorarlberg.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "sankt_gallen.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "thurgau.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "schaffhausen.mbtiles");


            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "andorra.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "monaco.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "luxemburg.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "san_marino.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "vatican_city.mbtiles");
            path = System.IO.Path.Combine(builder.Environment.WebRootPath, "maps", "liechtenstein.mbtiles");



            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder csb = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            csb.DataSource = path;
            csb.Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly;
            csb.Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
            csb.ForeignKeys = false;
            csb.Pooling = true;
            csb.RecursiveTriggers = false;
            csb.DefaultTimeout = 30;


            libWebAppBasics.Database.IConnectionFactory factory =
                new libWebAppBasics.Database.ConnectionFactory(
                  csb.ConnectionString
                , typeof(Microsoft.Data.Sqlite.SqliteFactory)
            );

            builder.Services.AddSingleton(factory);



            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            // add package Microsoft.AspNetCore.OpenApi
            builder.Services.AddOpenApi();




            // Add services to the container.
            builder.Services.AddRazorPages();

            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.

            if (!Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.IsDevelopment(app.Environment))
            {
                app.UseExceptionHandler(" / Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                app.MapOpenApi();
            }

            app.UseHttpsRedirection();



            DefaultFilesOptions dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add("index.htm");
            dfo.DefaultFileNames.Add("index.html");

            app.UseDefaultFiles(dfo);

            app.UseStaticFiles();



            app.UseRouting();

            app.UseAuthorization();



            app.MapGet("tiles/{x:int}/{y:int}/{z:int}", TileHandler.GetTileAsync);
            app.MapGet("fonts/{fontstack}/{range}", TileHandler.GetFont);


            app.MapGet("/styles/bright/v3.json", TileHandler.DynamicPathAdjustedJsonHandlerAsync);
            app.MapGet("/styles/bright/style.json", TileHandler.DynamicPathAdjustedJsonHandlerAsync);


            Todo[] sampleTodos = new Todo[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", System.DateOnly.FromDateTime(System.DateTime.Now)),
                new(3, "Do the laundry", System.DateOnly.FromDateTime(System.DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", System.DateOnly.FromDateTime(System.DateTime.Now.AddDays(2)))
            };

            Microsoft.AspNetCore.Routing.RouteGroupBuilder todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", () => sampleTodos);
            todosApi.MapGet("/{id}", (int id) =>
                System.Linq.Enumerable.FirstOrDefault(sampleTodos, a => a.Id == id) is { } todo
                    ? Microsoft.AspNetCore.Http.Results.Ok(todo)
                    : Microsoft.AspNetCore.Http.Results.NotFound());



            app.MapGet("/weatherforecast", (Microsoft.AspNetCore.Http.HttpContext httpContext) =>
            {
                string[] summaries = new string[]
                {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                };

                return summaries;
            })
            .WithName("GetWeatherForecast");



            app.MapStaticAssets();

            app.MapRazorPages()
               .WithStaticAssets();

            await app.RunAsync();

            return 0;
        } // End Task Main 


    } // End Class Program 

    public record Todo(int Id, string? Title, System.DateOnly? DueBy = null, bool IsComplete = false);


} // End Namespace 
