
namespace osmprocessor
{


    public static class ResumableDownloader
    {

        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();



        public static async System.Threading.Tasks.Task
           DownloadFileWithResumeAsync(string url, string destinationPath)
        {
            await DownloadFileWithResumeAsync(url, destinationPath, 81920);
        }


        public static async System.Threading.Tasks.Task
            DownloadFileWithResumeAsync(string url, string destinationPath, int bufferSize)
        {
            string tempPath = destinationPath + ".part";

            long existingLength = 0;

            if (System.IO.File.Exists(tempPath))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(tempPath);
                existingLength = fileInfo.Length;
            }

            using (System.Net.Http.HttpRequestMessage request = 
                new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
            )
            {
                if (existingLength > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);
                }

                using (System.Net.Http.HttpResponseMessage response = await _httpClient.SendAsync(
                    request,
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead)
                )
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && existingLength > 0)
                    {
                        System.Console.WriteLine("Server did not support resuming. Starting over...");
                        existingLength = 0;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
                    {
                        System.Console.WriteLine("File already fully downloaded.");
                        if (!System.IO.File.Exists(destinationPath))
                        {
                            System.IO.File.Move(tempPath, destinationPath, overwrite: true);
                        }
                        return;
                    }

                    response.EnsureSuccessStatusCode();

                    long? contentLength = response.Content.Headers.ContentLength;
                    if (contentLength == null)
                    {
                        throw new System.InvalidOperationException("Server did not send a Content-Length header.");
                    }

                    using (System.IO.Stream stream = await response.Content.ReadAsStreamAsync())
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(
                        tempPath,
                        System.IO.FileMode.Append,
                        System.IO.FileAccess.Write,
                        System.IO.FileShare.None,
                        bufferSize,
                        useAsync: true))
                    {
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead;
                        long totalRead = existingLength;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            System.Console.Write($"\rDownloaded {totalRead / (1024 * 1024)} MB...");
                        }
                    }

                    // Finalize file
                    System.IO.File.Move(tempPath, destinationPath, overwrite: true);
                    System.Console.WriteLine("\nDownload complete.");
                }
            }
        }

        // Example usage
        public static async System.Threading.Tasks.Task Test()
        {
            try
            {
                string url = "https://example.com/huge-file.osm.pbf";
                string dest = @"C:\osm\huge-file.osm.pbf";
                await DownloadFileWithResumeAsync(url, dest);
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine("Download failed: " + ex.Message);
            }
        }
    }

}
