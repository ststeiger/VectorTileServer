
namespace VectorTileServer
{


    public static class StreamHelper
    {


        private static bool StartsWith(System.IO.Stream stream, int signatureSize, string expectedSignature)
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
        

        public static bool IsGZipped(System.IO.Stream stream)
        {
            return StartsWith(stream, 3, "1F-8B-08");
        } // End Function IsGZipped 


        public static bool IsZipped(System.IO.Stream stream)
        {
            return StartsWith(stream, 4, "50-4B-03-04");
        } // End Function IsZipped 


        public static System.IO.Stream UngzipStream(System.IO.Stream stream)
        {
            if (!IsGZipped(stream))
                return stream;

            System.IO.MemoryStream resultStream = new System.IO.MemoryStream();

            using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
            {
                zipStream.CopyTo(resultStream);
                resultStream.Seek(0, System.IO.SeekOrigin.Begin);
                return resultStream;
            } // End Using zipStream 

        } // End Function UnzipStream 


        public static void Ungzip(System.IO.Stream stream, string fileName)
        {
            byte[] buffer = new byte[1024];

            try
            {
                using (System.IO.Stream decompressedStream = System.IO.File.Create(fileName))
                {
                    using (System.IO.Stream compressedStream = new System.IO.Compression.GZipStream(
                        stream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        int nRead;
                        while ((nRead = compressedStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            decompressedStream.Write(buffer, 0, nRead);
                        } // Whend 

                        decompressedStream.Flush();
                    } // End Using compressedStream

                } // End Using decompressedStream 
            }
            // catch (System.Exception ex) { } // TODO: LOG 
            finally
            {
                buffer = null;
            }

        } // End Sub Ungzip 


    } // End Class StreamHelper 


} // End Namespace VectorTileServer 
