
using System;
using Microsoft.Extensions.DependencyInjection; // for ConfigureHttpJsonOptions
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions; // for MapGroup, MapGet



namespace VectorTileServer3
{


    public class Program
    {

        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        } // End Function FromTmsY 
        
        
        public static async System.Threading.Tasks.Task DynamicPathAdjustedJsonHandlerAsync(
            Microsoft.AspNetCore.Http.HttpContext context
            , IWebHostEnvironment env 
        )
        {
            System.Console.Write("DisplayUrl: ");
            System.Console.WriteLine(context.Request.GetDisplayUrl());
            System.Console.Write("Host: ");
            System.Console.WriteLine(context.Request.Host.Host);
            System.Console.Write("PathBase: ");
            System.Console.WriteLine( context.Request.PathBase);
            
            
            string fn = System.IO.Path.GetFileName(context.Request.Path);
            
            // System.Console.WriteLine(context.Request.Protocol); // HTTP/1.1
            string server = context.Request.Scheme + System.Uri.SchemeDelimiter + context.Request.Host.Host;
                
            if(context.Request.Host.Port.HasValue && context.Request.Host.Port.Value != 80 && context.Request.Host.Port.Value != 443)
                server = server + ":" + System.Convert.ToString( context.Request.Host.Port.Value, System.Globalization.CultureInfo.InvariantCulture);

            if (!server.EndsWith("/"))
                server += "/";
            
            
            if ("style.json".Equals(fn))
            {
                string mapped = System.IO.Path.Combine(env.WebRootPath, "styles", "bright", "style_TEMPLATE.json");
                string content = await System.IO.File.ReadAllTextAsync(mapped, System.Text.Encoding.UTF8);
                content = content.Replace("{@server}", server);
                
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(content);
                return;
            }
            else if ("v3.json".Equals(fn))
            {
                string mapped = System.IO.Path.Combine(env.WebRootPath, "styles", "bright", "v3_TEMPLATE.json");
                string content = await System.IO.File.ReadAllTextAsync(mapped, System.Text.Encoding.UTF8);
                content = content.Replace("{@server}", server);
                
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(content);
                return;
            }
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            string styleJson = "{ \"error\": \"404\", \"message\": \"Not Found\", \"code\": 404 }";
            await context.Response.WriteAsync(styleJson);
        }
        
        

        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            string? appPath = System.Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
            System.Console.WriteLine(appPath);
            
            IIS_ConfigurationData props = IIS_ConfigurationData.Init();
            System.Console.WriteLine(props);

            
            
            
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                //options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Standard);
            });

            // string path = System.IO.Path.Combine(builder.Environment.WebRootPath, "switzerland.mbtiles");
            string path = System.IO.Path.Combine(builder.Environment.WebRootPath, "liechtenstein.mbtiles");

            //if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
            //    path = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";

            if ("prodesk".Equals(System.Environment.MachineName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                path = "/root/docker/openmaptiles/data/liechtenstein.mbtiles";
            }
            
            
            if ("ThinkPadT16".Equals(System.Environment.MachineName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                path = "/root/docker/openmaptiles/data/switzerland.mbtiles";
                // path = "/root/docker/openmaptiles/data/liechtenstein.mbtiles";
            }
            
            libWebAppBasics.Database.IConnectionFactory factory =
                new libWebAppBasics.Database.ConnectionFactory(
                  string.Format("Data Source={0};Version=3;", path)
                , typeof(System.Data.SQLite.SQLiteFactory)
            );

            builder.Services.AddSingleton(factory);


            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();
            
            
            app.MapGet("/styles/bright/v3.json", DynamicPathAdjustedJsonHandlerAsync);
            app.MapGet("/styles/bright/style.json", DynamicPathAdjustedJsonHandlerAsync);
            

            app.MapGet("/test", async delegate (Microsoft.AspNetCore.Http.HttpContext context) {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("Hello World !");
            });


            app.MapGet("fonts/{fontstack}/{range}", async delegate (Microsoft.AspNetCore.Http.HttpContext context, string fontstack, string range, IWebHostEnvironment env) {
                string basePath = System.IO.Path.Combine(env.WebRootPath, "fonts");
                string fontDir = System.IO.Path.Combine(basePath, "KlokanTech " + fontstack);
                string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");

                if (!System.IO.File.Exists(fontFile))
                {
                    context.Response.StatusCode = 404;
                    return;
                } // End if (!System.IO.File.Exists(fontFile)) 


                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/x-protobuf";

                context.Response.Headers["Date"] = "Wed, 13 Feb 2019 12:00:03 GMT";
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Cache-Control"] = "no-transform, public, max-age=86400";
                context.Response.Headers["Last-Modified"] = "Thu, 07 Feb 2019 09:43:32 GMT";

                using (System.IO.Stream strm = System.IO.File.OpenRead(fontFile))
                {
                    await strm.CopyToAsync(context.Response.Body);
                }

            });


            app.MapGet("tiles/{x:int}/{y:int}/{z:int}", async delegate (Microsoft.AspNetCore.Http.HttpContext httpContext, int x, int y, int z
                , libWebAppBasics.Database.IConnectionFactory factory
                ) 
            {

                try
                {
                    y = FromTmsY(y, z);
                    long fused64 = ((long)x << 32) | (long)y;

                    using (System.Data.Common.DbConnection conn = factory.Connection)
                    {
                        if (conn.State != System.Data.ConnectionState.Open)
                            conn.Open();

                        using (System.Data.Common.DbCommand cmd = conn.CreateCommand())
                        {
                            // cmd.CommandText = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", x, y, z);
                            cmd.CommandText = string.Format("SELECT tms_tile AS tile_data FROM tms_map WHERE tms_zoom = {0} and tms_xy = {1}", z, fused64);

                            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
                            {

                                if (reader.Read())
                                {

                                    using (System.IO.Stream stream = reader.GetStream(reader.GetOrdinal("tile_data")))
                                    {
                                        if (stream == null)
                                        {
                                            httpContext.Response.StatusCode = 404;
                                            return;
                                        } // End if (stream == null)

                         
                                        // ,map.tile_id = MD5 
                                        // using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                                        // {
                                        //     byte[] hashBytes = md5.ComputeHash(stream);
                                        //     string ba = System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                        //     System.Console.WriteLine("x: {0}, y: {1}, z: {2} md5: {3}", x, y, z, ba);
                                        //     stream.Position = 0;
                                        // }
                                        
                                        
                                        httpContext.Response.StatusCode = 200;
                                        httpContext.Response.ContentType = "application/x-protobuf";


                                        httpContext.Response.Headers["accept-ranges"] = "bytes";
                                        httpContext.Response.Headers["access-control-allow-origin"] = "*";
                                        httpContext.Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
                                        // context.Response.Headers["date"] = "Sat, 09 Feb 2019 11:06:10 GMT";
                                        // context.Response.Headers["expect-ct"] = "max-age=604800, report-uri="https://report-uri.cloudflare.com/cdn-cgi/beacon/expect-ct";
                                        // context.Response.Headers["last-modified"] = "Thu, 07 Feb 2019 11:44:10 GMT";
                                        httpContext.Response.Headers["vary"] = "Accept-Encoding";


                                        // http://www.binaryintellect.net/articles/1b60420c-39ca-4f01-9813-0951e553e146.aspx
                                        // https://weblog.west-wind.com/posts/2007/Jun/29/HttpWebRequest-and-GZip-Http-Responses
                                        string acceptencoding = httpContext.Request.Headers["Accept-Encoding"];
                                        if (acceptencoding != null && acceptencoding.ToLowerInvariant().Contains("gzip"))
                                        {
                                            httpContext.Response.Headers["content-encoding"] = "gzip";
                                            httpContext.Response.Headers["content-length"] = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                            await stream.CopyToAsync(httpContext.Response.Body);
                                        }
                                        else
                                        {
                                            using (System.IO.Stream rawStream = StreamHelper.Uncompress<System.IO.Compression.GZipStream>(stream))
                                            {

                                                if (acceptencoding.Contains("br"))
                                                {
                                                    httpContext.Response.Headers["content-encoding"] = "br";
                                                    using (System.IO.Stream brotliStream = StreamHelper.Compress<System.IO.Compression.BrotliStream>(rawStream))
                                                    {
                                                        httpContext.Response.Headers["content-length"] = brotliStream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                        await brotliStream.CopyToAsync(httpContext.Response.Body);
                                                    } // End Using brotliStream 
                                                }
                                                else if (acceptencoding.Contains("deflate"))
                                                {
                                                    httpContext.Response.Headers["content-encoding"] = "deflate";
                                                    using (System.IO.Stream deflateStream = StreamHelper.Compress<System.IO.Compression.DeflateStream>(rawStream))
                                                    {
                                                        httpContext.Response.Headers["content-length"] = deflateStream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                        await deflateStream.CopyToAsync(httpContext.Response.Body);
                                                    } // End Using deflateStream 
                                                }
                                                else
                                                {
                                                    httpContext.Response.Headers["content-length"] = rawStream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    await rawStream.CopyToAsync(httpContext.Response.Body);
                                                }
                                            } // End Using rawStream 

                                        } // End else of if gzip 

                                    } // End Using stream

                                } // End if (reader.Read())

                            } // End Using reader 

                        } // End Using cmd 

                        if (conn.State != System.Data.ConnectionState.Closed)
                            conn.Close();
                    } // End Using conn 
                } // End Try 
                catch
                {
                    throw new System.IO.InvalidDataException("Could not load tile from Mbtiles");
                } // End Catch 

            });


            app.MapGet("/ping", async context =>
            {
                string time = System.DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                await context.Response.WriteAsync("\r\nTime:" + time);
                await context.Response.WriteAsync("\r\nDomain:" + System.Environment.UserDomainName);
                await context.Response.WriteAsync("\r\nUser:" + System.Environment.UserName);
            });

            DefaultFilesOptions dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add("index.htm");
            dfo.DefaultFileNames.Add("index.html");

            app.UseDefaultFiles(dfo);
            app.UseStaticFiles();
            // app.UseCookiePolicy();
            
            app.Run();

            return await System.Threading.Tasks.Task.FromResult(0);
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
