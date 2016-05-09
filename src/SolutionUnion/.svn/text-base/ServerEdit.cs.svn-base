using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Web.Mvc;
using System.ComponentModel;

namespace SolutionUnion {
   
   public class ServerEdit {

      public ServerType Type { get; set; }

      [Required(ErrorMessage = "IP address is required.")]
      public string OrIpUrl { get; set; }

      [Required(ErrorMessage = "Description is required.")]
      public string Description { get; set; }

      [Required(ErrorMessage = "Username is required.")]
      public string UserName { get; set; }

      [Required(ErrorMessage = "Password is required.")]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [Required(ErrorMessage = "Please confirm password.")]
      [Compare("Password", ErrorMessage = "Passwords must match.")]
      [DataType(DataType.Password)]
      public string ConfirmPassword { get; set; }

      [Required(ErrorMessage = "Home Directory is required.")]
      public string HomeDirectory { get; set; }

      public Collection<ServerEditHomeDirectory> AdditionalHomeDirectories { get; private set; }

      public ServerEdit() {
         this.AdditionalHomeDirectories = new Collection<ServerEditHomeDirectory>();
      }
   }

   public class ServerEditHomeDirectory {

      public long Id { get; set; }

      [Required(ErrorMessage = "Home Directory is required.")]
      public string HomeDirectory { get; set; }
   }
}
