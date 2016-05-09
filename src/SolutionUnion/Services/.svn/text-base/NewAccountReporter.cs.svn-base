using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class NewAccountReporter {

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

         return repo.CreateQuery().ToDictionary(s => s.Id.ToString(), s => s.Description);
      }

      public IQueryable<NewAccountReportItem> GetNewAccounts(string az, string keyword, long? serverId) {

         var repo = Repository<NewAccount>.GetInstance();

         var query = repo.CreateQuery();

         if (keyword.HasValue())
            query = query.Where(a => a.Email.Contains(keyword) || a.AccountName.Contains(keyword) || a.Server.UserName.Contains(keyword));

         if (az.HasValue()) {
            string[] letters = az.Split('-');

            if (letters.Length == 2) {
               string first = letters[0], second = letters[1];

               query = query.Where(a => a.AccountName.StartsWith(first) || a.AccountName.StartsWith(second));
            }
         }

         if (serverId.HasValue) 
            query = query.Where(p => p.ServerId == serverId.Value);

         return
            from a in query
            select new NewAccountReportItem { 
               AccountName = a.AccountName,
               Created = a.Created,
               Email = a.Email,
               Id = a.Id,
               IsAcb = a.IsAcb,
               IsActive = a.IsActive,
               IsPaid = a.IsPaid,
               JobId = a.JobId,
               JobStatus = a.JobStatus,
               ListId = a.ListId,
               ServerId = a.ServerId,
               ServerDescription = a.Server.Description,
               SetId = a.SetId,
               SetName = a.SetName
            };
      }
   }
}
