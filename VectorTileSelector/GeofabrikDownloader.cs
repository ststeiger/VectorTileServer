
namespace VectorTileSelector
{


    public class GeofabrikDownloader
    {
        private readonly System.Net.Http.HttpClient m_httpClient;
        private readonly string m_baseUrl = "https://download.geofabrik.de";


        public GeofabrikDownloader()
        {
            this.m_httpClient = new System.Net.Http.HttpClient();
            this.m_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            this.m_httpClient.DefaultRequestHeaders.Accept.Clear();
            this.m_httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            this.m_httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            this.m_httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,de-CH;q=0.8,de;q=0.7");

            this.m_httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true,
                MaxAge = new System.TimeSpan(0, 0, 0),
                Private = true,
                MustRevalidate = true
            };

            this.m_httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
            // this.m_httpClient.DefaultRequestHeaders.Add("Expires", "0");
            // this.m_httpClient.DefaultRequestHeaders.Add("Expires", "Tue, 01 Jan 1980 1:00:00 GMT");

            this.m_httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            this.m_httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            this.m_httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            this.m_httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this.m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A;Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
            this.m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            this.m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        } // End Constructor 



        // https://download.geofabrik.de/russia/central-fed-district.kml
        // https://download.geofabrik.de/russia/siberian-fed-district.kml
        // https://download.geofabrik.de/north-america/us-midwest.kml
        // https://download.geofabrik.de/north-america/canada.kml
        // https://download.geofabrik.de/north-america/canada/british-columbia.kml
        // https://download.geofabrik.de/south-america/brazil/nordeste.kml
        // https://download.geofabrik.de/north-america/us/alabama.kml
        // https://download.geofabrik.de/asia.kml
        public async System.Threading.Tasks.Task DownloadFileAsync(string filename, string savePath)
        {
            string uri = $"{this.m_baseUrl}/{filename}";

            try
            {
                if (System.IO.File.Exists(savePath))
                    return; // skip those already downloaded

                using (System.Net.Http.HttpResponseMessage response = await this.m_httpClient.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();

                    using (System.IO.Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (System.IO.FileStream fileStream = System.IO.File.Create(savePath))
                        {
                            await responseStream.CopyToAsync(fileStream);
                        } // End Using fileStream 

                    } // End Using responseStream 

                } // End Using response 

            } // End Try 
            catch (System.Net.Http.HttpRequestException ex)
            {
                // Handle download errors here
                System.Console.WriteLine($"Download of {filename} failed with: {ex.Message}");
            } // End Catch 

        } // End Task DownloadFileAsync 


        public static async System.Threading.Tasks.Task FetchAndDownloadAsync(string kmlDirectory, System.Data.Common.DbConnection connection)
        {
            string sql = @"
SELECT 
	 -- tParamters.base_url + 
	 CASE 
		WHEN LOWER(continent) = 'earth' THEN LOWER(region_name) + '.kml' 
		WHEN LOWER(continent) = 'brazil' THEN 'south-america/' + LOWER(continent) + '/' + LOWER(region_name) + '.kml' 
		WHEN LOWER(continent) IN ('canada', 'us' ) THEN 'north-america/' + LOWER(continent) + '/' + LOWER(region_name) + '.kml' 
		WHEN LOWER(region_name) = 'russia' THEN LOWER(region_name) + '.kml' 
		ELSE continent + '/' + LOWER(region_name) + '.kml' 
	END AS download_name 
FROM region_data 

CROSS JOIN 
	( 
		SELECT 'https://download.geofabrik.de/' AS base_url 
	) AS tParamters 
    
WHERE (1=1) 

-- AND continent = 'brazil' 
-- AND continent = 'canada' 
-- AND continent = 'europe' 
-- AND region_name = 'russia' 
-- AND continent = 'us' 

-- AND continent NOT IN 
-- ( 
--	 'earth', 'africa', 'asia' , 'europe', 'north-america', 'central-america', 'south-america', 'australia-oceania', 'russia' 
-- ) 

ORDER BY 
     continent 
    ,size DESC 
";

            GeofabrikDownloader downloader = new GeofabrikDownloader();
            bool connectionOpened = false;


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
                    while (await reader.ReadAsync())
                    {
                        string download_name = reader.GetString(0);
                        string savePath = System.IO.Path.Combine(kmlDirectory, download_name.Replace("/", "-"));

                        await downloader.DownloadFileAsync(download_name, savePath);
                    } // Next 

                } // End Using reader 

            } // End using command 

            if (connectionOpened && connection.State != System.Data.ConnectionState.Closed)
                await connection.CloseAsync();
        } // End Task FetchAndDownloadAsync 


        public static async System.Threading.Tasks.Task Test(string kmlDirectory)
        {
            GeofabrikDownloader downloader = new GeofabrikDownloader();

            string continent = "asia";
            string region = $"israel-and-palestine";

            string downloadName = continent + "/" + region + ".kml";
            string savePath = System.IO.Path.Combine(kmlDirectory, continent + "-" + region + ".kml");
            await downloader.DownloadFileAsync(downloadName, savePath);
        } // End Task Test 


    } // End Class GeofabrikDownloader 


} // End Namespace 
