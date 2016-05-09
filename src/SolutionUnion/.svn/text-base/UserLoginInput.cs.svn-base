using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SolutionUnion.Resources.Model;
using SolutionUnion.Resources.Validation;

namespace SolutionUnion {
   
   public class UserLoginInput {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DisplayNameLocalized("User_Email", typeof(ModelRes))]
      public string Email { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DataType(DataType.Password)]
      [DisplayNameLocalized("User_Password", typeof(ModelRes))]
      public string Password { get; set; }
   }
}
