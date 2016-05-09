using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Ahsay {
   
   public class UserInfo : SuccessfulResult {

      public string ClientType { get; set; }
      public string LoginName { get; set; }
      public string Timezone { get; set; }
      public string UserType { get; set; }
      public decimal Quota { get; set; }
      public int ExchangeMailboxQuota { get; set; }
      public decimal DataSize { get; set; }
      public decimal RetainSize { get; set; }
      public string Status { get; set; }

      public Collection<UserInfoContact> Contacts { get; private set; }

      public UserInfo() {
         this.Contacts = new Collection<UserInfoContact>();
      }
   }

   public class UserInfoContact {
      public string Name { get; set; }
      public string Email { get; set; }
   }
}
