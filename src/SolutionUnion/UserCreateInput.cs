using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Resources.Model;
using SolutionUnion.Resources.Validation;

namespace SolutionUnion {
   
   public class UserCreateInput : UserEdit {

      // User Info

      [Remote("Add_EmailAvailability", "~User")]
      public override string Email {
         get {
            return base.Email;
         }
         set {
            base.Email = value;
         }
      }

      [Remote("Add_CompanyNameAvailability", "~User")]
      public override string CompanyName {
         get {
            return base.CompanyName;
         }
         set {
            base.CompanyName = value;
         }
      }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
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

      public CreditCardEdit CreditCard { get; set; }
      
      public UserCreateInput() {
         
         this.Role = UserRole.Reseller;

         var defaultSettings = ApplicationSetting.Instance;

         this.LiteAccountMinimumFee = defaultSettings.LiteAccountMinimumFee;
         this.LiteCostPerGb = defaultSettings.LiteCostPerGb;
         this.LiteVaultMinimumGb = defaultSettings.LiteVaultMinimumGb;
         this.MinimumMonthlyFee = defaultSettings.MinimumMonthlyFee;
         this.ProAccountMinimumFee = defaultSettings.ProAccountMinimumFee;
         this.ProVaultMinimumGb = defaultSettings.ProVaultMinimumGb;

         this.CreditCard = new CreditCardEdit();
      }
   }
}
