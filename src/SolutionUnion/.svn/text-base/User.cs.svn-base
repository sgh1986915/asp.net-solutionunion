using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Resources.Validation;
using SolutionUnion.Payment;

namespace SolutionUnion {

   public class User : IUserSettings {

      public static bool IsAuthenticated {
         get {
            return CurrentPrincipal.Identity.IsAuthenticated;
         }
      }

      public static long CurrentUserId {
         get { return (long)Membership.GetUser().ProviderUserKey; }
      }

      public static IPrincipal CurrentPrincipal {
         get { return Thread.CurrentPrincipal; }
      }

      public static User CurrentUser {
         get {
            if (!IsAuthenticated)
               return null;

            long id = CurrentUserId;

            var repo = Repository<User>.GetInstance();

            return repo.Find(id);
         }
      }

      // User Info

      public long Id { get; set; }
      public string Email { get; set; }
      public string Password { get; set; }
      public long UserRoleId { get; set; }

      // Personal Info

      public string CompanyName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Phone { get; set; }

      // Settings

      public decimal MinimumMonthlyFee { get; set; }
      public decimal ProVaultMinimumGb { get; set; }
      public decimal ProAccountMinimumFee { get; set; }
      public decimal? ProCostPerGb { get; set; }
      public decimal LiteVaultMinimumGb { get; set; }
      public decimal LiteAccountMinimumFee { get; set; }
      public decimal LiteCostPerGb { get; set; }
      public decimal PercentageDiscount { get; set; }
      public bool PercentageDiscountEnabled { get; set; }
      public decimal? FixedBackupBilling { get; set; }

      // System Info

      public string ListId { get; set; }
      public DateTime? NextChargeDate { get; set; }
      public bool MarkedAsDeleted { get; set; }
      public DateTime? MarkedAsDeletedDate { get; set; }
      public bool IsSuspended { get; set; }
      public bool IsLocked { get; set; }
      public DateTime Created { get; set; }
      public string CustomerId { get; set; }

      public decimal SubtotalCache { get; set; }

      // Associations

      public virtual UserRole UserRole { get; set; }
      public virtual ICollection<Invoice> Invoices { get; private set; }
      public virtual ICollection<Account> Accounts { get; private set; }
      public virtual ICollection<Service> Services { get; private set; }
      public virtual ICollection<Session> Sessions { get; private set; }
      public virtual ICollection<CreditCard> CreditCards { get; private set; }
      public virtual ICollection<Invitation> Invitations { get; private set; }

      internal IQueryable<Invoice> InvoicesQuery {
         get {
            var repo = Repository<User>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Invoices);
         }
      }

      internal IQueryable<Account> AccountsQuery {
         get {
            var repo = Repository<User>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Accounts);
         }
      }

      internal IQueryable<Service> ServicesQuery {
         get {
            var repo = Repository<User>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Services);
         }
      }

      internal IQueryable<Invitation> InvitationsQuery {
         get {
            var repo = Repository<User>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.Invitations);
         }
      }

      internal IQueryable<CreditCard> CreditCardsQuery {
         get {
            var repo = Repository<User>.GetInstance();
            return repo.CreateAssociationQuery(this, a => a.CreditCards);
         }
      }

      public static OperationResult Create(UserCreateInput input) {

         if (input == null) throw new ArgumentNullException("input");

         var repo = Repository<User>.GetInstance();
         var roleRepo = ServiceLocator.Current.GetInstance<RoleRepository>();

         var errors = new ErrorResult();

         if (errors.Not(ValidateCreate(input)))
            return errors;

         var user = new User {
            CompanyName = input.CompanyName,
            Created = DateTime.Now,
            Email = input.Email,
            FirstName = input.FirstName,
            FixedBackupBilling = input.FixedBackupBilling,
            LastName = input.LastName,
            ListId = CreateId(),
            LiteAccountMinimumFee = input.LiteAccountMinimumFee,
            LiteCostPerGb = input.LiteCostPerGb,
            LiteVaultMinimumGb = input.LiteVaultMinimumGb,
            MinimumMonthlyFee = input.MinimumMonthlyFee,
            Password = EncryptPassword(input.Password),
            PercentageDiscount = input.PercentageDiscount,
            PercentageDiscountEnabled = input.PercentageDiscountEnabled,
            Phone = input.Phone,
            ProAccountMinimumFee = input.ProAccountMinimumFee,
            ProVaultMinimumGb = input.ProVaultMinimumGb,
            ProCostPerGb = input.ProCostPerGb,
            UserRoleId = roleRepo.GetRoleId(input.Role),
            // TODO: Store card in vault and save token
            //CreditCards = { 
            //   new CreditCard {
            //      Created = DateTime.Now,
            //      CreditCardToken = null,
            //      CreditCardType = input.CreditCardType,
            //      ExpirationMonth = input.ExpiryMonth,
            //      ExpirationYear = input.ExpiryYear,
            //      IsDefault = true,
            //      NameOnCard = String.Join(" ", new[] { input.FirstName, input.LastName }.Where(s => !String.IsNullOrEmpty(s)))
            //   }
            //}
         };

         repo.Add(user);

         return new SuccessfulResult("User created successfully.");
      }

      public static OperationResult SignUp(string invitationCipher, out UserSignUpInput input) {

         input = new UserSignUpInput();

         if (invitationCipher.HasValue()) { 
            
            var invitationResult = Invitation.AcceptInvitation(invitationCipher, input);

            if (invitationResult.IsError)
               return invitationResult;
         }

         return new SuccessfulResult();
      }

      public static OperationResult SignUp(string invitationCipher, UserSignUpInput input, Controller controller) {

         if (input == null) throw new ArgumentNullException("input");

         var signUpRepo = Repository<SignUp>.GetInstance();
         var roleRepo = ServiceLocator.Current.GetInstance<RoleRepository>();

         var errors = new ErrorResult();

         if (errors.Not(ValidateCreate(input))
            || errors.Not(input.AgreedToTermsOfService, "You must agree to the Terms of Service.", () => input.AgreedToTermsOfService))
            return errors;

         long? invitationId = null;

         if (invitationCipher.HasValue()) {

            long iId;

            if (Invitation.TryGetInvitationId(invitationCipher, out iId)) {

               if (errors.Not(input.InvitationId == iId, "Invalid {1}.", () => input.InvitationId))
                  return errors;

               invitationId = iId; 
            }
         }

         var signUp = new SignUp { 
            CompanyName = input.CompanyName,
            Email = input.Email,
            FirstName = input.FirstName,
            InvitationId = invitationId,
            LastName = input.LastName,
            Password = EncryptPassword(input.Password),
            Phone = input.Phone,
            UserRoleId = roleRepo.GetRoleId(input.Role),
            Created = DateTime.Now
         };

         signUpRepo.Add(signUp);

         return new UserSignUpResult(signUp.Id);
      }

      public static OperationResult SignUpPayment(long signUpId, string confirmUrl, out UserSignUpPaymentInput input) {

         var signUpRepo = Repository<SignUp>.GetInstance();
         var signUp = signUpRepo.Find(signUpId);

         input = null;

         if (signUp == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         // TODO: Expire SignUp

         IUserSettings settings = signUp.GetUserSettings();

         var invoice = CreateSignUpInvoice(signUp.UserRole.Name, null, signUp.Created, settings);

         var paymentProc = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         input = paymentProc.CreateUserSignUpPaymentInput(signUp, invoice, confirmUrl);

         return new SuccessfulResult();
      }

      public static OperationResult SignUpPayment(string query, Controller controller) {

         var paymentProc = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var txResult = paymentProc.ConfirmUserSignUpPayment(query);

         var signUpRepo = Repository<SignUp>.GetInstance();
         SolutionUnion.SignUp signUp;

         if (txResult.IsError) {

            var txResultError = txResult as UserSignUpPaymentError;

            if (txResultError != null) {

               signUp = signUpRepo.Find(txResultError.SignUpId);

               if (signUp != null) 
                  signUp.UpdateError(txResult.Message);
            }

            return txResult;
         }

         var txResultOK = (UserSignUpTransactionResult)txResult;

         var repo = Repository<User>.GetInstance();

         signUp = signUpRepo.Find(txResultOK.SignUpId);

         if (signUp == null)
            return new ErrorResult("Couldn't find sign up data.");

         var settings = signUp.GetUserSettings();

         var user = new User {
            CompanyName = signUp.CompanyName,
            Created = DateTime.Now,
            Email = signUp.Email,
            FirstName = signUp.FirstName,
            FixedBackupBilling = settings.FixedBackupBilling,
            LastName = signUp.LastName,
            ListId = CreateId(),
            LiteAccountMinimumFee = settings.LiteAccountMinimumFee,
            LiteCostPerGb = settings.LiteCostPerGb,
            LiteVaultMinimumGb = settings.LiteVaultMinimumGb,
            MinimumMonthlyFee = settings.MinimumMonthlyFee,
            Password = signUp.Password,
            CustomerId = txResultOK.CustomerId,
            PercentageDiscount = settings.PercentageDiscount,
            PercentageDiscountEnabled = settings.PercentageDiscountEnabled,
            Phone = signUp.Phone,
            ProAccountMinimumFee = settings.ProAccountMinimumFee,
            ProVaultMinimumGb = settings.ProVaultMinimumGb,
            ProCostPerGb = settings.ProCostPerGb,
            UserRoleId = signUp.UserRoleId,
         };

         var card = txResultOK.CreditCard;
         card.Created = DateTime.Now;
         card.IsDefault = true;

         user.CreditCards.Add(card);

         var invoice = CreateSignUpInvoice(signUp.UserRole.Name, null, signUp.Created, settings);
         invoice.TransactionId = txResultOK.TransactionId;
         invoice.IsPaid = true;

         user.Invoices.Add(invoice);

         using (var txScope = new TransactionScope()) {

            repo.Add(user);

            signUp.Delete();

            if (signUp.Invitation != null)
               signUp.Invitation.Delete();

            txScope.Complete();
         }

         var mailModel = new UserSignUpMessage {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Password = DecryptPassword(signUp.Password),
            Username = user.Email
         };

         var message = new MailMessage {
            To = { user.Email },
            Subject = "Your Solution Union account has been created",
            Body = MailViewRenderer.Render("SignUp", mailModel, controller.ControllerContext)
         };

         var smtp = ServiceLocator.Current.GetInstance<SmtpClient>();

         try {
            smtp.Send(message);
         } catch (SmtpException) { }

         return new UserSignUpPaymentResult(() => user.Login(DecryptPassword(user.Password), rememberLogin: false), "SignUp successful.");
      }

      static OperationResult ValidateCreate(UserProfile input) { 
         
         var errors = new ErrorResult();

         if (errors.NotValid(input)
            || errors.Not(ValidateNewUserEmail(input.Email), () => input.Email)
            || errors.Not(ValidateNewUserCompanyName(input.CompanyName), () => input.CompanyName))
            return errors;

         return new SuccessfulResult();
      }

      public static OperationResult ValidateNewUserEmail(string email) {

         var repo = Repository<User>.GetInstance();

         var errors = new ErrorResult();

         if (errors.Not(!repo.Exists(u => u.Email == email), "The {1} '{0}' is taken.", () => email))
            return errors;

         return new SuccessfulResult();
      }

      public static OperationResult ValidateNewUserCompanyName(string companyName) {

         var repo = Repository<User>.GetInstance();

         var errors = new ErrorResult();

         if (errors.Not(!repo.Exists(u => u.CompanyName == companyName), "The Company Name '{0}' is taken.", () => companyName))
            return errors;

         return new SuccessfulResult();
      }

      public static User Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            return null;

         var repo = Repository<User>.GetInstance();

         User user = repo.Find(id);

         return user;
      }

      public static User Find(string email) {

         if (email == null) throw new ArgumentNullException("email");

         var repo = Repository<User>.GetInstance();

         User user = repo.CreateQuery().SingleOrDefault(u => u.Email == email);

         return user;
      }

      public User() {

         this.CreditCards = new Collection<CreditCard>();
         this.Invoices = new Collection<Invoice>();
      }

      public OperationResult Login(string password, bool rememberLogin) {

         if (password == null) throw new ArgumentNullException("password");

         var errors = new ErrorResult();

         if (errors.Not(!this.IsLocked, "Your account is locked, please contact support."))
            return errors;

         if (errors.Not(!this.IsSuspended, "Your account is suspended, please contact support."))
            return errors;

         if (errors.Not(this.Password.HasValue(), ValidationRes.User_MissingPasswordCannotAuthenticate))
            return errors.WithStatus(HttpStatusCode.Forbidden);

         if (errors.Not(this.Password == EncryptPassword(password), ValidationRes.User_EmailPassNotMatch))
            return errors;

         Session session = new Session { 
            Created = DateTime.Now,
            LastVisit = DateTime.Now,
            RememberLogin = rememberLogin,
            UserId = this.Id
         };

         var sessionRepo = Repository<Session>.GetInstance();
         sessionRepo.Add(session);

         return new UserLoginResult { Username = this.Email };
      }

      public OperationResult RecoverPassword(Controller controller) {

         var errors = new ErrorResult();

         if (errors.Not(this.Email.HasValue(), ValidationRes.User_MissingEmailCannotRecoverPassword))
            return errors.WithStatus(HttpStatusCode.Forbidden);

         dynamic mailModel = new ExpandoObject();
         mailModel.Username = this.Email;
         mailModel.Password = DecryptPassword(this.Password);

         string destinationEmail = this.Email;

         MailMessage message = new MailMessage {
            To = { destinationEmail },
            Subject = ValidationRes.User_PasswordRecoveryVerifSubject,
            Body = MailViewRenderer.Render("ForgotPassword", mailModel, controller.ControllerContext)
         };

         var smtp = ServiceLocator.Current.GetInstance<SmtpClient>();
         smtp.Send(message);

         string emailDisplay = String.Concat("******", destinationEmail.Substring(destinationEmail.IndexOf('@')));

         return new UserRecoverPasswordResult { EmailDisplay = emailDisplay };
      }

      public bool IsInRole(string role) {
         return Roles.IsUserInRole(this.Email, role);
      }

      public UserEdit Edit() {

         UserEdit input = new UserEdit {
            CompanyName = this.CompanyName,
            Email = this.Email,
            FirstName = this.FirstName,
            FixedBackupBilling = this.FixedBackupBilling,
            LastName = this.LastName,
            LiteAccountMinimumFee = this.LiteAccountMinimumFee,
            LiteCostPerGb = this.LiteCostPerGb,
            LiteVaultMinimumGb = this.LiteVaultMinimumGb,
            MinimumMonthlyFee = this.MinimumMonthlyFee,
            PercentageDiscount = this.PercentageDiscount,
            PercentageDiscountEnabled = this.PercentageDiscountEnabled,
            Phone = this.Phone,
            ProAccountMinimumFee = this.ProAccountMinimumFee,
            ProVaultMinimumGb = this.ProVaultMinimumGb,
            ProCostPerGb = this.ProCostPerGb,
            CurrentDiscount = GetDiscount(this.SubtotalCache)
         };

         return input;
      }

      public decimal ComputeDiscount(decimal percentageDiscount) {
         return GetDiscount(this.SubtotalCache, percentageDiscount);
      }

      public OperationResult Update(UserEdit input) {

         var repo = Repository<User>.GetInstance();

         var errors = new ErrorResult();

         if (errors.Not(ValidateUpdate(input)))
            return errors;

         this.CompanyName = input.CompanyName;
         this.Email = input.Email;
         this.FirstName = input.FirstName;
         this.FixedBackupBilling = input.FixedBackupBilling;
         this.LastName = input.LastName;
         this.LiteAccountMinimumFee = input.LiteAccountMinimumFee;
         this.LiteCostPerGb = input.LiteCostPerGb;
         this.LiteVaultMinimumGb = input.LiteVaultMinimumGb;
         this.MinimumMonthlyFee = input.MinimumMonthlyFee;
         this.PercentageDiscount = input.PercentageDiscount;
         this.PercentageDiscountEnabled = input.PercentageDiscountEnabled;
         this.Phone = input.Phone;
         this.ProAccountMinimumFee = input.ProAccountMinimumFee;
         this.ProVaultMinimumGb = input.ProVaultMinimumGb;
         this.ProCostPerGb = input.ProCostPerGb;

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} updated.", this.Email));

         return new SuccessfulResult("User updated successfully.");
      }

      OperationResult ValidateUpdate(UserProfile input) {

         var errors = new ErrorResult();

         if (errors.NotValid(input)
            || (input.Email != this.Email && errors.Not(ValidateNewUserEmail(input.Email), () => input.Email))
            || (input.CompanyName != this.CompanyName && errors.Not(ValidateNewUserCompanyName(input.CompanyName), () => input.CompanyName)))
            return errors;

         return new SuccessfulResult();
      }

      public OperationResult Suspend() {

         if (IsInRole(UserRole.Administrator))
            return new ErrorResult("Administrator cannot be suspended.");

         var repo = Repository<User>.GetInstance();

         var accounts = this.AccountsQuery.ToList();

         using (var tx = new TransactionScope()) {

            foreach (var account in accounts) {
               var accountSuspendResult = account.Suspend();

               if (accountSuspendResult.IsError)
                  return accountSuspendResult;
            }

            this.IsSuspended = true;

            repo.SaveChanges(this);

            tx.Complete();
         }

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} suspended.", this.Email));

         return new SuccessfulResult("User suspended successfully.");
      }

      public OperationResult Activate() {

         if (this.IsLocked)
            return new ErrorResult("User is locked, you must unlock first.");

         var repo = Repository<User>.GetInstance();

         var accounts = this.AccountsQuery.Where(a => !a.MarkedAsDeleted).ToList();

         using (var tx = new TransactionScope()) {

            foreach (var account in accounts) {
               var accountActivateResult = account.Activate();

               if (accountActivateResult.IsError)
                  return accountActivateResult;
            }

            this.IsSuspended = false;

            repo.SaveChanges(this);

            tx.Complete();
         }

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} activated.", this.Email));

         return new SuccessfulResult("User activated successfully.");
      }

      public OperationResult Lock() {

         if (IsInRole(UserRole.Administrator))
            return new ErrorResult("Administrator cannot be locked.");

         this.IsSuspended = true;
         this.IsLocked = true;

         var repo = Repository<User>.GetInstance();

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} locked.", this.Email));

         return new SuccessfulResult("User locked successfully.");
      }

      public OperationResult Unlock() {

         if (!this.IsLocked && this.IsSuspended)
            return new ErrorResult("User is suspended, you must activate first.");

         this.IsSuspended = false;
         this.IsLocked = false;

         var repo = Repository<User>.GetInstance();

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} unlocked.", this.Email));

         return new SuccessfulResult("User unlocked successfully.");
      }

      public OperationResult ChangePassword(UserChangePasswordInput input) {

         if (input == null) throw new ArgumentNullException("input");

         ErrorResult errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         var repo = Repository<User>.GetInstance();

         this.Password = EncryptPassword(input.NewPassword);

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} password changed.", this.Email));

         return new SuccessfulResult("Password changed.");
      }

      public OperationResult MarkAsDeleted() {

         if (this.MarkedAsDeleted)
            return new SuccessfulResult("User already marked as deleted.");

         if (IsInRole(UserRole.Administrator))
            return new ErrorResult("Administrator cannot be deleted.");

         var repo = Repository<User>.GetInstance();
         var accountRepo = Repository<Account>.GetInstance();

         var accounts = this.AccountsQuery.ToList();

         using (var tx = new TransactionScope()) {

            foreach (var account in accounts) {

               var accountDeleteResult = account.MarkAsDeleted();

               if (accountDeleteResult.IsError)
                  return accountDeleteResult;
            }

            var lockResult = Lock();

            if (lockResult.IsError)
               return lockResult;

            this.MarkedAsDeleted = true;
            this.MarkedAsDeletedDate = DateTime.Now;

            repo.SaveChanges(this);

            tx.Complete();
         }

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} marked as deleted.", this.Email));

         return new SuccessfulResult("User successfully marked as deleted.");
      }

      public OperationResult Restore() {

         var accounts = this.AccountsQuery.Where(a => a.MarkedAsDeleted).ToList();

         using (var tx = new TransactionScope()) {

            foreach (var account in accounts) {

               var accountRestoreResult = account.Restore();

               if (accountRestoreResult.IsError)
                  return accountRestoreResult;
            }

            var unlockResult = Unlock();

            if (unlockResult.IsError)
               return unlockResult;

            this.MarkedAsDeleted = false;
            this.MarkedAsDeletedDate = null;

            var repo = Repository<User>.GetInstance();

            repo.SaveChanges(this);

            tx.Complete();
         }

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} restored.", this.Email));

         return new SuccessfulResult("User successfully restored.");
      }

      public OperationResult ValidateVaultSize(decimal vaultSize, ServerType serverType) {

         decimal minSize = (serverType == ServerType.Lite) ?
            this.LiteVaultMinimumGb : this.ProVaultMinimumGb;

         if (vaultSize < minSize) 
            return new ErrorResult(String.Format("Vault Size must be at least {0} for {1} account.", minSize, serverType.ToString().ToUpperInvariant()));

         return new SuccessfulResult();
      }

      internal decimal GetLiteAdditionalGbPrice(int additionalGb) {
         return this.LiteCostPerGb;
      }

      internal decimal GetProAdditionalGbPrice(int additionalGb) {

         if (this.ProCostPerGb.HasValue)
            return this.ProCostPerGb.Value;

         if (additionalGb <= 0) throw new ArgumentOutOfRangeException("additionalGb", "additionalGb must be greater than zero.");

         var pricing =
            (from p in ApplicationSetting.Instance.ProPricingScale
             where p.StorageGb <= additionalGb
             orderby p.StorageGb descending
             select p).First();

         return pricing.PricePerGb;
      }

      public UserProfile EditProfile() {

         return new UserProfile { 
            CompanyName = this.CompanyName,
            Email = this.Email,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Phone = this.Phone,
         };
      }

      public OperationResult UpdateProfile(UserProfile input) {

         if (input == null) throw new ArgumentNullException("input");

         var repo = Repository<User>.GetInstance();

         var errors = new ErrorResult();

         if (errors.Not(ValidateUpdate(input)))
            return errors;

         this.CompanyName = input.CompanyName;
         this.Email = input.Email;
         this.FirstName = input.FirstName;
         this.LastName = input.LastName;
         this.Phone = input.Phone;

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} profile updated.", this.Email));

         return new SuccessfulResult("Profile updated successfully.");
      }

      public OperationResult ProfileChangePassword(UserProfileChangePassword input) {

         if (input == null) throw new ArgumentNullException("input");

         var errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         var repo = Repository<User>.GetInstance();

         this.Password = EncryptPassword(input.NewPassword);

         repo.SaveChanges(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} password updated.", this.Email));

         return new SuccessfulResult("Password changed successfully.");
      }

      public OperationResult Delete() {

         var repo = Repository<User>.GetInstance();

         foreach (Account account in this.Accounts.ToList())
            account.Delete();

         repo.Delete(this);

         Session.Log(String.Format(CultureInfo.InvariantCulture, "User {0} deleted.", this.Email));

         return new SuccessfulResult("User deleted.");
      }

      // Services

      public OperationResult AddServices(ServiceEdit[] input) {

         if (input.Length == 0)
            return new SuccessfulResult();

         var repo = Repository<User>.GetInstance();

         var errors = new ErrorResult();

         for (int i = 0; i < input.Length; i++) {

            if (errors.NotValid(input[i]))
               return errors;
         }

         foreach (var item in input) {
            this.Services.Add(new Service {
               Created = DateTime.Now,
               Description = item.Description,
               Price = item.Price
            });
         }

         repo.SaveChanges(this);

         return new SuccessfulResult(((input.Length == 1) ? "Service" :  "Services") + " created.");
      }

      public OperationResult EditService(long id, out ServiceEdit input) {

         input = null;

         Service service = this.ServicesQuery.Where(s => s.Id == id).SingleOrDefault();

         if (service == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         input = new ServiceEdit {
            Description = service.Description,
            Price = service.Price
         };

         return new SuccessfulResult();
      }

      public OperationResult UpdateService(long id, ServiceEdit input) {

         var errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         Service service = this.ServicesQuery.Where(s => s.Id == id).SingleOrDefault();

         if (service == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         var repo = Repository<User>.GetInstance();

         service.Description = input.Description;
         service.Price = input.Price;

         repo.SaveChanges(this);

         return new SuccessfulResult("Service updated.");
      }

      public OperationResult DeleteService(long id) {

         var repo = Repository<User>.GetInstance();

         var service = this.ServicesQuery.Where(s => s.Id == id).SingleOrDefault();

         if (service != null) {
            repo.DeleteRelated(service, u => u.Services);
            repo.SaveChanges(this);
         }

         return new SuccessfulResult("Service deleted.");
      }

      // Credit Cards

      public OperationResult AddCreditCard(CreditCardEdit input) {

         var errors = new ErrorResult();

         if (errors.NotValid(input)
            | errors.Not(input.CreditCardNumber.HasValue(), ValidationRes.Required, () => input.CreditCardNumber)
            | errors.Not(input.CreditCardCVV.HasValue(), ValidationRes.Required, () => input.CreditCardCVV))
            return errors;

         var repo = Repository<User>.GetInstance();
         var payment = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var result = payment.AddCreditCard(this.CustomerId, input);

         if (result.IsError)
            return result;

         var resultOK = (AddCreditCardResult)result;

         var creditCard = resultOK.CreditCard;
         creditCard.Created = DateTime.Now;

         this.CreditCards.Add(creditCard);

         using (var tx = new TransactionScope()) {

            repo.SaveChanges(this);

            if (input.MakeDefault) {
               var makeDefaultResult = MakeCreditCardDefault(creditCard.Id);

               if (makeDefaultResult.IsError)
                  return makeDefaultResult;
            }

            tx.Complete();
         }

         return new CreditCardCreatedResult(creditCard.Id, "Credit Card created.");
      }

      public OperationResult EditCreditCard(long id, out CreditCardEdit input) {

         input = null;

         CreditCard creditCard = this.CreditCardsQuery.Where(s => s.Id == id).SingleOrDefault();

         if (creditCard == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         input = new CreditCardEdit {
            BillingAddress = creditCard.Address,
            City = creditCard.City,
            Country = creditCard.Country,
            ExpiryMonth = creditCard.ExpirationMonth,
            ExpiryYear = creditCard.ExpirationYear,
            NameOnCard = creditCard.NameOnCard,
            PostalCode = creditCard.PostalCode,
            State = creditCard.State
         };

         return new SuccessfulResult();
      }

      public OperationResult UpdateCreditCard(long id, CreditCardEdit input) {

         var errors = new ErrorResult();

         if (errors.NotValid(input))
            return errors;

         CreditCard creditCard = this.CreditCardsQuery.Where(s => s.Id == id).SingleOrDefault();

         if (creditCard == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         var repo = Repository<User>.GetInstance();
         var payment = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var result = payment.UpdateCreditCard(creditCard.Token, input);

         if (result.IsError)
            return result;

         var resultOK = (UpdateCreditCardResult)result;

         creditCard.Address = input.BillingAddress;
         creditCard.City = input.City;
         creditCard.Country = input.Country;
         creditCard.ExpirationMonth = input.ExpiryMonth;
         creditCard.ExpirationYear = input.ExpiryYear;
         creditCard.LastFour = resultOK.LastFour;
         creditCard.NameOnCard = input.NameOnCard;
         creditCard.PostalCode = input.PostalCode;
         creditCard.State = input.State;
         creditCard.Type = resultOK.Type;

         repo.SaveChanges(this);

         return new SuccessfulResult("Credit Card updated.");
      }

      public OperationResult MakeCreditCardDefault(long creditCardId) {

         var repo = Repository<User>.GetInstance();
         var payment = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var creditCard = this.CreditCardsQuery.Where(c => c.Id == creditCardId).SingleOrDefault();

         if (creditCard == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         var result = payment.MakeCreditCardDefault(creditCard.Token);

         if (result.IsError)
            return result;

         using (var tx = new TransactionScope()) {
            
            creditCard.IsDefault = true;

            foreach (var cc in this.CreditCardsQuery.Where(c => c.Id != creditCardId))
               cc.IsDefault = false;

            repo.SaveChanges(this);

            tx.Complete();
         }

         return new SuccessfulResult("Credit Card updated.");
      }

      public OperationResult DeleteCreditCard(long creditCardId) {

         var repo = Repository<User>.GetInstance();
         var payment = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var creditCard = this.CreditCardsQuery.Where(c => c.Id == creditCardId).SingleOrDefault();

         if (creditCard == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         int count = this.CreditCardsQuery.Count();

         if (count == 1)
            return new ErrorResult("You must keep at least one credit card.");

         var result = payment.DeleteCreditCard(creditCard.Token);

         if (result.IsError)
            return result;

         repo.DeleteRelated(creditCard, u => u.CreditCards);

         return new SuccessfulResult("Credit Card deleted.");
      }

      // Billing

      Invoice CreateSignUpInvoice() {
         return CreateSignUpInvoice(this.UserRole.Name, this.Services.ToArray(), DateTime.Now, this);
      }

      static Invoice CreateSignUpInvoice(string role, Service[] services, DateTime now, IUserSettings settings) {

         bool isReseller = role == UserRole.Reseller;

         DateTime firstThisMonth = new DateTime(now.Year, now.Month, 1);
         DateTime nextMonth = firstThisMonth.AddMonths(1);
         int daysThisMonth = nextMonth.AddDays(-1).Day;
         TimeSpan remainingTimeThisMonth = nextMonth.Subtract(now);
         decimal remainingDaysThisMonth = Convert.ToDecimal(Math.Round(remainingTimeThisMonth.TotalDays, 2));

         Func<decimal, decimal> prorate = (a) =>
            Decimal.Round((a / daysThisMonth) * remainingDaysThisMonth, 2);

         Invoice invoice = new Invoice {
            Created = now
         };

         decimal accountMonthlyAmount = (settings.FixedBackupBilling.HasValue) ?
            settings.FixedBackupBilling.Value
            : settings.MinimumMonthlyFee;

         invoice.InvoiceItemLines.Add(new InvoiceItemLine { 
            Description = (isReseller) ? 
               "Private Label Online Backup Services"
               : "Online Backup Services",

            Amount = prorate(accountMonthlyAmount)
         });

         if (services != null) {
            foreach (Service service in services) {

               invoice.InvoiceItemLines.Add(
                  new InvoiceItemLine {
                     Description = service.Description,
                     Amount = prorate(service.Price)
                  }
               );
            } 
         }

         invoice.Subtotal = invoice.InvoiceItemLines.Sum(i => i.Amount);

         if (settings.PercentageDiscountEnabled
            && settings.PercentageDiscount > 0) {

            invoice.Discount = Decimal.Round(invoice.Subtotal * (settings.PercentageDiscount / 100m), 2);
         }

         invoice.Total = invoice.Subtotal - invoice.Discount;

         return invoice;
      }

      internal OperationResult ChargeMonth() {

         Invoice lastInvoice = GetLastInvoice();

         if (lastInvoice == null)
            return new ErrorResult("Couldn't find signup invoice.");

         DateTime now = DateTime.Now;
         DateTime lastInvoiceDate = lastInvoice.Created;

         if (now.Year == lastInvoiceDate.Year
            && now.Month == lastInvoiceDate.Month)
            return new SuccessfulResult("Already charged this month.");

         Invoice invoice = CreateMonthInvoice(now, lastInvoiceDate);

         this.Invoices.Add(invoice);

         Repository<User>.GetInstance().SaveChanges(this);

         return invoice.Pay();
      }

      Invoice CreateMonthInvoice(DateTime now, DateTime lastInvoiceDate) {

         if (lastInvoiceDate > now)
            throw new InvalidOperationException("The last invoice date cannot be greater than now.");

         DateTime startInclusive = lastInvoiceDate.Date;
         DateTime endExclusive = now.Date;

         bool isReseller = IsInRole(UserRole.Reseller);

         Invoice invoice = new Invoice {
            Created = now
         };

         string defaultAccountItemDescription = (isReseller) ?
            "Private Label Online Backup Services"
            : "Online Backup Services";

         if (this.FixedBackupBilling.HasValue) {

            invoice.InvoiceItemLines.Add(new InvoiceItemLine {
               Description = defaultAccountItemDescription,
               Amount = this.FixedBackupBilling.Value
            });

         } else {

            List<InvoiceItemLine> accountItems = new List<InvoiceItemLine>();

            Func<decimal, string> getDisplayForBytes = d => {
               d = d / 1024 / 1024;
               if (d < 1024) {
                  return String.Format("{0}M", Math.Round(d, 1));
               } else {
                  d = d / 1024;
                  return String.Format("{0}G", Math.Round(d, 1));
               }
            };

            foreach (var account in this.AccountsQuery.Where(a => a.IsPaid)) {

               var period = account.GetDailyUsagesForCost(startInclusive, endExclusive);

               decimal size = account.GetUsageAverage(period);
               decimal cost = account.GetCost(period);

               accountItems.Add(new InvoiceItemLine {
                  Description = String.Format(CultureInfo.InvariantCulture, "{0}: {1} (average)", 
                     account.AccountName, getDisplayForBytes(account.TotalSize)
                  ),
                  Amount = cost
               });
            }

            if (accountItems.Count == 0
               || accountItems.Sum(a => a.Amount) < this.MinimumMonthlyFee) {

               accountItems.Clear();

               accountItems.Add(new InvoiceItemLine {
                  Description = defaultAccountItemDescription,
                  Amount = this.MinimumMonthlyFee
               });
            }

            foreach (var item in accountItems) 
               invoice.InvoiceItemLines.Add(item);
         }

         foreach (Service service in this.Services) {

            invoice.InvoiceItemLines.Add(
               new InvoiceItemLine {
                  Description = service.Description,
                  Amount = service.Price
               }
            );
         }

         invoice.Subtotal = invoice.InvoiceItemLines.Sum(i => i.Amount);
         invoice.Discount = GetDiscount(invoice.Subtotal);
         invoice.Total = invoice.Subtotal - invoice.Discount;

         return invoice;
      }

      Invoice GetLastInvoice() {
         
         Invoice lastInvoice = this.InvoicesQuery.OrderByDescending(i => i.Id).FirstOrDefault();
         return lastInvoice;
      }

      internal decimal GetDiscount(decimal subtotal) {

         if (!this.PercentageDiscountEnabled)
            return 0;

         return GetDiscount(subtotal, this.PercentageDiscount);
      }

      decimal GetDiscount(decimal subtotal, decimal percentageDiscount) {

         if (percentageDiscount <= 0)
            return 0;

         return Decimal.Round(subtotal * (percentageDiscount / 100m), 2);
      }

      // System

      internal OperationResult UpdateCachedMembers() {

         RefreshCachedMembers();

         var repo = Repository<User>.GetInstance();
         repo.SaveChanges(this);

         return new SuccessfulResult();
      }

      void RefreshCachedMembers() {

         Invoice lastInvoice = GetLastInvoice();

         DateTime now = DateTime.Now;
         DateTime lastInvoiceDate = (lastInvoice != null) ? lastInvoice.Created : now.AddDays(-30);

         Invoice invoice = CreateMonthInvoice(now, lastInvoiceDate);

         this.SubtotalCache = invoice.Subtotal;
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
