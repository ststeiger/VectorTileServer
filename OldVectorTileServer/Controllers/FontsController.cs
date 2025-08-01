
namespace VectorTileServer.Controllers
{


    public class FontsController 
        : Microsoft.AspNetCore.Mvc.Controller
    {

        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment m_env;
        


        public FontsController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.m_env = env;
        } // End Constructor 


        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("fonts/{fontstack}/{range}")]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetFont(string fontstack, string range)
        {
            string basePath = System.IO.Path.Combine(this.m_env.WebRootPath, "fonts");
            string fontDir = System.IO.Path.Combine(basePath, "KlokanTech " + fontstack);
            string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");

            if (!System.IO.File.Exists(fontFile))
            {
                Response.StatusCode = 404;
                return null;
            } // End if (!System.IO.File.Exists(fontFile)) 


            Response.StatusCode = 200;

            Response.Headers["Date"] = "Wed, 13 Feb 2019 12:00:03 GMT";
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Cache-Control"] = "no-transform, public, max-age=86400";
            Response.Headers["Last-Modified"] = "Thu, 07 Feb 2019 09:43:32 GMT";

            return new Microsoft.AspNetCore.Mvc.FileStreamResult(System.IO.File.OpenRead(fontFile), "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };
        } // End Function GetFont 


    } // End Class FontsController : Controller 


} // End Namespace VectorTileServer.Controllers 
