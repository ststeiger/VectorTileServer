
namespace VectorTileServer2
{


    public enum SQLOptions
    { 
    
    }



    /// <summary>
    /// Some Middleware that exposes something 
    /// </summary>
    public class TilesMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate m_next;
        private readonly SQLOptions m_options;
        private readonly libWebAppBasics.Database.IConnectionFactory m_factory;


        /// <summary>
        /// Creates a new instance of <see cref="SomeMiddleware"/>.
        /// </summary>
        public TilesMiddleware(
              Microsoft.AspNetCore.Http.RequestDelegate next
            , Microsoft.AspNetCore.Hosting.IWebHostEnvironment env
            , Microsoft.Extensions.Options.IOptions<object> someOptions
            , libWebAppBasics.Database.IConnectionFactory db_factory
        )
        {
            if (next == null)
            {
                throw new System.ArgumentNullException(nameof(next));
            }

            if (someOptions == null)
            {
                throw new System.ArgumentNullException(nameof(someOptions));
            }

            if (db_factory == null)
            {
                throw new System.ArgumentNullException(nameof(db_factory));
            }

            this.m_next = next;
            if(someOptions != null && someOptions.Value != null)
                this.m_options = (SQLOptions)someOptions.Value;

            this.m_factory = db_factory;
        } // End Constructor 




        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        } // End Function FromTmsY 




        /// <summary>
        /// Processes a request.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task InvokeAsync(
            Microsoft.AspNetCore.Http.HttpContext httpContext
        )
        {
            if (httpContext == null)
            {
                throw new System.ArgumentNullException(nameof(httpContext));
            }


            // [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("tiles/{x:int}/{y:int}/{z:int}")]

            Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues = 
                Microsoft.AspNetCore.Routing.RoutingHttpContextExtensions.GetRouteData(httpContext)
                .Values;

            object objX = routeValues["x"];
            object objY = routeValues["y"];
            object objZ = routeValues["z"];

            int x = System.Int32.Parse(System.Convert.ToString(objX, System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);
            int y = System.Int32.Parse(System.Convert.ToString(objY, System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);
            int zoom = System.Int32.Parse(System.Convert.ToString(objZ, System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);

            y = FromTmsY(y, zoom);



            // httpContext.Response.StatusCode = 200;
            // The Cache-Control is per the HTTP 1.1 spec for clients and proxies
            // If you don't care about IE6, then you could omit Cache-Control: no-cache.
            // (some browsers observe no-store and some observe must-revalidate)
            httpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            // Other Cache-Control parameters such as max-age are irrelevant 
            // if the abovementioned Cache-Control parameters (no-cache,no-store,must-revalidate) are specified.


            // Expires is per the HTTP 1.0 and 1.1 specs for clients and proxies. 
            // In HTTP 1.1, the Cache-Control takes precedence over Expires, so it's after all for HTTP 1.0 proxies only.
            // If you don't care about HTTP 1.0 proxies, then you could omit Expires.
            httpContext.Response.Headers["Expires"] = "-1, 0, Tue, 01 Jan 1980 1:00:00 GMT";

            // The Pragma is per the HTTP 1.0 spec for prehistoric clients, such as Java WebClient
            // If you don't care about IE6 nor HTTP 1.0 clients 
            // (HTTP 1.1 was introduced 1997), then you could omit Pragma.
            httpContext.Response.Headers["pragma"] = "no-cache";


            // On the other hand, if the server auto-includes a valid Date header, 
            // then you could theoretically omit Cache-Control too and rely on Expires only.

            // Date: Wed, 24 Aug 2016 18:32:02 GMT
            // Expires: 0


            try
            {
                using (System.Data.Common.DbConnection conn = this.m_factory.Connection)
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    using (System.Data.Common.DbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", x, y, zoom); ;

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

        } // End Task InvokeAsync 


    } // End Class TilesMiddleware 


} // End Namespace 
