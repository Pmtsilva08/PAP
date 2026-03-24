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
                // Usar o MESMO método de hash que o CreateAccount usa
                var hashedPassword = PasswordHash.HashPassword(newPassword);

                // Atualizar no MongoDB
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var update = Builders<Account>.Update.Set(a => a.Password, hashedPassword);

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

        internal static async Task<bool> VerifySecurityAnswers(string email, string maleNameAnswer, string femaleNameAnswer)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var user = await collection.Find(filter).FirstOrDefaultAsync();

                if (user == null) return false;

                // Verificar as respostas com hash (mesmo método que a password)
                bool maleCorrect = PasswordHash.VerifyPassword(maleNameAnswer.Trim().ToLower(), user.SecurityAnswerMale);
                bool femaleCorrect = PasswordHash.VerifyPassword(femaleNameAnswer.Trim().ToLower(), user.SecurityAnswerFemale);

                return maleCorrect && femaleCorrect;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static async Task<bool> EmailExists(string email)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var user = await collection.Find(filter).FirstOrDefaultAsync();
                return user != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static async Task<bool> UpdateSecurityAnswers(string email, string newMaleAnswer, string newFemaleAnswer)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                // Hash das novas respostas
                var hashedMale = PasswordHash.HashPassword(newMaleAnswer.Trim().ToLower());
                var hashedFemale = PasswordHash.HashPassword(newFemaleAnswer.Trim().ToLower());

                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var update = Builders<Account>.Update
                    .Set(a => a.SecurityAnswerMale, hashedMale)
                    .Set(a => a.SecurityAnswerFemale, hashedFemale);

                var result = await collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Obter preferência de verificação
        internal static async Task<bool> GetRequireVerificationSetting(string email)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var user = await collection.Find(filter).FirstOrDefaultAsync();
                return user?.RequireVerificationForActions ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Atualizar preferência de verificação
        internal static async Task<bool> UpdateRequireVerificationSetting(string email, bool requireVerification)
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, email);
                var update = Builders<Account>.Update.Set(a => a.RequireVerificationForActions, requireVerification);

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