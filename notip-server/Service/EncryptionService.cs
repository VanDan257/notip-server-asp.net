using notip_server.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace notip_server.Service
{
    public class EncryptionService : IEncryptionService
    {
        private readonly RSA rsa;
        private readonly string privateKey = System.IO.File.ReadAllText("private_key.pem");

        public EncryptionService()
        {
            rsa = RSA.Create();
            rsa.ImportFromPem(privateKey.ToCharArray());
        }

        public string DecryptSymmetricKey(string encryptedSymmetricKey)
        {
            byte[] encryptedKeyBytes = Convert.FromBase64String(encryptedSymmetricKey);
            byte[] decryptedKeyBytes = rsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedKeyBytes);
        }

        public string DecryptMessage(string encryptedMessage, string decryptedSymmetricKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(decryptedSymmetricKey);
            byte[] iv = new byte[16]; // AES block size is 16 bytes
            byte[] cipherBytes = Convert.FromBase64String(encryptedMessage);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream(cipherBytes))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
