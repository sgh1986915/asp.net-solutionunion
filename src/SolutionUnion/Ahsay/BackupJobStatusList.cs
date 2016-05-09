using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Ahsay {
   
   public class BackupJobStatusList : SuccessfulResult {

      public DateTime BackupDate { get; set; }
      public Collection<BackupJobStatusListBackupJob> BackupJobs { get; private set; }

      public BackupJobStatusList() {
         this.BackupJobs = new Collection<BackupJobStatusListBackupJob>();
      }
   }

   public class BackupJobStatusListBackupJob {

      public string ID { get; set; }
      public string LoginName { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime? EndTime { get; set; }
      public string BackupJobStatus { get; set; }
      public long BackupSetID { get; set; }
      public string BackupSetName { get; set; }
      public double UploadSize { get; set; }
      public string RunVersion { get; set; }
   }
}
