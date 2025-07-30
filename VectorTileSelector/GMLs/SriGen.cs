
using System.Diagnostics.Tracing;

namespace VectorTileSelector
{


    public static class SriGenerator
    {


        /// <summary>
        /// Generates a Subresource Integrity (SRI) hash string for a given file.
        /// The hash is calculated on the raw bytes of the file and then Base64 encoded.
        /// </summary>
        /// <param name="filePath">The full path to the file for which to generate the SRI hash.</param>
        /// <param name="algorithm">The hashing algorithm to use (e.g., "SHA256", "SHA384", "SHA512").</param>
        /// <returns>A string formatted as an integrity tag (e.g., "sha256-BASE64HASH"), or null if the file cannot be read or hash generation fails.</returns>
        public static string GenerateSriHash(string filePath, string algorithm = "SHA256")
        {
            if (!System.IO.File.Exists(filePath))
            {
                System.Console.WriteLine($"Error: File not found at '{filePath}'");
                return null;
            }


            try
            {
                string algorithmPrefix = algorithm.ToLowerInvariant();

                using (System.IO.FileStream fileStream = System.IO.File.OpenRead(filePath))
                {
                    // Create the hash algorithm instance
                    using (System.Security.Cryptography.HashAlgorithm hashAlgorithm = algorithmPrefix switch
                    {
                        "sha256" => System.Security.Cryptography.SHA256.Create(),
                        "sha384" => System.Security.Cryptography.SHA384.Create(),
                        "sha512" => System.Security.Cryptography.SHA512.Create(),
                        _ => throw new System.NotSupportedException($"Unsupported hash algorithm: {algorithm}")
                    })
                    {
                        if (hashAlgorithm == null)
                        {
                            System.Console.WriteLine($"Error: Could not create hash algorithm for '{algorithm}'.");
                            return null;
                        }

                        // Compute the hash of the file stream
                        byte[] hashBytes = hashAlgorithm.ComputeHash(fileStream);

                        // Base64 encode the hash bytes
                        string base64Hash = System.Convert.ToBase64String(hashBytes);

                        // Return the formatted integrity string
                        return $"{algorithmPrefix}-{base64Hash}";
                    }
                }
            }
            catch (System.IO.IOException ex)
            {
                System.Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
                return null;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                System.Console.WriteLine($"Error computing hash for '{filePath}': {ex.Message}");
                return null;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return null;
            }
        }


        public static void Test()
        {
            // --- Example Usage ---

            // 1. Create a dummy file for testing
            string dummyFilePath = "test_script.js";

            string fooPath = @"D:\stefan.steiger\Downloads\leaflet.js";

            string fileContent = @"
            // This is a dummy JavaScript file for SRI testing
            function greet() {
                console.log('Hello, world!');
            }
            greet();
        ";
            try
            {
                System.IO.File.WriteAllText(dummyFilePath, fileContent, System.Text.Encoding.UTF8);
                System.Console.WriteLine($"Created dummy file: {dummyFilePath}");


                dummyFilePath = fooPath;

                // 2. Generate SHA-256 hash
                string sha256IntegrityTag = GenerateSriHash(dummyFilePath, "SHA256");
                if (sha256IntegrityTag != null)
                {
                    System.Console.WriteLine($"\nSRI Tag (SHA256) for '{dummyFilePath}':");
                    System.Console.WriteLine($"<script src=\"your-script.js\" integrity=\"{sha256IntegrityTag}\" crossorigin=\"anonymous\"></script>");
                }

                // 3. Generate SHA-384 hash (optional)
                string sha384IntegrityTag = GenerateSriHash(dummyFilePath, "SHA384");
                if (sha384IntegrityTag != null)
                {
                    System.Console.WriteLine($"\nSRI Tag (SHA384) for '{dummyFilePath}':");
                    System.Console.WriteLine($"<script src=\"your-script.js\" integrity=\"{sha384IntegrityTag}\" crossorigin=\"anonymous\"></script>");
                }

                // 4. Generate SHA-512 hash (optional)
                string sha512IntegrityTag = GenerateSriHash(dummyFilePath, "SHA512");
                if (sha512IntegrityTag != null)
                {
                    System.Console.WriteLine($"\nSRI Tag (SHA512) for '{dummyFilePath}':");
                    System.Console.WriteLine($"<script src=\"your-script.js\" integrity=\"{sha512IntegrityTag}\" crossorigin=\"anonymous\"></script>");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"An error occurred during example usage: {ex.Message}");
            }
            finally
            {
                // Clean up the dummy file
                if(false)
                if (System.IO.File.Exists(dummyFilePath))
                {
                    System.IO.File.Delete(dummyFilePath);
                    System.Console.WriteLine($"\nCleaned up dummy file: {dummyFilePath}");
                }
            }
        }
    }


}
