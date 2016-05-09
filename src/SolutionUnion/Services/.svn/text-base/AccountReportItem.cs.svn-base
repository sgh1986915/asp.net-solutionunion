using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Services {

   public class AccountReportItem {

      public long AccountId { get; set; }
      public long ServerId { get; set; }

      public long UserId { get; set; }
      public string AccountName { get; set; }
      public string CompanyName { get; set; }
      public string Email { get; set; }
      public string ServerDescription { get; set; }
      public decimal Used { get; set; }
      public decimal Quota { get; set; }
      public bool IsAcb { get; set; }
      public bool IsPaid { get; set; }
      public bool IsActive { get; set; }
      public DateTime DateCreated { get; set; }
      public DateTime MarkedAsDeletedDate { get; set; }
      public bool IndividualMicrosoftExchange { get; set; }

      public decimal Average { get; set; }
      public decimal Cost { get; set; }

      public TimeSpan TimeLeft {
         get {
            return MarkedAsDeletedDate.AddDays(3) - DateTime.Now;
         }
      }
   }
}
