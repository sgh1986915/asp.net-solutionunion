using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SolutionUnion.EntityFramework {
   
   public class SolutionUnionContext : DbContext {

      public DbSet<User> Users { get; set; }
      public DbSet<UserRole> UserRoles { get; set; }
      public DbSet<Country> Countries { get; set; }
      public DbSet<State> States { get; set; }
      public DbSet<Invoice> Invoices { get; set; }
      public DbSet<InvoicePdf> InvoicePdfs { get; set; }
      public DbSet<Account> Accounts { get; set; }
      public DbSet<DailyUsage> DailyUsages { get; set; }
      public DbSet<Server> Servers { get; set; }
      public DbSet<NewAccount> NewAccounts { get; set; }
      public DbSet<Contact> Contacts { get; set; }
      public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
      public DbSet<ApplicationSettingsStoragePrice> ApplicationSettingsStoragePrices { get; set; }
      public DbSet<HomeDirectory> HomeDirectories { get; set; }
      public DbSet<Session> Sessions { get; set; }
      public DbSet<Invitation> Invitations { get; set; }
      public DbSet<Service> Services { get; set; }
      public DbSet<SignUp> SignUps { get; set; }
      public DbSet<CreditCard> CreditCards { get; set; }

      public SolutionUnionContext(string connectionName)
         : base(connectionName) {

         // Validation must be invoked in the Domain layer.
         // Existing data in the database can be invalid according to the domain layer,
         // but that cannot prevent the saving of changes.
         this.Configuration.ValidateOnSaveEnabled = false;
      }

      protected override void OnModelCreating(DbModelBuilder modelBuilder) {
         
         base.OnModelCreating(modelBuilder);
      }
   }
}
