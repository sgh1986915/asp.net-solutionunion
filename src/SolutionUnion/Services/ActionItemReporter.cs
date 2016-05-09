using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class ActionItemReporter {

      public OperationResult Run(Controller controller) {

         var repo = Repository<User>.GetInstance();
         var smtp = new SmtpClient();

         var users = from u in repo.CreateQuery()
                     where !u.IsSuspended
                        && (u.UserRole.Name == UserRole.Administrator
                           || u.UserRole.Name == UserRole.Reseller)
                     select u;

         foreach (var user in users.ToList()) {

            var accounts = from a in user.AccountsQuery
                           where a.IsActive
                           select a;

            var report = new ActionItemReport {
               UserCompanyName = user.CompanyName
            };

            foreach (var account in accounts) {

               var jobsSummResult = account.Server.GetBackupJobReportSummaries(account.AccountName);

               if (jobsSummResult.IsError)
                  continue;

               var jobsSummResultOK = (ServerGetBackupJobReportSummariesResult)jobsSummResult;

               foreach (var job in jobsSummResultOK.Jobs) 
                  report.Jobs.Add(job);
            }

            var mail = new MailMessage {
               To = { user.Email },
               Subject = "Solution Union Dashboard: Daily Action Items",
               Body = MailViewRenderer.Render("ActionItemReport", report, controller.ControllerContext),
               IsBodyHtml = true
            };

            try {
               smtp.Send(mail);
            } catch (SmtpException) { }
         }

         return new SuccessfulResult();
      }
   }
}
