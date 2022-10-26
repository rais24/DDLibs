using System.Security.Cryptography;
using System.Text;

namespace Utils
{

    public static class SimpleAes6
    {
        //change the key and ivSalt with random string...
        static readonly string key = "abc";
        static readonly string ivSalt = "xyz";
        static SimpleAes6()
        {

        }
        // The public outer or wrapper class "SimpleAes6" works with string inputs and outputs.    
        // The private inner class "AesHelper" works with byte arrays.

        // The suffix 6 was used since this was written and tested specifically for .NET 6.
        // It has been confirmed to work with .NET Framework 4.8, that is to say that 
        // cipherText encrypted from a .NET 6 application can be decrypted back to plainText
        // by a .NET Framework application using the same code base contained here.

        /// <summary>
        /// Simple AES encryption of a plain text string using the specified secret Key phrase and IV Salt.  
        /// The same Key phrase and IV Salt must be used later when you decrypt the cipher text back to plain text.
        /// </summary>
        /// <param name="plainText">The plain text you wish to encrypt to cipher text.</param>
        /// <returns>An encrypted cipher text string suitable for passwords to be stored safely into XML or JSON files.</returns>
        public static string Encrypt(string plainText)
        {
            // Get the bytes of the respective strings
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(ivSalt);

            // Hash the Key with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[]? bytesEncrypted = AesHelper.Encrypt(plainBytes, keyBytes, ivBytes);

            #pragma warning disable CS8604 // Possible null reference argument.
            return Convert.ToBase64String(bytesEncrypted);
            #pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Decrypts the cipher text back to plain text using the same secret Key phrase and IV Salt that was
        /// previously used to encrypt.
        /// </summary>
        /// <param name="cipherText">A string encryped earlier using the same secret Key phrase and IV Salt.</param>
        /// <returns>A plain text string.</returns>
        public static string DecryptToString(string cipherText)
        {
            // Get the bytes of the string
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(ivSalt);

            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[]? bytesDecrypted = AesHelper.Decrypt(cipherBytes, keyBytes, ivBytes);

#pragma warning disable CS8604 // Possible null reference argument.
            return Encoding.UTF8.GetString(bytes: bytesDecrypted);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private static class AesHelper
        {
            // The public outer or wrapper class "SimpleAes6" works with string inputs and outputs.    
            // The private inner class "AesHelper" works with byte arrays.

            const int KeySize = 256;
            const int BlockSize = 128;
            const int Iterations = 1000;

            private static Aes CreateAesInstance(byte[] key, byte[] iv)
            {
                var aes = Aes.Create();

                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;

                var derived = new Rfc2898DeriveBytes(key, iv, Iterations);
                aes.Key = derived.GetBytes(aes.KeySize / 8);
                aes.IV = derived.GetBytes(aes.BlockSize / 8);

                return aes;
            }

            public static byte[]? Encrypt(byte[] bytesToBeEncrypted, byte[] key, byte[] iv)
            {
                byte[]? encryptedBytes = null;

                using (var aes = CreateAesInstance(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.FlushFinalBlock();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            public static byte[]? Decrypt(byte[] bytesToBeDecrypted, byte[] key, byte[] iv)
            {
                byte[]? decryptedBytes = null;

                using (var aes = CreateAesInstance(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.FlushFinalBlock();
                        }
                        decryptedBytes = TrimZeroPadding(ms.ToArray());
                    }
                }

                return decryptedBytes;
            }

            private static byte[]? TrimZeroPadding(byte[] array)
            {
                if (array == null || array.Length == 0)
                {
                    return null;
                }
                var lastZeroIndex = array.Length;
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    if (array[i] == char.MinValue)
                    {
                        lastZeroIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }
                return array.Where((item, index) => index < lastZeroIndex).ToArray();
            }

        } // private inner class

    } // public wrapper class
}

