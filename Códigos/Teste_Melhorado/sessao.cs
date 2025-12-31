using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Teste_Melhorado
{
    public static class Session //Classe estática = existe apenas uma, partilhada por toda a aplicação, sem criar objetos com new.
    {
        public static Account UserLogin { get; set; } = null!;
    }
}
