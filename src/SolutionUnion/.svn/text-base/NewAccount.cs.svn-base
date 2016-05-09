using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Ahsay;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class NewAccount {

      public long Id { get; set; }
      public string ListId { get; set; }
      public long ServerId { get; set; }
      public string AccountName { get; set; }
      public string Email { get; set; }
      public string SetId { get; set; }
      public string SetName { get; set; }
      public string JobId { get; set; }
      public string JobStatus { get; set; }
      public bool IsAcb { get; set; }
      public bool IsPaid { get; set; }
      public bool IsActive { get; set; }
      public DateTime Created { get; set; }

      public virtual Server Server { get; set; }

      public static NewAccount Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            return null;

         var repo = Repository<NewAccount>.GetInstance();

         return repo.Find(id);
      }

      public OperationResult AssignUser(long userId) {

         Dictionary<string, object> returnMessage = new Dictionary<string, object>();
         
         Server server = this.Server;

         if (server == null) 
            return new ErrorResult(HttpStatusCode.InternalServerError, "Server not found. Administrator has been informed about the error.");

         var repo = Repository<NewAccount>.GetInstance();
         var accountRepo = Repository<Account>.GetInstance();

         if (accountRepo.CreateQuery().Count(a => a.ServerId == server.Id && a.AccountName == this.AccountName) > 0) 
            return new ErrorResult("Accountname already exists.");

         var result = server.GetUser(this.AccountName);

         if (result.IsError)
            return result;

         UserInfo userData = (UserInfo)result;

         Account account = new Account { 
            ListId = CreateId(),
            UserId = userId,
            ServerId = server.Id,
            IsAcb = userData.ClientType.Equals("ACB", StringComparison.OrdinalIgnoreCase),
            AccountName = userData.LoginName,
            TimeZone = userData.Timezone,
            IsPaid = !userData.UserType.Equals("TRIAL", StringComparison.OrdinalIgnoreCase),
            VaultSize = userData.Quota,
            IndividualMicrosoftExchange = userData.ExchangeMailboxQuota > 0,
            NumberOfIndividualMicrosoftExchangeMailboxes = userData.ExchangeMailboxQuota,
            DataSize = userData.DataSize,
            RetainSize = userData.RetainSize,
            TotalSize = userData.DataSize + userData.RetainSize,
            IsActive = userData.Status.Equals("ENABLE", StringComparison.OrdinalIgnoreCase),
            Created = DateTime.Now,
            MarkedAsDeletedDate = DateTime.Now.AddYears(-1)
         };

         foreach (var userContact in userData.Contacts.Take(10)) {

            Contact contact = new Contact { 
               ListId = CreateId(),
               IsDefault = true,
               Name = userContact.Name,
               Email = userContact.Email,
               Created = DateTime.Now
            };
         }

         using (var tx = new TransactionScope()) {
            accountRepo.Add(account);
            repo.Delete(this);

            tx.Complete();
         }

         return new SuccessfulResult("Account assigned successfully.");
      }

      public OperationResult Convert() {

         var result = this.Server.ConvertToPaidAccount(this.AccountName);

         if (result.IsError)
            return result;

         if (this.IsPaid)
            return new ErrorResult("This Account is already paid");

         var repo = Repository<NewAccount>.GetInstance();

         this.IsPaid = true;

         repo.SaveChanges(this);

         return new SuccessfulResult("Account converted successfully.");
      }

      public OperationResult Activate() {

         var result = this.Server.ActivateAccount(this.AccountName);

         if (result.IsError)
            return result;

         var repo = Repository<NewAccount>.GetInstance();

         this.IsActive = true;

         repo.SaveChanges(this);

         return new SuccessfulResult("Account activated successfully.");
      }

      public OperationResult Suspend() {

         var result = this.Server.SuspendAccount(this.AccountName);

         if (result.IsError)
            return result;

         var repo = Repository<NewAccount>.GetInstance();

         this.IsActive = false;

         repo.SaveChanges(this);

         return new SuccessfulResult("Account suspended successfully.");
      }

      public OperationResult Delete() {

         var result = this.Server.RemoveUser(this.AccountName);

         if (result.IsError)
            return result;

         var repo = Repository<NewAccount>.GetInstance();

         repo.Delete(this);

         return new SuccessfulResult("Account successfully deleted.");
      }

      public OperationResult GetBackupJobReport() {

         var jobsResult = this.Server.ListBackupJobs(this.AccountName);

         if (jobsResult.IsError)
            return jobsResult;

         var jobsResultOK = (BackupJobList)jobsResult;

         var lastSet = jobsResultOK.BackupSets.LastOrDefault(s => s.BackupJobs.Count > 0);

         if (lastSet == null)
            return new ErrorResult("Didn't find any backup jobs.");

         var lastJob = lastSet.BackupJobs.Last();

         var jobResult = this.Server.GetBackupJobReport(this.AccountName, lastSet.ID, lastJob.ID);

         return jobResult;
      }

      internal OperationResult SynchronizeWithServer() {

         var repo = Repository<NewAccount>.GetInstance();

         var getUserResult = this.Server.GetUser(this.AccountName);

         if (getUserResult.IsError) {

            if (getUserResult.StatusCode == HttpStatusCode.NotFound) {

               repo.Delete(this);

               return new SuccessfulResult(String.Format("New trial account '{0}' deleted.", this.AccountName));
            }

            return getUserResult;
         }

         var getUserResultOK = (UserInfo)getUserResult;

         this.IsAcb = getUserResultOK.ClientType.ToUpper() == "ACB";
         this.IsActive = getUserResultOK.Status.ToUpper() == "ENABLE";
         this.IsPaid = getUserResultOK.UserType.ToUpper() != "TRIAL";

         if (getUserResultOK.Contacts.Count > 0) 
            this.Email = getUserResultOK.Contacts[0].Email;

         repo.SaveChanges(this);

         return new SuccessfulResult(String.Format("New trial account '{0}' synchronized.", this.AccountName));
      }

      string CreateId() {

         StringBuilder sb = new StringBuilder(Guid.NewGuid().ToString().ToUpperInvariant());
         sb.Replace("{", "");
         sb.Replace("-", "");
         sb.Replace("}", "");

         return sb.ToString();
      }
   }
}
