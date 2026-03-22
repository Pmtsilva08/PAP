using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal static class LoginQuery
    {
        // ALTERADO: Adicionado TotpSecret ao retorno
        internal static async Task<(bool Success, string Message, byte[] Salt, string? TotpSecret)> LoginVerification(string email, string password)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = collection
                    .Find(a => a.EmailAddress == email)
                    .FirstOrDefault();

                if (filter == null)
                {
                    return (false, "Email not found", null, null);
                }

                var mongo_pass = filter.Password;

                var verification = PasswordHash.VerifyPassword(password, mongo_pass);

                if (verification)
                {
                    byte[] hashBytes = Convert.FromBase64String(mongo_pass);
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes, 0, salt, 0, 16);

                    // Retorna também o TotpSecret (pode ser null se ainda não configurou)
                    return (true, "Correct Password", salt, filter.TotpSecret);
                }
                else
                {
                    return (false, "Incorrect Password", null, null);
                }
            }
            catch (TimeoutException)
            {
                return (false, "Connection to the Data Base was lost. Verify your Wifi connection", null, null);
            }
        }

        //Método para guardar o TotpSecret depois de configurar
        internal static async Task<bool> SaveTotpSecret(string email, string totpSecret)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var update = Builders<Account>.Update.Set(a => a.TotpSecret, totpSecret);

                var result = await collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static async Task<string> GetUserFirstName(string email)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var account = await collection.Find(filter).FirstOrDefaultAsync();

                return account?.FirstName ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        // Buscar dados do utilizador
        internal static async Task<(string FirstName, string LastName, string Email)?> GetUserData(string email)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var account = await collection.Find(filter).FirstOrDefaultAsync();

                if (account == null) return null;

                return (account.FirstName, account.Lastname, account.EmailAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Atualizar password
        internal static async Task<bool> UpdatePassword(string email, string newPassword)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                byte[] salt = new byte[16];
                RandomNumberGenerator.Fill(salt);

                var argon2 = new Konscious.Security.Cryptography.Argon2id(
                    System.Text.Encoding.UTF8.GetBytes(newPassword));
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 2;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] hash = argon2.GetBytes(32);

                byte[] combined = new byte[salt.Length + hash.Length];
                Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
                Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);

                string base64Hash = Convert.ToBase64String(combined);

                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var update = Builders<Account>.Update.Set(a => a.Password, base64Hash);

                var result = await collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Apagar conta
        internal static async Task<bool> DeleteAccount(string email)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var result = await collection.DeleteOneAsync(filter);

                return result.DeletedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}