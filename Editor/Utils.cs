using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    public class Utils : EditorWindow
    {
        // 初期化ベクトル"<半角16文字（1byte=8bit, 8bit*16=128bit>"
        private const string AES_IV_256 = @"mEa9VeVjZfFACY%~";

        // 暗号化鍵<半角32文字（8bit*32文字=256bit）>
        private const string AES_Key_256 = @"k653A&k|WDRkjaef7yh6uhwFzk73MNf3";
        
        public static string EncryptAES(string plainText)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.IV = Encoding.UTF8.GetBytes(AES_IV_256);
                aesAlg.Key = Encoding.UTF8.GetBytes(AES_Key_256);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return (System.Convert.ToBase64String(encrypted));
        }

        public static string DecryptAES(string cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.IV = Encoding.UTF8.GetBytes(AES_IV_256);
                aesAlg.Key = Encoding.UTF8.GetBytes(AES_Key_256);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(System.Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
