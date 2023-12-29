
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VectorTileServer2
{


    public class Startup
    {

        private readonly Microsoft.Extensions.Configuration.IConfiguration m_configuration;
        private readonly IWebHostEnvironment m_webHostEnvironment;

        public Startup(
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            IWebHostEnvironment env
        )
        {
            this.m_configuration = configuration;
            this.m_webHostEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string path = System.IO.Path.Combine(this.m_webHostEnvironment.WebRootPath, @"2017-07-03_france_monaco.mbtiles");
            if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
                path = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";

            libWebAppBasics.Database.IConnectionFactory factory = 
                new libWebAppBasics.Database.ConnectionFactory(
                  string.Format("Data Source={0};Version=3;", path)
                , typeof(System.Data.SQLite.SQLiteFactory)
            );

            services.AddSingleton(factory);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }



            DefaultFilesOptions dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add("index.htm");
            dfo.DefaultFileNames.Add("index.html");

            app.UseDefaultFiles(dfo);
            app.UseStaticFiles();
            // app.UseCookiePolicy();


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Caution: style.json, v3.json has path/port
                endpoints.MapEndpoint<FontsMiddleware>("fonts/{fontstack}/{range}");
                endpoints.MapEndpoint<TilesMiddleware>("tiles/{x:int}/{y:int}/{z:int}");

                endpoints.MapGet("/ping", async context =>
                {
                    string time = System.DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                    await context.Response.WriteAsync("Time:" + time);
                });
            });
        }


    }


}
