
namespace VectorTileServer.Controllers
{


    public class TilesController 
        : Microsoft.AspNetCore.Mvc.Controller
    {

        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment m_env;
        

        public TilesController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.m_env = env;
        } // End Constructor 



        private static string s_tileSQL = @"
CREATE VIEW tiles AS 
SELECT 
        map.zoom_level as zoom_level 
    ,map.tile_column as tile_column 
    ,map.tile_row as tile_row 
    ,images.tile_data as tile_data 
FROM map 
JOIN images ON map.tile_id = images.tile_id 
; ";


        protected System.IO.Stream GetTileStream(int x, int y, int z)
        {
            string webRoot = this.m_env.WebRootPath;
            string path = System.IO.Path.Combine(webRoot, @"2017-07-03_france_monaco.mbtiles");

            VectorTileRenderer.Sources.MbTilesSource source =
                new VectorTileRenderer.Sources.MbTilesSource(path);


            // https://stackoverflow.com/questions/4377106/asp-net-webservice-handling-gzip-compressed-request
            // https://weblog.west-wind.com/posts/2007/Jun/29/HttpWebRequest-and-GZip-Http-Responses


            System.IO.Stream gzippedTileStream = source.GetRawTile(x, y, z);

            // if(this.HttpContext.Request.Headers.ContainsKey(""))
            bool supportsGzip = true;
            if (supportsGzip)
                return gzippedTileStream;

            return UnzipStream(gzippedTileStream);
        } // End Function GetTileStream 


        private System.IO.Stream UnzipStream(System.IO.Stream stream)
        {
            if (isGZipped(stream))
            {
                System.IO.MemoryStream resultStream = new System.IO.MemoryStream();

                using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                {

                    zipStream.CopyTo(resultStream);
                    resultStream.Seek(0, System.IO.SeekOrigin.Begin);
                    // return await loadStream(resultStream);
                    return resultStream;

                } // End Using zipStream 
            } // End if (isGZipped(stream)) 

            return stream;
        } // End Function UnzipStream 
        

        // D:\username\Documents\Visual Studio 2017\Projects\OsmInvestigate\VectorTileRenderer\VectorTileRenderer\Sources\PbfTileSource.cs
        private async System.Threading.Tasks.Task<VectorTileRenderer.VectorTile> LoadStream(System.IO.Stream stream)
        {
            if (isGZipped(stream))
            {
                using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (System.IO.MemoryStream resultStream = new System.IO.MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        resultStream.Seek(0, System.IO.SeekOrigin.Begin);
                        // return await loadStream(resultStream);
                    } // End Using resultStream 

                } // End Using zipStream 
            }
            else
            {
                // return await loadStream(stream);
            }

            return await System.Threading.Tasks.Task.FromResult<VectorTileRenderer.VectorTile>(null);
        }


        private bool isGZipped(System.IO.Stream stream)
        {
            return StreamStartsWith(stream, 3, "1F-8B-08");
        }



        private bool isZipped(System.IO.Stream stream)
        {
            return StreamStartsWith(stream, 4, "50-4B-03-04");
        }


        private bool StreamStartsWith(System.IO.Stream stream, int signatureSize, string expectedSignature)
        {
            if (stream.Length < signatureSize)
                return false;

            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            } // Whend 

            stream.Seek(0, System.IO.SeekOrigin.Begin);
            string actualSignature = System.BitConverter.ToString(signature);

            return string.Equals(actualSignature, expectedSignature, System.StringComparison.OrdinalIgnoreCase);
        } // End Function isZipped 



        // https://blogs.msdn.microsoft.com/webdev/2013/10/17/attribute-routing-in-asp-net-mvc-5/
        // https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.2
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("tiles/{x:int}/{y:int}/{z:int}")]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTile(int x, int y, int z)
        {

#if true
            Wgs84Coordinates wgs84 = Wgs84Coordinates.FromTile(x, y, z);
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(wgs84.Latitude, wgs84.Longitude, wgs84.ZoomLevel);
            System.IO.Stream stream = GetTileStream(ta.X, ta.Y, z);
#else
            System.IO.Stream stream = GetTileStream(x, y, z);
#endif


            if (stream == null)
                return null;

            // accept-ranges: bytes
            // access-control-allow-origin: *
            // cache-control: public, max-age=86400, no-transform
            // content-encoding: gzip
            // content-length: 38832
            // date: Sat, 09 Feb 2019 11:06:10 GMT
            // expect-ct: max-age=604800, report-uri="https://report-uri.cloudflare.com/cdn-cgi/beacon/expect-ct"
            // last-modified: Thu, 07 Feb 2019 11:44:10 GMT
            // status: 200
            // vary: Accept-Encoding

            Response.StatusCode = 200;
            Response.Headers["accept-ranges"] = "bytes";
            Response.Headers["access-control-allow-origin"] = "*";
            Response.Headers["cache-control"] = "public, max-age=86400, no-transform";
            Response.Headers["content-encoding"] = "gzip";
            Response.Headers["content-length"] = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Response.Headers["date"] = "gzip";
            Response.Headers["expect-ct"] = "max-age=604800";
            Response.Headers["vary"] = "Accept-Encoding";


            return new Microsoft.AspNetCore.Mvc.FileStreamResult(stream, "application/x-protobuf")
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };

        } // End Function GetTile 


    } // End Class TilesController : Controller 


} // End Namespace VectorTileServer.Controllers 
