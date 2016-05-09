using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Repositories {
   
   public abstract class RoleRepository {

      public abstract string[] GetAllRoles();
      public abstract string[] GetRolesForUser(string username);
      public abstract string[] GetUsersInRole(string roleName);
      public abstract long GetRoleId(string roleName);
      public abstract bool IsUserInRole(string username, string roleName);
      public abstract bool RoleExists(string roleName);
      public abstract void AddRole(string roleName);
      public abstract void DeleteRole(string roleName);
   }
}
