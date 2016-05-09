using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class BackupJobReportSummary {

      internal static readonly Expression<Func<BackupJobReportSummary, bool>> JobFinishedSuccessfullyExpr =
         j => j.JobStatus == "BS_STOP_SUCCESS";

      static readonly Func<BackupJobReportSummary, bool> JobFinishedSuccessfully =
         JobFinishedSuccessfullyExpr.Compile();

      public long Id { get; set; }
      public long AccountId { get; set; }
      public long SetId { get; set; }
      public string SetName { get; set; }
      public string JobId { get; set; }
      public DateTime JobDate { get; set; }
      public string JobStatus { get; set; }
      public string JobDescription { get; set; }
      public DateTime DateCreated { get; set; }

      public virtual Account Account { get; set; }

      public bool FinishedSuccessfully { 
         get {
            return JobFinishedSuccessfully(this);
         } 
      }

      public static BackupJobReportSummary Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         var repo = Repository<Account>.GetInstance();

         var query =
            from a in repo.CreateQuery()
            from j in a.BackUpJobReportSummaries
            where j.Id == id
            select j;

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            query = query.Where(a => a.Account.UserId == User.CurrentUserId);

         return query.FirstOrDefault();
      }

      public OperationResult GetBackupJobReport() {
         return this.Account.Server.GetBackupJobReport(this.Account.AccountName, this.SetId, this.JobId);
      }
   }
}
