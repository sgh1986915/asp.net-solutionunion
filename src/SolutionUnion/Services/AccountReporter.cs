using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Security.Principal;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {

   public class AccountReporter {

      readonly Repository<Account> repo;

      public static IDictionary<string, string> GetAlphabetList() {

         IDictionary<string, string> pairs = new Dictionary<string, string>();

         char c = 'A';
         for (int i = 0; i < 13; i++) {
            string item = c + "-" + (char)(c + 1);
            c = (char)((int)c + 2);
            pairs.Add(item, item);
         }

         return pairs;
      }

      public static IDictionary<string, string> GetServerList() {

         var repo = Repository<Server>.GetInstance();

         return repo.CreateQuery().ToList().ToDictionary(s => s.Id.ToString(), s => s.Description);
      }

      public AccountReporter(Repository<Account> repo) {

         if (repo == null) throw new ArgumentNullException("repo");

         this.repo = repo;
      }

      public IQueryable<AccountReportItem> GetAccounts(string az, string company, string keyword, AccountType? type, AccountStatus? status) {

         var query = this.repo.CreateQuery().Where(a => !a.MarkedAsDeleted);

         IPrincipal currentPrincipal = User.CurrentPrincipal;

         if (currentPrincipal.Identity.IsAuthenticated
            && currentPrincipal.IsInRole(UserRole.Reseller)) { 
            
            long currentUserId = User.CurrentUserId;
            
            query = query.Where(a => a.UserId == currentUserId);
         }

         if (az.HasValue()) {
            string[] letters = az.Split('-');

            if (letters.Length == 2) {
               string first = letters[0], second = letters[1];

               query = query.Where(a => a.AccountName.StartsWith(first) || a.AccountName.StartsWith(second));
            }
         }

         if (company.HasValue()) 
            query = query.Where(a => a.User.CompanyName == company);

         if (keyword.HasValue()) 
            query = query.Where(a => a.User.Email.Contains(keyword) || a.User.FirstName.Contains(keyword) || a.User.LastName.Contains(keyword) || a.User.CompanyName.Contains(keyword) || a.AccountName.Contains(keyword));

         if (type.HasValue) {
            bool isPaid = type.Value == AccountType.Paid;
            query = query.Where(a => a.IsPaid == isPaid);
         }

         if (status.HasValue) {
            bool isActive = status.Value == AccountStatus.Active;
            query = query.Where(a => a.IsActive == isActive);
         }

         return
            from a in query
            select new AccountReportItem {
               AccountId = a.Id,
               ServerId = a.ServerId,
               AccountName = a.AccountName,
               CompanyName = a.User.CompanyName,
               DateCreated = a.Created,
               Email = a.User.Email,
               IndividualMicrosoftExchange = a.IndividualMicrosoftExchange,
               IsAcb = a.IsAcb,
               IsActive = a.IsActive,
               IsPaid = a.IsPaid,
               ServerDescription = a.Server.Description,
               UserId = a.UserId,
               Quota = a.VaultSize,
               Used = a.TotalSize,
               Average = a.UsageAverageCache,
               Cost = a.CostCache,
            };
      }

      public IQueryable<AccountReportItem> GetMarkedAsDeleted(string az, long? serverId, string keyword, AccountStatus? status) {

         var query = this.repo.CreateQuery().Where(a => a.MarkedAsDeleted);

         IPrincipal currentPrincipal = User.CurrentPrincipal;

         if (currentPrincipal.Identity.IsAuthenticated
            && currentPrincipal.IsInRole(UserRole.Reseller)) {

            long currentUserId = User.CurrentUserId;

            query = query.Where(a => a.UserId == currentUserId);
         }

         if (az.HasValue()) {
            string[] letters = az.Split('-');

            if (letters.Length == 2) {
               string first = letters[0], second = letters[1];

               query = query.Where(a => a.AccountName.StartsWith(first) || a.AccountName.StartsWith(second));
            }
         }

         if (serverId.HasValue)
            query = query.Where(a => a.ServerId == serverId.Value);

         if (keyword.HasValue())
            query = query.Where(a => a.User.Email.Contains(keyword) || a.User.FirstName.Contains(keyword) || a.User.LastName.Contains(keyword) || a.User.CompanyName.Contains(keyword) || a.AccountName.Contains(keyword));

         if (status.HasValue) {
            bool isActive = status.Value == AccountStatus.Active;
            query = query.Where(a => a.IsActive == isActive);
         }

         return
            from a in query
            select new AccountReportItem {
               AccountId = a.Id,
               ServerId = a.ServerId,
               AccountName = a.AccountName,
               CompanyName = a.User.CompanyName,
               DateCreated = a.Created,
               Email = a.User.Email,
               IndividualMicrosoftExchange = a.IndividualMicrosoftExchange,
               IsAcb = a.IsAcb,
               IsActive = a.IsActive,
               IsPaid = a.IsPaid,
               ServerDescription = a.Server.Description,
               UserId = a.UserId,
               Quota = a.VaultSize,
               Used = a.TotalSize,
               MarkedAsDeletedDate = a.MarkedAsDeletedDate,
               Average = a.UsageAverageCache,
               Cost = a.CostCache
            };
      }
   }
}
