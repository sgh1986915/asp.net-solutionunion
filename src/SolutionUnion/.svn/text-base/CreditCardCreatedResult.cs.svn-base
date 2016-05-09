using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SolutionUnion {
   
   public class CreditCardCreatedResult : SuccessfulResult {

      public long Id { get; private set; }

      public CreditCardCreatedResult(long id, string message)
         : base(HttpStatusCode.Created, message) {

         this.Id = id;
      }
   }
}
