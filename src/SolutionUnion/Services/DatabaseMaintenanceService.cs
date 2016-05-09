using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class DatabaseMaintenanceService {

      public void Run() {

         var repo = Repository<User>.GetInstance();

         DeleteMarkedUsers();
         DeleteMarkedAccounts();
         MarkAsDeletedOldTrialAccounts();

         foreach (User user in repo.CreateQuery().ToList()) {

            bool wasLocked = user.IsLocked;

            if (!wasLocked) {
               user.IsLocked = true;
               repo.SaveChanges(user);
            }

            try {
               foreach (Account account in user.Accounts)
                  account.CreateDailyUsage();

               user.ChargeMonth();
               user.UpdateCachedMembers();

            } finally {

               user.IsLocked = wasLocked;
               repo.SaveChanges(user);
            }
         }

         ClearDatabase();
      }

      void DeleteMarkedUsers() {

         var repo = Repository<User>.GetInstance();

         var markedAsDeleted = repo.CreateQuery().Where(u => u.MarkedAsDeleted);

         foreach (User user in markedAsDeleted.ToList()) {

            if (user.MarkedAsDeletedDate.GetValueOrDefault().AddDays(3) > DateTime.Now)
               continue;

            user.Delete();
         }
      }

      void DeleteMarkedAccounts() {

         var repo = Repository<Account>.GetInstance();

         var markedAsDeleted = repo.CreateQuery().Where(a => a.MarkedAsDeleted);

         foreach (Account account in markedAsDeleted.ToList()) {
            
            if (account.MarkedAsDeletedDate.AddDays(3) > DateTime.Now) 
               continue;

            account.Delete();
         }
      }

      void MarkAsDeletedOldTrialAccounts() {

         var repo = Repository<Account>.GetInstance();

         var date = DateTime.Now.AddDays(-57);

         var oldAccounts =
            from a in repo.CreateQuery()
            where !a.IsPaid
               && !a.MarkedAsDeleted
               && a.Created <= date
            select a;

         foreach (var account in oldAccounts.ToList()) 
            account.MarkAsDeleted();
      }
      
      void ClearDatabase() {

         var repo = Repository<Account>.GetInstance();
         var sessionRepo = Repository<Session>.GetInstance();

         DateTime sixtyDaysAgo = DateTime.Now.AddDays(-60);
         DateTime yearAgo = DateTime.Now.AddYears(-1);

         var oldUsages =
            (from a in repo.CreateQuery()
             let da = a.DailyUsages.Where(da => da.Date <= yearAgo)
             select da).SelectMany(p => p);

         var oldSessions =
            from s in sessionRepo.CreateQuery()
            where s.Created <= sixtyDaysAgo
            select s;

         using (var tx = new TransactionScope()) {
            
            foreach (var item in oldUsages.ToList()) 
               repo.DeleteRelated(item, a => a.DailyUsages);

            foreach (var item in oldSessions.ToList()) 
               sessionRepo.Delete(item);

            tx.Complete();
         }
      }
   }
}
