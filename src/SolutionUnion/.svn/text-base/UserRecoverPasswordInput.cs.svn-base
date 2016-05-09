using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using SolutionUnion.Resources.Validation;
using SolutionUnion.Resources.Model;

namespace SolutionUnion {
   
   public class UserRecoverPasswordInput {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DisplayNameLocalized("User_Email", typeof(ModelRes))]
      public string Email { get; set; }
   }
}
