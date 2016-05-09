using System;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Web {

   // Note: Thread safety is required
   // http://msdn.microsoft.com/library/8fw7xh74

   public class ApplicationRoleProvider : RoleProvider {

      public override string ApplicationName { get; set; }

      public override void AddUsersToRoles(string[] usernames, string[] roleNames) {
         throw new NotImplementedException("AddUsersToRoles not implemented.");
      }

      public override void CreateRole(string roleName) {
         GetRepository().AddRole(roleName);
      }

      public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {
         throw new NotImplementedException("DeleteRole not implemented.");
      }

      public override string[] FindUsersInRole(string roleName, string usernameToMatch) {
         throw new NotImplementedException("FindUsersInRole not implemented.");
      }

      public override string[] GetAllRoles() {
         return GetRepository().GetAllRoles();
      }

      public override string[] GetRolesForUser(string username) {
         return GetRepository().GetRolesForUser(username);
      }

      public override string[] GetUsersInRole(string roleName) {
         return GetRepository().GetUsersInRole(roleName);
      }

      public override bool IsUserInRole(string username, string roleName) {
         return GetRepository().IsUserInRole(username, roleName);
      }

      public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {
         throw new NotImplementedException("RemoveUsersFromRoles not implemented.");
      }

      public override bool RoleExists(string roleName) {
         return GetRepository().RoleExists(roleName);
      }

      RoleRepository GetRepository() { 
         return ServiceLocator.Current.GetInstance<RoleRepository>();
      }
   }
}
