using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;
using Windows.UI.WebUI;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Projeto_PAP
{
    internal class PasswordEncrypt
    {
        private readonly byte[] _key; 

        public PasswordEncrypt(byte[] key)
        {
            if (key == null || key.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes (256 bits)");
            }
            _key = key;
        }

        public string Encrypt(string originalText)
        {
            if (string.IsNullOrEmpty(originalText))
            {
                throw new ArgumentException("Text cannot be empty");
            }

            byte[] iv = RandomNumberGenerator.GetBytes(12);
            byte[] textBytes = Encoding.UTF8.GetBytes(originalText);
            byte[] encryptedData = new byte[textBytes.Length];
            byte[] tag = new byte[16];

            using (AesGcm aes = new AesGcm(_key, 16))
            {
                aes.Encrypt(iv, textBytes, encryptedData, tag); //tag é o valor de autenticação, necessário para garantir a integridade dos dados durante a desencriptação
                //iv é um valor aleatório usado para para garantir que cada encriptação é diferente
                //ecrypted data é o resultazdo da encriptação de textBytes
            }

            byte[] result = new byte[iv.Length + tag.Length + encryptedData.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(tag, 0, result, iv.Length, tag.Length);
            Buffer.BlockCopy(encryptedData, 0, result, iv.Length + tag.Length, encryptedData.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentException("Text cannot be empty");
            }

            byte[] fullData = Convert.FromBase64String(encryptedText);
            byte[] iv = new byte[12];
            byte[] tag = new byte[16];
            byte[] encryptedData = new byte[fullData.Length - iv.Length - tag.Length];

            Buffer.BlockCopy(fullData, 0, iv, 0, 12);
            Buffer.BlockCopy(fullData, 12, tag, 0, 16);
            Buffer.BlockCopy(fullData, 28, encryptedData, 0, encryptedData.Length);

            byte[] decryptedBytes = new byte[encryptedData.Length];

            using (AesGcm aes = new AesGcm(_key, 16))
            {
                aes.Decrypt(iv, encryptedData, tag, decryptedBytes); //decryptedBytes é a saída onde os dados serão guardados
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public void Clear()
        {
            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length); 
            }
        }

        
        internal byte[] GetKey()
        {
            return (byte[])_key.Clone();
        }
    }
}
                                      