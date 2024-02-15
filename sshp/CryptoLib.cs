using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace sshp
{
    public class CryptoLib
    {
        private string initCharSpace()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 33; i <= 125; i++)
            {
                if (new[] { 34, 39, 92, 94, 96 }.Any(e => e == i)) continue; // 34:" 39:' 92:\ 94:^ 96:`
                sb.Append((char)i);
            }

            return sb.ToString();
        }

        private string initUserCharSpace(string password)
        {
            Guid GenerateGuid(Random r)
            {
                byte[] bytes = new byte[16];
                r.NextBytes(bytes);
                return new Guid(bytes);
            }

            try
            {
                Random r = new Random(Encoding.UTF8.GetBytes(password).Sum(x => x));

                using (MD5 sha256Hash = MD5.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                    // return new string(charSpace.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
                    // return new string(charSpace.ToCharArray().OrderBy(x => r.Next()).ToArray());
                    return new string(charSpace.ToCharArray().OrderBy(x => GenerateGuid(r)).ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // !#$%&()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz{|}
        private readonly string charSpace;
        private readonly string userCharSpace;

        public CryptoLib(string password)
        {
            charSpace = initCharSpace();
            userCharSpace = initUserCharSpace(password);
        }

        public string CreateOutput(string input)
        {
            // string inputHash = string.Concat(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(x => x.ToString("x2")));
            string inputHash = string.Concat(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(x => x.ToString("x2")));

            Random rnd = new Random(inputHash.Select(x => (int)x - '0').Sum());

            StringBuilder sb = new StringBuilder();

            foreach (char c in inputHash)
            {
                sb.Append(userCharSpace[rnd.Next(0, userCharSpace.Length)]);
            }

            return sb.ToString();
        }

        public string KeySpace()
        {
            return $"System: {charSpace} User: {userCharSpace}";
        }

        public static void SerializeObject<T>(T obj, string path)
        {
            using (Stream stream = new FileStream(Path.Combine(path, "init.io"), FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Close();
            }
        }

        public static T DeSerializeObject<T>(string path)
        {
            using (Stream stream = new FileStream(Path.Combine(path, "init.io"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                T obj = (T)formatter.Deserialize(stream);
                stream.Close();
                return obj;
            }
        }

    }
}
