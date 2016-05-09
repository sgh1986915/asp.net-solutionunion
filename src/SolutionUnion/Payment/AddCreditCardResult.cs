using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Payment {
   
   public class AddCreditCardResult : SuccessfulResult {

      public CreditCard CreditCard { get; private set; }

      public AddCreditCardResult(CreditCard creditCard) {
         this.CreditCard = creditCard;
      }
   }
}
