
namespace VectorTileServer2
{


    /// <summary>
    /// Some Middleware that exposes something 
    /// </summary>
    public class FontsMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate m_next;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment m_env;



        /// <summary>
        /// Creates a new instance of <see cref="SomeMiddleware"/>.
        /// </summary>
        public FontsMiddleware(
              Microsoft.AspNetCore.Http.RequestDelegate next
            , Microsoft.AspNetCore.Hosting.IWebHostEnvironment env
        )
        {
            if (next == null)
            {
                throw new System.ArgumentNullException(nameof(next));
            }

            if (env == null)
            {
                throw new System.ArgumentNullException(nameof(env));
            }

            this.m_next = next;
            this.m_env = env;
        } // End Constructor 


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

            // [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("fonts/{fontstack}/{range}")]

            Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues =
                Microsoft.AspNetCore.Routing.RoutingHttpContextExtensions.GetRouteData(httpContext)
                .Values;

            object objFontStack = routeValues["fontstack"];
            object objRange = routeValues["range"];

            string fontstack = System.Convert.ToString(objFontStack, System.Globalization.CultureInfo.InvariantCulture);
            string range = System.Convert.ToString(objRange, System.Globalization.CultureInfo.InvariantCulture);
            

            string basePath = System.IO.Path.Combine(this.m_env.WebRootPath, "fonts");
            string fontDir = System.IO.Path.Combine(basePath, "KlokanTech " + fontstack);
            string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");

            if (!System.IO.File.Exists(fontFile))
            {
                httpContext.Response.StatusCode = 404;
                return;
            } // End if (!System.IO.File.Exists(fontFile)) 


            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "application/x-protobuf";

            httpContext.Response.Headers["Date"] = "Wed, 13 Feb 2019 12:00:03 GMT";
            httpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            httpContext.Response.Headers["Cache-Control"] = "no-transform, public, max-age=86400";
            httpContext.Response.Headers["Last-Modified"] = "Thu, 07 Feb 2019 09:43:32 GMT";

            using (System.IO.Stream strm = System.IO.File.OpenRead(fontFile))
            {
                await strm.CopyToAsync(httpContext.Response.Body);
            }

        } // End Task InvokeAsync 


    } // End Class FontsMiddleware 


} // End Namespace 
