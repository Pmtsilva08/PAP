using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal class PasswordQuery
    {
        private readonly IMongoCollection<Account> _collection;

        public PasswordQuery()
        {
            var client = new MongoClient(DataBaseConfig.ConnectionString);
            var database = client.GetDatabase(DataBaseConfig.DatabaseName);
            _collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);
        }

        // Adicionar uma nova password à conta do utilizador
        public async Task<bool> AddPasswordAsync(string userEmail, PasswordItem newPassword)
        {
            try
            {
                // Gera um novo Id para a password
                newPassword.Id = ObjectId.GenerateNewId();

                // Filtro para encontrar o utilizador pelo email
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, userEmail);

                // Update para adicionar a password à lista
                var update = Builders<Account>.Update.Push(a => a.PasswordItems, newPassword);

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Obter todas as passwords do utilizador
        public async Task<List<PasswordItem>> GetPasswordsAsync(string userEmail)
        {
            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, userEmail);
                var account = await _collection.Find(filter).FirstOrDefaultAsync();

                return account?.PasswordItems ?? new List<PasswordItem>();
            }
            catch (Exception)
            {
                return new List<PasswordItem>();
            }
        }

        // Apagar uma password
        public async Task<bool> DeletePasswordAsync(string userEmail, ObjectId passwordId)
        {
            try
            {
                var filter = Builders<Account>.Filter.Eq(a => a.EmailAddress, userEmail);
                var update = Builders<Account>.Update.PullFilter(
                    a => a.PasswordItems,
                    p => p.Id == passwordId
                );

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Atualizar uma password
        public async Task<bool> UpdatePasswordAsync(string userEmail, PasswordItem updatedPassword)
        {
            try
            {
                var filter = Builders<Account>.Filter.And(
                    Builders<Account>.Filter.Eq(a => a.EmailAddress, userEmail),
                    Builders<Account>.Filter.ElemMatch(a => a.PasswordItems, p => p.Id == updatedPassword.Id)
                );

                var update = Builders<Account>.Update.Set("list_passwords.$", updatedPassword);

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
