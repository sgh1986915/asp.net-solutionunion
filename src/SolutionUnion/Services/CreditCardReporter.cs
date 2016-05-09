using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Services {
   
   public class CreditCardReporter {

      public IQueryable<CreditCardReportItem> GetCreditCards() {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         User user = User.CurrentUser;

         return
            from c in user.CreditCardsQuery
            select new CreditCardReportItem {
               ExpirationMonth = c.ExpirationMonth,
               ExpirationYear = c.ExpirationYear,
               Id = c.Id,
               IsDefault = c.IsDefault,
               LastFour = c.LastFour,
               NameOnCard = c.NameOnCard,
               Type = c.Type
            };
      }
   }
}
