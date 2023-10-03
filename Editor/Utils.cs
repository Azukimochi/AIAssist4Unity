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
        
        // AES暗号化 key生成するための文字列 (256bitキー(32文字))
        private const string _aesKey = "12345678901234567890123456789012";
        // AES暗号化 初期化ベクトルを生成するための文字列 (128bit(16文字))
        private const string _aesIv = "1234567890123456";

        // 参照元：c#の暗号化クラスを使ってみた（AES,RSA）
        // https://qiita.com/YoshijiGates/items/6c331924d4fcbcf6627a

        public static string AesEncrypt(string plain_text)
        {
            // 暗号化した文字列格納用
            string encrypted_str;

            // Aesオブジェクトを作成
            using (Aes aes = Aes.Create())
            {
                // Encryptorを作成
                using (ICryptoTransform encryptor =
                       aes.CreateEncryptor(Encoding.UTF8.GetBytes(_aesKey), Encoding.UTF8.GetBytes(_aesIv)))
                {
                    // 出力ストリームを作成
                    using (MemoryStream out_stream = new MemoryStream())
                    {
                        // 暗号化して書き出す
                        using (CryptoStream cs = new CryptoStream(out_stream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                // 出力ストリームに書き出し
                                sw.Write(plain_text);
                            };
                        }
                        // Base64文字列にする
                        byte[] result = out_stream.ToArray();
                        encrypted_str = Convert.ToBase64String(result);
                    }
                    
                }
                
            }

            return encrypted_str;
        }

        /// <summary>
        /// 対称鍵暗号を使って暗号文を復号する
        /// </summary>
        /// <param name="cipher">暗号化された文字列</param>
        /// <param name="iv">対称アルゴリズムの初期ベクター</param>
        /// <param name="key">対称アルゴリズムの共有鍵</param>
        /// <returns>復号された文字列</returns>
        public static string AesDecrypt(string base64_text)
        {
            string plain_text = null;

            if (string.IsNullOrEmpty(base64_text))
            {
                Debug.Log("base64_text is null or empty");
                return "";
            }
            Debug.Log("base64_text: " + base64_text);

            try
            {
                // Base64文字列をバイト型配列に変換
                byte[] cipher = Convert.FromBase64String(base64_text);

                // AESオブジェクトを作成
                using (Aes aes = Aes.Create())
                {
                    // 復号器を作成
                    using (ICryptoTransform decryptor =
                           aes.CreateDecryptor(Encoding.UTF8.GetBytes(_aesKey), Encoding.UTF8.GetBytes(_aesIv)))
                    {
                        // 復号用ストリームを作成
                        using (MemoryStream in_stream = new MemoryStream(cipher))
                        {
                            // 一気に復号
                            using (CryptoStream cs = new CryptoStream(in_stream, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader sr = new StreamReader(cs))
                                {
                                    plain_text = sr.ReadToEnd();
                                    Debug.Log("plain_text: " + plain_text);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Decryption failed: " + ex.Message);
            }
            
            return plain_text;
        }
    }
}
