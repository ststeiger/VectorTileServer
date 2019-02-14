
namespace VectorTileServer.Trash 
{


    public class VectorTileTrash 
    {


        private static Mapbox.VectorTile.VectorTile StreamToTile(System.IO.Stream stream)
        {
            Mapbox.VectorTile.VectorTile mbLayers = new Mapbox.VectorTile.VectorTile(StreamToByteArray(stream));

            return mbLayers;
        }
        

        private static byte[] StreamToByteArray(System.IO.Stream input)
        {
            byte[] retValue = null;

            byte[] buffer = new byte[16 * 1024];
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                retValue = ms.ToArray();
            }

            return retValue;
        }


        private static async System.Threading.Tasks.Task<VectorTileRenderer.VectorTile> LoadStream(System.IO.Stream stream)
        {
            if (StreamHelper.IsGZipped(stream))
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


    } // End Class VectorTileTrash 
    


    public class ControllerTrash 
    {

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment m_env;
        private Microsoft.AspNetCore.Http.HttpResponse Response;


        public ControllerTrash(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.m_env = env;
        }
        

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTestTest()
        {
            int zoom = 14;
            double latitude = 43.735076;
            double longitude = 7.421051;

            // https://epsg.io/900913
            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(latitude, longitude, zoom);


            int x = ta.X;
            int y = ta.Y;
            int z = zoom;

            return GetTest(x, y, z);
        }


        public Microsoft.AspNetCore.Mvc.FileStreamResult GetTest(int x, int y, int z)
        {
            return null;
        }


        public static void Test()
        {
            string basePath = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot";
            basePath = @"/root/github/VectorTileServer/VectorTileServer/wwwroot";
            string path = System.IO.Path.Combine(basePath, @"2017-07-03_france_monaco.mbtiles");

            VectorTileRenderer.Sources.MbTilesSource source =
                new VectorTileRenderer.Sources.MbTilesSource(path);

            // L.tileLayer("{server}/{style}/{z}/{x}/{y}{scalex}.png",
            // https://maps.wikimedia.org/osm-intl/18/136472/95588.png?lang=en
            // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#VB.Net

            int x = 136472;
            int y = 95588;
            int zoom = 14;

            double latitude = 43.735076;
            double longitude = 7.421051;
            TileCoords tile = TileCoords.FromWgs84(latitude, longitude, zoom);

            Wgs84Coordinates wgs84 = Wgs84Coordinates.FromTile(tile);
            System.Console.WriteLine(wgs84);
            System.Console.WriteLine(tile);


            // function long2tile(lon,zoom) { return (Math.floor((lon+180)/360*Math.pow(2,zoom))); }
            // function lat2tile(lat,zoom)  { return (Math.floor((1-Math.log(Math.tan(lat*Math.PI/180) + 1/Math.cos(lat*Math.PI/180))/Math.PI)/2 *Math.pow(2,zoom))); }
            // console.log({ x: lat2tile(43.735076,14), y: long2tile(7.421051,14) } );
            // {x: 5974, y: 8529}

            /*
            function tile2long(x,z) {
                return (x/Math.pow(2,z)*360-180);
            }
            function tile2lat(y,z) {
                var n=Math.PI-2*Math.PI*y/Math.pow(2,z);
                return (180/Math.PI*Math.atan(0.5*(Math.exp(n)-Math.exp(-n))));
            }
            */



            // SELECT * FROM tiles WHERE tile_column = 8529 and tile_row = 5974 and zoom_level = 14
            string constr = string.Format("Data Source={0};Version=3;", path);
            string sql = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", tile.X, tile.Y, zoom);
            System.Console.WriteLine(sql, constr);

            VectorTileRenderer.GlobalMercator gm = new VectorTileRenderer.GlobalMercator();
            VectorTileRenderer.GlobalMercator.TileAddress ta = gm.LatLonToTile(latitude, longitude, zoom);
            System.Console.WriteLine(ta);
            // using (System.IO.Stream strm = source.GetRawTile(tile.X, tile.Y, zoom))
            // using (System.IO.Stream strm = source.GetRawTile(ta.X, ta.Y, zoom))

            byte[] result = null;

            using (System.IO.Stream strm = source.GetRawTile(x, y, zoom))
            {

                try
                {

#if STREAM_HAS_NO_LENGTH
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        strm.CopyTo(ms);
                        result = ms.ToArray();
                    } // End Using ms 
#else
                    result = new byte[strm.Length];
                    strm.Read(result, 0, result.Length);
#endif
                    System.Console.WriteLine(result);
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    // System.Console.ReadKey();
                } // End Catch 

            } // End Using strm 

        } // End Sub Test 


        public Microsoft.AspNetCore.Mvc.FileStreamResult GetFontByProxy(string fontstack, string range)
        {
            // "glyphs": "https://maps.tilehosting.com/fonts/{fontstack}/{range}.pbf?key=egrA25FNSYcFuvl6Lb8Y",
            // "glyphs": "https://localhost:44378/fonts/{fontstack}/{range}",
            // string url = "https://maps.tilehosting.com/fonts/Noto%20Sans%20Regular/36096-36351.pbf?key=egrA25FNSYcFuvl6Lb8Y";

            FontData data = DownloadFont(fontstack, range);

            if (data.File == null)
                return null;

            Response.StatusCode = 200;

            foreach (string key in data.Headers.Keys)
            {
                Response.Headers[key] = data.Headers[key];
            }


            return new Microsoft.AspNetCore.Mvc.FileStreamResult(data.File, data.ContentType)
            {
                // EntityTag = 
                // LastModified = 
                // FileDownloadName = "test.txt"
            };

        } // End Function GetTest 


        // url = "https://maps.tilehosting.com/fonts/Noto%20Sans%20Regular/36096-36351.pbf?key=egrA25FNSYcFuvl6Lb8Y";
        public FontData DownloadFont(string fontstack, string range)
        {
            string fontStackComponent = System.Uri.EscapeUriString(fontstack);
            string rangeComponent = System.Uri.EscapeUriString(range);
            string url = $"https://maps.tilehosting.com/fonts/{fontStackComponent}/{rangeComponent}.pbf?key=egrA25FNSYcFuvl6Lb8Y";


            string[] headersToRemove = new string[] { "Connection", "Transfer-Encoding", "Set-Cookie", "ETag", "X-IPLB-Instance", "CF-Cache-Status", "Expect-CT", "Server", "CF-RAY" };
            System.Collections.Generic.HashSet<string> hs = new System.Collections.Generic.HashSet<string>(headersToRemove, System.StringComparer.OrdinalIgnoreCase);

            string basePath = System.IO.Path.Combine(this.m_env.WebRootPath, "fonts");

            string fontDir = System.IO.Path.Combine(basePath, fontstack);
            string fontFile = System.IO.Path.Combine(fontDir, range + ".pbf");
            string fontJson = System.IO.Path.ChangeExtension(fontFile, ".json");


            if (!System.IO.Directory.Exists(fontDir))
                System.IO.Directory.CreateDirectory(fontDir);

            if (System.IO.File.Exists(fontFile))
            {
                FontData data = new FontData();
                data.File = System.IO.File.OpenRead(fontFile);

                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                using (System.IO.FileStream jsonStream = System.IO.File.OpenRead(fontJson))
                {

                    using (System.IO.TextReader sr = new System.IO.StreamReader(jsonStream, System.Text.Encoding.UTF8))
                    {
                        using (Newtonsoft.Json.JsonTextReader jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                        {
                            try
                            {
                                data.Headers = serializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(jsonTextReader);
                            }
                            catch (System.Exception ex)
                            {
                                System.Console.WriteLine(ex.Message);
                            }

                        } // End Using jsonTextReader 
                    } // End Using sr 

                } // End Using jsonStream

                return data;
            } // End if (System.IO.File.Exists(fontFile)) 


            System.Collections.Generic.Dictionary<string, string> headers = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                wc.Headers["Origin"] = "https://openmaptiles.org";
                wc.Headers["Referer"] = "https://openmaptiles.org/maps/leaflet-mapbox-gl.html";
                wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36";

                wc.DownloadFile(url, fontFile);

                foreach (string key in wc.ResponseHeaders.AllKeys)
                {
                    // System.Console.WriteLine("key=" + wc.ResponseHeaders[key]);
                    if (!hs.Contains(key))
                        headers[key] = wc.ResponseHeaders[key];
                } // Next key 

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(headers);
                System.Console.WriteLine(json);
                System.IO.File.WriteAllText(fontJson, json, System.Text.Encoding.UTF8);

                return new FontData() { File = System.IO.File.OpenRead(fontFile), Headers = headers };
            } // End Using wc 

        } // End Function DownloadFont 


    } // End Class ControllerTrash 


} // End Namespace VectorTileServer 
