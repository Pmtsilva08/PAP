using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

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
    }
}