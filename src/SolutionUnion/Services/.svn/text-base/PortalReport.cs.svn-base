using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Services {
   
   public class PortalReport : SuccessfulResult {

      public IList<PortalReportLog> PortalLog { get; set; }
      public IList<PortalReportBackupJob> FailedJobs { get; set; }
      public IList<PortalReportBackupJob> SuccessfulJobs { get; set; }

      public PortalReport() {

         this.PortalLog = new Collection<PortalReportLog>();
         this.FailedJobs = new Collection<PortalReportBackupJob>();
         this.SuccessfulJobs = new Collection<PortalReportBackupJob>();
      }
   }

   public class PortalReportLog {
      public long? AccountId { get; set; }
      public string AccountName { get; set; }
      public string CompanyName { get; set; }
      public string Message { get; set; }
      public DateTime Created { get; set; }
   }

   public class PortalReportBackupJob {

      public long SummaryId { get; set; }
      public long AccountId { get; set; }
      public string AccountName { get; set; }
      public string SetName { get; set; }
      public DateTime JobDate { get; set; }
      public string JobDescription { get; set; }
   }
}
