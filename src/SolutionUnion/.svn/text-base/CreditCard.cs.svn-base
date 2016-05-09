using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SolutionUnion {
   
   public class CreditCard {

      public long Id { get; set; }
      public long UserId { get; set; }
      public string Type { get; set; }
      public string Token { get; set; }
      public string LastFour { get; set; }
      public int ExpirationMonth { get; set; }
      public int ExpirationYear { get; set; }
      public string NameOnCard { get; set; }
      public string Address { get; set; }
      public string City { get; set; }
      public string State { get; set; }
      public string PostalCode { get; set; }
      public string Country { get; set; }
      public bool IsDefault { get; set; }
      public DateTime Created { get; set; }

      internal static string EncryptCardNumber(string clearTextPassword) {

         var crypto = new Cryptographer();
         return crypto.Encrypt(clearTextPassword);
      }

      internal static string DecryptCardNumber(string cipher) {

         var crypto = new Cryptographer();
         return crypto.Decrypt(cipher);
      }
   }
}
