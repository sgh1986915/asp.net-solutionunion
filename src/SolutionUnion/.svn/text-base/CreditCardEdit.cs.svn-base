using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;
using SolutionUnion.Resources.Validation;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using System.Globalization;

namespace SolutionUnion {
   
   public class CreditCardEdit {

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(30, MinimumLength = 3)]
      [DisplayName("Name on Card")]
      public string NameOnCard { get; set; }

      [DisplayName("Credit Card Number")]
      public string CreditCardNumber { get; set; }

      [DisplayName("CVV")]
      public string CreditCardCVV { get; set; }

      public string Expiry { get; set; } // for metadata only, ExpiryMonth and ExpiryYear are used

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [UIHint("DropDownList")]
      [AdditionalMetadata("DropDownList.OptionLabel", "Month")]
      public int ExpiryMonth { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [UIHint("DropDownList")]
      [AdditionalMetadata("DropDownList.OptionLabel", "Year")]
      public int ExpiryYear { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(41, MinimumLength = 6)]
      [DisplayName("Billing Address")]
      public string BillingAddress { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(31, MinimumLength = 3)]
      [DisplayName("City / Province")]
      public string City { get; set; }

      [DisplayName("State")]
      [UIHint("DropDownList")]
      public string State { get; set; }

      [DisplayName("Country")]
      [UIHint("DropDownList")]
      public string Country { get; set; }

      [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(ValidationRes))]
      [StringLength(13, MinimumLength = 3)]
      [DisplayName("Zip")]
      public string PostalCode { get; set; }

      [DisplayName("Make Default")]
      public bool MakeDefault { get; set; }

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

      public CreditCardEdit() {

         this.Country = "US";
         this.MakeDefault = true;
      }
   }
}
