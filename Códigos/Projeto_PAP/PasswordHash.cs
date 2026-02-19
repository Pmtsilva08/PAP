using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace Projeto_PAP
{
    internal static class PasswordHash
    {
        internal static string HashPassword(string password) //recebe uma string como parâmetro
        {
            byte[] salt = new byte[16]; //cria um array de 16 bytes para guardar o valor do salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt); //preenche o array de bytes com os valores aleatórios
            }
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) //converte a password de texto para bytes
            {
                Salt = salt,
                DegreeOfParallelism = 8, // número de threads a serem usadas
                Iterations = 8, // número de iterações
                MemorySize = 131072 // memória usada em KB (128 MB)
            };

            byte[] hash = argon2.GetBytes(32); //gera o hash com 32 bytes de comprimento

            byte[] hashBytes = new byte[48];    
            Array.Copy(salt, 0, hashBytes, 0, 16); //copia o salt para os primeiros 16 bytes
            Array.Copy(hash, 0, hashBytes, 16, 32); //copia o hash para os próximos 32 bytes
            // o primeiro 0 é a posição do array de cópia que vai começar a cópia
            // o segundo 0 é a posição do array de destino onde a cópia vai começar
            // o último número é o número de bytes a serem copiados

            return Convert.ToBase64String(hashBytes); //retorna a converção do array de bytes para uma string 

        }

        internal static bool VerifyPassword(string password, string storeHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storeHash); //converte a string armazenada em base64 para um array de bytes
            byte[] salt = new byte[16]; //cria um array de 16 bytes para guardar o valor do salt que vamos extrair
            Array.Copy(hashBytes, 0, salt, 0, 16); //copia os primeiros 16 bytes do hash armazenado para o array de salt
            byte[] originalHash = new byte[32]; //cria um array de 32 bytes para guardar o hash original
            Array.Copy(hashBytes, 16, originalHash, 0, 32); //copia os próximos 32 bytes do hash armazenado para o array de hash original. // Estes próximos 32 bytes são o HASH original (não a senha)
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) //converte a password de texto para bytes
            {
                Salt = salt,
                DegreeOfParallelism = 8, // número de threads a serem usadas
                Iterations = 8, // número de iterações
                MemorySize = 131072 // memória usada em KB (128 MB)
            };

            byte[] newHash = argon2.GetBytes(32); //gera o hash com 32 bytes de comprimento
            
            return originalHash.SequenceEqual(newHash);
            
        }

    }
}
