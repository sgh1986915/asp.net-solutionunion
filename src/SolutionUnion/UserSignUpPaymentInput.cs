using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SolutionUnion.Resources.Validation;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class UserSignUpPaymentInput {

      public IDictionary<string, string> HiddenFields { get; private set; }

      public string Country { get; set; }

      public UserSignUpPaymentInputMetadata Metadata { get; private set; }

      public static IDictionary<string, string> GetMonths() {

         return CultureInfo.CurrentUICulture
            .DateTimeFormat
            .MonthNames
            .Where(m => m.HasValue())
            .Select((monthName, index) => new { Value = (index + 1).ToString(), Text = monthName })
            .ToDictionary(p => p.Value, p => p.Text);
      }

      public static IDictionary<string, string> GetYears() {

         int thisYear = DateTime.Today.Year;

         return Enumerable.Range(0, 18).ToDictionary(i => (thisYear + i).ToString(), i => (thisYear + i).ToString());
      }

      public static IDictionary<string, string> GetCountries() {

         var repo = Repository<Country>.GetInstance();

         return
            (from c in repo.CreateQuery()
             orderby c.Code == "US" descending, c.Name
             select c)
            .ToDictionary(c => c.Code, c => c.Name);
      }

      public static IDictionary<string, string> GetStates(string countryCode) {

         var repo = Repository<Country>.GetInstance();

         Country country = repo.CreateQuery().SingleOrDefault(c => c.Code == countryCode);

         if (country == null)
            return new Dictionary<string, string>();

         return country.GetStates().ToDictionary(s => s.Code, s => s.Name);
      }

      public UserSignUpPaymentInput() {

         this.HiddenFields = new Dictionary<string, string>();
         this.Country = "US";
         this.Metadata = new UserSignUpPaymentInputMetadata();
      }
   }

   public class UserSignUpPaymentInputMetadata {
      
      public string CardHolderNameField { get; set; }
      public string CardNumberField { get; set; }
      public string ExpiryMonthField { get; set; }
      public string ExpiryYearField { get; set; }
      public string CvvField { get; set; }
      public string AddressField { get; set; }
      public string CityField { get; set; }
      public string StateField { get; set; }
      public string PostalCodeField { get; set; }
      public string CountryField { get; set; }
      
      public string PaymentUrl { get; set; }
   }
}
