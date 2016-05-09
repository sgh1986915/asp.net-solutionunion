using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class AccountSynchronizer {

      public OperationResult SynchronizeAll() {

         var repo = Repository<Account>.GetInstance();

         foreach (var account in repo.CreateQuery().Where(a => a.IsActive && !a.User.IsSuspended).ToList()) {
            account.UpdateCachedMembers();
            account.SynchronizeWithServer();
         }

         var newAccRepo = Repository<NewAccount>.GetInstance();

         foreach (var newAccount in newAccRepo.CreateQuery().ToList()) {
            newAccount.SynchronizeWithServer();
         }

         return new SuccessfulResult();
      }

      public OperationResult SynchronizeAllSuperFrequent() {

         var repo = Repository<Account>.GetInstance();

         foreach (var account in repo.CreateQuery().Where(a => a.IsActive && !a.User.IsSuspended).ToList()) {
            account.ImportBackUpJobReportSummaries();
         }

         return new SuccessfulResult();
      }
   }
}
