using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Services {
   
   public class InvoiceReportItem {

      public long Id { get; set; }
      public decimal Amount { get; set; }
      public bool IsPaid { get; set; }
      public string CompanyName { get; set; }
      public DateTime Created { get; set; }
   }
}
