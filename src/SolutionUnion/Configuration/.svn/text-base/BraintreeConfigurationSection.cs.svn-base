using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SolutionUnion.Configuration {
   
   public class BraintreeConfigurationSection : ConfigurationSection {

      static readonly ConfigurationProperty _MerchantIdProperty = 
         new ConfigurationProperty("merchantId", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
      
      static readonly ConfigurationProperty _PublicKeyProperty = 
         new ConfigurationProperty("publicKey", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
      
      static readonly ConfigurationProperty _PrivateKeyProperty = 
         new ConfigurationProperty("privateKey", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

      static readonly ConfigurationPropertyCollection _Properties = 
         new ConfigurationPropertyCollection { 
            _MerchantIdProperty,
            _PublicKeyProperty,
            _PrivateKeyProperty
         };

      static BraintreeConfigurationSection _Instance;
      static readonly object padlock = new object();
      public static readonly string SectionName = "braintree";

      public static BraintreeConfigurationSection Instance {
         get {
            if (_Instance == null) {
               lock (padlock) {
                  if (_Instance == null) {

                     var configSection = ConfigurationManager.GetSection(SectionName) as BraintreeConfigurationSection;

                     if (configSection == null)
                        throw new InvalidOperationException();

                     _Instance = configSection;
                  }
               }
            }
            return _Instance;
         }
      }

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public string MerchantId {
         get {
            return (string)base[_MerchantIdProperty];
         }
      }

      public string PublicKey {
         get {
            return (string)base[_PublicKeyProperty];
         }
      }

      public string PrivateKey {
         get {
            return (string)base[_PrivateKeyProperty];
         }
      }
   }
}
