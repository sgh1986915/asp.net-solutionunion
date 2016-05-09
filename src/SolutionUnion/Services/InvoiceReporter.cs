using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class InvoiceReporter {

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

      public InvoiceReporter(Repository<User> repo) {

         if (repo == null) throw new ArgumentNullException("repo");

         this.repo = repo;
      }

      public IQueryable<InvoiceReportItem> GetInvoices(string az = null, string company = null, string keyword = null, string role = null, long? userId = null) {

         var query = (from u in this.repo.CreateQuery()
                      where !u.MarkedAsDeleted
                      select u.Invoices).SelectMany(i => i);

         if (userId.HasValue)
            query = query.Where(i => i.UserId == userId.Value);

         if (az.HasValue()) {
            string[] letters = az.Split('-');

            if (letters.Length == 2) {
               string first = letters[0], second = letters[1];

               query = query.Where(i => i.User.CompanyName.StartsWith(first) || i.User.CompanyName.StartsWith(second));
            }
         }

         if (company.HasValue())
            query = query.Where(i => i.User.CompanyName == company);

         if (keyword.HasValue()) 
            query = query.Where(i => i.User.Email.Contains(keyword) || i.User.CompanyName.Contains(keyword));

         if (role.HasValue())
            query = query.Where(i => i.User.UserRole.Name == role);

         return
            from i in query
            select new InvoiceReportItem {
               Amount = i.Total,
               Created = i.Created,
               Id = i.Id,
               CompanyName = i.User.CompanyName,
               IsPaid = i.IsPaid
            };
      }
   }
}
