using System;

namespace Projeto_PAP
{
    internal static class DataBaseConfig
    {
        // Connection string lida da variável de ambiente
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("MONGODB_CONNECTION_STRING environment variable is not set!");

        // Nome da base de dados
        public static string DatabaseName => "PAP";

        // Nome da coleção
        public static string CollectionName => "Users";
    }
}