using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal class Setup2FAParameters
    {
        public string UserEmail { get; set; }
        public PasswordEncrypt Encryptor { get; set; }
        public byte[] Salt { get; set; }
        public string Password { get; set; }
    }
}
