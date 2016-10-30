using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Images
{
    public static class Encryption
    {
        public static string Encrypt(string text)
        {
            var cryptoProvider = CreateCryptoProvider();
            var encrypter = cryptoProvider.CreateEncryptor();
            var textArray = Encoding.UTF8.GetBytes(text);
            var encryptedArray = TransformArray(encrypter, textArray, cryptoProvider);
            return Convert.ToBase64String(encryptedArray, 0, encryptedArray.Length);
        }

        public static string Decrypt(string text)
        {
            var cryptoProvider = CreateCryptoProvider();
            try
            {
                var textArray = Convert.FromBase64String(text);
                var decrypter = cryptoProvider.CreateDecryptor();
                var decryptedArray = TransformArray(decrypter, textArray, cryptoProvider);
                return Encoding.UTF8.GetString(decryptedArray);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static TripleDESCryptoServiceProvider CreateCryptoProvider()
        {
            var key = new AppSettingsReader().GetValue("EncryptionKey", typeof(string)).ToString();
            var md5Provider = new MD5CryptoServiceProvider();
            var keyArray = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5Provider.Clear();

            var desProvider = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            return desProvider;
        }

        private static byte[] TransformArray(ICryptoTransform decrypter, byte[] textArray,
            TripleDESCryptoServiceProvider cryptoProvider)
        {
            var decryptedArray = decrypter.TransformFinalBlock(textArray, 0, textArray.Length);
            cryptoProvider.Clear();
            return decryptedArray;
        }
    }
}