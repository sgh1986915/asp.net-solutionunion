using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using System.Net;
using System.Collections.ObjectModel;
using System.Transactions;

namespace SolutionUnion {
   
   public class ApplicationSetting {

      static readonly ApplicationSetting _Instance;

      ICollection<ApplicationSettingsStoragePrice> _ProPricingScale;

      public static ApplicationSetting Instance { get { return _Instance; } }

      public long Id { get; set; }
      public bool IsClosedForMaintenance { get; set; }
      public string ApplicationLogin { get; set; }
      public string ApplicationId { get; set; }
      public string ApplicationVersion { get; set; }
      public string ConnectionTicket { get; set; }
      public decimal MinimumMonthlyFee { get; set; }
      public decimal ProAccountMinimumFee { get; set; }
      public decimal ProVaultMinimumGb { get; set; }
      public decimal LiteAccountMinimumFee { get; set; }
      public decimal LiteCostPerGb { get; set; }
      public decimal LiteVaultMinimumGb { get; set; }
      public string ApiToken { get; set; }

      // internal to avoid EF one-to-many convention
      internal ICollection<ApplicationSettingsStoragePrice> ProPricingScale {
         get {
            if (_ProPricingScale == null) {
               _ProPricingScale = new ReadOnlyCollection<ApplicationSettingsStoragePrice>(
                  Repository<ApplicationSettingsStoragePrice>.GetInstance()
                     .CreateQuery()
                     .ToArray()
               );
            }
            return _ProPricingScale;
         }
      }

      static ApplicationSetting() {

         var repo = Repository<ApplicationSetting>.GetInstance();

         _Instance = repo.CreateQuery().First();
      }

      public ApplicationSettingUserDefaultsEdit EditUserDefaults() {

         var input = new ApplicationSettingUserDefaultsEdit { 
            LiteAccountMinimumFee = this.LiteAccountMinimumFee,
            LiteCostPerGb = this.LiteCostPerGb,
            LiteVaultMinimumGb = this.LiteVaultMinimumGb,
            MinimumMonthlyFee = this.MinimumMonthlyFee,
            ProAccountMinimumFee = this.ProAccountMinimumFee,
            ProVaultMinimumGb = this.ProVaultMinimumGb
         };

         foreach (var item in this.ProPricingScale) {
            input.ProPricingScale.Add(new ApplicationSettingUserStoragePrice {
               StorageGb = item.StorageGb,
               PricePerGb = item.PricePerGb
            });
         }

         return input;
      }

      public OperationResult UpdateUserDefaults(ApplicationSettingUserDefaultsEdit input) {

         var errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         ValidateProPricingScale(input, errors);

         if (errors.HasErrors)
            return errors;

         var repo = Repository<ApplicationSetting>.GetInstance();
         var priceRepo = Repository<ApplicationSettingsStoragePrice>.GetInstance();

         this.LiteAccountMinimumFee = input.LiteAccountMinimumFee;
         this.LiteCostPerGb = input.LiteCostPerGb;
         this.LiteVaultMinimumGb = input.LiteVaultMinimumGb;
         this.MinimumMonthlyFee = input.MinimumMonthlyFee;
         this.ProAccountMinimumFee = input.ProAccountMinimumFee;
         this.ProVaultMinimumGb = input.ProVaultMinimumGb;

         Func<ApplicationSettingsStoragePrice, ApplicationSettingUserStoragePrice, bool> eqComparer = (p, pi) => p.StorageGb == pi.StorageGb;
         var inputPrices = input.ProPricingScale.ToList();
         var currentPrices = this.ProPricingScale.ToList();

         var updatedPrices = currentPrices.Where(p => inputPrices.Any(pi => eqComparer(p, pi))).ToArray();

         using (var tx = new TransactionScope()) {

            foreach (var price in updatedPrices) {

               var inputPrice = inputPrices.Single(pi => eqComparer(price, pi));

               price.PricePerGb = inputPrice.PricePerGb;

               priceRepo.SaveChanges(price);

               currentPrices.Remove(price);
               inputPrices.Remove(inputPrice);
            }

            var deletedPrices = currentPrices.Where(p => !inputPrices.Any(pi => eqComparer(p, pi))).ToArray();

            foreach (var price in deletedPrices)
               priceRepo.Delete(price);

            foreach (var newPrice in inputPrices) {

               priceRepo.Add(
                  new ApplicationSettingsStoragePrice {
                     StorageGb = newPrice.StorageGb,
                     PricePerGb = newPrice.PricePerGb
                  }
               );
            }

            repo.SaveChanges(this);

            tx.Complete();
         }

         this._ProPricingScale = null;

         return new SuccessfulResult("Settings updated successfully.");
      }

      void ValidateProPricingScale(ApplicationSettingUserDefaultsEdit input, ErrorResult errors) {

         if (errors.Not(input.ProPricingScale.Count(p => p.StorageGb == 1) > 0, "The {1} must have a 1 GB entry.", () => input.ProPricingScale))
            return;

         if (errors.Not(input.ProPricingScale.Select(p => p.StorageGb).Distinct().Count() == input.ProPricingScale.Count, "Duplicates are not allowed.", () => input.ProPricingScale))
            return;
      }
   }
}
