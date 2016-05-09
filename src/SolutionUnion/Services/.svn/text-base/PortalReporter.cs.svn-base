using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SolutionUnion.Repositories;
using System.Linq.Expressions;
using LinqKit;
using Microsoft.Practices.ServiceLocation;

namespace SolutionUnion.Services {
   
   public class PortalReporter {

      readonly Repository<Account> repo;

      public static IDictionary<string, string> GetDates() {

         return
            (from i in Enumerable.Range(0, 30)
             let date = DateTime.Today.AddDays(i * -1)
             select date).ToDictionary(d => d.ToString("yyyy-MM-dd"), d => d.ToString("d", CultureInfo.CurrentUICulture));
      }

      public PortalReporter(Repository<Account> repo) {
         this.repo = repo;
      }

      public OperationResult GetReport(DateTime date, bool portalLog, bool failedJobs, bool successfulJobs, long? accountId = null, string keyword = null) {

         var currentPrincipal = User.CurrentPrincipal;

         if (!currentPrincipal.Identity.IsAuthenticated)
            throw new InvalidOperationException();

         long userId = User.CurrentUserId;
         bool userIsAdmin = currentPrincipal.IsInRole(UserRole.Administrator);
         
         var result = new PortalReport();

         DateTime startInclusive = date.Date;
         DateTime endExclusive = startInclusive.AddDays(1);

         if (portalLog) {

            var sessionRepo = Repository<Session>.GetInstance();
            var accountRepo = Repository<Account>.GetInstance();

            var sessions = sessionRepo.CreateQuery();

            if (!userIsAdmin)
               sessions = sessions.Where(s => s.UserId == userId);

            var portalLogs = from s in sessions
                             from l in s.Logs
                             where (l.Created >= startInclusive && l.Created < endExclusive)
                             select l;

            if (accountId.HasValue) 
               portalLogs = portalLogs.Where(l => l.AccountId == accountId);

            if (keyword.HasValue()) 
               portalLogs = portalLogs.Where(l => l.Message.Contains(keyword) || l.AccountName.Contains(keyword));

            var logs =
               from l in portalLogs
               select new PortalReportLog {
                  AccountId = l.AccountId,
                  AccountName = l.AccountName,
                  Created = l.Created,
                  Message = l.Message
               };

            var accountIds =
               (from l in logs
                where l.AccountId.HasValue
                select l.AccountId.Value)
               .Distinct()
               .ToArray();

            var companyNames =
               (from a in accountRepo.CreateQuery()
                where accountIds.Contains(a.Id)
                select new {
                   AccountId = a.Id,
                   CompanyName = a.User.CompanyName
                }).ToDictionary(p => p.AccountId, p => p.CompanyName);


            foreach (var log in logs) {

               if (log.AccountId.HasValue) {
                  string companyName;

                  if (companyNames.TryGetValue(log.AccountId.Value, out companyName))
                     log.CompanyName = companyName;
               }

               result.PortalLog.Add(log);
            }
         }

         var jobFinishedSuccessfullyExpr = BackupJobReportSummary.JobFinishedSuccessfullyExpr;

         var accounts = this.repo.CreateQuery();

         if (!userIsAdmin || !accountId.HasValue)
            accounts = accounts.Where(a => a.UserId == userId);

         if (accountId.HasValue)
            accounts = accounts.Where(a => a.Id == accountId.Value);

         if (failedJobs) {

            var query =
               (from a in accounts
                let jobs =
                  from j in a.BackUpJobReportSummaries
                  let success = jobFinishedSuccessfullyExpr.Invoke(j)
                  where !success
                     && (j.JobDate >= startInclusive && j.JobDate < endExclusive)
                  select new {
                     Account = a,
                     Job = new PortalReportBackupJob {
                        SummaryId = j.Id,
                        AccountId = a.Id,
                        AccountName = a.AccountName,
                        JobDate = j.JobDate,
                        JobDescription = j.JobDescription,
                        SetName = j.SetName
                     }
                  }
                select jobs).SelectMany(jl => jl);

            if (keyword.HasValue()) {
               query = query.Where(p => p.Account.User.Email.Contains(keyword)
                  || p.Account.User.FirstName.Contains(keyword)
                  || p.Account.User.LastName.Contains(keyword)
                  || p.Account.User.CompanyName.Contains(keyword)
                  || p.Account.AccountName.Contains(keyword)
                  || p.Job.SetName.Contains(keyword)
                  || p.Job.JobDescription.Contains(keyword));
            }

            foreach (var job in query.Select(p => p.Job)) 
               result.FailedJobs.Add(job);
         }

         if (successfulJobs) {

            var query =
               (from a in accounts
                let jobs =
                   from j in a.BackUpJobReportSummaries
                   let success = jobFinishedSuccessfullyExpr.Invoke(j)
                   where success
                      && (j.JobDate >= startInclusive && j.JobDate < endExclusive)
                   select new {
                      Account = a,
                      Job = new PortalReportBackupJob {
                         SummaryId = j.Id,
                         AccountId = a.Id,
                         AccountName = a.AccountName,
                         JobDate = j.JobDate,
                         JobDescription = j.JobDescription,
                         SetName = j.SetName
                      }
                   }
                select jobs).SelectMany(jl => jl);

            if (keyword.HasValue()) {
               query = query.Where(p => p.Account.User.Email.Contains(keyword)
                  || p.Account.User.FirstName.Contains(keyword)
                  || p.Account.User.LastName.Contains(keyword)
                  || p.Account.User.CompanyName.Contains(keyword)
                  || p.Account.AccountName.Contains(keyword)
                  || p.Job.SetName.Contains(keyword)
                  || p.Job.JobDescription.Contains(keyword));
            }

            foreach (var job in query.Select(p => p.Job)) 
               result.SuccessfulJobs.Add(job);
         }

         return result;
      }
   }
}
