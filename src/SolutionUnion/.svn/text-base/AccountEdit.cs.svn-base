using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.ObjectModel;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using System.ComponentModel;

namespace SolutionUnion {
   
   public class AccountEdit {

      public bool IsAcb { get; set; }

      [Required(ErrorMessage = "Please select a Server.")]
      [UIHint("DropDownList")]
      [AdditionalMetadata("DropDownList.OmitOptionLabel", null)]
      [DisplayName("Server")]
      public long ServerId { get; set; }

      [Required(ErrorMessage = "Account Name Is Required.")]
      [Remote("_AccountAvailability", "~Account", AdditionalFields = "IsAcb,ServerId", ErrorMessage = "Account Name not available")]
      [DisplayName("Account Name")]
      public string AccountName { get; set; }

      [Required(ErrorMessage = "TimeZone is required.")]
      [UIHint("DropDownList")]
      [DisplayName("Time Zone")]
      public string TimeZone { get; set; }

      [Required(ErrorMessage = "Vault Size is required")]
      [Range(0.0001, 1000, ErrorMessage = "Size should between 0 and 1000.")]
      [Remote("_VaultSize", "~Account", AdditionalFields = "IsAcb")]
      [DisplayName("Vault Size")]
      public decimal VaultSize { get; set; }

      public bool IndividualMicrosoftExchange { get; set; }

      [Range(0, Int32.MaxValue, ErrorMessage = "Number of mailboxes can not be less than Zero.")]
      public int NumberOfIndividualMicrosoftExchangeMailboxes { get; set; }

      [Required(ErrorMessage = "Password is required.")]
      [StringLength(250)]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [Required(ErrorMessage = "Please confirm password.")]
      [Compare("Password", ErrorMessage = "Passwords must match.")]
      [DataType(DataType.Password)]
      [DisplayName("Confirm Password")]
      public string ConfirmPassword { get; set; }

      [Required(ErrorMessage = "Contact Name is required.")]
      [DisplayName("Contact Name")]
      public string ContactName { get; set; }

      [Required(ErrorMessage = "Contact Email is required..")]
      [RegularExpression(@"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$", ErrorMessage = "Please enter a valid email address.")]
      public string Email { get; set; }

      [UIHint("DropDownList")]
      [DisplayName("Account Type")]
      public AccountType AccountType { get; set; }

      public Collection<AccountEditContact> AdditionalContacts { get; private set; }

      public SelectList GetServerList() {

         var serverRepo = Repository<Server>.GetInstance();
         
         var servers = serverRepo.CreateQuery().Where(s => s.IsAcb == this.IsAcb).ToList();

         long defaultValue;

         if (servers.Any(s => s.Id == this.ServerId))
            defaultValue = this.ServerId;
         else
            defaultValue = servers.Where(s => s.IsDefault).Select(s => s.Id).FirstOrDefault();

         var list = new SelectList(servers, "Id", "Description", defaultValue);

         return list;
      }

      public AccountEdit() {
         this.AdditionalContacts = new Collection<AccountEditContact>();
      }
   }

   public class AccountEditContact {

      public long Id { get; set; }

      [Required(ErrorMessage = "Contact Name is required.")]
      [DisplayName("Contact Name")]
      public string Name { get; set; }
      
      [Required(ErrorMessage = "Contact Email is required..")]
      [RegularExpression(@"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$", ErrorMessage = "Please enter a valid email address.")]
      public string Email { get; set; }
   }
}
