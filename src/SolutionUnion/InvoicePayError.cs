using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion {
   
   public class InvoicePayError : ErrorResult {

      public InvoicePayErrorCreditCardInfo AttemptedCreditCard { get; private set; }
      public InvoicePayErrorCreditCardInfo[] OtherCardsAvailable { get; private set; }

      public InvoicePayError(InvoicePayErrorCreditCardInfo attemptedCreditCard, InvoicePayErrorCreditCardInfo[] otherCardsAvailable, string message) 
         : base(message) {

         this.AttemptedCreditCard = attemptedCreditCard;
         this.OtherCardsAvailable = otherCardsAvailable;
      }
   }

   public class InvoicePayErrorCreditCardInfo {
      
      public long Id { get; private set; }
      public string LastFour { get; private set; }

      public InvoicePayErrorCreditCardInfo(long id, string lastFour) {
         
         this.Id = id;
         this.LastFour = lastFour;
      }
   }
}
