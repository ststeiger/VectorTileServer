
namespace WikiDataImporter
{
    public enum CompressionType
    {
        Zip,
        Brotli,
        BZip2,
        GZip,
        CAB,
        RAR,
        SevenZip,
        Xz,
        Zlib,
        Lha,
        LZ4,
        Arj,
        Deb,
        Tar,
        Zstandard,
        LZIP,
        Unknown // For any other file types
    }

    public class FileTypeChecker
    {
        // Define the magic numbers (byte signatures) for various file types
        private static readonly byte[] gzipMagicNumber = { 0x1F, 0x8B };
        private static readonly byte[] bzip2MagicNumber = { 0x42, 0x5A, 0x68 };
        private static readonly byte[] cabMagicNumber = { 0x4D, 0x53, 0x43, 0x46 };
        private static readonly byte[] zipMagicNumber = { 0x50, 0x4B, 0x03, 0x04 };
        private static readonly byte[] rarMagicNumber = { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 };
        private static readonly byte[] sevenZMagicNumber = { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };
        private static readonly byte[] xzMagicNumber = { 0xFD, 0x37, 0x7A, 0x58, 0x5A, 0x00 };
        private static readonly byte[] zlibMagicNumber = { 0x78, 0x9C }; // A common Zlib header
        private static readonly byte[] lhaMagicNumber = { 0x2D, 0x6C, 0x68 }; // '-lh'
        private static readonly byte[] arjMagicNumber = { 0x60, 0xEA };
        private static readonly byte[] debMagicNumber = { 0x21, 0x3C, 0x61, 0x72, 0x63, 0x68, 0x3E, 0x0A }; // '!<arch>\n'
        private static readonly byte[] tarMagicNumber = { 0x75, 0x73, 0x74, 0x61, 0x72 }; // 'ustar' for .tar files


        private static readonly byte[] brotliMagicNumber = { 0x1B, 0x63, 0x29, 0x32 };
        private static readonly byte[] lz4MagicNumber = { 0x04, 0x22, 0x4D, 0x18 };
        private static readonly byte[] zStandardMagicNumber = { 0x28, 0xB5, 0x2F, 0xFD };
        private static readonly byte[] lzipMagicNumber = { 0x4C, 0x5A, 0x49, 0x50 };


        /// <summary>
        /// Helper method to compare a portion of two byte arrays.
        /// </summary>
        private static bool AreBytesEqual(byte[] buffer, byte[] magic, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (buffer[i] != magic[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static CompressionType GetFileCompressionType(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                System.Console.Error.WriteLine($"Error: File not found at {filePath}");
                return CompressionType.Unknown;
            }

            try
            {
                // The maximum length of our magic numbers is 8 bytes (for .deb).
                // For .tar, the signature is at an offset, which requires special handling.
                byte[] buffer = new byte[8];
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);

                    // Prioritize checks with longer magic numbers to avoid false positives
                    if (bytesRead >= 8 && AreBytesEqual(buffer, debMagicNumber, 8))
                    {
                        return CompressionType.Deb;
                    }
                    else if (bytesRead >= 7 && AreBytesEqual(buffer, rarMagicNumber, 7))
                    {
                        return CompressionType.RAR;
                    }
                    else if (bytesRead >= 6 && AreBytesEqual(buffer, sevenZMagicNumber, 6))
                    {
                        return CompressionType.SevenZip;
                    }
                    else if (bytesRead >= 6 && AreBytesEqual(buffer, xzMagicNumber, 6))
                    {
                        return CompressionType.Xz;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, zipMagicNumber, 4))
                    {
                        return CompressionType.Zip;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, cabMagicNumber, 4))
                    {
                        return CompressionType.CAB;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, brotliMagicNumber, 4))
                    {
                        return CompressionType.Brotli;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, lz4MagicNumber, 4))
                    {
                        return CompressionType.LZ4;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, zStandardMagicNumber, 4))
                    {
                        return CompressionType.Zstandard;
                    }
                    else if (bytesRead >= 4 && AreBytesEqual(buffer, lzipMagicNumber, 4))
                    {
                        return CompressionType.LZIP;
                    }
                    else if (bytesRead >= 3 && AreBytesEqual(buffer, bzip2MagicNumber, 3))
                    {
                        return CompressionType.BZip2;
                    }
                    else if (bytesRead >= 3 && AreBytesEqual(buffer, lhaMagicNumber, 3))
                    {
                        return CompressionType.Lha;
                    }
                    else if (bytesRead >= 2 && AreBytesEqual(buffer, gzipMagicNumber, 2))
                    {
                        return CompressionType.GZip;
                    }
                    else if (bytesRead >= 2 && AreBytesEqual(buffer, arjMagicNumber, 2))
                    {
                        return CompressionType.Arj;
                    }
                    else if (bytesRead >= 2 && AreBytesEqual(buffer, zlibMagicNumber, 2))
                    {
                        return CompressionType.Zlib;
                    }


                    // Special case for .tar files: the magic number is located at a specific offset (257)
                    if (fs.Length > 262) // The file must be large enough to contain the magic number
                    {
                        fs.Seek(257, System.IO.SeekOrigin.Begin);
                        byte[] tarBuffer = new byte[5];
                        fs.Read(tarBuffer, 0, tarBuffer.Length);
                        if (AreBytesEqual(tarBuffer, tarMagicNumber, 5))
                        {
                            return CompressionType.Tar;
                        }
                    }
                }

                return CompressionType.Unknown;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }

            return CompressionType.Unknown;
        }


        public static void Test()
        {
            // --- Example Usage ---
            // Create dummy files with correct magic numbers for testing
            System.IO.File.WriteAllBytes("test.bz2", bzip2MagicNumber);
            System.IO.File.WriteAllBytes("test.zip", zipMagicNumber);
            System.IO.File.WriteAllBytes("test.rar", rarMagicNumber);
            System.IO.File.WriteAllBytes("test.cab", cabMagicNumber);
            System.IO.File.WriteAllBytes("test.tar.gz", gzipMagicNumber);
            System.IO.File.WriteAllBytes("test.7z", sevenZMagicNumber);
            System.IO.File.WriteAllBytes("test.xz", xzMagicNumber);
            System.IO.File.WriteAllBytes("test.zlib", zlibMagicNumber);
            System.IO.File.WriteAllBytes("test.lha", lhaMagicNumber);
            System.IO.File.WriteAllBytes("test.arj", arjMagicNumber);
            System.IO.File.WriteAllBytes("test.deb", debMagicNumber);

            // Special case for tar
            using (var fs = new System.IO.FileStream("test.tar", System.IO.FileMode.Create))
            {
                fs.SetLength(263); // Ensure file is big enough
                fs.Seek(257, System.IO.SeekOrigin.Begin);
                fs.Write(tarMagicNumber, 0, tarMagicNumber.Length);
            }

            System.Console.WriteLine($"Checking test.bz2: {GetFileCompressionType("test.bz2")}");
            System.Console.WriteLine($"Checking test.zip: {GetFileCompressionType("test.zip")}");
            System.Console.WriteLine($"Checking test.rar: {GetFileCompressionType("test.rar")}");
            System.Console.WriteLine($"Checking test.cab: {GetFileCompressionType("test.cab")}");
            System.Console.WriteLine($"Checking test.tar.gz: {GetFileCompressionType("test.tar.gz")}");
            System.Console.WriteLine($"Checking test.7z: {GetFileCompressionType("test.7z")}");
            System.Console.WriteLine($"Checking test.xz: {GetFileCompressionType("test.xz")}");
            System.Console.WriteLine($"Checking test.zlib: {GetFileCompressionType("test.zlib")}");
            System.Console.WriteLine($"Checking test.lha: {GetFileCompressionType("test.lha")}");
            System.Console.WriteLine($"Checking test.arj: {GetFileCompressionType("test.arj")}");
            System.Console.WriteLine($"Checking test.deb: {GetFileCompressionType("test.deb")}");
            System.Console.WriteLine($"Checking test.tar: {GetFileCompressionType("test.tar")}");

            // Clean up dummy files
            System.IO.File.Delete("test.bz2"); System.IO.File.Delete("test.zip"); System.IO.File.Delete("test.rar");
            System.IO.File.Delete("test.cab"); System.IO.File.Delete("test.tar.gz"); System.IO.File.Delete("test.7z");
            System.IO.File.Delete("test.xz"); System.IO.File.Delete("test.zlib"); System.IO.File.Delete("test.lha");
            System.IO.File.Delete("test.arj"); System.IO.File.Delete("test.deb"); System.IO.File.Delete("test.tar");
        } // End Sub Test


    } // End Class FileTypeChecker


} // End Namespace
