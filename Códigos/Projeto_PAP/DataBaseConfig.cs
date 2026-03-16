using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal static class DataBaseConfig
    {
        // Connection string num só lugar
        public static string ConnectionString => "mongodb+srv://Pmtsilva08:008421Apmcjmo!@cluster0.quvshkk.mongodb.net/?appName=Cluster0";

        // Nome da base de dados
        public static string DatabaseName => "PAP";

        // Nome da coleção
        public static string CollectionName => "Users";
    }
}
