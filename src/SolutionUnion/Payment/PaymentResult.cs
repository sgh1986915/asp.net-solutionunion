using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Payment {
   
   public class PaymentResult : SuccessfulResult {

      public string TransactionId { get; private set; }

      public PaymentResult(string transactionId) {
         this.TransactionId = transactionId;
      }
   }
}
