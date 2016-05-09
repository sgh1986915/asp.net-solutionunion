using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion {
   
   public class UserSignUpPaymentResult : SuccessfulResult {

      public Func<OperationResult> Login { get; private set; }

      public UserSignUpPaymentResult(Func<OperationResult> login, string message) 
         : base(message) {

         this.Login = login;
      }
   }
}
