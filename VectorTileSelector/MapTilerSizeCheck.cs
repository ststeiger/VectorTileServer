
namespace VectorTileSelector
{


    public class MapTilerSizeCheck
    {


        public static async System.Threading.Tasks.Task UpdateTileSize(string tilesDirectory, System.Data.Common.DbConnection connection)
        {
            // Get the maptiler-urls from DB
            // Note: not always the same, especially brazil regions and whitespace separated

            string sql = @"
SELECT 
     continent 
    ,subregion 
    ,-- tParamters.base_url + 
	 CASE 
		WHEN LOWER(continent) = 'earth' THEN LOWER(region_name) 
		WHEN LOWER(continent) = 'brazil' THEN 'south-america/' + LOWER(continent) + '/' + LOWER(region_name) 
		WHEN LOWER(continent) IN ('canada', 'us' ) THEN 'north-america/' + LOWER(continent) + '/' + LOWER(region_name) 
		WHEN LOWER(region_name) = 'russia' THEN LOWER(region_name) 
		ELSE 
			continent 
			+ '/' 
			+ -- LOWER(region_name) 
			CASE 
				-- Exceptions where maptiler <> geofabrik 
				WHEN LOWER(subregion) IN( 'britain and ireland', 'great britain', 'united kingdom') THEN LOWER('great-britain') 
				WHEN LOWER(subregion) = 'bosnia-herzegovina' THEN 'bosnia-and-herzegovina' 
				WHEN LOWER(subregion) = 'sudan' THEN 'republic-of-sudan' 
				WHEN LOWER(subregion) = 'south sudan' THEN 'south-sudan' 
				ELSE LOWER(region_name)  
			END 
	END AS download_name 
FROM region_data 

CROSS JOIN 
	( 
		SELECT 'https://data.maptiler.com/downloads/dataset/osm/' AS base_url 
	) AS tParamters 

WHERE (1=1) 
AND (tile_size IS NULL OR tile_size = -1)

-- AND region_name = 'albania' 
-- AND region_name = 'virginia' 

-- AND continent = 'europe' -- smaller failures
-- AND region_name = 'russia' 
-- AND continent = 'brazil' -- different regions 
-- AND continent = 'canada' -- same regions ? looks like it 
-- AND continent = 'earth' -- fails only on antarctica 
-- AND continent = 'us' -- same regions ? looks like it  

-- List of fails (there is no maptiler dataset) 
-- AND subregion NOT IN ('Guyana', 'Venezuela', 'Puerto Rico', 'United States Virgin Islands', 'Guernsey and Jersey') 
-- AND subregion NOT IN ('Bahamas', 'Costa Rica', 'El Salvador', 'Honduras', 'Jamaica', 'Panama')
-- AND subregion NOT IN ('Laos', 'Armenia', 'Bhutan', 'East Timor') 
-- AND subregion NOT IN ('American Oceania', 'Cook Islands', 'Île de Clipperton', 'Kiribati', 'Marshall Islands', 'Micronesia', 'Nauru', 'Niue', 'Palau', 'Pitcairn Islands', 'Polynésie française (French Polynesia)', 'Samoa', 'Solomon Islands', 'Tokelau', 'Tonga', 'Tuvalu', 'Vanuatu', 'Wallis et Futuna' ) 
";

            string sqlUpdate = @"
UPDATE region_data 
    SET tile_size = @TileSize  
WHERE continent = @Continent 
AND subregion = @Subregion 
";


            bool connectionOpened = false;


            using (System.Data.Common.DbConnection updateConnection = Db.Connection)
            {

                if (updateConnection.State != System.Data.ConnectionState.Open)
                    await updateConnection.OpenAsync();



                using (System.Data.Common.DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                        connectionOpened = true;
                    }


                    using (System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        int CONTINENT_ORD = reader.GetOrdinal("continent");
                        int SUBREGION_ORD = reader.GetOrdinal("subregion");
                        int DOWNLOADNAME_ORD = reader.GetOrdinal("download_name");

                        while (await reader.ReadAsync())
                        {
                            string continent = reader.GetString(CONTINENT_ORD);
                            string subregion = reader.GetString(SUBREGION_ORD);
                            string download_name = reader.GetString(DOWNLOADNAME_ORD);

                            string savePath = System.IO.Path.Combine(tilesDirectory, download_name.Replace("/", "-") + ".htm");

                            string page = await DownloadMapTilerPage(
                                "https://data.maptiler.com/downloads/dataset/osm/" 
                                + download_name
                            );

                            await System.IO.File.WriteAllTextAsync(savePath, page, System.Text.Encoding.UTF8);

                            // TODO - extract size 
                            long size = ExtractSize(page);

                            await Dapper.SqlMapper.ExecuteAsync(updateConnection, sqlUpdate, new
                            {
                                TileSize = (long)size, // Replace with the actual value
                                Continent = continent, // Replace with the actual value
                                Subregion = subregion // Replace with the actual value
                            });



                        } // Next 

                    } // End Using reader 

                } // End using command 

                if (connectionOpened && connection.State != System.Data.ConnectionState.Closed)
                    await connection.CloseAsync();


                if (updateConnection.State != System.Data.ConnectionState.Closed)
                    await updateConnection.CloseAsync();
            } // End Using updateConnection 

        } // End Task UpdateTileSize 


        public static async System.Threading.Tasks.Task Test()
        {
            string fileName = "south-america-brazil1.htm";

            // Set the URL to download
            string url = "https://data.maptiler.com/downloads/dataset/osm/europe/germany/";
            url = "https://data.maptiler.com/downloads/dataset/osm/russia/#1.34/69.4/105.2";
            url = "https://data.maptiler.com/downloads/dataset/osm/europe/germany/hanover/";
            url = "https://data.maptiler.com/downloads/dataset/osm/south-america/brazil/";

            string retValue = null;

            if (System.IO.File.Exists(fileName))
                retValue = System.IO.File.ReadAllText(fileName, System.Text.Encoding.UTF8);
            else
            {
                retValue = await DownloadMapTilerPage(url);
                System.IO.File.WriteAllText(fileName, retValue, System.Text.Encoding.UTF8);
            }

            // Print the downloaded content
            System.Console.WriteLine(retValue);
            ExtractSize(retValue);
        } // End Task Test 


        public static long ExtractSize(string html)
        {
            long byteCount = -1;

            if (string.IsNullOrWhiteSpace(html))
                return byteCount;


            // Load the HTML document
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html); // Load from file or provide HTML content directly

            // Define the CSS class to select
            string cssClass = "bg-lighter";

            // Define the XPath expression
            // Variant 1 
            string xpathExpression = "//*[starts-with(normalize-space(text()), 'OpenStreetMap') and " +
                                     "(contains(normalize-space(text()), ' GiB') or " +
                                     "contains(normalize-space(text()), ' MiB') or " +
                                     "contains(normalize-space(text()), ' KiB'))]";

            // Variant 2 
            xpathExpression = $"//*[contains(concat(' ', normalize-space(@class), ' '), ' {cssClass} ')]";

            // Select nodes with the specified CSS class
            HtmlAgilityPack.HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(xpathExpression);

            // Check if any matching nodes were found
            if (nodes != null)
            {
                foreach (HtmlAgilityPack.HtmlNode node in nodes)
                {
                    System.Console.WriteLine("Matching node found: " + node.InnerText);
                    byteCount = ExtractByteCount(node.InnerText);
                    System.Console.WriteLine("Bytes: " + byteCount.ToString("N0"));
                }
            }
            else
            {
                System.Console.WriteLine("No matching nodes found.");
            }

            System.Console.WriteLine("Finished Test !");

            return byteCount;
        } // End Function ExtractSize 


        private static long ConvertToBytes(double number, string unit)
        {
            // Define conversion factors
            const long GibInBytes = 1073741824; // 1024^3
            const long MibInBytes = 1048576;  // 1024^2
            const long KibInBytes = 1024;  // 1024^1

            // Convert to bytes based on the unit
            switch (unit)
            {
                case "GiB":
                    return (long)(number * GibInBytes);
                case "MiB":
                    return (long)(number * MibInBytes);
                case "KiB":
                    return (long)(number * KibInBytes);
                default:
                    throw new System.ArgumentException("Invalid unit: '" + unit + "'.");
            } // End Switch 

        } // End Function ConvertToBytes 


        private static long ExtractByteCount(string input)
        {
            long bytes = -1;

            // Define the regular expression pattern
            string pattern = @"\b(\d+(?:\.\d+)?)\s*(GiB|MiB|KiB)\b";

            // Match the pattern in the input string
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(input, pattern, System.Text.RegularExpressions.RegexOptions.Multiline);

            // Check if a match was found
            if (match.Success)
            {
                // Extract the number and unit
                string numberStr = match.Groups[1].Value;
                string unit = match.Groups[2].Value;

                System.Console.WriteLine("Number: " + numberStr);
                System.Console.WriteLine("Unit: " + unit);


                // Parse the number string to double
                double number;
                if (!double.TryParse(numberStr, out number))
                {
                    System.Console.WriteLine("Failed to parse number.");
                    return bytes;
                } // End if (!double.TryParse(numberStr, out number)) 

                // Convert the number to bytes based on the unit
                bytes = ConvertToBytes(number, unit);
            }
            else
            {
                System.Console.WriteLine("No matching SIZE found.");
            }

            return bytes;
        } // End Function ExtractByteCount 


        public static async System.Threading.Tasks.Task<string> DownloadMapTilerPage(string url)
        {
            string retValue = null;


            using (System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
                  | System.Net.DecompressionMethods.Deflate
                  | System.Net.DecompressionMethods.Brotli
            })
            {

                // Create HttpClient instance
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler))
                {
                    // Set user agent
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");

                    // Set request headers
                    client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9,de-CH;q=0.8,de;q=0.7");
                    client.DefaultRequestHeaders.Add("cache-control", "no-cache");
                    client.DefaultRequestHeaders.Add("dnt", "1");
                    client.DefaultRequestHeaders.Add("pragma", "no-cache");
                    client.DefaultRequestHeaders.Add("referer", "https://www.google.com/");
                    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand)\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
                    client.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
                    client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                    client.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
                    client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");


                    try
                    {
                        // Send GET request
                        using (System.Net.Http.HttpResponseMessage response = await client.GetAsync(url))
                        {

                            // Check if request was successful
                            if (response.IsSuccessStatusCode)
                            {
                                // Read content as string
                                retValue = await response.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                System.Console.WriteLine("Failed to download the content. Status code: " + response.StatusCode);
                            }

                        } // End Using response 

                    } // End Try 
                    catch (System.Net.Http.HttpRequestException e)
                    {
                        System.Console.WriteLine("Error: " + e.Message);
                    }
                } // End Using client 

            } // End Using handler 

            return retValue;
        } // End Task DownloadMapTilerPage 


    } // End Class 


} // End Namespace 
