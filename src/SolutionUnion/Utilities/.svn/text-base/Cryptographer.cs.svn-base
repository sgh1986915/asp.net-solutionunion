using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace SolutionUnion {
   
   sealed class Cryptographer {
      
      ICryptoTransform encryptor;
      ICryptoTransform decryptor;

      public Cryptographer() {
         string key = ConfigurationManager.AppSettings["Key"];
         string iv = ConfigurationManager.AppSettings["IV"];

         SymmetricAlgorithm symmetricAlgorithm = new RijndaelManaged();

         symmetricAlgorithm.Key = Convert.FromBase64CharArray(key.ToCharArray(), 0, key.ToCharArray().Length);
         symmetricAlgorithm.IV = Convert.FromBase64CharArray(iv.ToCharArray(), 0, iv.ToCharArray().Length);

         this.encryptor = symmetricAlgorithm.CreateEncryptor();
         this.decryptor = symmetricAlgorithm.CreateDecryptor();
      }

      public string Encrypt(string input) {
         if (input == null) {
            throw new ArgumentNullException("input");
         }

         byte[] encodedBytes = (new ASCIIEncoding()).GetBytes(input);

         MemoryStream memoryStream = new MemoryStream();

         CryptoStream cryptoStream = new CryptoStream(memoryStream, this.encryptor, CryptoStreamMode.Write);

         cryptoStream.Write(encodedBytes, 0, encodedBytes.Length);
         cryptoStream.FlushFinalBlock();

         byte[] encryptedBytes = memoryStream.ToArray();

         return Convert.ToBase64String(encryptedBytes);
      }

      public string Decrypt(string input) {
         if (input == null) {
            throw new ArgumentNullException("input");
         }

         byte[] encryptedBytes = Convert.FromBase64String(input);
         byte[] encodedBytes = new byte[encryptedBytes.Length];

         MemoryStream memoryStream = new MemoryStream(encryptedBytes);

         CryptoStream cryptoStream = new CryptoStream(memoryStream, this.decryptor, CryptoStreamMode.Read);

         cryptoStream.Read(encodedBytes, 0, encodedBytes.Length);

         string output = (new ASCIIEncoding()).GetString(encodedBytes);

         return output.Trim('\0');
      }


      public static string Encrypt(string input, string key) {
         if (input == null) {
            throw new ArgumentNullException("input");
         }

         if ((key == null) || (key.Trim() == "")) {
            throw new ArgumentNullException("key");
         }

         byte[] salt = ASCIIEncoding.ASCII.GetBytes(key.ToLower());

         PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(key, salt);

         RijndaelManaged algorithm = new RijndaelManaged();

         algorithm.Padding = PaddingMode.PKCS7;

         ICryptoTransform encryptor = algorithm.CreateEncryptor(passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));

         byte[] encodedBytes = (new ASCIIEncoding()).GetBytes(input);

         MemoryStream memoryStream = new MemoryStream();

         CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

         cryptoStream.Write(encodedBytes, 0, encodedBytes.Length);
         cryptoStream.FlushFinalBlock();

         byte[] encryptedBytes = memoryStream.ToArray();

         return Convert.ToBase64String(encryptedBytes);
      }

      public static string Decrypt(string input, string key) {
         if (input == null) {
            throw new ArgumentNullException("input");
         }

         if ((key == null) || (key.Trim() == "")) {
            throw new ArgumentNullException("key");
         }

         byte[] salt = ASCIIEncoding.ASCII.GetBytes(key.ToLower());

         PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(key, salt);

         RijndaelManaged algorithm = new RijndaelManaged();

         algorithm.Padding = PaddingMode.PKCS7;

         ICryptoTransform decryptor = algorithm.CreateDecryptor(passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));

         byte[] encryptedBytes = Convert.FromBase64String(input);
         byte[] encodedBytes = new byte[encryptedBytes.Length];

         MemoryStream memoryStream = new MemoryStream(encryptedBytes);

         CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

         cryptoStream.Read(encodedBytes, 0, encodedBytes.Length);

         string output = (new ASCIIEncoding()).GetString(encodedBytes);

         return output.Trim('\0');
      }


      public static string MD5Hash(string input) {
         if (input == null) {
            throw new ArgumentNullException("input");
         }

         byte[] auxInput = (new UnicodeEncoding()).GetBytes(input);
         byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(auxInput);

         string output = BitConverter.ToString(hash);

         output = output.Replace("-", "");

         return output;
      }
   }
}