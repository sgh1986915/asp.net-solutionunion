using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion {
   
   public class UserSignUpPaymentError : ErrorResult {

      public long SignUpId { get; private set; }

      public UserSignUpPaymentError(long signUpId, string message) 
         : base(message) {

         this.SignUpId = signUpId;
      }
   }
}
