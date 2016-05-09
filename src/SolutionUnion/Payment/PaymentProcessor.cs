using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Payment {
   
   public abstract class PaymentProcessor {

      public abstract UserSignUpPaymentInput CreateUserSignUpPaymentInput(SignUp signUp, Invoice invoice, string confirmUrl);
      public abstract OperationResult ConfirmUserSignUpPayment(string query);

      public abstract OperationResult Pay(Invoice invoice, CreditCard card);

      // Credit cards

      public abstract OperationResult AddCreditCard(string customerId, CreditCardEdit input);
      public abstract OperationResult UpdateCreditCard(string token, CreditCardEdit input);
      public abstract OperationResult MakeCreditCardDefault(string token);
      public abstract OperationResult DeleteCreditCard(string token);
   }
}
