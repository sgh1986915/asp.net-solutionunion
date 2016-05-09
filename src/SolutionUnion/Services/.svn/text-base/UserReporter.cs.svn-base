using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class UserReporter {

      readonly Repository<User> repo;

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

      public static IDictionary<string, string> GetRoleList() {

         return new[] { UserRole.Administrator, UserRole.Reseller, UserRole.Retail }
            .ToDictionary(r => r, r => r);
      }

      public UserReporter(Repository<User> repo) {

         if (repo == null) throw new ArgumentNullException("repo");

         this.repo = repo;
      }

      public IQueryable<UserReportItem> GetUsers(string az = null, string company = null, string role = null, string keyword = null) {

         var query = this.repo.CreateQuery().Where(u => !u.MarkedAsDeleted);

         return Map(ApplyFilters(query, az: az, company: company, role: role, keyword: keyword));
      }

      public IQueryable<UserReportItem> GetMarkedAsDeleted(string az = null, string company = null, string role = null, string keyword = null) {

         var query = this.repo.CreateQuery().Where(u => u.MarkedAsDeleted);

         return Map(ApplyFilters(query, az: az, company: company, role: role, keyword: keyword));
      }

      IQueryable<User> ApplyFilters(IQueryable<User> query, string az = null, string company = null, string role = null, string keyword = null) {

         if (az.HasValue()) {
            string[] letters = az.Split('-');

            if (letters.Length == 2) {
               string first = letters[0], second = letters[1];

               query = query.Where(u => u.CompanyName.StartsWith(first) || u.CompanyName.StartsWith(second));
            }
         }

         if (company.HasValue())
            query = query.Where(u => u.CompanyName == company);

         if (role.HasValue())
            query = query.Where(u => u.UserRole.Name == role);

         if (keyword.HasValue())
            query = query.Where(u => u.Email.Contains(keyword) || u.CompanyName.Contains(keyword) || u.FirstName.Contains(keyword) || u.LastName.Contains(keyword));

         return query;
      }

      IQueryable<UserReportItem> Map(IQueryable<User> query) {

         return
            from u in query
            let unpaidInvoices = u.Invoices.Where(p => !p.IsPaid)
            select new UserReportItem {
               CompanyName = u.CompanyName,
               Email = u.Email,
               IsSuspended = u.IsSuspended,
               IsLocked = u.IsLocked,
               FirstName = u.FirstName,
               LastName = u.LastName,
               DateCreated = u.Created,
               UserId = u.Id,
               UserRoleName = u.UserRole.Name,
               MarkedAsDeletedDate = u.MarkedAsDeletedDate,
               Balance = ((decimal?)unpaidInvoices.Sum(p => p.Total) ?? 0) * -1,
               Subtotal = u.SubtotalCache,
               
               User = u
            };
      }
   }
}
