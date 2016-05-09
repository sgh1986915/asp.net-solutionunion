using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqKit;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class DashboardReporter {

      public DashboardReport GetReport() {

         var principal = User.CurrentPrincipal;

         if (!principal.Identity.IsAuthenticated)
            throw new InvalidOperationException();

         User user = User.CurrentUser;

         var report = new DashboardReport();

         if (principal.IsInRole(UserRole.Administrator)
            || principal.IsInRole(UserRole.Reseller)) {

            var accounts =
               from a in user.AccountsQuery
               where !a.MarkedAsDeleted
               select a;

            report.NumberOfAccounts = accounts.LongCount();
            report.NumberOfTrialAccounts = accounts.Where(a => !a.IsPaid).LongCount();

            report.ProStorageGb = 
               (from a in accounts
                where !a.Server.IsAcb
                select (decimal?)a.TotalSize)
                .Sum()
                .GetValueOrDefault() / (1024 * 1024 * 1024);

            var finishedSuccessfulyExpr = BackupJobReportSummary.JobFinishedSuccessfullyExpr;

            report.NumberOfSuccessfulJobs =
               (from a in user.AccountsQuery
                from j in a.BackUpJobReportSummaries
                where finishedSuccessfulyExpr.Invoke(j)
                select j.Id).LongCount();

            decimal usageBytes =
               (from a in accounts
                where !a.IsAcb
                select (decimal?)a.UsageAverageCache)
                .Sum()
                .GetValueOrDefault();

            int usageGb = (usageBytes > 0) ? (int)Decimal.Ceiling(usageBytes / (1024 * 1024 * 1024)) : 1;

            report.PerGbRate = user.GetProAdditionalGbPrice(usageGb);

            var topUsers =
               (from a in accounts
                where a.TotalSize > 0
                orderby a.TotalSize descending
                select new DashboardReportTopUsageUser {
                   AccountName = a.AccountName,
                   Email = a.User.Email,
                   Usage = a.TotalSize
                }).Take(4);

            foreach (var item in topUsers) 
               report.TopUsageUsers.Add(item);

            var topUsedQuota =
               (from a in accounts
                where a.VaultSize > 0
                let percent = (a.TotalSize / a.VaultSize) * 100
                where percent > 0
                orderby percent descending
                select new DashboardReportTopUsedQuotaUser {
                   AccountId = a.Id,
                   AccountName = a.AccountName,
                   Percent = percent,
                   QuotaGb = a.VaultSize / (1024 * 1024 * 1024)
                }).Take(6);

            foreach (var item in topUsedQuota) 
               report.TopUsedQuotaUsers.Add(item);

            var jobFinishedSuccessfulyExpr = BackupJobReportSummary.JobFinishedSuccessfullyExpr;
            var now = DateTime.Now;
            var oneDayAgo = now.AddDays(-1);

            var nonSuccessfulJobs =
               (from a in accounts
                from j in a.BackUpJobReportSummaries
                where !jobFinishedSuccessfulyExpr.Invoke(j)
                   && (j.JobDate > oneDayAgo && j.JobDate <= now)
                orderby j.JobDate descending
                select new DashboardNonSuccessfulJob {
                   AccountId = a.Id,
                   AccountName = j.Account.AccountName,
                   SetName = j.SetName,
                   JobDate = j.JobDate,
                   JobDescription = j.JobDescription,
                   JobSummaryId = j.Id
                });

            foreach (var item in nonSuccessfulJobs.Take(10)) 
               report.NonSuccessfulJobs.Add(item);

            report.HasAllNonSuccessfulJobs = report.NonSuccessfulJobs.Count == nonSuccessfulJobs.Count();
         }

         return report;
      }

      public DashboardDailyUsageReport GetDailyUsage(DateTime? startDate) {

         if (startDate == null)
            startDate = DateTime.Today.AddMonths(-1);

         long userId = User.CurrentUserId;

         var repo = Repository<Account>.GetInstance();

         var endDate = startDate.Value.AddMonths(1);

         var report = new DashboardDailyUsageReport {
            StartDate = startDate.Value,
            EndDate = endDate
         };

         foreach (var servType in new[] { ServerType.Lite, ServerType.Pro  }) {
		      
            bool isAcb = servType == ServerType.Lite;

            var query =
               from du in repo.CreateAssociationQuery(a => a.DailyUsages)
               where du.UserId == userId
                  && du.IsAcb == isAcb
                  && du.Date >= startDate.Value
                  && du.Date <= endDate
                  && du.IsActive
               group du by du.Date into grp
               orderby grp.Key
               select new DashboardDailyUsageReportItem {
                  Date = grp.Key,
                  Usage = grp.Sum(d => d.TotalSize)
               };

            var collection = (servType == ServerType.Pro) ?
               report.ProUsage : report.LiteUsage;

            foreach (var item in query) 
               collection.Add(item);

            if (endDate == DateTime.Today) {
               collection.Add(
                  new DashboardDailyUsageReportItem {
                     Date = endDate,
                     Usage =
                        (from a in repo.CreateQuery()
                         where a.UserId == userId
                           && !a.MarkedAsDeleted
                           && a.Server.IsAcb == isAcb
                         select (decimal?)a.TotalSize)
                        .Sum()
                        .GetValueOrDefault()
                  }
               );
            }
	      }

         var allItems = report.LiteUsage.Concat(report.ProUsage);

         decimal maxUsageMb = allItems
            .Select(p => (decimal?)p.Usage)
            .Max().GetValueOrDefault() / (1024 * 1024);

         decimal divisor;
         string unit;

         if (maxUsageMb < 1024) {
            divisor = (1024 * 1024);
            unit = "Mb";

         } else { 
            divisor = (1024 * 1024 * 1024);
            unit = "Gb";
         }

         foreach (var item in allItems) 
            item.Usage = Decimal.Round(item.Usage / divisor, 1);

         report.Unit = unit;

         return report;
      }
   }
}
