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
        internal static async Task<(bool Success, string Message, byte[] Salt)> LoginVerification(string email, string password)
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
                    return (false, "Email not found", null);
                }

                var mongo_pass = filter.Password;

                var verification = PasswordHash.VerifyPassword(password, mongo_pass);

                if (verification)
                {
                    byte[] hashBytes = Convert.FromBase64String(mongo_pass);
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes, 0, salt, 0, 16);
                    return (true, "Correct Password", salt);
                }
                else
                {
                    return (false, "Incorrect Password", null);
                }
            }
            catch (TimeoutException)
            {
                return (false, "Connection to the Data Base was lost. Verify your Wifi connection", null);
            }

            

        }
    }
}
