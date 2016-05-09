using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SolutionUnion.Resources.Validation;

namespace SolutionUnion {

   public class InvitationEdit : IUserSettings {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [DisplayName("Role")]
      public string Role { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(255)]
      [RegularExpression(@"\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Please enter a valid email address.")]
      [DisplayName("Email Address")]
      public virtual string Email { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(41, MinimumLength = 3)]
      [DisplayName("Company")]
      public virtual string CompanyName { get; set; }

      [DisplayName("Monthly Minimum")]
      public decimal MinimumMonthlyFee { get; set; }

      [DisplayName("PRO Vault Minimum")]
      public decimal ProVaultMinimumGb { get; set; }

      [DisplayName("PRO Base Pricing")]
      public decimal ProAccountMinimumFee { get; set; }

      [DisplayName("PRO Gb Pricing")]
      public decimal? ProCostPerGb { get; set; }

      [DisplayName("LITE Vault Minimum")]
      public decimal LiteVaultMinimumGb { get; set; }

      [DisplayName("LITE Base Pricing")]
      public decimal LiteAccountMinimumFee { get; set; }

      [DisplayName("LITE Gb Pricing")]
      public decimal LiteCostPerGb { get; set; }

      [DisplayName("Percentage Discount")]
      [Range(0, 100)]
      public decimal PercentageDiscount { get; set; }

      public bool PercentageDiscountEnabled { get; set; }

      [DisplayName("Fixed Backup Billing")]
      public decimal? FixedBackupBilling { get; set; }

      public InvitationEdit() {
         
         this.Role = UserRole.Reseller;

         var defaultSettings = ApplicationSetting.Instance;

         this.LiteAccountMinimumFee = defaultSettings.LiteAccountMinimumFee;
         this.LiteCostPerGb = defaultSettings.LiteCostPerGb;
         this.LiteVaultMinimumGb = defaultSettings.LiteVaultMinimumGb;
         this.MinimumMonthlyFee = defaultSettings.MinimumMonthlyFee;
         this.ProAccountMinimumFee = defaultSettings.ProAccountMinimumFee;
         this.ProVaultMinimumGb = defaultSettings.ProVaultMinimumGb;
      }
   }
}
