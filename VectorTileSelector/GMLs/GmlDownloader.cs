
namespace VectorTileSelector
{
    

    public static class GmlDownloader
    {
        private static readonly System.Net.Http.HttpClient _httpClient = 
            new System.Net.Http.HttpClient();


        public static async System.Threading.Tasks.Task Test()
        {
            string outputDir = System.AppContext.BaseDirectory;
            outputDir = System.IO.Path.Combine(outputDir, "..", "..", "..", "GMLs");
            outputDir = System.IO.Path.GetFullPath(outputDir);

            // string zipFileListCsvFile = System.IO.Path.Combine(outputDir, "ZIP", "ch.swisstopo.swissbuildings3d_3_0-853tbcxI.csv");
            string zipFileListCsvFile = System.IO.Path.Combine(outputDir, "ZIP", "swisstopo_sulgen.csv");

            System.Collections.Generic.List<string> zipUrls = new System.Collections.Generic.List<string>();
            // zipUrls.Add(@"https://data.geo.admin.ch/ch.swisstopo.swissbuildings3d_3_0/swissbuildings3d_3_0_2023_1244-32/swissbuildings3d_3_0_2023_1244-32_2056_5728.citygml.zip");

            System.Collections.Generic.List<System.Collections.Generic.List<string>> csvContent = 
                CsvParser.ParseFileSimple(zipFileListCsvFile, ';', '"');

            foreach (System.Collections.Generic.List<string> row in csvContent)
            {
                foreach (string column in row)
                {
                    zipUrls.Add(column);
                } // Next column 

            } // Next row 

            // System.Console.WriteLine(zipUrls);

            await DownloadAndExtractGmlFilesAsync(zipUrls, outputDir);
        } // End Task Test 


        public static async System.Threading.Tasks.Task DownloadAndExtractGmlFilesAsync(
            System.Collections.Generic.List<string> zipUrls, 
            string outputDir
        )
        {
            if(!System.IO.Directory.Exists(outputDir))
                System.IO.Directory.CreateDirectory(outputDir);

            string tempDir = System.IO.Path.Combine(outputDir, "ZIP");

            if (!System.IO.Directory.Exists(tempDir))
                System.IO.Directory.CreateDirectory(tempDir);

            // https://data.geo.admin.ch/ch.swisstopo.swissbuildings3d_3_0/swissbuildings3d_3_0_2022_1155-33/swissbuildings3d_3_0_2022_1155-33_2056_5728.citygml.zip
            foreach (string url in zipUrls)
            {
                string zipFileName = System.IO.Path.GetFileName(new System.Uri(url).AbsolutePath);
                string tempZipPath = System.IO.Path.Combine(tempDir, zipFileName);

                System.Console.WriteLine($"📥 Downloading: {url}");

                try
                {
                    byte[] zipData = await _httpClient.GetByteArrayAsync(url);
                    await System.IO.File.WriteAllBytesAsync(tempZipPath, zipData);

                    System.Console.WriteLine($"📦 Extracting .gml files from: {zipFileName}");

                    using (System.IO.FileStream fs = System.IO.File.OpenRead(tempZipPath))
                    using (System.IO.Compression.ZipArchive archive = 
                        new System.IO.Compression.ZipArchive(fs, 
                        System.IO.Compression.ZipArchiveMode.Read))
                    {
                        foreach (System.IO.Compression.ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".gml", System.StringComparison.OrdinalIgnoreCase))
                            {
                                string outPath = System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName(entry.FullName));
                                System.Console.WriteLine($"→ Extracting: {entry.FullName} to {outPath}");
                                System.IO.Compression.ZipFileExtensions
                                    .ExtractToFile(entry, outPath, overwrite: true);
                            } // End if 

                        } // Next entry 

                    } // End Using archive 

                    System.IO.File.Delete(tempZipPath); // Clean up
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"❌ Error processing {url}: {ex.Message}");
                } // End catch 

                await System.Threading.Tasks.Task.Delay(4000);
            } // Next url 

            System.Console.WriteLine("✅ Done.");
        } // End Task DownloadAndExtractGmlFilesAsync 


    } // End Class GmlDownloader 


} // End Namespace 
