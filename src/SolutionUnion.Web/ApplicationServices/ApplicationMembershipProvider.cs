using System;
using System.Linq;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Web {

   // Note: Thread safety is required
   // http://msdn.microsoft.com/library/f1kyba5e

   public class ApplicationMembershipProvider : MembershipProvider {

      public override string ApplicationName { get; set; }

      public override bool EnablePasswordReset {
         get { throw new NotImplementedException("EnablePasswordReset not implemented."); }
      }

      public override bool EnablePasswordRetrieval {
         get { throw new NotImplementedException("EnablePasswordRetrieval not implemented."); }
      }

      public override int MaxInvalidPasswordAttempts {
         get { throw new NotImplementedException("MaxInvalidPasswordAttempts not implemented."); }
      }

      public override int MinRequiredNonAlphanumericCharacters {
         get { throw new NotImplementedException("MinRequiredNonAlphanumericCharacters not implemented."); }
      }

      public override int MinRequiredPasswordLength {
         get { throw new NotImplementedException("MinRequiredPasswordLength not implemented."); }
      }

      public override int PasswordAttemptWindow {
         get { throw new NotImplementedException("PasswordAttemptWindow not implemented."); }
      }

      public override MembershipPasswordFormat PasswordFormat {
         get { throw new NotImplementedException("PasswordFormat not implemented."); }
      }

      public override string PasswordStrengthRegularExpression {
         get { throw new NotImplementedException("PasswordStrengthRegularExpression not implemented."); }
      }

      public override bool RequiresQuestionAndAnswer {
         get { throw new NotImplementedException("RequiresQuestionAndAnswer not implemented."); }
      }

      public override bool RequiresUniqueEmail {
         get { throw new NotImplementedException("RequiresUniqueEmail not implemented."); }
      }

      public override bool ChangePassword(string username, string oldPassword, string newPassword) {
         throw new NotImplementedException("ChangePassword not implemented.");
      }

      public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
         throw new NotImplementedException("ChangePasswordQuestionAndAnswer not implemented.");
      }

      public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
         throw new NotImplementedException("CreateUser not implemented.");
      }

      public override bool DeleteUser(string username, bool deleteAllRelatedData) {
         throw new NotImplementedException("DeleteUser not implemented.");
      }

      public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
         throw new NotImplementedException("FindUsersByEmail not implemented.");
      }

      public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
         throw new NotImplementedException("FindUsersByName not implemented.");
      }

      public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {

         var repo = GetRepository();

         var query = repo.CreateQuery();

         totalRecords = query.Count();

         var collection = new MembershipUserCollection();

         foreach (var u in query.Skip(pageIndex * pageSize).Take(pageSize))
            collection.Add(CreateMembershipUser(u));

         return collection;
      }

      public override int GetNumberOfUsersOnline() {
         throw new NotImplementedException("GetNumberOfUsersOnline not implemented.");
      }

      public override string GetPassword(string username, string answer) {
         throw new NotImplementedException("GetPassword not implemented.");
      }

      public override MembershipUser GetUser(string username, bool userIsOnline) {

         var user = SolutionUnion.User.Find(username);

         if (user == null)
            return null;

         return CreateMembershipUser(user);
      }

      public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
         throw new NotImplementedException("GetUser not implemented.");
      }

      public override string GetUserNameByEmail(string email) {
         throw new NotImplementedException("GetUserNameByEmail not implemented.");
      }

      public override string ResetPassword(string username, string answer) {
         throw new NotImplementedException("ResetPassword not implemented.");
      }

      public override bool UnlockUser(string userName) {
         throw new NotImplementedException("UnlockUser not implemented.");
      }

      public override void UpdateUser(MembershipUser user) {
         throw new NotImplementedException("UpdateUser not implemented.");
      }

      public override bool ValidateUser(string username, string password) {
         throw new NotImplementedException("ValidateUser not implemented.");
      }

      Repository<User> GetRepository() {
         return Repository<User>.GetInstance();
      }

      MembershipUser CreateMembershipUser(SolutionUnion.User user) {
         return new MembershipUser(this.Name, user.Email, user.Id, user.Email, null, null, true, false, user.Created, default(DateTime), default(DateTime), default(DateTime), default(DateTime));
      }
   }
}
