using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SolutionUnion.Resources.Validation;
using SolutionUnion.Resources.Model;

namespace SolutionUnion {
   
   public class UserChangePasswordInput {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(255, MinimumLength = 6)]
      [DataType(DataType.Password)]
      [DisplayNameLocalized("User_NewPassword", typeof(ModelRes))]
      public string NewPassword { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [Compare("NewPassword", ErrorMessageResourceName = "User_PasswordsNotMatch", ErrorMessageResourceType = typeof(ValidationRes))]
      [DataType(DataType.Password)]
      [DisplayNameLocalized("User_ConfirmNewPassword", typeof(ModelRes))]
      public string ConfirmNewPassword { get; set; }
   }
}
