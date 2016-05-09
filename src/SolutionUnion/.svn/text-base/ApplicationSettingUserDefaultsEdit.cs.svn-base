using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SolutionUnion {
   
   public class ApplicationSettingUserDefaultsEdit {

      [DisplayName("PRO Vault Minimum")]
      public decimal ProVaultMinimumGb { get; set; }

      [DisplayName("PRO Base Pricing")]
      public decimal ProAccountMinimumFee { get; set; }

      [DisplayName("LITE Vault Minimum")]
      public decimal LiteVaultMinimumGb { get; set; }

      [DisplayName("LITE Base Pricing")]
      public decimal LiteAccountMinimumFee { get; set; }

      [DisplayName("LITE Gb Pricing")]
      public decimal LiteCostPerGb { get; set; }

      [DisplayName("Monthly Minimum")]
      public decimal MinimumMonthlyFee { get; set; }

      [DisplayName("PRO Pricing Scale")]
      public IList<ApplicationSettingUserStoragePrice> ProPricingScale { get; private set; }

      public ApplicationSettingUserDefaultsEdit() {
         this.ProPricingScale = new Collection<ApplicationSettingUserStoragePrice>();
      }
   }

   public class ApplicationSettingUserStoragePrice {

      [DisplayName("Storage Gb")]
      public int StorageGb { get; set; }

      [DisplayName("Price per Gb")]
      public decimal PricePerGb { get; set; }
   }
}
