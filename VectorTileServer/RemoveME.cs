namespace VectorTileServer
{
    
    
    public class RemoveME
    {
        // https://github.com/mattosaurus/PgpCore
        // https://blog.bitscry.com/2018/07/05/pgp-encryption-and-decryption-in-c/
        // https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
        
        
        // https://codereview.stackexchange.com/questions/175141/encrypt-and-decrypt-a-message-using-aes-256-with-gcm-mode-using-bouncy-castle-c
        // https://stackoverflow.com/questions/29701401/encrypt-string-with-bouncy-castle-aes-cbc-pkcs7
        // https://stackoverflow.com/questions/28086321/c-sharp-bouncycastle-rsa-encryption-with-public-private-keys
        // https://stackoverflow.com/questions/6987699/pgp-encryption-and-decryption-using-bouncycastle-c-sharp
        // https://codereview.stackexchange.com/questions/163989/aes-encryption-decryption-of-files-and-bytes
        // https://github.com/lukemerrett/Bouncy-Castle-AES-GCM-Encryption/blob/master/EncryptionService.cs
        
        
        // https://www.codeproject.com/Articles/1265115/Cross-Platform-AES-256-GCM-Encryption-Decryption
        
        // https://stackoverflow.com/questions/36547443/decrypting-aes-file-chunk-by-chunk
        // https://github.com/neoeinstein/bouncycastle/blob/master/crypto/src/security/CipherUtilities.cs
        public static void DecryptFile(string inputFilename, Org.BouncyCastle.Crypto.Parameters.KeyParameter key, string destFilename)
        {
            using (System.IO.Stream inputStream = System.IO.File.OpenRead(inputFilename))
            {
                using (System.IO.Stream outputStream = System.IO.File.OpenWrite(destFilename))
                {
                    Org.BouncyCastle.Crypto.IBufferedCipher engine = Org.BouncyCastle.Security.CipherUtilities.GetCipher("AES/ECB/PKCS7Padding");
                    // engine.Init(false, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(pwBuffer.ToArray().Take(16).ToArray()));
                    engine.Init(false, key);

                    long size = inputStream.Length;
                    long current = inputStream.Position;
                    long chunkSize = 1024 * 1024L;
                    bool lastChunk = false;

                    // Initialize DataReader and DataWriter for reliable reading and writing
                    // to a stream. Writing directly to a stream is unreliable.
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(inputStream))
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(outputStream))
                    {
                        while (current < size)
                        {
                            if (size - current < chunkSize)
                            {
                                chunkSize = (uint)(size - current);
                                lastChunk = true;
                            }

                            byte[] chunk = new byte[chunkSize];
                            reader.Read(chunk, 0, (int)chunkSize);

                            // The last chunk must call DoFinal() as it appends additional bytes
                            byte[] processedBytes = lastChunk ? engine.DoFinal(chunk) : engine.ProcessBytes(chunk);

                            writer.Write(processedBytes);

                            current = inputStream.Position;
                        }

                        outputStream.Flush();
                    }
                }
            }

        }



        public string RsaEncryptWithPublic(string clearText, string publicKey)
        {
            var bytesToEncrypt = System.Text.Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());

            using (var txtreader = new System.IO.StringReader(publicKey))
            {
                var keyParameter = (Org.BouncyCastle.Crypto.AsymmetricKeyParameter)new Org.BouncyCastle.OpenSsl.PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            var encrypted = System.Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;

        }


        public static Org.BouncyCastle.Crypto.AsymmetricKeyParameter GetKey(string publicKey)
        {
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter key = null;

            using (var txtreader = new System.IO.StringReader(publicKey))
            {
                key = (Org.BouncyCastle.Crypto.AsymmetricKeyParameter)new Org.BouncyCastle.OpenSsl.PemReader(txtreader).ReadObject();
            }

            return key;
        }


        // https://stackoverflow.com/questions/15236419/encrypting-decrypting-with-rsa-bouncy-castle-not-working-properly
        public static void DecryptFile(string inputFileName, string outputFileName, string keyFileName)
        {
            System.IO.Stream encryptedTextFile = new System.IO.FileStream(inputFileName, System.IO.FileMode.Open);

            System.IO.Stream decryptedTextFile = new System.IO.FileStream(outputFileName, System.IO.FileMode.Create);
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter key = GetKey(keyFileName);

            // RsaEngine does not add padding, you will lose any leading zeros in your data blocks as a result.
            // You need to use one of the encoding modes available as well.
            Org.BouncyCastle.Crypto.Engines.RsaEngine rsaEngine = new Org.BouncyCastle.Crypto.Engines.RsaEngine();
            var decryptEngine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(rsaEngine);


            decryptEngine.Init(false, key);
            byte[] bytesReaded;
            int nBytesReaded;
            int inputBlockSize = decryptEngine.GetInputBlockSize();
            do
            {
                bytesReaded = new byte[inputBlockSize];
                nBytesReaded = encryptedTextFile.Read(bytesReaded);
                if (nBytesReaded > -1)
                {
                    byte[] decryptedText = decryptEngine.ProcessBlock(bytesReaded, 0, inputBlockSize);
                    decryptedTextFile.Write(decryptedText);
                }
            } while (nBytesReaded > -1);
            decryptedTextFile.Flush();
            decryptedTextFile.Close();
            encryptedTextFile.Close();
        }


        public static void EncryptFile(string fileName, string keyFileName, string nameClearTextFile)
        {
            System.IO.Stream clearTextFile;
            System.IO.Stream textFileProcessed = new  System.IO.FileStream(fileName, System.IO.FileMode.Create);
            //getKey is a method I implemented and works correctly
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter key = GetKey(keyFileName);
            Org.BouncyCastle.Crypto.Engines.RsaEngine rsaEngine = new Org.BouncyCastle.Crypto.Engines.RsaEngine();

            // RsaEngine does not add padding, you will lose any leading zeros in your data blocks as a result.
            // You need to use one of the encoding modes available as well.
            var encryptEngine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(rsaEngine);


            encryptEngine.Init(true, key);
            clearTextFile = new System.IO.FileStream(nameClearTextFile, System.IO.FileMode.Create);
            byte[] bytesReaded;
            int nBytesReaded;
            int inputBlockSize = encryptEngine.GetInputBlockSize();
            do
            {
                bytesReaded = new byte[inputBlockSize];
                nBytesReaded = clearTextFile.Read(bytesReaded);
                if (nBytesReaded > -1)
                {       //This is for the last block if it's not 256 byte length
                    if (nBytesReaded < inputBlockSize)
                    {
                        byte[] temp = new byte[nBytesReaded];
                        for (int i = 0; i < nBytesReaded; i++)
                        {
                            temp[i] = bytesReaded[i];
                        }
                        byte[] encryptedText = encryptEngine.ProcessBlock(temp, 0, nBytesReaded);
                        textFileProcessed.Write(encryptedText);
                    }
                    else
                    {
                        byte[] encryptedText = encryptEngine.ProcessBlock(bytesReaded, 0, inputBlockSize);
                        textFileProcessed.Write(encryptedText);
                    }
                }
            } while (nBytesReaded > -1);
            textFileProcessed.Flush();
            textFileProcessed.Close();
            textFileProcessed.Close();
        }



        public static void TestCompression()
        {


            string inputfile = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\LZF.cs";
            string outputfile = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\LZF.brotli";
            string decompressed = @"D:\Stefan.Steiger\Documents\Visual Studio 2017\TFS\Tools\wkHtmlToPdfSharp\CompressFile\Decompressed.txt";

            StreamHelper.Compress<System.IO.Compression.DeflateStream>(inputfile, outputfile);
            StreamHelper.Uncompress<System.IO.Compression.DeflateStream>(outputfile, decompressed);

            // StreamHelper.Compress<System.IO.Compression.GZipStream>(inputfile, outputfile);
            // StreamHelper.Uncompress<System.IO.Compression.GZipStream>(outputfile, decompressed);

            // StreamHelper.Compress<System.IO.Compression.BrotliStream>(inputfile, outputfile);
            // StreamHelper.Uncompress<System.IO.Compression.BrotliStream>(outputfile, decompressed);


        } // End Sub TestCompression 
        
        
    }
}