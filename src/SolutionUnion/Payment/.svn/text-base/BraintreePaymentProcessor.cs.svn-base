using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Braintree;
using System.Globalization;
using System.Net;

namespace SolutionUnion.Payment {
   
   public class BraintreePaymentProcessor : PaymentProcessor {

      readonly BraintreeGateway gateway = BraintreeGatewayFactory.Create();

      public override UserSignUpPaymentInput CreateUserSignUpPaymentInput(SignUp signUp, Invoice invoice, string confirmUrl) {

         var input = new UserSignUpPaymentInput {
            HiddenFields = { 
               { 
                  "tr_data",  
                  this.gateway.Transaction.SaleTrData(new TransactionRequest {
                     Amount = invoice.Total,
                     CustomFields = { 
                        { "signup_id", signUp.Id.ToString(CultureInfo.InvariantCulture) }
                     },
                     Options = new TransactionOptionsRequest { 
                        StoreInVaultOnSuccess = true
                     }
                  }, confirmUrl)
               }
            }
         };

         input.Metadata.PaymentUrl = this.gateway.TransparentRedirect.Url;

         input.Metadata.CardHolderNameField = "transaction[credit_card][cardholder_name]";
         input.Metadata.CardNumberField = "transaction[credit_card][number]";
         input.Metadata.CvvField = "transaction[credit_card][cvv]";
         input.Metadata.ExpiryMonthField = "transaction[credit_card][expiration_month]";
         input.Metadata.ExpiryYearField = "transaction[credit_card][expiration_year]";
         
         input.Metadata.AddressField = "transaction[billing][street_address]";
         input.Metadata.CityField = "transaction[billing][locality]";
         input.Metadata.StateField = "transaction[billing][region]";
         input.Metadata.PostalCodeField = "transaction[billing][postal_code]";
         input.Metadata.CountryField = "transaction[billing][country_code_alpha2]";

         return input;
      }

      public override OperationResult ConfirmUserSignUpPayment(string query) {

         Result<Braintree.Transaction> txResult = null;

         try {
            txResult = this.gateway.TransparentRedirect.ConfirmTransaction(query);
         
         } catch (Braintree.Exceptions.NotFoundException) { }

         if (txResult == null)
            return new ErrorResult(HttpStatusCode.NotFound, "Transaction Not Found.");

         var tx = txResult.Target;

         long? signUpId = null;

         if (tx != null && tx.CustomFields.ContainsKey("signup_id"))
            signUpId = Int64.Parse(tx.CustomFields["signup_id"], CultureInfo.InvariantCulture);

         else if (txResult.Parameters != null && txResult.Parameters.ContainsKey("transaction[custom_fields][signup_id]"))
            signUpId = Int64.Parse(txResult.Parameters["transaction[custom_fields][signup_id]"], CultureInfo.InvariantCulture);

         if (signUpId == null)
            return new ErrorResult(txResult.Message);

         if (!(txResult.IsSuccess()
            && tx != null
            && tx.Status == Braintree.TransactionStatus.AUTHORIZED))
            return new UserSignUpPaymentError(signUpId.Value, txResult.Message);

         var billingAddress = tx.BillingAddress;

         var result = new UserSignUpTransactionResult { 
            SignUpId = signUpId.Value,
            TransactionId = tx.Id,
            CustomerId = tx.Customer.Id,
            CreditCard = new CreditCard {
               Token = tx.CreditCard.Token,
               Type = tx.CreditCard.CardType.ToString(),
               LastFour = tx.CreditCard.LastFour,
               ExpirationMonth = Int32.Parse(tx.CreditCard.ExpirationMonth, CultureInfo.InvariantCulture),
               ExpirationYear = Int32.Parse(tx.CreditCard.ExpirationYear, CultureInfo.InvariantCulture),
               NameOnCard = tx.CreditCard.CardholderName,
               Address = billingAddress.StreetAddress,
               City = billingAddress.Locality,
               PostalCode = billingAddress.PostalCode,
               State = billingAddress.Region,
               Country = billingAddress.CountryCodeAlpha2
            }
         };

         return result;
      }

      public override OperationResult Pay(Invoice invoice, CreditCard card) {

         var request = new TransactionRequest {
            Amount = invoice.Total,
            CustomerId = invoice.User.CustomerId,
            PaymentMethodToken = card.Token,
            OrderId = invoice.Id.ToString(CultureInfo.InvariantCulture)
         };

         var result = this.gateway.Transaction.Sale(request);
         var tx = result.Target;

         if (!(result.IsSuccess()
            && tx != null
            && tx.Status == Braintree.TransactionStatus.AUTHORIZED))
            return new ErrorResult(result.Message);

         return new PaymentResult(tx.Id);
      }

      // Credit cards

      public override OperationResult AddCreditCard(string customerId, CreditCardEdit input) {

         var request = new CreditCardRequest {
            CustomerId = customerId,
            Number = input.CreditCardNumber,
            ExpirationMonth = input.ExpiryMonth.ToString("00", CultureInfo.InvariantCulture),
            ExpirationYear = input.ExpiryYear.ToString(CultureInfo.InvariantCulture),
            CardholderName = input.NameOnCard,
            CVV = input.CreditCardCVV,
            BillingAddress = new CreditCardAddressRequest {
               StreetAddress = input.BillingAddress,
               Locality = input.City,
               Region = input.State,
               PostalCode = input.PostalCode,
               CountryCodeAlpha2 = input.Country
            }
         };

         var result = this.gateway.CreditCard.Create(request);

         var target = result.Target;

         if (!result.IsSuccess()
            || target == null)
            return new ErrorResult(result.Message);

         return new AddCreditCardResult(new CreditCard { 
            Address = input.BillingAddress,
            City = input.City,
            Country = input.Country,
            ExpirationMonth = input.ExpiryMonth,
            ExpirationYear = input.ExpiryYear,
            LastFour = target.LastFour,
            NameOnCard = input.NameOnCard,
            PostalCode = input.PostalCode,
            State = input.State,
            Token = target.Token,
            Type = target.CardType.ToString()
         });
      }

      public override OperationResult UpdateCreditCard(string token, CreditCardEdit input) {

         var request = new CreditCardRequest {
            CardholderName = input.NameOnCard,
            ExpirationMonth = input.ExpiryMonth.ToString("00", CultureInfo.InvariantCulture),
            ExpirationYear = input.ExpiryYear.ToString(CultureInfo.InvariantCulture),
            BillingAddress = new CreditCardAddressRequest {
               StreetAddress = input.BillingAddress,
               Locality = input.City,
               Region = input.State,
               PostalCode = input.PostalCode,
               CountryCodeAlpha2 = input.Country,
               Options = new CreditCardAddressOptionsRequest {
                  UpdateExisting = true
               }
            }
         };

         if (input.CreditCardNumber.HasValue())
            request.Number = input.CreditCardNumber;

         if (input.CreditCardCVV.HasValue())
            request.CVV = input.CreditCardCVV;

         Result<Braintree.CreditCard> result = null;

         try {
            result = this.gateway.CreditCard.Update(token, request);
         } catch (Braintree.Exceptions.NotFoundException) { }

         if (result == null)
            return new ErrorResult(HttpStatusCode.NotFound, "Credit Card Not Found.");

         var target = result.Target;

         if (!result.IsSuccess()
            || target == null)
            return new ErrorResult(result.Message);

         return new UpdateCreditCardResult { 
            LastFour = target.LastFour,
            Type = target.CardType.ToString()
         };
      }

      public override OperationResult MakeCreditCardDefault(string token) {

         var request = new CreditCardRequest {
            Options = new CreditCardOptionsRequest {
               MakeDefault = true
            }
         };

         Result<Braintree.CreditCard> result = null;

         try {
            result = this.gateway.CreditCard.Update(token, request);
         } catch (Braintree.Exceptions.NotFoundException) { }

         if (result == null)
            return new ErrorResult(HttpStatusCode.NotFound, "Credit Card Not Found.");

         if (!result.IsSuccess())
            return new ErrorResult(result.Message);

         return new SuccessfulResult();
      }

      public override OperationResult DeleteCreditCard(string token) {

         gateway.CreditCard.Delete(token);

         return new SuccessfulResult();
      }
   }
}
