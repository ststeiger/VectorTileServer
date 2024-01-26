
namespace VectorTileSelector
{


    class TestAgility
    {
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
                    "https://download.geofabrik.de/south-america.html"
                };


                foreach (string page in pages)
                {
                    System.Collections.Generic.List<RegionData> extractedData = 
                        await ExtractDataFromWebsiteAsync(page);

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

        static async System.Threading.Tasks.Task<System.Collections.Generic.List<RegionData>>
            ExtractDataFromWebsiteAsync(string url)
        {
            string html = null;

            string cacheFile = System.IO.Path.GetFileNameWithoutExtension(url);
            if (string.IsNullOrEmpty(cacheFile))
            {
                cacheFile = "earth";
            }

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
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    html = await response.Content.ReadAsStringAsync();
                    System.IO.File.WriteAllText(cacheLocation, html, System.Text.Encoding.UTF8);
                }
            }

            System.Collections.Generic.List<RegionData> regionData = ExtractData(html);


            for (int i = 0; i < regionData.Count; ++i)
            {
                regionData[i].Continent = cacheFile;
            }

            string json = System.Text.Json.JsonSerializer.Serialize(regionData, regionData.GetType(),
                new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            System.IO.File.WriteAllText(jsonLocation, json, System.Text.Encoding.UTF8);

            return regionData;
        }


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


        static void RemoveInvisibleDivs(HtmlAgilityPack.HtmlDocument doc)
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

                    }
                }
            }

        }


        public static System.Collections.Generic.List<RegionData> ExtractData(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            RemoveInvisibleDivs(doc);

            System.Collections.Generic.List<RegionData> extractedData = ExtractTableData(doc, "subregions");

            // Select the table with id "subregions"
            extractedData.AddRange(ExtractTableData(doc, "specialsubregions"));

            return extractedData;
        }


    }


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
        }


        public string SizeForHumans
        {
            get
            {
                return SizeParser.ConvertBytesToSize(this.Size);
            }
        }


        public string RegionNameWithPrefix
        {
            get
            {
                string regname = SizeParser.RemoveSuffix(this.Href, "-latest.osm.pbf");

                return regname;
            }
        }



    }



}
