
namespace VectorTileServer5
{

    using Microsoft.AspNetCore.Builder;
    // using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;


    public record Todo(int Id, string? Title, System.DateOnly? DueBy = null, bool IsComplete = false);



    public class Program
    {


        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            await TestSQLite.Test();

            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = 
                Microsoft.AspNetCore.Builder.WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });


            string path = System.IO.Path.Combine(builder.Environment.WebRootPath, "liechtenstein.mbtiles");
            path = @"D:\stefan.steiger\Documents\Visual Studio 2022\gitlab\VectorTileServer\VectorTileServer\App_Data\COR_switzerland.mbtiles";
            path = @"D:\Programme\LessPortableApps\osm\planetiler\downloaded\dach.mbtiles";



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

            // Serve files from "wwwroot/files", under "/downloads", with 200 KB/s
            app.UseMiddleware<ThrottledStaticFileMiddleware>(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "files"),
                "/downloads",
                200 * 1024*5
            );

            app.UseRouting();


            // app.UseAuthorization();

            // app.MapRazorPages();

            app.MapGet("tiles/{x:int}/{y:int}/{z:int}", TileHandler.GetTileAsync);
            app.MapGet("fonts/{fontstack}/{range}", TileHandler.GetFontAsync);


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

            await app.RunAsync();

            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
