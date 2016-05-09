using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Ahsay;
using System.Collections.ObjectModel;

namespace SolutionUnion.Services {
   
   public class ActionItemReport {

      public string UserCompanyName { get; set; }
      public Collection<BackupJobReport> Jobs { get; private set; }

      public ActionItemReport() {
         this.Jobs = new Collection<BackupJobReport>();
      }
   }
}
