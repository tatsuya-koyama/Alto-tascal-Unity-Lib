using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace AltoLib
{
    public class AltoCrypto
    {
        const int KeySize = 256;
        const int BlockSize = 128;

        public static byte[] Encrypt(byte[] dataBytes, string key, string ivSeed)
        {
            byte[] encrypted;

            using (var aes = new AesManaged())
            {
                SetUpAes(aes, key, ivSeed);
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var encryptedStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(dataBytes, 0, dataBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    encrypted = encryptedStream.ToArray();
                }
            }
            return encrypted;
        }

        public static byte[] Decrypt(byte[] encryptedBytes, string key, string ivSeed)
        {
            byte[] decrypted;

            using (var aes = new AesManaged())
            {
                SetUpAes(aes, key, ivSeed);
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var decryptedStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(decryptedStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    decrypted = decryptedStream.ToArray();
                }
            }
            return decrypted;
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        static void SetUpAes(AesManaged aes, string key, string ivSeed)
        {
            aes.KeySize   = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode      = CipherMode.CBC;
            aes.Padding   = PaddingMode.PKCS7;

            string keyDigest = DigestUtil.GetMD5(key);
            string ivDigest  = DigestUtil.GetMD5AsBase64(ivSeed).Substring(0, BlockSize / 8);
            aes.Key = Encoding.UTF8.GetBytes(keyDigest);
            aes.IV  = Encoding.UTF8.GetBytes(ivDigest);
        }
    }
}
