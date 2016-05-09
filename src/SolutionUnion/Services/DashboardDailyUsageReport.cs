using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Services {
   
   public class DashboardDailyUsageReport {

      public Collection<DashboardDailyUsageReportItem> ProUsage { get; private set; }
      public Collection<DashboardDailyUsageReportItem> LiteUsage { get; private set; }
      public DateTime StartDate { get; internal set; }
      public DateTime EndDate { get; internal set; }
      public string Unit { get; set; }

      public DashboardDailyUsageReport() {

         this.ProUsage = new Collection<DashboardDailyUsageReportItem>();
         this.LiteUsage = new Collection<DashboardDailyUsageReportItem>();
      }
   }

   public class DashboardDailyUsageReportItem {
      public DateTime Date { get; set; }
      public decimal Usage { get; set; }
   }
}
