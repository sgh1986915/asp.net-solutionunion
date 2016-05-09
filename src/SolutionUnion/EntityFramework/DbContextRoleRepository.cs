using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;

namespace SolutionUnion.EntityFramework {
   
   public class DbContextRoleRepository : RoleRepository {

      readonly DbContext context;
      readonly DbSet<UserRole> roles;
      readonly DbSet<User> users;

      public DbContextRoleRepository(DbContext context) {
         
         this.context = context;
         this.roles = context.Set<UserRole>();
         this.users = context.Set<User>();
      }

      public override string[] GetAllRoles() {
         return this.roles.Select(r => r.Name).ToArray();
      }

      public override string[] GetRolesForUser(string username) {
         return this.users.Where(u => u.Email == username).Select(u => u.UserRole.Name).ToArray();
      }

      public override string[] GetUsersInRole(string roleName) {
         return this.users.Where(u => u.UserRole.Name == roleName).Select(u => u.Email).ToArray();
      }

      public override long GetRoleId(string roleName) {
         return this.roles.Where(r => r.Name == roleName).Select(r => r.Id).Single();
      }

      public override bool IsUserInRole(string username, string roleName) {
         return this.users.Count(u => u.Email == username && u.UserRole.Name == roleName) > 0;
      }

      public override bool RoleExists(string roleName) {
         return this.roles.Count(r => r.Name == roleName) > 0;
      }

      public override void AddRole(string roleName) {
         
         this.roles.Add(new UserRole { Name = roleName });
         this.context.SaveChanges();
      }

      public override void DeleteRole(string roleName) {

         UserRole role = this.roles.SingleOrDefault(r => r.Name == roleName);

         if (role != null) {
            this.roles.Remove(role);
            this.context.SaveChanges();
         }
      }
   }
}
