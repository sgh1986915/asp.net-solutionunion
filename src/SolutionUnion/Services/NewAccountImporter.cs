using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class NewAccountImporter {

      public OperationResult Import() {

         var repo = Repository<NewAccount>.GetInstance();
         var serverRepo = Repository<Server>.GetInstance();

         StringBuilder mail = new StringBuilder();

         foreach (Server server in serverRepo.CreateQuery().ToList()) {

            var newAccountsResult = server.GetNewAccounts();

            if (newAccountsResult.IsError)
               continue;

            var newAccountsResultOK = (ServerGetNewAccountsResult)newAccountsResult;

            foreach (NewAccount newAccount in newAccountsResultOK.Accounts) {

               repo.Add(newAccount);

               mail.AppendFormat("Server: {0}; Account Name: {1}", server.Description, newAccount.AccountName)
                  .AppendLine();
            }
         }

         if (mail.Length > 0) {

            var smtp = ServiceLocator.Current.GetInstance<SmtpClient>();

            try {
               smtp.Send(new MailMessage {
                  To = { "suerrors@gmail.com" },
                  Subject = "Solution Union Dashboard: New Trials",
                  Body = mail.ToString()
               });
            } catch (SmtpException) { }
         }

         return new SuccessfulResult();
      }
   }
}
