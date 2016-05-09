using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Resources.Validation;

namespace SolutionUnion {
   
   public class UserProfile {

      // User Info

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(255)]
      [RegularExpression(@"\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Please enter a valid email address.")]
      [DisplayName("Email Address")]
      public virtual string Email { get; set; }

      // Personal Info

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(41, MinimumLength = 3)]
      [DisplayName("Company")]
      public virtual string CompanyName { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(25)]
      [DisplayName("First Name")]
      public string FirstName { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(25)]
      [DisplayName("Last Name")]
      public string LastName { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(21, MinimumLength = 6)]
      [DisplayName("Phone Number")]
      public string Phone { get; set; }
   }
}
