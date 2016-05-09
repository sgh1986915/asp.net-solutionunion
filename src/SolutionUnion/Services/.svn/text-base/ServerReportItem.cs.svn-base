using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionUnion.Services {
   
   public class ServerReportItem {
      
      public long Id { get; set; }
      public string Description { get; set; }
      public string OrIpUrl { get; set; }
      public bool IsAcb { get; set; }
      public bool IsDefault { get; set; }

      public ServerType Type {
         get {
            return (IsAcb) ? ServerType.Lite : ServerType.Pro;
         }
      }
   }
}
