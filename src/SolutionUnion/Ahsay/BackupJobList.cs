using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Ahsay {
   
   public class BackupJobList : SuccessfulResult {

      public Collection<BackupJobListBackupSet> BackupSets { get; private set; }

      public BackupJobList() {
         this.BackupSets = new Collection<BackupJobListBackupSet>();
      }
   }

   public class BackupJobListBackupSet {

      public long ID { get; set; }
      public Collection<BackupJobListBackupJob> BackupJobs { get; private set; }

      public BackupJobListBackupSet() {
         this.BackupJobs = new Collection<BackupJobListBackupJob>();
      }
   }

   public class BackupJobListBackupJob {

      DateTime? _JobDate;

      public string ID { get; set; }

      public DateTime JobDate { 
         get {
            if (_JobDate == null) {
               string[] jobIdComponent = ID.Split('-');

               _JobDate = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}", jobIdComponent[0], jobIdComponent[1], jobIdComponent[2], jobIdComponent[3], jobIdComponent[4], jobIdComponent[5]));
            }
            return _JobDate.Value;
         }
      }
   }
}
