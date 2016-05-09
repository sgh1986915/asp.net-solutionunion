using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class SignUp {

      public long Id { get; set; }
      public long? InvitationId { get; set; }
      public long UserRoleId { get; set; }
      public string Email { get; set; }
      public string Password { get; set; }
      public string CompanyName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Phone { get; set; }
      public string Error { get; set; }
      public DateTime Created { get; set; }

      public virtual UserRole UserRole { get; set; }
      public virtual Invitation Invitation { get; set; }

      internal OperationResult UpdateError(string error) {

         var repo = Repository<SignUp>.GetInstance();

         this.Error = error;

         repo.SaveChanges(this);

         return new SuccessfulResult();
      }

      internal IUserSettings GetUserSettings() {

         if (this.Invitation != null)
            return this.Invitation;

         var defaultSettings = ApplicationSetting.Instance;

         return new Invitation { 
            LiteAccountMinimumFee = defaultSettings.LiteAccountMinimumFee,
            LiteCostPerGb = defaultSettings.LiteCostPerGb,
            LiteVaultMinimumGb = defaultSettings.LiteVaultMinimumGb,
            MinimumMonthlyFee = defaultSettings.MinimumMonthlyFee,
            ProAccountMinimumFee = defaultSettings.ProAccountMinimumFee,
            ProVaultMinimumGb = defaultSettings.ProVaultMinimumGb
         };
      }

      internal OperationResult Delete() {

         var repo = Repository<SignUp>.GetInstance();

         repo.Delete(this);

         return new SuccessfulResult();
      }

   }
}
