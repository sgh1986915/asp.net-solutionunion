using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Resources.Validation;
using SolutionUnion.Resources.Model;

namespace SolutionUnion {
   
   public class UserSignUpInput : UserProfile {

      // User Info

      [Remote("SignUp_EmailAvailability", "~Home")]
      public override string Email {
         get {
            return base.Email;
         }
         set {
            base.Email = value;
         }
      }

      [Remote("SignUp_CompanyNameAvailability", "~Home")]
      public override string CompanyName {
         get {
            return base.CompanyName;
         }
         set {
            base.CompanyName = value;
         }
      }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DisplayName("User Type")]
      public string Role { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(255, MinimumLength = 6)]
      [DataType(DataType.Password)]
      [DisplayNameLocalized("User_Password", typeof(ModelRes))]
      public string Password { get; set; }

      [Compare("Password", ErrorMessageResourceName = "User_PasswordsNotMatch", ErrorMessageResourceType = typeof(ValidationRes))]
      [DataType(DataType.Password)]
      [DisplayNameLocalized("User_ConfirmPassword", typeof(ModelRes))]
      public string ConfirmPassword { get; set; }

      // Other

      public bool AgreedToTermsOfService { get; set; }

      public long InvitationId { get; set; }

      public static IDictionary<string, string> GetRoles() {

         return new[] { UserRole.Retail, UserRole.Reseller }
            .ToDictionary(s => s, s => s);
      }
   }
}
