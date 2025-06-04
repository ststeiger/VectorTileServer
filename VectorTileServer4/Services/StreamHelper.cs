
namespace VectorTileServer4
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


        public static bool IsDeflated(System.IO.Stream stream)
        {
            return true; // TODO: Implement real check
        } // End Function IsZipped 


        public static bool IsBrotlied(System.IO.Stream stream)
        {
            return true; // TODO: Implement real check
        } // End Function IsZipped 
        


        // public BrotliStream(Stream stream, CompressionLevel compressionLevel);
        // public BrotliStream(Stream stream, CompressionMode mode);
        // public BrotliStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen);
        // public BrotliStream(Stream stream, CompressionMode mode, bool leaveOpen);

        public static void Compress<T>(System.IO.Stream inputStream, System.IO.Stream output)
            where T : System.IO.Stream
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB

            using (System.IO.Stream compressor = (T)System.Activator.CreateInstance(typeof(T), output, System.IO.Compression.CompressionMode.Compress))
            {
                long bytesRead = 0;
                while (bytesRead < inputStream.Length)
                {
                    int ReadLength = inputStream.Read(buffer, 0, buffer.Length);
                    compressor.Write(buffer, 0, ReadLength);
                    compressor.Flush();
                    bytesRead += ReadLength;
                } // Whend

                output.Flush();
            } // End Using output


            if (inputStream.CanSeek)
                inputStream.Seek(0, System.IO.SeekOrigin.Begin);

            if(output.CanSeek)
                output.Seek(0, System.IO.SeekOrigin.Begin);
        } // End Sub Compress
        


        public static void Compress<T>(string inputfile, string outputfile)
            where T : System.IO.Stream
        {

            using (System.IO.Stream output = System.IO.File.OpenWrite(outputfile))
            {
                using (System.IO.Stream input = System.IO.File.OpenRead(inputfile))
                {
                    Compress<T>(input, output);
                } // End Using input 

            } // End Using output 

        } // End Sub Compress 
        

        // https://github.com/ststeiger/wkHtmlToPdfSharp/blob/master/CompressFile/LZMA/SevenZipHelper.cs
        public static System.IO.Stream Compress<T>(System.IO.Stream inputStream)
            where T : System.IO.Stream
        {
            System.IO.MemoryStream compressedStream = new System.IO.MemoryStream();
            Compress<T>(inputStream, compressedStream);

            if (compressedStream.CanSeek)
                compressedStream.Seek(0, System.IO.SeekOrigin.Begin);

            return compressedStream;
        } // End Sub Compress


        private static void Uncompress<T>(System.IO.Stream source, System.IO.Stream destination)
           where T : System.IO.Stream
        {
            byte[] buffer = new byte[1024];
             
            try
            {
                if (
                       (object.ReferenceEquals(typeof(T), typeof(System.IO.Compression.GZipStream)) && !IsGZipped(source))
                    || (object.ReferenceEquals(typeof(T), typeof(System.IO.Compression.DeflateStream)) && !IsDeflated(source))
                    || (object.ReferenceEquals(typeof(T), typeof(System.IO.Compression.BrotliStream)) && !IsBrotlied(source))
                    )
                {
                    // Do not uncompress if it's not a compressed source ...
                    int nRead;
                    while ((nRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destination.Write(buffer, 0, nRead);
                    } // Whend 

                    destination.Flush();
                    return;
                } // End if (stream is uncompressed)

                using (System.IO.Stream compressedStream = (T)System.Activator.CreateInstance(typeof(T), source, System.IO.Compression.CompressionMode.Decompress))
                {
                    int nRead;
                    while ((nRead = compressedStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destination.Write(buffer, 0, nRead);
                    } // Whend 

                    destination.Flush();
                } // End Using compressedStream

            }
            // catch (System.Exception ex) { } // TODO: LOG 
            finally
            {
                buffer = null;
            }

        } // End Sub Uncompress


        public static System.IO.Stream Uncompress<T>(System.IO.Stream inputStream)
            where T : System.IO.Stream
        {
            System.IO.MemoryStream uncompressedStream = new System.IO.MemoryStream();
            Uncompress<T>(inputStream, uncompressedStream);

            if (uncompressedStream.CanSeek)
                uncompressedStream.Seek(0, System.IO.SeekOrigin.Begin);

            return uncompressedStream;
        } // End Sub Uncompress



        public static void Uncompress<T>(string inputfile, string outputfile)
            where T : System.IO.Stream
        {

            using (System.IO.Stream output = System.IO.File.OpenWrite(outputfile))
            {
                using (System.IO.Stream input = System.IO.File.OpenRead(inputfile))
                {
                    Uncompress<T>(input, output);
                } // End Using input 

            } // End Using output 

        } // End Sub Compress 

        public static void Uncompress<T>(System.IO.Stream source, string fileName)
            where T : System.IO.Stream
        {

            using (System.IO.Stream destinationStream = System.IO.File.Create(fileName))
            {
                Uncompress<T>(source, destinationStream);
            } // End Using destinationStream 

        } // End Sub Uncompress 
        

    } // End Class StreamHelper 


} // End Namespace VectorTileServer 
