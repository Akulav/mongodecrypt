using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Database
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new MongoClient("mongodb://akulav:0504229@cluster0-shard-00-00.nwl94.mongodb.net:27017,cluster0-shard-00-01.nwl94.mongodb.net:27017,cluster0-shard-00-02.nwl94.mongodb.net:27017/<dbname>?ssl=true&replicaSet=atlas-esr827-shard-0&authSource=admin&retryWrites=true&w=majority");

            var dbList = client.ListDatabases().ToList();

            //Console.WriteLine("The list of databases on this server is: ");
            foreach (var db in dbList)
            {
                //Console.WriteLine(db);
            }

            var database = client.GetDatabase("user_data");


            //get mongodb collection
            //var collection = database.GetCollection<Entity>("data"); in case of insert
            var collection = database.GetCollection<BsonDocument>("data");

            /*
            await collection.InsertOneAsync(new Entity { Name = "Mia", password = Encrypt("password4"), postcode = "1230", address = "Casa 11" });
            */

            var documents = await collection.Find(new BsonDocument()).ToListAsync();
            Console.WriteLine("\n**********\n");
            for (int i = 0; i < documents.Count; i++) {
                Console.WriteLine(documents[i].ToString());
                string pass = documents[i].GetValue("password", new BsonString(string.Empty)).ToString();
                string dec = Decrypt(pass);
                Console.WriteLine("DECRYPTED PASSWORD: " + dec);

            }

        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }

}

public class Entity
{
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string password { get; set; }
    public string postcode { get; set; }
    public string address { get; set; }
}

