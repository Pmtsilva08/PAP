using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Windows.Devices.PointOfService;

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
    }
}
                                      