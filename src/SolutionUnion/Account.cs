using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Ahsay;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class Account {

      event PropertyChangedEventHandler PropertyChanged;

      string _TimeZone;
      decimal _VaultSize;
      bool _IndividualMicrosoftExchange;
      int _NumberOfIndividualMicrosoftExchangeMailboxes;

      public long Id { get; set; }
      public string ListId { get; set; }
      public long UserId { get; set; }
      public long ServerId { get; set; }
      public bool IsAcb { get; set; } // TODO: Duplicate with Server.IsAcb ?
      public string AccountName { get; set; }
      public string Password { get; set; }
      
      public string TimeZone {
         get { return _TimeZone; }
         set {
            if (_TimeZone != value) {
               _TimeZone = value;
               SendPropertyChanged("TimeZone");
            }
         }
      }
      
      public decimal VaultSize {
         get { return _VaultSize; }
         set {
            if (_VaultSize != value) {
               _VaultSize = value;
               SendPropertyChanged("VaultSize");
            }
         }
      }

      public bool IsPaid { get; set; }

      public bool IndividualMicrosoftExchange {
         get { return _IndividualMicrosoftExchange; }
         set {
            if (_IndividualMicrosoftExchange != value) {
               _IndividualMicrosoftExchange = value;
               SendPropertyChanged("IndividualMicrosoftExchange");
            }
         }
      }
      
      public int NumberOfIndividualMicrosoftExchangeMailboxes {
         get { return _NumberOfIndividualMicrosoftExchangeMailboxes; }
         set {
            if (_NumberOfIndividualMicrosoftExchangeMailboxes != value) {
               _NumberOfIndividualMicrosoftExchangeMailboxes = value;
               SendPropertyChanged("NumberOfIndividualMicrosoftExchangeMailboxes");
            }
         }
      }
      
      public decimal DataSize { get; set; }
      public decimal RetainSize { get; set; }
      public decimal TotalSize { get; set; }
      public bool IsActive { get; set; }
      public bool AuxIsActive { get; set; }
      public bool MarkedAsDeleted { get; set; }
      public DateTime Created { get; set; }
      public DateTime MarkedAsDeletedDate { get; set; }

      public decimal CostCache { get; set; }
      public decimal UsageAverageCache { get; set; }

      public AccountType Type { 
         get { 
            return (IsPaid) ? AccountType.Paid : AccountType.Trial; 
         }
         private set { 
            bool val = value == AccountType.Paid;

            if (val != IsPaid) {
               IsPaid = val;
               SendPropertyChanged("Type");
            }
         }
      }

      public virtual User User { get; set; }
      public virtual Server Server { get; set; }

      public virtual ICollection<Contact> Contacts { get; private set; }
      public virtual ICollection<DailyUsage> DailyUsages { get; private set; }
      public virtual ICollection<BackupJobReportSummary> BackUpJobReportSummaries { get; private set; }

      internal IQueryable<Contact> ContactsQuery {
         get {
            var repo = Repository<Account>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Contacts);
         }
      }

      internal IQueryable<DailyUsage> DailyUsagesQuery {
         get {
            var repo = Repository<Account>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.DailyUsages);
         }
      }

      internal IQueryable<BackupJobReportSummary> BackUpJobReportSummariesQuery {
         get {
            var repo = Repository<Account>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.BackUpJobReportSummaries);
         }
      }

      public static AccountEdit New() {
         return New(new AccountEdit());
      }

      public static AccountEdit New(ServerType serverType) {

         User user = User.CurrentUser;

         var input = new AccountEdit { 
            IsAcb = (serverType == ServerType.Lite)
         };

         return New(input);
      }

      static AccountEdit New(AccountEdit input) {

         User user = User.CurrentUser;

         if (user != null)
            input.VaultSize = (input.IsAcb) ? user.LiteVaultMinimumGb : user.ProVaultMinimumGb;

         return input;
      }

      public static OperationResult Create(AccountEdit input) {

         if (input == null) throw new ArgumentNullException("input");
         
         var currentPrincipal = User.CurrentPrincipal;
         
         if (!currentPrincipal.Identity.IsAuthenticated) 
            throw new InvalidOperationException();

         ServerType serverType = input.IsAcb ? ServerType.Lite : ServerType.Pro;

         var errors = new ErrorResult();

         if (errors.NotValid(input)
            || errors.Not(ValidateNewAccountVaultSize(input.VaultSize, serverType), () => input.VaultSize))
            return errors;

         var serverRepo = Repository<Server>.GetInstance();

         Server server;

         if (currentPrincipal.IsInRole(UserRole.Administrator)) {
            server = serverRepo.Find(input.ServerId);
         } else {

            server = (from s in serverRepo.CreateQuery()
                      where s.IsAcb == input.IsAcb
                         && s.IsDefault
                      select s).First();
         }

         if (errors.Not(ValidateNewAccountName(input.AccountName, serverType, server.Id), () => input.AccountName))
            return errors;

         var repo = Repository<Account>.GetInstance();

         var result = server.AddUser(input);

         if (result.IsError)
            return result;

         Account account = new Account {
            ListId = CreateId(),
            UserId = User.CurrentUserId,
            ServerId = server.Id,
            IsAcb = server.IsAcb,
            AccountName = input.AccountName,
            Password = EncryptPassword(input.Password),
            TimeZone = input.TimeZone,
            IsPaid = input.AccountType == AccountType.Paid,
            VaultSize = input.VaultSize * (1024 * 1024 * 1024),
            IndividualMicrosoftExchange = input.IndividualMicrosoftExchange,
            NumberOfIndividualMicrosoftExchangeMailboxes = input.NumberOfIndividualMicrosoftExchangeMailboxes,
            IsActive = true,
            Created = DateTime.Now,
            MarkedAsDeletedDate = DateTime.Now.AddYears(-1),
            Contacts = { 
               new Contact { 
                  ListId = CreateId(),
                  Name = input.ContactName,
                  Email = input.Email,
                  IsDefault = true,
                  Created = DateTime.Now
               }
            }
         };

         foreach (var contact in input.AdditionalContacts) {
            account.Contacts.Add(
               new Contact { 
                  ListId = CreateId(),
                  Name = contact.Name,
                  Email = contact.Email,
                  IsDefault = false,
                  Created = DateTime.Now
               }
            );
         }

         repo.Add(account);

         account.UpdateCachedMembers();

         var createResult = new SuccessfulResult("Account created successfully.");

         Session.Log(createResult, account);

         return createResult;
      }

      public static OperationResult ValidateNewAccountName(string accountName, ServerType serverType, long? serverId) {

         var repo = Repository<Account>.GetInstance();
         var serverRepo = Repository<Server>.GetInstance();

         var currentPrincipal = User.CurrentPrincipal;

         if (!currentPrincipal.Identity.IsAuthenticated)
            throw new InvalidOperationException();

         var errors = new ErrorResult();

         Server server;

         if (currentPrincipal.IsInRole(UserRole.Administrator) 
            && serverId.HasValue) {
            
            server = serverRepo.Find(serverId.Value);

            if (errors.Not(server != null, "Server not found.", () => serverId))
               return errors;

         } else {

            bool isAcb = serverType == ServerType.Lite;

            server = (from s in serverRepo.CreateQuery()
                      where s.IsAcb == isAcb
                         && s.IsDefault
                      select s).First();
         }

         var count = repo.CreateQuery().Count(a => a.AccountName == accountName && a.ServerId == server.Id);

         if (errors.Not(count == 0, "Account Name already exists for this Server.", () => accountName))
            return errors;

         var userResult = server.GetUser(accountName);

         if (errors.Not(userResult.IsError, "Account Name already exists on Backup Server.", () => accountName))
            return errors;

         return new SuccessfulResult();
      }

      public static OperationResult ValidateNewAccountVaultSize(decimal vaultSize, ServerType serverType) {

         if (!User.IsAuthenticated) 
            throw new InvalidOperationException();
         
         User user = User.CurrentUser;

         return user.ValidateVaultSize(vaultSize, serverType);
      }

      public static Account Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         var repo = Repository<Account>.GetInstance();

         var query = repo.CreateQuery().Where(a => a.Id == id);

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            query = query.Where(a => a.UserId == User.CurrentUserId);

         return query.SingleOrDefault();
      }

      static string EncryptPassword(string clearTextPassword) {

         var crypto = new Cryptographer();
         return crypto.Encrypt(clearTextPassword);
      }

      static string DecryptPassword(string cipher) {

         var crypto = new Cryptographer();
         return crypto.Decrypt(cipher);
      }

      static string CreateId() {

         StringBuilder sb = new StringBuilder(Guid.NewGuid().ToString().ToUpperInvariant());
         sb.Replace("{", "");
         sb.Replace("-", "");
         sb.Replace("}", "");

         return sb.ToString();
      }

      public Account() {
         
         this.Contacts = new Collection<Contact>();
         this.DailyUsages = new Collection<DailyUsage>();
         this.BackUpJobReportSummaries = new Collection<BackupJobReportSummary>();
      }

      internal decimal GetUsageAverage(IQueryable<DailyUsage> period) {

         decimal average =
            (from du in period
             let size = (du.IsActive) ? du.TotalSize : 0m
             select (decimal?)size)
            .Average()
            .GetValueOrDefault();

         return average;
      }

      decimal GetMicrosoftExchangeMailboxesAverage(IQueryable<DailyUsage> period) {

         decimal average =
            (from du in period
             let size = (du.IsActive) ? du.NumberOfIndividualMicrosoftExchangeMailboxes : 0
             select (decimal?)size)
            .Average()
            .GetValueOrDefault();

         return average;
      }

      IQueryable<DailyUsage> GetDailyUsagesForCost() {

         var today = DateTime.Today;
         var thirtyDaysAgo = today.AddDays(-30);

         return GetDailyUsagesForCost(thirtyDaysAgo, today);
      }

      internal IQueryable<DailyUsage> GetDailyUsagesForCost(DateTime startInclusive, DateTime endExclusive) {
         
         var today = DateTime.Today;
         var thirtyDaysAgo = today.AddDays(-30);

         return
            from da in this.DailyUsagesQuery
            where da.Date >= thirtyDaysAgo && da.Date < today
            select da;
      }

      public decimal GetCost() {

         var period = GetDailyUsagesForCost();

         return GetCost(period);
      }

      internal decimal GetCost(IQueryable<DailyUsage> period) {
         return GetCost(GetUsageAverage(period), GetMicrosoftExchangeMailboxesAverage(period));
      }

      decimal GetCost(decimal usageAverage, decimal microsoftExchangeMailboxesAverage) {

         decimal cost = 0;

         if (this.Type == AccountType.Trial)
            return cost;

         decimal usage = usageAverage;

         bool isPro = this.Server.Type == ServerType.Pro;

         decimal baseStorageGb = (isPro) ? this.User.ProVaultMinimumGb : this.User.LiteVaultMinimumGb;
         decimal baseStorage = baseStorageGb * (1024 * 1024 * 1024);
         decimal basePrice = (isPro) ? this.User.ProAccountMinimumFee : this.User.LiteAccountMinimumFee;

         cost += basePrice;

         if (usage < baseStorage)
            usage = baseStorage;

         int additionalGb = (int)Decimal.Ceiling((usage - baseStorage) / (1024 * 1024 * 1024));

         if (additionalGb > 0) {

            decimal additionalGbPrice = (isPro) ? 
               this.User.GetProAdditionalGbPrice(additionalGb)
               : this.User.GetLiteAdditionalGbPrice(additionalGb);

            cost += additionalGb * additionalGbPrice;
         }

         if (this.IndividualMicrosoftExchange) 
            cost += Decimal.Ceiling(microsoftExchangeMailboxesAverage) * 1m; 

         return cost;
      }

      public AccountEdit Edit() {

         AccountEdit input = new AccountEdit { 
            AccountName = this.AccountName,
            AccountType = this.Type,
            IndividualMicrosoftExchange = this.IndividualMicrosoftExchange,
            IsAcb = IsAcb,
            NumberOfIndividualMicrosoftExchangeMailboxes = this.NumberOfIndividualMicrosoftExchangeMailboxes,
            ServerId = this.ServerId,
            TimeZone = this.TimeZone,
            VaultSize = this.VaultSize / (1024 * 1024 * 1024)
         };

         Contact defaultContact = this.Contacts.Where(c => c.IsDefault).FirstOrDefault();

         if (defaultContact != null) {
            input.ContactName = defaultContact.Name;
            input.Email = defaultContact.Email;
         }

         foreach (Contact contact in this.Contacts.Where(c => !c.IsDefault)) {
            input.AdditionalContacts.Add(
               new AccountEditContact { 
                  Email = contact.Email,
                  Id = contact.Id,
                  Name = contact.Name
               }
            );
         }

         return input;
      }

      public OperationResult Update(AccountEdit input) {

         if (input == null) throw new ArgumentNullException("input");

         var currentPrincipal = User.CurrentPrincipal;
         
         if (!currentPrincipal.Identity.IsAuthenticated) 
            throw new InvalidOperationException();

         var errors = new ErrorResult();
         errors.Valid(input);

         var errorsToRemove = errors.Errors.Where(e => e.MemberNames.Contains("Password")
            || e.MemberNames.Contains("ConfirmPassword")
            || e.MemberNames.Contains("AccountName"))
            .ToArray();

         foreach (var error in errorsToRemove) 
            errors.Errors.Remove(error);

         if (errors.HasErrors)
            return errors;

         Server server = this.Server;

         var vaultSizeResult = this.User.ValidateVaultSize(input.VaultSize, server.Type);

         if (vaultSizeResult.IsError)
            return vaultSizeResult;

         input.AccountName = this.AccountName;

         if (!currentPrincipal.IsInRole(UserRole.Administrator))
            input.AccountType = this.Type;

         var result = server.UpdateAccount(input);

         if (result.IsError)
            return result;

         var repo = Repository<Account>.GetInstance();

         StringBuilder log = new StringBuilder();

         PropertyChangedEventHandler accountChanged = (sender, e) => {
            log.Append(", ").Append(e.PropertyName);
         };

         this.PropertyChanged += accountChanged;

         this.Type = input.AccountType;
         this.TimeZone = input.TimeZone;
         this.VaultSize = input.VaultSize * (1024 * 1024 * 1024);
         this.IndividualMicrosoftExchange = input.IndividualMicrosoftExchange;
         this.NumberOfIndividualMicrosoftExchangeMailboxes = input.NumberOfIndividualMicrosoftExchangeMailboxes;

         this.PropertyChanged -= accountChanged;

         if (log.Length > 0) 
            log.Append(" changed");

         log.Insert(0, "Account updated successfully")
            .Append(".");

         RefreshCachedMembers();

         Func<bool> lastCharIsPeriod = () => log[log.Length - 1] == '.';

         PropertyChangedEventHandler contactChanged = (sender, e) => {

            if (lastCharIsPeriod())
               log.Append(" ");
            else
               log.Append(", ");

            log.Append(e.PropertyName);
         };

         List<AccountEditContact> inputContacts = input.AdditionalContacts.ToList();
         List<Contact> currentContacts = this.Contacts.ToList();

         Contact defaultContact = currentContacts.FirstOrDefault(c => c.IsDefault);

         if (defaultContact != null) {

            defaultContact.PropertyChanged += contactChanged;

            defaultContact.Name = input.ContactName;
            defaultContact.Email = input.Email;

            defaultContact.PropertyChanged -= contactChanged;

            if (!lastCharIsPeriod()) 
               log.Append(" of default contact changed.");

            currentContacts.Remove(defaultContact);
         }

         var updatedContacts = currentContacts.Where(c => inputContacts.Any(ic => ic.Id == c.Id)).ToArray();

         foreach (var contact in updatedContacts) {
            var contactInput = inputContacts.Single(c => c.Id == contact.Id);

            contact.PropertyChanged += contactChanged;

            contact.Name = contactInput.Name;
            contactInput.Email = contactInput.Email;

            contact.PropertyChanged -= contactChanged;

            if (!lastCharIsPeriod())
               log.AppendFormat(" of contact {0} changed.", contact.Id);

            currentContacts.Remove(contact);
            inputContacts.Remove(contactInput);
         }

         var deletedContacts = currentContacts.Where(c => !inputContacts.Any(ic => ic.Id == c.Id)).ToArray();

         foreach (var contact in deletedContacts) {
            repo.DeleteRelated(contact, a => a.Contacts);

            log.AppendFormat(" Contact {0} deleted.", contact.Id);
         }

         var newContacts = inputContacts.ToList();

         foreach (var newContact in newContacts) {

            this.Contacts.Add(
               new Contact { 
                  AccountId = this.Id,
                  Created = DateTime.Now,
                  Email = newContact.Email,
                  IsDefault = false,
                  ListId = CreateId(),
                  Name = newContact.Name
               }
            );
         }

         if (newContacts.Count > 0) 
            log.AppendFormat(" {0} new contacts added.", newContacts.Count);

         repo.SaveChanges(this);

         Session.Log(log.ToString(), this);

         return new SuccessfulResult("Account updated successfully.");
      }

      public OperationResult AssignUser(long userId) {

         var principal = User.CurrentPrincipal;

         if (!(principal.IsInRole(UserRole.Administrator)))
            return new ErrorResult("Only administrators can perform this operation.").WithStatus(HttpStatusCode.Forbidden);

         this.UserId = userId;

         var repo = Repository<Account>.GetInstance();
         repo.SaveChanges(this);

         var result = new SuccessfulResult("Account successfully assigned to user.");

         Session.Log(result, this);

         return result;
      }

      public OperationResult Convert() {
         
         var result = this.Server.ConvertToPaidAccount(this.AccountName);

         if (result.IsError)
            return result;

         if (this.IsPaid)
            return new ErrorResult("Account is already paid");

         var repo = Repository<Account>.GetInstance();

         this.IsPaid = true;

         repo.SaveChanges(this);

         var convertResult = new SuccessfulResult("Account converted successfully.");

         Session.Log(convertResult, this);

         return convertResult;
      }

      public OperationResult Suspend() {

         var result = this.Server.SuspendAccount(this.AccountName);

         if (result.IsError)
            return result;

         var repo = Repository<Account>.GetInstance();

         this.IsActive = false;

         repo.SaveChanges(this);

         var suspendResult = new SuccessfulResult("Account suspended successfully.");

         Session.Log(suspendResult, this);

         return suspendResult;
      }

      public OperationResult Activate() {
         
         var result = this.Server.ActivateAccount(this.AccountName);

         if (result.IsError)
            return result;

         var repo = Repository<Account>.GetInstance();

         this.IsActive = true;

         repo.SaveChanges(this);

         var activateResult = new SuccessfulResult("Account activated successfully.");

         Session.Log(activateResult, this);

         return activateResult;
      }
      
      public OperationResult MarkAsDeleted() {

         if (this.MarkedAsDeleted)
            return new SuccessfulResult("Account is already marked as deleted.");

         var repo = Repository<Account>.GetInstance();

         using (var tx = new TransactionScope()) {

            var suspendResult = Suspend();

            if (suspendResult.IsError)
               return suspendResult;

            this.MarkedAsDeleted = true;
            this.MarkedAsDeletedDate = DateTime.Now;

            repo.SaveChanges(this);

            tx.Complete();
         }

         var deleteResult = new SuccessfulResult("Account successfully marked as deleted.");

         Session.Log(deleteResult, this);

         return deleteResult;
      }

      public OperationResult Restore() {

         using (var tx = new TransactionScope()) {

            var activateResult = Activate();

            if (activateResult.IsError)
               return activateResult;

            this.MarkedAsDeleted = false;
            this.MarkedAsDeletedDate = DateTime.Now;

            var repo = Repository<Account>.GetInstance();

            repo.SaveChanges(this);

            tx.Complete();
         }

         var restoreResult = new SuccessfulResult("Account successfully restored.");

         Session.Log(restoreResult, this);

         return restoreResult;
      }

      public OperationResult ChangePassword(AccountChangePasswordInput input) {

         if (input == null) throw new ArgumentNullException("input");

         ErrorResult errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         var repo = Repository<Account>.GetInstance();

         this.Password = EncryptPassword(input.NewPassword);

         repo.SaveChanges(this);

         var passwordChangeResult = new SuccessfulResult("Password changed.");

         Session.Log(passwordChangeResult, this);

         return passwordChangeResult;
      }

      internal OperationResult SynchronizeWithServer() {

         var repo = Repository<Account>.GetInstance();

         var getUserResult = this.Server.GetUser(this.AccountName);

         if (getUserResult.IsError) {

            if (getUserResult.StatusCode == HttpStatusCode.NotFound) {
               
               repo.Delete(this);

               return new SuccessfulResult("Account deleted."); 
            }

            return getUserResult;
         }

         var getUserResultOK = (UserInfo)getUserResult;

         this.IsAcb = getUserResultOK.ClientType.ToUpper() == "ACB";
         this.TimeZone = getUserResultOK.Timezone;
         this.IsPaid = getUserResultOK.UserType.ToUpper() != "TRIAL";
         this.VaultSize = getUserResultOK.Quota;
         this.DataSize = getUserResultOK.DataSize;
         this.RetainSize = getUserResultOK.RetainSize;
         this.TotalSize = this.DataSize + this.RetainSize;

         int exchangeMailboxQuota = getUserResultOK.ExchangeMailboxQuota;

         if (exchangeMailboxQuota <= 0) {
            this.IndividualMicrosoftExchange = false;
            this.NumberOfIndividualMicrosoftExchangeMailboxes = 0;
         } else {
            this.IndividualMicrosoftExchange = true;
            this.NumberOfIndividualMicrosoftExchangeMailboxes = exchangeMailboxQuota;
         }

         this.IsActive = getUserResultOK.Status.ToUpper() == "ENABLE";

         if (this.IsActive && this.MarkedAsDeleted)
            this.IsActive = false;

         using (var tx = new TransactionScope()) {

            var orderedContacts = this.Contacts.OrderBy(c => c.Id).ToList();

            for (int i = 0; i < getUserResultOK.Contacts.Count; i++) {

               var getUserContact = getUserResultOK.Contacts[i];

               if (i <= orderedContacts.Count - 1) {
                  var contact = orderedContacts[i];

                  contact.Name = getUserContact.Name;
                  contact.Email = getUserContact.Email;

               } else { 

                  this.Contacts.Add(new Contact {
                     ListId = CreateId(),
                     AccountId = this.Id,
                     IsDefault = i == 0,
                     Name = getUserContact.Name,
                     Email = getUserContact.Email,
                     Created = DateTime.Now
                  });
               }
            }

            if (orderedContacts.Count > getUserResultOK.Contacts.Count) {
               int contactsToDelete = orderedContacts.Count - getUserResultOK.Contacts.Count;

               orderedContacts.Reverse();

               foreach (var contact in orderedContacts.Take(contactsToDelete)) 
                  repo.DeleteRelated(contact, a => a.Contacts);
            }

            repo.SaveChanges(this);

            tx.Complete();
         }

         return new SuccessfulResult(String.Format("Account '{0}' synchronized.", this.AccountName));
      }

      internal OperationResult CreateDailyUsage() {

         var repo = Repository<Account>.GetInstance();

         var now = DateTime.Now;
         var date = (now.Hour == 23) ? now.Date : now.Date.AddDays(-1);

         int existing = this.DailyUsagesQuery.Where(d => d.Date == date).Count();

         if (existing > 0)
            return new SuccessfulResult("Already created.");

         var getUserResult = this.Server.GetUser(this.AccountName);

         if (getUserResult.IsError)
            return getUserResult;

         var getUserResultOK = (UserInfo)getUserResult;

         this.DataSize = getUserResultOK.DataSize;
         this.RetainSize = getUserResultOK.RetainSize;
         this.TotalSize = this.DataSize + this.RetainSize;

         using (var tx = new TransactionScope()) {

            this.DailyUsages.Add(
               new DailyUsage { 
                  UserId = this.UserId,
                  AccountId = this.Id,
                  DataSize = this.DataSize,
                  RetainSize = this.RetainSize,
                  TotalSize = this.TotalSize,
                  NumberOfIndividualMicrosoftExchangeMailboxes = (this.IndividualMicrosoftExchange) ?
                     this.NumberOfIndividualMicrosoftExchangeMailboxes : 0,
                  IsAcb = this.Server.IsAcb,
                  IsActive = !this.MarkedAsDeleted,
                  Date = date
               }
            );

            repo.SaveChanges(this);

            tx.Complete();
         }

         return new SuccessfulResult();
      }

      internal OperationResult UpdateCachedMembers() {

         RefreshCachedMembers();

         var repo = Repository<Account>.GetInstance();
         repo.SaveChanges(this);

         return new SuccessfulResult();
      }

      void RefreshCachedMembers() {

         var period = GetDailyUsagesForCost();

         this.UsageAverageCache = GetUsageAverage(period);
         this.CostCache = GetCost(this.UsageAverageCache, GetMicrosoftExchangeMailboxesAverage(period));
      }

      internal OperationResult ImportBackUpJobReportSummaries() {

         var repo = Repository<Account>.GetInstance();

         var jobsSummResult = this.Server.GetBackupJobReportSummaries(this.AccountName);

         if (jobsSummResult.IsError)
            return jobsSummResult;

         var jobsSummResultOK = (ServerGetBackupJobReportSummariesResult)jobsSummResult;

         foreach (var job in jobsSummResultOK.Jobs) {

            BackupJobReportSummary summary = this.BackUpJobReportSummariesQuery.FirstOrDefault(p => p.SetId == job.SetId && p.JobId == job.ID);
            bool summaryIsNew = false;

            if (summary == null) {
               summary = new BackupJobReportSummary { 
                  DateCreated = DateTime.Now
               };
               summaryIsNew = true;
            }

            summary.AccountId = this.Id;
            summary.JobDate = job.StartTime;
            summary.JobDescription = job.JobStatusDescription;
            summary.JobId = job.ID;
            summary.JobStatus = job.BackupJobStatus;
            summary.SetId = job.SetId;
            summary.SetName = job.SetName;

            if (summaryIsNew)
               this.BackUpJobReportSummaries.Add(summary);

            repo.SaveChanges(this);
         }

         return new SuccessfulResult("BackUpJobReportSummaries import finished succesfully.");
      }

      public OperationResult Delete() {

         var repo = Repository<Account>.GetInstance();

         var getUserResult = this.Server.GetUser(this.AccountName);

         if (!getUserResult.IsError)
            this.Server.RemoveUser(this.AccountName);

         repo.Delete(this);

         var deleteResult = new SuccessfulResult("Account deleted.");

         Session.Log(deleteResult, this);

         return deleteResult;
      }

      void SendPropertyChanged(string propertyName) {

         var propChanged = this.PropertyChanged;

         if (propChanged != null)
            propChanged(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
