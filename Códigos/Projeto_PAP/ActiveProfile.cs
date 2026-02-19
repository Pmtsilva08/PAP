using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal static class ActiveProfile
    {
        internal static Account current_user { get; set; } = null!;
    }
}
