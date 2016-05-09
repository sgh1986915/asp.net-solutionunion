using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Ahsay;
using System.Net;
using System.Globalization;

namespace SolutionUnion {
   
   public class Server {

      public long Id { get; set; }
      public string ListId { get; set; }
      public string Description { get; set; }
      public string OrIpUrl { get; set; }
      public string UserName { get; set; }
      public string Password { get; set; }
      public bool IsAcb { get; set; }
      public bool IsDefault { get; set; }
      public DateTime Created { get; set; }

      public ServerType Type {
         get {
            return (IsAcb) ? ServerType.Lite : ServerType.Pro;
         }
         private set {
            IsAcb = value == ServerType.Lite;
         }
      }

      public virtual ICollection<HomeDirectory> HomeDirectories { get; private set; }
      public virtual ICollection<Account> Accounts { get; private set; }
      public virtual ICollection<NewAccount> NewAccounts { get; private set; }

      internal IQueryable<HomeDirectory> HomeDirectoriesQuery {
         get {
            var repo = Repository<Server>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.HomeDirectories);
         }
      }

      internal IQueryable<Account> AccountsQuery {
         get {
            var repo = Repository<Server>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Accounts);
         }
      }

      internal IQueryable<NewAccount> NewAccountsQuery {
         get {
            var repo = Repository<Server>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.NewAccounts);
         }
      }

      public static OperationResult Create(ServerEdit input) {

         if (input == null) throw new ArgumentNullException("input");

         var repo = Repository<Server>.GetInstance();
         
         var errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         Server server = new Server {
            ListId = CreateId(),
            Type = input.Type,
            OrIpUrl = input.OrIpUrl,
            UserName = input.UserName,
            Description = input.Description,
            Password = EncryptPassword(input.Password),
            IsDefault = repo.CreateQuery().Count(s => s.IsAcb) == 0,
            Created = DateTime.Now,
            HomeDirectories = { 
               new HomeDirectory {
                  ListId = CreateId(),
                  Path = input.HomeDirectory,
                  Created = DateTime.Now,
                  IsDefault = true
               }
            }
         };

         int sameUrlCount = repo.CreateQuery().Count(
            s => s.OrIpUrl.ToUpper() == input.OrIpUrl.ToUpper()
               && s.IsAcb == server.IsAcb
         );

         if (errors.Not(sameUrlCount == 0, "Server already exists", () => input.OrIpUrl))
            return errors;

         foreach (var directory in input.AdditionalHomeDirectories) {

            server.HomeDirectories.Add(
               new HomeDirectory {
                  ListId = CreateId(),
                  Path = directory.HomeDirectory,
                  Created = DateTime.Now,
                  IsDefault = false
               }
            );
         }

         repo.Add(server);

         return new SuccessfulResult("Server created successfully.");
      }

      public static Server Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            return null;

         var repo = Repository<Server>.GetInstance();

         return repo.Find(id);
      }

      public Server() {
         this.HomeDirectories = new Collection<HomeDirectory>();
      }

      public ServerEdit Edit() {

         ServerEdit input = new ServerEdit { 
            Description = this.Description,
            OrIpUrl = this.OrIpUrl,
            UserName = this.UserName
         };

         HomeDirectory defaultDirectory = this.HomeDirectories.Where(p => p.IsDefault).FirstOrDefault();

         if (defaultDirectory != null) 
            input.HomeDirectory = defaultDirectory.Path;

         foreach (HomeDirectory directory in this.HomeDirectories.Where(p => !p.IsDefault)) {
            input.AdditionalHomeDirectories.Add(
               new ServerEditHomeDirectory {
                  Id = directory.Id,
                  HomeDirectory = directory.Path
               }
            );
         }

         return input;
      }

      public OperationResult Update(ServerEdit input) {

         if (input == null) throw new ArgumentNullException("input");

         var errors = new ErrorResult();
         errors.Valid(input);

         var errorsToRemove = errors.Errors.Where(e => e.MemberNames.Contains("Password")
            || e.MemberNames.Contains("ConfirmPassword"))
            .ToArray();

         foreach (var error in errorsToRemove)
            errors.Errors.Remove(error);

         if (errors.Errors.Count > 0)
            return errors;

         var repo = Repository<Server>.GetInstance();

         this.OrIpUrl = input.OrIpUrl;
         this.Description = input.Description;

         List<ServerEditHomeDirectory> inputDirectories = input.AdditionalHomeDirectories.ToList();
         List<HomeDirectory> currentDirectories = this.HomeDirectories.ToList();

         HomeDirectory defaultDirectory = currentDirectories.FirstOrDefault(c => c.IsDefault);

         if (defaultDirectory != null) {
            defaultDirectory.Path = input.HomeDirectory;

            currentDirectories.Remove(defaultDirectory);
         }

         var updatedDirectories = currentDirectories.Where(c => inputDirectories.Any(ic => ic.Id == c.Id)).ToArray();

         foreach (var directory in updatedDirectories) {
            var directoryInput = inputDirectories.Single(c => c.Id == directory.Id);

            directory.Path = directoryInput.HomeDirectory;

            currentDirectories.Remove(directory);
            inputDirectories.Remove(directoryInput);
         }

         var deletedDirectories = currentDirectories.Where(c => !inputDirectories.Any(ic => ic.Id == c.Id)).ToArray();

         foreach (var directory in deletedDirectories) {
            repo.DeleteRelated(directory, s => s.HomeDirectories);
         }

         foreach (var newDirectory in inputDirectories) {

            this.HomeDirectories.Add(
               new HomeDirectory {
                  ListId = CreateId(),
                  Path = newDirectory.HomeDirectory,
                  Created = DateTime.Now,
                  IsDefault = false
               }
            );
         }

         repo.SaveChanges(this);

         return new SuccessfulResult();
      }

      public OperationResult ChangePassword(ServerChangePasswordInput input) {

         if (input == null) throw new ArgumentNullException("input");

         ErrorResult errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         var repo = Repository<Server>.GetInstance();

         this.Password = EncryptPassword(input.NewPassword);

         repo.SaveChanges(this);

         return new SuccessfulResult("Password changed.");
      }

      public OperationResult Verify() {

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/ListUsers.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "Type", AccountType.Paid.ToString().ToUpperInvariant() }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();
         
         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult("Server successfully verified");
      }

      public OperationResult MakeDefault() {

         if (this.IsDefault)
            return new ErrorResult("This is already the default Server");

         var repo = Repository<Server>.GetInstance();

         var allOtherServersOfThisType =
            from s in repo.CreateQuery()
            where s.IsAcb == this.IsAcb
               && s.Id != this.Id
            select s;
       
         using (var tx = new TransactionScope()) {

            // Make all other servers of this type as not default.
            foreach (var server in allOtherServersOfThisType) {
               server.IsDefault = false;
               repo.SaveChanges(server);
            }

            this.IsDefault = true;

            repo.SaveChanges(this);

            tx.Complete();
         }

         return new SuccessfulResult("Server successfully made default.");
      }

      public OperationResult Delete() {

         if (this.Accounts.Count > 0)
            return new ErrorResult("Server can't be deleted as there are existing accounts for the server.");

         if (this.IsDefault)
            return new ErrorResult("Can't delete default server.");

         var repo = Repository<Server>.GetInstance();

         repo.Delete(this);

         return new SuccessfulResult("Server deleted successfully.");
      }

      internal OperationResult AddUser(AccountEdit input) {

         if (input == null) throw new ArgumentNullException("input");

         NameValueCollection query = new NameValueCollection {
            { "SysUser", this.UserName },
            { "SysPwd", DecryptPassword(this.Password) },
            { "LoginName", input.AccountName },
            { "Password", input.Password },
            { "Timezone", input.TimeZone },
            { "Type", input.AccountType.ToString().ToUpperInvariant() },
            { "Quota", Decimal.Round(input.VaultSize * 1024 * 1024 * 1024).ToString() },
            { "EnableExchangeMailbox", input.IndividualMicrosoftExchange ? "Y" : "N" },
            { "ExchangeMailboxQuota", input.NumberOfIndividualMicrosoftExchangeMailboxes.ToString() },
            { "UserHome", this.HomeDirectories.First().Path },
            { "Alias", input.ContactName },
            { "Contact1", input.ContactName },
            { "Email", input.Email },
            { "ClientType", "OBM" },
            { "EnableMSSQL", "Y" },
            { "EnableMSExchange", "Y" },
            { "EnableOracle", "Y" },
            { "EnableLotusNotes", "Y" },
            { "EnableLotusDomino", "Y" },
            { "EnableMySQL", "Y" },
            { "EnableInFileDelta", "Y" },
            { "EnableShadowCopy", "Y" }
         };

         for (int i = 0; i < input.AdditionalContacts.Count; i++) {
            
            var contact = input.AdditionalContacts[i];
            string index = (i + 2).ToString();

            query.Add("Contact" + index, contact.Name);
            query.Add("Email" + index, contact.Email);
         }

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/AddUser.do",
            Query = query.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult ConvertToPaidAccount(string accountName) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) { 
            Path = "/obs/api/ModifyUser.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName },
               { "Type", AccountType.Paid.ToString().ToUpperInvariant() }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err") 
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult ActivateAccount(string accountName) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/ModifyUser.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName },
               { "Status", "ENABLE" }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult SuspendAccount(string accountName) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/ModifyUser.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName },
               { "Status", "SUSPENDED" }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult UpdateAccount(AccountEdit input) {

         NameValueCollection query = new NameValueCollection {
            { "SysUser", this.UserName },
            { "SysPwd", DecryptPassword(this.Password) },
            { "LoginName", input.AccountName },
            { "Timezone", input.TimeZone },
            { "Type", input.AccountType.ToString().ToUpperInvariant() },
            { "Quota", Decimal.Round(input.VaultSize * 1024 * 1024 * 1024).ToString() },
            { "EnableExchangeMailbox", input.IndividualMicrosoftExchange ? "Y" : "N" },
            { "ExchangeMailboxQuota", input.NumberOfIndividualMicrosoftExchangeMailboxes.ToString() },
            { "Alias", input.ContactName },
            { "AppendContact", "N" },
            { "Contact1", input.ContactName },
            { "Email1", input.Email },
         };

         for (int i = 0; i < input.AdditionalContacts.Count; i++) {

            var contact = input.AdditionalContacts[i];
            string index = (i + 2).ToString();

            query.Add("Contact" + index, contact.Name);
            query.Add("Email" + index, contact.Email);
         }

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/ModifyUser.do",
            Query = query.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult GetUser(string accountName) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/GetUser.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err") {

            string message = responseDoc.DocumentElement.InnerText;

            var error = new ErrorResult(message);

            if (message.StartsWith("[UserCacheManager.NoSuchUserExpt]"))
               error.StatusCode = HttpStatusCode.NotFound;

            return error;
         }

         UserInfo result = new UserInfo { 
            ClientType = responseDoc.DocumentElement.Attributes["ClientType"].Value,
            LoginName = responseDoc.DocumentElement.Attributes["LoginName"].Value,
            Timezone = responseDoc.DocumentElement.Attributes["Timezone"].Value,
            UserType = responseDoc.DocumentElement.Attributes["UserType"].Value,
            Quota = Decimal.Parse(responseDoc.DocumentElement.Attributes["Quota"].Value),
            ExchangeMailboxQuota = Int32.Parse(responseDoc.DocumentElement.Attributes["ExchangeMailboxQuota"].Value),
            DataSize = Decimal.Parse(responseDoc.DocumentElement.Attributes["DataSize"].Value),
            RetainSize = Decimal.Parse(responseDoc.DocumentElement.Attributes["RetainSize"].Value),
            Status = responseDoc.DocumentElement.Attributes["Status"].Value
         };

         foreach (XmlNode xmlNodeContact in responseDoc.DocumentElement.GetElementsByTagName("Contact")) {

            result.Contacts.Add(new UserInfoContact { 
               Name = xmlNodeContact.Attributes["Name"].Value,
               Email = xmlNodeContact.Attributes["Email"].Value
            });
         }

         return result;
      }

      internal OperationResult RemoveUser(string accountName) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/RemoveUser.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName }
            }.ToQueryString()
         }.Uri;

         XmlDocument responseDoc = new XmlDocument();

         try {
            responseDoc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (responseDoc.DocumentElement.Name == "err")
            return new ErrorResult(responseDoc.DocumentElement.InnerText);

         return new SuccessfulResult();
      }

      internal OperationResult GetBackupSet(string accountName, long setId, bool showStatusOnly = true) {

         if (accountName == null) throw new ArgumentNullException("accountName");

         if (!showStatusOnly)
            throw new NotImplementedException();

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/GetBackupSet.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName },
               { "BackupSetID", setId.ToString(CultureInfo.InvariantCulture) },
               { "ShowStatusOnly", showStatusOnly ? "Y" : "N" }
            }.ToQueryString()
         }.Uri;

         XmlDocument doc = new XmlDocument();

         try {
            doc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (doc.DocumentElement.Name == "err")
            return new ErrorResult(doc.DocumentElement.InnerText);

         return new BackupSetStatusOnly {
            ID = doc.DocumentElement.Attributes["ID"].Value,
            Name = doc.DocumentElement.Attributes["Name"].Value,
            Type = doc.DocumentElement.Attributes["Type"].Value
         };
      }

      internal OperationResult ListBackupJobs(string accountName) {

         XmlDocument doc = new XmlDocument();

         try {
            doc.Load(new UriBuilder("http://" + this.OrIpUrl) {
               Path = "/obs/api/ListBackupJobs.do",
               Query = new NameValueCollection {
                     { "SysUser", this.UserName },
                     { "SysPwd", DecryptPassword(this.Password) },
                     { "LoginName", accountName }
                  }.ToQueryString()
            }.Uri.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (doc.DocumentElement.Name == "err")
            return new ErrorResult(doc.DocumentElement.InnerText);

         var result = new BackupJobList();

         foreach (var setNode in doc.DocumentElement.SelectNodes("BackupSet").Cast<XmlElement>()) {

            var set = new BackupJobListBackupSet { 
               ID = Int64.Parse(setNode.Attributes["ID"].Value)
            };

            foreach (var jobNode in setNode.SelectNodes("BackupJob").Cast<XmlElement>()) {

               set.BackupJobs.Add(
                  new BackupJobListBackupJob { 
                     ID = jobNode.Attributes["ID"].Value
                  }
               );
            }

            result.BackupSets.Add(set);
         }

         return result;
      }

      internal OperationResult ListBackupJobStatus(string accountName, DateTime backupDate) {

         XmlDocument doc = new XmlDocument();

         try {
            doc.Load(new UriBuilder("http://" + this.OrIpUrl) {
               Path = "/obs/api/ListBackupJobStatus.do",
               Query = new NameValueCollection {
                  { "SysUser", this.UserName },
                  { "SysPwd", DecryptPassword(this.Password) },
                  { "LoginName", accountName },
                  { "BackupDate", backupDate.ToString("yyyy-MM-dd") }
               }.ToQueryString()
            }.Uri.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (doc.DocumentElement.Name == "err")
            return new ErrorResult(doc.DocumentElement.InnerText);

         var result = new BackupJobStatusList {
            BackupDate = DateTime.Parse(doc.DocumentElement.Attributes["BackupDate"].Value)
         };

         foreach (XmlElement backupNode in doc.DocumentElement.SelectNodes("BackupJob").Cast<XmlElement>()) {

            var backup = new BackupJobStatusListBackupJob {
               BackupJobStatus = backupNode.Attributes["BackupJobStatus"].Value,
               BackupSetID = Int64.Parse(backupNode.Attributes["BackupSetID"].Value),
               BackupSetName = backupNode.Attributes["BackupSetName"].Value,
               ID = backupNode.Attributes["ID"].Value,
               LoginName = backupNode.Attributes["LoginName"].Value,
               RunVersion = backupNode.Attributes["RunVersion"].Value,
               StartTime = DateTime.Parse(backupNode.Attributes["StartTime"].Value),
               UploadSize = Double.Parse(backupNode.Attributes["UploadSize"].Value)
            };

            string endTimeStr = backupNode.Attributes["EndTime"].Value;

            if (endTimeStr.HasValue()) 
               backup.EndTime = DateTime.Parse(endTimeStr);

            result.BackupJobs.Add(backup);
         }

         return result;
      }

      internal OperationResult GetBackupJobReport(string accountName, long setId, string jobId) {

         if (accountName == null) throw new ArgumentNullException("accountName");
         if (jobId == null) throw new ArgumentNullException("jobId");

         Uri serviceUrl = new UriBuilder("http://" + this.OrIpUrl) {
            Path = "/obs/api/GetBackupJobReport.do",
            Query = new NameValueCollection {
               { "SysUser", this.UserName },
               { "SysPwd", DecryptPassword(this.Password) },
               { "LoginName", accountName },
               { "BackupSetID", setId.ToString(CultureInfo.InvariantCulture) },
               { "BackupJobID", jobId }
            }.ToQueryString()
         }.Uri;

         XmlDocument doc = new XmlDocument();

         try {
            doc.Load(serviceUrl.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (doc.DocumentElement.Name == "err")
            return new ErrorResult(doc.DocumentElement.InnerText);

         var report = new BackupJobReport {
            ServerId = this.Id,
            AccountName = accountName,
            SetId = setId,
            ID = doc.DocumentElement.Attributes["ID"].Value,
            BackupJobStatus = doc.DocumentElement.Attributes["BackupJobStatus"].Value,
            NumOfNewFiles = Int64.Parse(doc.DocumentElement.Attributes["NumOfNewFiles"].Value),
            TotalNewFilesSize = Double.Parse(doc.DocumentElement.Attributes["TotalNewFilesSize"].Value),
            NumOfUpdatedFiles = Int64.Parse(doc.DocumentElement.Attributes["NumOfUpdatedFiles"].Value),
            TotalUpdatedFilesSize = Double.Parse(doc.DocumentElement.Attributes["TotalUpdatedFilesSize"].Value),
            NumOfUpdatedPermissionFiles = Int64.Parse(doc.DocumentElement.Attributes["NumOfUpdatedPermissionFiles"].Value),
            TotalUpdatedPermissionFileSize = Double.Parse(doc.DocumentElement.Attributes["TotalUpdatedPermissionFileSize"].Value),
            NumOfDeletedFiles = Int64.Parse(doc.DocumentElement.Attributes["NumOfDeletedFiles"].Value),
            TotalDeletedFilesSize = Double.Parse(doc.DocumentElement.Attributes["TotalDeletedFilesSize"].Value),
            NumOfMovedFiles = Int64.Parse(doc.DocumentElement.Attributes["NumOfMovedFiles"].Value),
            TotalMovedFilesSize = Double.Parse(doc.DocumentElement.Attributes["TotalMovedFilesSize"].Value)
         };

         string startTime = doc.DocumentElement.Attributes["StartTime"].Value;
         string endTime = doc.DocumentElement.Attributes["EndTime"].Value;

         report.StartTime = DateTime.Parse(startTime);

         if (endTime.HasValue()) 
            report.EndTime = DateTime.Parse(endTime);

         Func<XmlElement, BackupJobReportLogEntry> parseLogEntry = x => {
            
            var entry = new BackupJobReportLogEntry {
               Type = x.LocalName,
               Message = x.Attributes["Message"].Value
            };

            string timestamp = x.Attributes["Timestamp"].Value;

            entry.TimeStamp = DateTime.Parse(timestamp);
            
            return entry;
         };

         Func<XmlElement, BackupJobReportFileEntry, BackupJobReportFileEntry> loadFileEntry = (x, f) => {

            string lastModified = x.Attributes["LastModified"].Value;
            string unzipFilesSize = x.Attributes["UnzipFilesSize"].Value;

            f.FileSize = Double.Parse(x.Attributes["FileSize"].Value);
            f.Ratio = x.Attributes["Ratio"].Value;
            f.LastModified = DateTime.Parse(lastModified);
            f.UnzipFilesSize = Double.Parse(unzipFilesSize);

            return f;
         };

         foreach (XmlElement backupLog in doc.DocumentElement.SelectNodes("Info").Cast<XmlElement>())
            report.LogEntries.Add(parseLogEntry(backupLog));

         foreach (XmlElement backupLog in doc.DocumentElement.SelectNodes("Warn").Cast<XmlElement>())
            report.LogEntries.Add(parseLogEntry(backupLog));

         foreach (XmlElement backupLog in doc.DocumentElement.SelectNodes("Error").Cast<XmlElement>())
            report.LogEntries.Add(parseLogEntry(backupLog));

         foreach (XmlElement backupFile in doc.DocumentElement.SelectNodes("NewFile").Cast<XmlElement>()) {
            
            var entry = new BackupJobReportNewFileEntry { 
               Name = backupFile.Attributes["Name"].Value 
            };
            
            loadFileEntry(backupFile, entry);
            report.NewFiles.Add(entry);
         }

         foreach (XmlElement backupFile in doc.DocumentElement.SelectNodes("UpdatedFile").Cast<XmlElement>()) {
            
            var entry = new BackupJobReportUpdatedFileEntry { 
               Name = backupFile.Attributes["Name"].Value 
            };
            
            loadFileEntry(backupFile, entry);
            report.UpdatedFiles.Add(entry);
         }

         foreach (XmlElement backupFile in doc.DocumentElement.SelectNodes("UpdatedPermissionFile").Cast<XmlElement>()) {

            var entry = new BackupJobReportUpdatedPermissionFileEntry {
               Name = backupFile.Attributes["Name"].Value
            };

            loadFileEntry(backupFile, entry);
            report.UpdatedPermissionFiles.Add(entry);
         }

         foreach (XmlElement backupFile in doc.DocumentElement.SelectNodes("DeletedFile").Cast<XmlElement>()) {
            
            var entry = new BackupJobReportDeletedFileEntry { 
               Name = backupFile.Attributes["Name"].Value 
            };

            loadFileEntry(backupFile, entry);
            report.DeletedFiles.Add(entry);
         }

         foreach (XmlElement backupFile in doc.DocumentElement.SelectNodes("MovedFile").Cast<XmlElement>()) {
            
            var entry = new BackupJobReportMovedFileEntry { 
               FromFile = backupFile.Attributes["FromFile"].Value,
               ToFile = backupFile.Attributes["ToFile"].Value
            };

            loadFileEntry(backupFile, entry);
            report.MovedFiles.Add(entry);
         }

         report.JobStatusDescription = (!report.FinishedSuccessfully 
            && report.LogEntries.Count > 0) ?

            (from l in report.LogEntries
             orderby l.TimeStamp descending
             select l.Message).First()

            : GetJobStatusDescription(report.BackupJobStatus);

         var setResult = GetBackupSet(accountName, setId);

         if (!setResult.IsError) {
            var setResultOK = (BackupSetStatusOnly)setResult;

            report.SetName = setResultOK.Name;
         }

         return report;
      }

      internal OperationResult GetBackupJobReportSummaries(string accountName) {

         var backupJobs = ListBackupJobs(accountName);

         if (backupJobs.IsError)
            return backupJobs;

         var backupJobsOK = (BackupJobList)backupJobs;

         var result = new ServerGetBackupJobReportSummariesResult();

         foreach (var backupSet in backupJobsOK.BackupSets) {

            var setStatus = GetBackupSet(accountName, backupSet.ID);

            if (setStatus.IsError)
               continue;

            var setStatusOK = (BackupSetStatusOnly)setStatus;

            foreach (var backupJob in backupSet.BackupJobs) {

               bool mustContinue = (backupJob.JobDate >= DateTime.Now.AddDays(-1))
                  && (backupJob.JobDate <= DateTime.Now);

               if (!mustContinue)
                  continue;

               var jobReport = GetBackupJobReport(accountName, backupSet.ID, backupJob.ID);

               if (jobReport.IsError)
                  continue;

               var jobReportOK = (BackupJobReport)jobReport;

               result.Jobs.Add(jobReportOK);
            }
         }

         // Check for Missed Backups

         var backupJobsStatus = ListBackupJobStatus(accountName, DateTime.Today);

         if (!backupJobsStatus.IsError) {

            var backupJobsStatusOK = (BackupJobStatusList)backupJobsStatus;
            var missedBackups =
               from b in backupJobsStatusOK.BackupJobs
               where !result.Jobs.Any(r => r.ID == b.ID && r.SetId == b.BackupSetID)
               select b;

            foreach (var backup in missedBackups) {

               result.Jobs.Add(
                  new BackupJobReport { 
                     AccountName = backup.LoginName,
                     BackupJobStatus = backup.BackupJobStatus,
                     EndTime = backup.EndTime,
                     ID = backup.ID,
                     JobStatusDescription = GetJobStatusDescription(backup.BackupJobStatus),
                     ServerId = this.Id,
                     SetId = backup.BackupSetID,
                     SetName = backup.BackupSetName,
                     StartTime = backup.StartTime
                  }
               );
            }
         }

         return result;
      }

      internal OperationResult GetNewAccounts() {

         XmlDocument usersDoc = new XmlDocument();

         try {
            usersDoc.Load(new UriBuilder("http://" + this.OrIpUrl) {
               Path = "/obs/api/ListUsers.do",
               Query = new NameValueCollection {
                  { "SysUser", this.UserName },
                  { "SysPwd", DecryptPassword(this.Password) }
               }.ToQueryString()
            }.Uri.AbsoluteUri);
         } catch (Exception ex) {
            return new ErrorResult(ex.Message);
         }

         if (usersDoc.DocumentElement.Name == "err")
            return new ErrorResult(usersDoc.DocumentElement.InnerText);

         var result = new ServerGetNewAccountsResult();

         foreach (XmlElement user in usersDoc.DocumentElement.GetElementsByTagName("User")) {

            bool userIsAcb = user.Attributes["ClientType"].Value.Trim().ToUpperInvariant() == "ACB";

            if (this.IsAcb != userIsAcb)
               continue;

            string accountName = user.Attributes["LoginName"].Value.Trim();

            int existingAccounts = this.AccountsQuery.Count(a => a.AccountName.ToLower() == accountName.ToLower());
            int existingNewAccounts = this.NewAccountsQuery.Count(a => a.AccountName.ToLower() == accountName.ToLower());

            if (existingAccounts > 0
               || existingNewAccounts > 0)
               continue;

            NewAccount newAccount = new NewAccount {
               ListId = CreateId(),
               ServerId = this.Id,
               AccountName = accountName,
               Email = user.GetElementsByTagName("Contact").Cast<XmlElement>().Select(x => x.Attributes["Email"].Value).FirstOrDefault(),
               IsAcb = userIsAcb,
               IsPaid = user.Attributes["UserType"].Value.Trim().ToUpper() == "PAID",
               IsActive = user.Attributes["Status"].Value.Trim().ToUpper() == "ENABLE",
               SetId = "",
               SetName = "",
               JobId = "",
               JobStatus = "",
               Created = DateTime.Now
            };

            result.Accounts.Add(newAccount);

            XmlDocument jobsDoc = new XmlDocument();

            try {
               jobsDoc.Load(new UriBuilder("http://" + this.OrIpUrl) {
                  Path = "/obs/api/ListBackupJobs.do",
                  Query = new NameValueCollection {
                     { "SysUser", this.UserName },
                     { "SysPwd", DecryptPassword(this.Password) },
                     { "LoginName", newAccount.AccountName }
                  }.ToQueryString()
               }.Uri.AbsoluteUri);
            } catch (Exception) {
               continue;
            }

            if (jobsDoc.DocumentElement.Name == "err")
               continue;

            XmlNodeList backupSets = jobsDoc.GetElementsByTagName("BackupSet");

            foreach (XmlNode backupSet in backupSets) {

               newAccount.JobId = backupSet.LastChild.Attributes["ID"].Value;

               string[] jobIdComponent = newAccount.JobId.Split('-');

               DateTime jobDate = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}", jobIdComponent[0], jobIdComponent[1], jobIdComponent[2], jobIdComponent[3], jobIdComponent[4], jobIdComponent[5]));

               if (jobDate > DateTime.MinValue)
                  newAccount.SetId = backupSet.Attributes["ID"].Value;
            }

            if (!newAccount.JobId.HasValue())
               continue;

            XmlDocument reportDoc = new XmlDocument();

            try {
               reportDoc.Load(new UriBuilder("http://" + this.OrIpUrl) {
                  Path = "/obs/api/GetBackupJobReportSummary.do",
                  Query = new NameValueCollection {
                     { "SysUser", this.UserName },
                     { "SysPwd", DecryptPassword(this.Password) },
                     { "LoginName", newAccount.AccountName },
                     { "BackupSetID", newAccount.SetId },
                     { "BackupJobID", newAccount.JobId }
                  }.ToQueryString()
               }.Uri.AbsoluteUri);
            } catch (Exception) {
               continue;
            }

            if (reportDoc.DocumentElement.Name == "err")
               continue;

            newAccount.JobStatus = reportDoc.DocumentElement.Attributes["BackupJobStatus"].Value;

            XmlDocument setDoc = new XmlDocument();

            try {
               setDoc.Load(new UriBuilder("http://" + this.OrIpUrl) {
                  Path = "/obs/api/GetBackupSet.do",
                  Query = new NameValueCollection {
                     { "SysUser", this.UserName },
                     { "SysPwd", DecryptPassword(this.Password) },
                     { "LoginName", newAccount.AccountName },
                     { "BackupSetID", newAccount.SetId },
                  }.ToQueryString()
               }.Uri.AbsoluteUri);
            } catch (Exception) {
               continue;
            }

            if (setDoc.DocumentElement.Name == "err")
               continue;

            newAccount.SetName = setDoc.DocumentElement.Attributes["Name"].Value;
         }

         return result;
      }

      internal string GetJobStatusDescription(string jobStatus) {

         switch (jobStatus) {
            case "BS_STOP_SUCCESS":
               return "Finished successfully";

            case "BS_STOP_BY_SYSTEM_ERROR":
               return "Stopped due to system error";

            case "BS_STOP_BY_SCHEDULER":
               return "Stopped by scheduler";

            case "BS_STOP_BY_USER":
               return "Stopped by userDetails";

            case "BS_STOP_BY_QUOTA_EXCEEDED":
               return "Stopped due to quota exceeded";

            case "BS_STOP_SUCCESS_WITH_ERROR":
               return "Completed with errors";

            case "BS_STOP_SUCCESS_WITH_WARNING":
               return "Completed with warnings";

            case "BS_STOP_MISSED_BACKUP":
               return "Missed Backup";

            case "":
               return "In progress";

            default:
               return "Undefined error";
         }
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
   }
}
