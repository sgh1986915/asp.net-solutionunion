using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Services {
   
   public class DashboardReport {

      public long NumberOfAccounts { get; set; }
      public decimal ProStorageGb { get; set; }
      public long NumberOfTrialAccounts { get; set; }
      public long NumberOfSuccessfulJobs { get; set; }
      public decimal PerGbRate { get; set; }
      public Collection<DashboardReportTopUsageUser> TopUsageUsers { get; private set; }
      public Collection<DashboardReportTopUsedQuotaUser> TopUsedQuotaUsers { get; private set; }
      public Collection<DashboardNonSuccessfulJob> NonSuccessfulJobs { get; private set; }
      public bool HasAllNonSuccessfulJobs { get; set; }

      public DashboardReport() {
         this.TopUsageUsers = new Collection<DashboardReportTopUsageUser>();
         this.TopUsedQuotaUsers = new Collection<DashboardReportTopUsedQuotaUser>();
         this.NonSuccessfulJobs = new Collection<DashboardNonSuccessfulJob>();
      }
   }

   public class DashboardReportTopUsageUser {
      public string AccountName { get; set; }
      public string Email { get; set; }
      public decimal Usage { get; set; }
   }

   public class DashboardReportTopUsedQuotaUser {
      public long AccountId { get; set; }
      public string AccountName { get; set; }
      public decimal QuotaGb { get; set; }
      public decimal Percent { get; set; }
   }

   public class DashboardNonSuccessfulJob {
      public long AccountId { get; set; }
      public string AccountName { get; set; }
      public string SetName { get; set; }
      public DateTime JobDate { get; set; }
      public string JobDescription { get; set; }
      public long JobSummaryId { get; set; }
   }
}
