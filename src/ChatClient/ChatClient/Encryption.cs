using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ChatClient
{
    public class Encryption
    {
        private byte[] _key;

        public Encryption(byte[] key)
        {
            _key = key;
        }


        /// <summary>
        /// Шифрует исходное сообщение AES ключом (добавляет соль)
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public byte[] ToAes256(string src)
        {
            //Объявляем объект класса AES
            Aes aes = Aes.Create();
            //Генерируем соль
            aes.GenerateIV();
            aes.Key = _key;
            byte[] encrypted;
            var crypt = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(src);
                    }
                }
                //Записываем в переменную encrypted зашиврованный поток байтов
                encrypted = ms.ToArray();
            }

            //Возвращаем поток байт + крепим соль
            return encrypted.Concat(aes.IV).ToArray();
        }

        /// <summary>
        /// Расшифровывает криптованного сообщения
        /// </summary>
        /// <param name="shifr">Зашифрованный текст в байтах</param>
        /// <returns>Возвращает исходную строку</returns>
        public string FromAes256(byte[] shifr)
        {
            var bytesIv = new byte[16];
            var mess = new byte[shifr.Length - 16];
            
            //Списываем соль
            for (int i = shifr.Length - 16, j = 0; i < shifr.Length; i++, j++)
                bytesIv[j] = shifr[i];
            
            //Списываем оставшуюся часть сообщения
            for (int i = 0; i < shifr.Length - 16; i++)
                mess[i] = shifr[i];
            
            Aes aes = Aes.Create();
            //Задаем тот же ключ, что и для шифрования
            aes.Key = _key;
            //Задаем соль
            aes.IV = bytesIv;
            //Строковая переменная для результата
            var text = "";
            var data = mess;
            var crypt = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        //Результат записываем в переменную text в виде исходной строки
                        text = sr.ReadToEnd();
                    }
                }
            }
            return text;
        }

        public static byte[] CreatyKeyByInt(int num, int keyLenght = 16)
        {
            var kb = BitConverter.GetBytes(num);
            var res = new byte[keyLenght];

            for (int i = 0; i < keyLenght; i++)
            {
                res[i] = kb[i % 4];
            }

            return res;
        }

    }
}
