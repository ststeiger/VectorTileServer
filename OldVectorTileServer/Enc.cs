
namespace VectorTileServer
{

    // https://codereview.stackexchange.com/questions/163989/aes-encryption-decryption-of-files-and-bytes/164038
    // https://www.reddit.com/r/csharp/comments/64oewd/any_way_to_specify_custom_ctr_counter_in/
    // https://p.teknik.io/7LMMS
    // https://github.com/lukemerrett/Bouncy-Castle-AES-GCM-Encryption
    // https://github.com/lukemerrett/Bouncy-Castle-AES-GCM-Encryption/blob/master/EncryptionService.cs
    public static class Encryptions
    {

        public const int AES256KeySize = 256;


        public static byte[] RandomByteArray(int length)
        {
            byte[] result = new byte[length];

            using (System.Security.Cryptography.RNGCryptoServiceProvider provider = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                provider.GetBytes(result);

                return result;
            }

        }


        public static bool AESEncryptFile(string filePath, byte[] password, bool delete)
        {

            byte[] salt = RandomByteArray(16);

            using (System.IO.FileStream fs = new System.IO.FileStream(filePath + ".enc", System.IO.FileMode.Create))
            {

                System.Security.Cryptography.Rfc2898DeriveBytes key = GenerateKey(password, salt);

                password = null;
                System.GC.Collect();

                using (System.Security.Cryptography.Aes aes = new System.Security.Cryptography.AesManaged())
                {
                    aes.KeySize = AES256KeySize;
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
                    aes.Mode = System.Security.Cryptography.CipherMode.CBC;

                    fs.Write(salt, 0, salt.Length);

                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(fs, aes.CreateEncryptor()
                        , System.Security.Cryptography.CryptoStreamMode.Write))
                    {

                        using (System.IO.FileStream fsIn = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                        {

                            byte[] buffer = new byte[1];
                            int read;

                            key.Dispose();

                            try
                            {

                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    cs.Write(buffer, 0, read);
                                }

                                if (delete)
                                {
                                    System.IO.File.Delete(filePath);
                                }

                                cs.Close();
                                fs.Close();
                                fsIn.Close();

                                return true;

                            }
                            catch (System.Exception e)
                            {

                                return false;

                            }

                        }

                    }

                }

            }

        }

        public static bool AESDecryptFile(string filePath, byte[] password, bool keep)
        {

            byte[] salt = new byte[16];

            using (System.IO.FileStream fsIn = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                fsIn.Read(salt, 0, salt.Length);

                System.Security.Cryptography.Rfc2898DeriveBytes key = GenerateKey(password, salt);

                password = null;
                System.GC.Collect();

                using (System.Security.Cryptography.Aes aes = new System.Security.Cryptography.AesManaged())
                {
                    aes.KeySize = AES256KeySize;
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
                    aes.Mode = System.Security.Cryptography.CipherMode.CBC;

                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(fsIn, aes.CreateDecryptor()
                        , System.Security.Cryptography.CryptoStreamMode.Read))
                    {

                        using (System.IO.FileStream fsOut = new System.IO.FileStream(filePath.Remove(filePath.Length - 4), System.IO.FileMode.Create))
                        {

                            byte[] buffer = new byte[1];
                            int read;

                            key.Dispose();

                            try
                            {

                                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fsOut.Write(buffer, 0, buffer.Length);
                                }

                                cs.FlushFinalBlock();

                                fsOut.Close();
                                fsIn.Close();
                                cs.Close();

                                return true;
                            }
                            catch (System.Exception e)
                            {
                                return false;
                            }

                        }

                    }

                }

            }

        }


        public static byte[] AESEncryptBytes(byte[] clear, byte[] password, byte[] salt)
        {

            byte[] encrypted = null;

            System.Security.Cryptography.Rfc2898DeriveBytes key = GenerateKey(password, salt);

            password = null;
            System.GC.Collect();

            using (System.Security.Cryptography.Aes aes = new System.Security.Cryptography.AesManaged())
            {
                aes.KeySize = AES256KeySize;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {

                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, aes.CreateEncryptor()
                        , System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(clear, 0, clear.Length);
                        cs.Close();
                    }

                    encrypted = ms.ToArray();
                }

                key.Dispose();
            }

            return encrypted;
        }


        public static byte[] AESDecryptBytes(byte[] encrypted, byte[] password, byte[] salt)
        {

            byte[] decrypted = null;

            System.Security.Cryptography.Rfc2898DeriveBytes key = GenerateKey(password, salt);

            password = null;
            System.GC.Collect();

            using (System.Security.Cryptography.Aes aes = new System.Security.Cryptography.AesManaged())
            {
                aes.KeySize = AES256KeySize;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {

                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, aes.CreateDecryptor()
                        , System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(encrypted, 0, encrypted.Length);
                        cs.Close();
                    }

                    decrypted = ms.ToArray();
                }

                key.Dispose();
            }

            return decrypted;
        }


        public static bool CheckPassword(byte[] password, byte[] salt, byte[] key)
        {

            using (System.Security.Cryptography.Rfc2898DeriveBytes r = GenerateKey(password, salt))
            {
                byte[] newKey = r.GetBytes(AES256KeySize / 8);
                return System.Linq.Enumerable.SequenceEqual(newKey, key);
            }

        }


        public static System.Security.Cryptography.Rfc2898DeriveBytes GenerateKey(byte[] password, byte[] salt)
        {
            return new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 52768);
        }

    }
}
