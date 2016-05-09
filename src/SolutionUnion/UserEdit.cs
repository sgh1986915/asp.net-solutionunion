using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SolutionUnion {
   
   public class UserEdit : UserProfile, IUserSettings {

      // Settings

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

      // Other

      [HiddenInput]
      public decimal CurrentDiscount { get; set; }
   }
}
