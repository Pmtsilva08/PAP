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
        internal static async Task<(bool Success, string Message)> LoginVerification(string email, string password)
        {
            var client = new MongoClient("mongodb+srv://Pmtsilva08:008421Apmcjmo!@cluster0.quvshkk.mongodb.net/?appName=Cluster0");
            var database = client.GetDatabase("PAP");
            var collection = database.GetCollection<Account>("Users");

            var filter = collection
                .Find(a => a.EmailAddress == email)
                .FirstOrDefault();

            if (filter == null)
            {
                return (false, "Email not found");
            }

            var mongo_pass = filter.Password;

            var verification = PasswordHash.VerifyPassword(password, mongo_pass);

            if (verification) 
            {
                return(true, "Correct Password");
            }
            else
            {
                return (false, "Incorrect Password");
            }

        }
    }
}
