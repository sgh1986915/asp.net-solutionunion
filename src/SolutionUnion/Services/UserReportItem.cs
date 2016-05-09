using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Services {
   
   public class UserReportItem {

      [NonSerialized]
      User _User;

      public long UserId { get; set; }
      public string UserRoleName { get; set; }
      public string Email { get; set; }
      public string CompanyName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public decimal Balance { get; set; }
      public decimal Subtotal { get; set; }
      public DateTime DateCreated { get; set; }
      public DateTime? MarkedAsDeletedDate { get; set; }
      public bool IsSuspended { get; set; }
      public bool IsLocked { get; set; }

      public TimeSpan TimeLeft { 
         get {
            return MarkedAsDeletedDate.GetValueOrDefault().AddDays(3) - DateTime.Now;
         } 
      }

      public decimal Total { 
         get { 
            return this.Subtotal - _User.GetDiscount(this.Subtotal); 
         } 
      }

      internal User User { 
         set { _User = value; } 
      }
   }
}
