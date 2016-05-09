using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SolutionUnion {
   
   public class InvoicePdf {

      [Key, ForeignKey("Invoice")]
      public long InvoiceId { get; set; }

      public byte[] Pdf { get; set; }

      public virtual Invoice Invoice { get; set; }
   }
}
