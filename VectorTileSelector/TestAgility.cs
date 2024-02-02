
namespace VectorTileSelector
{


    class TestAgility
    {
        
        // Vector-tile rendering
        // https://gis.stackexchange.com/questions/339396/openlayers-vector-tile-rendering-problem-on-net-core-2-2
        // https://github.com/pramsey/crunchyblog/blob/master/tile-server/index.md
        // https://github.com/pramsey/minimal-mvt/tree/master
        // https://github.com/Oslandia/postile
        // https://github.com/tobinbradley/dirt-simple-postgis-http-api
        // https://github.com/maplibre/martin
        
        // https://github.com/search?q=+ST_AsMVT+language%3AC%23+&type=code
        // https://github.com/apdevelop/tile-map-service
        // https://github.com/lucasvra/postgis-tile-server
        // https://github.com/giserver/giserver-dotnet-libs
        // https://github.com/NieneB/webmapping_for_developers
        // https://github.com/WernerMairl/tile-map-service-net5/blob/e87a62917ee230fb5d30238845c2f4e7da219887/Src/TileMapService/TileSources/PostGISTileSource.cs#L130
        
        // https://www.youtube.com/watch?v=E1YJV6I_rhY
        // https://github.com/apdevelop/linq2db-postgis-extensions/blob/ed9425f1b22101e8eccb0bce4896b841d4f448c3/LinqToDBPostGisNetTopologySuite/GeometryOutput.cs#L403
        // https://github.com/kmichael500/planarian/blob/0869520c38f60755f5085ede8efe389aa55721a9/Planarian/Planarian/Modules/Map/Controllers/MapRepository.cs#L185
        // https://github.com/lucasvra/postgis-tile-server/blob/e36f48cbbbd0402a91d50f3599e1274613f2a4f7/Controllers/TileController.cs#L80
        public static async System.Threading.Tasks.Task Test()
        {
            try
            {
                string[] pages = new string[] {
                    "https://download.geofabrik.de/",
                    "https://download.geofabrik.de/africa.html",
                    "https://download.geofabrik.de/antarctica.html",
                    "https://download.geofabrik.de/asia.html",
                    "https://download.geofabrik.de/australia-oceania.html",
                    "https://download.geofabrik.de/central-america.html",
                    "https://download.geofabrik.de/europe.html",
                    "https://download.geofabrik.de/north-america.html",
                    "https://download.geofabrik.de/south-america.html",
                    "https://download.geofabrik.de/north-america/canada.html",
                    "https://download.geofabrik.de/north-america/us.html",
                    "https://download.geofabrik.de/russia.html",
                    "https://download.geofabrik.de/south-america/brazil.html"
                };
                
                
                await RegionData.ClearDb();

                foreach (string page in pages)
                {
                    System.Collections.Generic.List<RegionData> extractedData = 
                        await ExtractDataFromWebsiteAsync(page);

                    await RegionData.InsertList(extractedData);
                    
                    foreach (RegionData data in extractedData)
                    {
                        System.Console.WriteLine($"Subregion: {data.Subregion}, RegionName: {data.RegionName}, Special: {data.IsSpecialSubRegion}, Size: {SizeParser.ConvertBytesToSize(data.Size)}");
                    } // Next data 

                } // Next page 

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"An error occurred: {ex.Message}");
            }

        } // End Task Test 


        private static async System.Threading.Tasks.Task<System.Collections.Generic.List<RegionData>>
            ExtractDataFromWebsiteAsync(string url)
        {
            string html = null;

            string cacheFile = System.IO.Path.GetFileNameWithoutExtension(url);
            if (string.IsNullOrEmpty(cacheFile))
            {
                cacheFile = "earth";
            } // End if (string.IsNullOrEmpty(cacheFile)) 

            string cacheLocation = System.IO.Path.Combine(@"D:\", "osm-" + cacheFile+".htm");
            string jsonLocation = System.IO.Path.Combine(@"D:\", "osm-" + cacheFile+".json");
            System.Console.WriteLine(cacheFile);

            if (System.IO.File.Exists(cacheLocation))
            {
                html = System.IO.File.ReadAllText(cacheLocation, System.Text.Encoding.UTF8);
            }
            else
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    using (System.Net.Http.HttpResponseMessage response = await client.GetAsync(url))
                    { 
                        response.EnsureSuccessStatusCode();
                        html = await response.Content.ReadAsStringAsync();
                        System.IO.File.WriteAllText(cacheLocation, html, System.Text.Encoding.UTF8);
                    }// End Using response 

                } // End Using client 
            }

            System.Collections.Generic.List<RegionData> regionData = ExtractData(html);


            for (int i = 0; i < regionData.Count; ++i)
            {
                regionData[i].Continent = cacheFile;
            } // Next i 

            string json = System.Text.Json.JsonSerializer.Serialize(regionData, regionData.GetType(),
                new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            
            await System.IO.File.WriteAllTextAsync(jsonLocation, json, System.Text.Encoding.UTF8);
            return regionData;
        } // End Task ExtractDataFromWebsiteAsync 


        public static System.Collections.Generic.List<RegionData> ExtractTableData(
            HtmlAgilityPack.HtmlDocument doc, 
            string tableId
        )
        {
            System.Collections.Generic.List<RegionData> extractedData =
              new System.Collections.Generic.List<RegionData>();

            bool specialSubRegion = "specialsubregions".Equals(tableId, System.StringComparison.InvariantCultureIgnoreCase);

            // specialsubregions
            HtmlAgilityPack.HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@id='"+ tableId + "']");
            if (table != null)
            {
                // Iterate over table rows skipping the header rows
                foreach (HtmlAgilityPack.HtmlNode row in table.SelectNodes("tr[position()>2]"))
                {
                    HtmlAgilityPack.HtmlNode subregionNode = row.SelectSingleNode("./td[@class='subregion']/a");
                    HtmlAgilityPack.HtmlNode hrefNode = row.SelectSingleNode("./td[2]/a");
                    HtmlAgilityPack.HtmlNode sizeNode = row.SelectSingleNode("./td[3]");

                    if (subregionNode != null && hrefNode != null && sizeNode != null)
                    {
                        string subregion = System.Web.HttpUtility.HtmlDecode(subregionNode.InnerText.Trim());
                        string href = hrefNode.GetAttributeValue("href", "").Trim();
                        string size = System.Web.HttpUtility.HtmlDecode(sizeNode.InnerText.Trim());
                        long byteSize = SizeParser.ParseSizeToBytes(size);
                        
                        extractedData.Add(new RegionData()
                        {
                            Subregion = subregion,
                            Href = href,
                            Size = byteSize,
                            IsSpecialSubRegion = specialSubRegion
                        });
                    } // End if (subregionNode != null && hrefNode != null && sizeNode != null) 

                } // Next row 

            } // End if (table != null) 

            return extractedData;
        } // End Function ExtractTableData 


        private static void RemoveInvisibleDivs(HtmlAgilityPack.HtmlDocument doc)
        {
            // Select all div elements
            HtmlAgilityPack.HtmlNodeCollection divs = doc.DocumentNode.SelectNodes("//div");

            if (divs != null)
            {
                foreach (HtmlAgilityPack.HtmlNode div in divs)
                {
                    // Check if the div has a style attribute
                    HtmlAgilityPack.HtmlAttribute styleAttribute = div.Attributes["style"];
                    if (styleAttribute != null)
                    {

                        // Check if the style attribute contains "invisible"
                        if (System.Text.RegularExpressions.Regex.IsMatch(
                            styleAttribute.Value, 
                            @"\bdisplay\s*:\s*none\b")
                        )
                        {
                            div.Remove(); // Remove the div
                        }

                    } // End if (styleAttribute != null) 

                } // Next div 

            } // End if (divs != null) 

        } // End Sub RemoveInvisibleDivs 


        public static System.Collections.Generic.List<RegionData> ExtractData(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            RemoveInvisibleDivs(doc);

            System.Collections.Generic.List<RegionData> extractedData = ExtractTableData(doc, "subregions");

            // Select the table with id "subregions"
            extractedData.AddRange(ExtractTableData(doc, "specialsubregions"));
            
            return extractedData;
        } // End Function ExtractData 


    } // End Class TestAgility 


    class RegionData
    {
        public string Continent { get; set; }
        public string Subregion { get; set; }
        public string Href { get; set; }
        public long Size { get; set; }

        public bool IsSpecialSubRegion { get; set; }





        public string RegionName
        {
            get
            {
                string regname = SizeParser.RemoveSuffix(this.Href, "-latest.osm.pbf");
                regname = SizeParser.ExtractAfterFirstSlash(regname);

                return regname;
            }
        } // End Property RegionName 


        public string SizeForHumans
        {
            get
            {
                return SizeParser.ConvertBytesToSize(this.Size);
            }
        } // End Property SizeForHumans 


        public string RegionNameWithPrefix
        {
            get
            {
                string regname = SizeParser.RemoveSuffix(this.Href, "-latest.osm.pbf");

                return regname;
            }
        } // End Property RegionNameWithPrefix 

        
        public static async System.Threading.Tasks.Task ClearDb()
        {
            // SQL query for insertion
            string sql = @"DELETE FROM region_data; ";
            
            using (System.Data.Common.DbConnection connection = Db.Connection)
            {
                // Open the connection
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();
                
                await Dapper.SqlMapper.ExecuteAsync(connection, sql);

                if (connection.State != System.Data.ConnectionState.Closed)
                    await connection.CloseAsync();
            } // End Using connection 
        } // End Task InsertList 




        public static async System.Threading.Tasks.Task InsertList(System.Collections.Generic.IEnumerable<RegionData> list)
        {
            // SQL query for insertion
            string sql = @"
INSERT INTO region_data (continent, subregion, href, size, is_special_subregion) 
VALUES (@Continent, @Subregion, @Href, @Size, @IsSpecialSubRegion); 
";

            using (System.Data.Common.DbConnection connection = Db.Connection)
            {
                // Open the connection
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();
                
                await Dapper.SqlMapper.ExecuteAsync(connection, sql, list);

                if (connection.State != System.Data.ConnectionState.Closed)
                    await connection.CloseAsync();
            } // End Using connection 
        } // End Task InsertList 


    } // End Class RegionData 


} // End Namespace 
