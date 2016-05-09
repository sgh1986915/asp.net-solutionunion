using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion {

   public class Invitation : IUserSettings {

      public long Id { get; set; }
      public long UserId { get; set; }
      public long UserRoleId { get; set; }

      public string Email { get; set; }
      public string CompanyName { get; set; }

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

      public DateTime DateSent { get; set; }
      public int MessageId { get; set; }

      public virtual User User { get; set; }
      public virtual UserRole UserRole { get; set; }

      public static OperationResult Create(InvitationCreateInput input, Controller controller) {

         if (!User.IsAuthenticated)
            return new ErrorResult(HttpStatusCode.Unauthorized);

         var repo = Repository<Invitation>.GetInstance();
         var roleRepo = ServiceLocator.Current.GetInstance<RoleRepository>();

         var errors = new ErrorResult();

         if (errors.NotValid(input)
            || errors.Not(ValidateNewInvitationEmail(input.Email))
            || errors.Not(ValidateNewInvitationCompanyName(input.CompanyName)))
            return errors;

         var invitation = new Invitation {
            CompanyName = input.CompanyName,
            DateSent = DateTime.Now,
            Email = input.Email,
            FixedBackupBilling = input.FixedBackupBilling,
            LiteAccountMinimumFee = input.LiteAccountMinimumFee,
            LiteCostPerGb = input.LiteCostPerGb,
            LiteVaultMinimumGb = input.LiteVaultMinimumGb,
            MinimumMonthlyFee = input.MinimumMonthlyFee,
            PercentageDiscount = input.PercentageDiscount,
            PercentageDiscountEnabled = input.PercentageDiscountEnabled,
            ProAccountMinimumFee = input.ProAccountMinimumFee,
            ProCostPerGb = input.ProCostPerGb,
            ProVaultMinimumGb = input.ProVaultMinimumGb,
            UserId = User.CurrentUserId,
            UserRoleId = roleRepo.GetRoleId(input.Role)
         };

         repo.Add(invitation);

         var sendResult = invitation.Resend(controller);

         if (sendResult.IsError)
            return sendResult;

         return new SuccessfulResult("Invitation created successfully.");
      }

      public static OperationResult ValidateNewInvitationEmail(string email) {
         return User.ValidateNewUserEmail(email);
      }

      public static OperationResult ValidateNewInvitationCompanyName(string companyName) {
         return User.ValidateNewUserCompanyName(companyName);
      }

      internal static OperationResult AcceptInvitation(string cipher, UserSignUpInput input) {

         var data = DeserializeData(cipher);

         if (data == null)
            return new ErrorResult(HttpStatusCode.NotFound);

         var repo = Repository<Invitation>.GetInstance();

         var invitation = repo.Find(data.Id);

         if (invitation == null
            || invitation.MessageId != data.MessageId
            || invitation.DateSent.AddDays(3) < DateTime.Now)
            return new ErrorResult(HttpStatusCode.Gone, "The invitation has expired.");

         input.CompanyName = invitation.CompanyName;
         input.Email = invitation.Email;
         input.Role = invitation.UserRole.Name;
         input.InvitationId = invitation.Id;

         return new SuccessfulResult();
      }

      internal static bool TryGetInvitationId(string cipher, out long id) {

         var data = DeserializeData(cipher);

         id = default(long);

         if (data == null)
            return false;

         id = data.Id;

         return true;
      }

      public static Invitation Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            return null;

         var repo = Repository<Invitation>.GetInstance();

         return repo.Find(id);
      }

      public InvitationEdit Edit() {

         var input = new InvitationEdit {
            CompanyName = this.CompanyName,
            Email = this.Email,
            FixedBackupBilling = this.FixedBackupBilling,
            LiteAccountMinimumFee = this.LiteAccountMinimumFee,
            LiteCostPerGb = this.LiteCostPerGb,
            LiteVaultMinimumGb = this.LiteVaultMinimumGb,
            MinimumMonthlyFee = this.MinimumMonthlyFee,
            PercentageDiscount = this.PercentageDiscount,
            PercentageDiscountEnabled = this.PercentageDiscountEnabled,
            ProAccountMinimumFee = this.ProAccountMinimumFee,
            ProCostPerGb = this.ProCostPerGb,
            ProVaultMinimumGb = this.ProVaultMinimumGb,
            Role = this.UserRole.Name
         };

         return input;
      }

      public OperationResult Update(InvitationEdit input, Controller controller) {

         var errors = new ErrorResult();

         if (errors.NotValid(input)
            || (input.Email != this.Email && errors.Not(ValidateNewInvitationEmail(input.Email), () => input.Email))
            || (input.CompanyName != this.CompanyName && errors.Not(ValidateNewInvitationCompanyName(input.CompanyName), () => input.CompanyName)))
            return errors;

         var repo = Repository<Invitation>.GetInstance();
         var roleRepo = ServiceLocator.Current.GetInstance<RoleRepository>();

         this.CompanyName = input.CompanyName;
         this.Email = input.Email;
         this.FixedBackupBilling = input.FixedBackupBilling;
         this.LiteAccountMinimumFee = input.LiteAccountMinimumFee;
         this.LiteCostPerGb = input.LiteCostPerGb;
         this.LiteVaultMinimumGb = input.LiteVaultMinimumGb;
         this.MinimumMonthlyFee = input.MinimumMonthlyFee;
         this.PercentageDiscount = input.PercentageDiscount;
         this.PercentageDiscountEnabled = input.PercentageDiscountEnabled;
         this.ProAccountMinimumFee = input.ProAccountMinimumFee;
         this.ProCostPerGb = input.ProCostPerGb;
         this.ProVaultMinimumGb = input.ProVaultMinimumGb;
         this.UserRoleId = roleRepo.GetRoleId(input.Role);

         repo.SaveChanges(this);

         var sendResult = Resend(controller);

         if (sendResult.IsError)
            return sendResult;

         return new SuccessfulResult("Invitation updated successfully.");
      }

      public OperationResult Resend(Controller controller) {

         var data = InvitationData.Create(this.Id);
#warning Change action name later
         var mailModel = new InvitationMessage {
            Url = controller.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) + controller.Url.Action("SignUp2", "~Home", new { i = SerializeData(data) })
         };

         var message = new MailMessage {
            To = { this.Email },
            Subject = "User Account Setup Invitation",
            Body = MailViewRenderer.Render("Invitation", mailModel, controller.ControllerContext),
         };

         var smtp = ServiceLocator.Current.GetInstance<SmtpClient>();

         try {
            smtp.Send(message);
         } catch (SmtpException) {
            return new ErrorResult("An error ocurred while trying to send invitation.");
         }

         var repo = Repository<Invitation>.GetInstance();

         this.DateSent = DateTime.Now;
         this.MessageId = data.MessageId;

         repo.SaveChanges(this);

         return new SuccessfulResult("Invitation sent.");
      }

      string SerializeData(InvitationData data) {

         return MachineKey.Encode(
            Encoding.Unicode.GetBytes(new NameValueCollection { 
                  { "Id", data.Id.ToString(CultureInfo.InvariantCulture) },
                  { "Random", data.MessageId.ToString(CultureInfo.InvariantCulture) }
               }.ToQueryString()), 
               MachineKeyProtection.Encryption
            );
      }

      static InvitationData DeserializeData(string cipher) {

         if (cipher == null) throw new ArgumentNullException("cipher");

         string query;

         try {
            query = Encoding.Unicode.GetString(MachineKey.Decode(cipher, MachineKeyProtection.Encryption));
            
         } catch {
            return null;
         }

         NameValueCollection values = HttpUtility.ParseQueryString(query);

         InvitationData data = new InvitationData {
            Id = Int64.Parse(values["Id"]),
            MessageId = Int32.Parse(values["Random"])
         };

         return data;
      }

      public OperationResult Delete() {

         var repo = Repository<Invitation>.GetInstance();

         repo.Delete(this);

         return new SuccessfulResult("Invitation deleted.");
      }

      class InvitationData {

         public long Id { get; set; }
         public int MessageId { get; set; }

         public static InvitationData Create(long id) {

            return new InvitationData { 
               Id = id,
               MessageId = new Random().Next(100, 1000)
            };
         }
      }
   }
}
