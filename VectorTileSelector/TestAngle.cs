
#if false

namespace VectorTileSelector
{


    using AngleSharp;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    class TestAngle
    {


        public static async Task Test()
        {
            try
            {
                List<RegionData> extractedData = await ExtractDataFromWebsiteAsync("https://download.geofabrik.de/");
                foreach (var data in extractedData)
                {
                    Console.WriteLine($"Subregion: {data.Subregion}, Href: {data.Href}, Size: {data.Size}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task<List<RegionData>> ExtractDataFromWebsiteAsync(string url)
        {
            List<RegionData> extractedData = new List<RegionData>();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var document = await context.OpenAsync(url);

            // Select the table with id "subregions"
            var table = document.QuerySelector("#subregions");
            if (table != null)
            {
                // Iterate over table rows skipping the header rows
                foreach (var row in table.QuerySelectorAll("tr").Skip(2))
                {
                    var subregionNode = row.QuerySelector("td.subregion a");
                    var hrefNode = row.QuerySelector("td:nth-child(2) a");
                    var sizeNode = row.QuerySelector("td:nth-child(3)");

                    if (subregionNode != null && hrefNode != null && sizeNode != null)
                    {
                        string subregion = subregionNode.TextContent.Trim();
                        string href = hrefNode.GetAttribute("href").Trim();
                        string size = sizeNode.TextContent.Trim();

                        extractedData.Add(new RegionData
                        {
                            Subregion = subregion,
                            Href = href,
                            Size = size
                        });
                    }
                }
            }

            return extractedData;
        }
    }

    class RegionData
    {
        public string Subregion { get; set; }
        public string Href { get; set; }
        public string Size { get; set; }
    }


}

#endif 
