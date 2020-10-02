using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Hashing
{
    class Hasher
    {
        public static byte[] GetHash(string input)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
            {
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        public static string HashString(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(input)) {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
