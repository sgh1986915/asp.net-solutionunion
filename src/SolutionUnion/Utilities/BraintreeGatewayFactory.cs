using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Braintree;
using SolutionUnion.Configuration;

namespace SolutionUnion {
   
   static class BraintreeGatewayFactory {

      public static BraintreeGateway Create() {

         var config = BraintreeConfigurationSection.Instance;

         return new BraintreeGateway { 
            MerchantId = config.MerchantId,
            PrivateKey = config.PrivateKey,
            PublicKey = config.PublicKey,
            Environment = Braintree.Environment.SANDBOX
         };
      }
   }
}
