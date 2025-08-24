using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace TheCloud.config
{
    

    public class ConfigEncryptor
    {
        // ✅ Encrypt config.json to config.enc
        public void EncryptConfig(string inputFile, string outputFile, string key)
        {
            string json = File.ReadAllText(inputFile);

            using (var aes = Aes.Create())
            {
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var fs = new FileStream(outputFile, FileMode.Create))
                {
                    fs.Write(aes.IV, 0, aes.IV.Length); // ✅ prepend IV

                    using (var cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(json);
                    }
                }
            }

            Console.WriteLine($"✅ Encrypted to: {outputFile}");
        }

        // ✅ Decrypt config.enc to JSONStructure
        public JSONStructure DecryptConfig(string encryptedFile, string key)
        {
            byte[] encryptedData = File.ReadAllBytes(encryptedFile);

            using (var aes = Aes.Create())
            {
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(encryptedData, iv, iv.Length);
                aes.IV = iv;

                using (var ms = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    string json = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<JSONStructure>(json);
                }
            }
        }
    }
}