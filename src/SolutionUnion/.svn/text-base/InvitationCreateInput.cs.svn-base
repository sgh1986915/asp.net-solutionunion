using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SolutionUnion {
   
   public class InvitationCreateInput : InvitationEdit {

      [Remote("Add_EmailAvailability", "~User.Invitation")]
      public override string Email {
         get {
            return base.Email;
         }
         set {
            base.Email = value;
         }
      }

      [Remote("Add_CompanyNameAvailability", "~User.Invitation")]
      public override string CompanyName {
         get {
            return base.CompanyName;
         }
         set {
            base.CompanyName = value;
         }
      }
   }
}
