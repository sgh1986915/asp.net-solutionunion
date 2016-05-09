using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SolutionUnion.Resources.Validation;

namespace SolutionUnion {
   
   public class ServiceEdit {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DisplayName("Service Description")]
      public string Description { get; set; }

      [DisplayName("Monthly Price")]
      public decimal Price { get; set; }
   }
}
