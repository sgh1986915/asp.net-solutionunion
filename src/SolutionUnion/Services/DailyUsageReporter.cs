using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;
using System.Globalization;

namespace SolutionUnion.Services {
   
   public class DailyUsageReporter {

      readonly Repository<Account> repo;

      public static IDictionary<string, string> GetMonths() {

         DateTime thisMonth = DateTime.Today.AddDays((DateTime.Today.Day - 1) * -1);

         DateTimeFormatInfo dateInfo = CultureInfo.CurrentUICulture.DateTimeFormat;

         var months = from i in Enumerable.Range(0, 2)
                      let m = thisMonth.AddMonths(i * -1)
                      select new { 
                        Key = m.ToString("yyyy-MM-dd"),
                        Value = dateInfo.GetMonthName(m.Month)
                      };

         return months.ToDictionary(m => m.Key, m => m.Value);
      }

      public DailyUsageReporter(Repository<Account> repo) {
         this.repo = repo;
      }

      public IQueryable<DailyUsageReportItem> GetReport(long accountId, DateTime? month = null) {

         if (month == null)
            month = DateTime.Today.AddDays((DateTime.Today.Day - 1) * -1);

         long userId = User.CurrentUserId;

         var nextMonth = month.Value.AddMonths(1);

         var query = from a in repo.CreateQuery()
                     where a.Id == accountId
                     from da in a.DailyUsages
                     where da.Date >= month.Value && da.Date < nextMonth
                     orderby da.Date descending
                     select new DailyUsageReportItem { 
                        Id = da.Id,
                        Date = da.Date,
                        DataSize = da.DataSize,
                        NumberOfIndividualMicrosoftExchangeMailboxes = da.NumberOfIndividualMicrosoftExchangeMailboxes,
                        RetainSize = da.RetainSize,
                        TotalSize = da.TotalSize
                     };

         return query;
      }
   }
}
