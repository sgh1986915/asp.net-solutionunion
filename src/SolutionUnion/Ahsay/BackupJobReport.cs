using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SolutionUnion.Ahsay {
   
   public class BackupJobReport : SuccessfulResult {

      public string ID { get; set; }
      public string BackupJobStatus { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime? EndTime { get; set; }

      public long NumOfNewFiles { get; set; }
      public long NumOfUpdatedFiles { get; set; }
      public long NumOfUpdatedPermissionFiles { get; set; }
      public long NumOfDeletedFiles { get; set; }
      public long NumOfMovedFiles { get; set; }

      public double TotalNewFilesSize { get; set; }
      public double TotalUpdatedFilesSize { get; set; }
      public double TotalUpdatedPermissionFileSize { get; set; }
      public double TotalDeletedFilesSize { get; set; }
      public double TotalMovedFilesSize { get; set; }

      public Collection<BackupJobReportLogEntry> LogEntries { get; private set; }
      public Collection<BackupJobReportNewFileEntry> NewFiles { get; private set; }
      public Collection<BackupJobReportUpdatedFileEntry> UpdatedFiles { get; private set; }
      public Collection<BackupJobReportUpdatedPermissionFileEntry> UpdatedPermissionFiles { get; private set; }
      public Collection<BackupJobReportMovedFileEntry> MovedFiles { get; private set; }
      public Collection<BackupJobReportDeletedFileEntry> DeletedFiles { get; private set; }

      // Solution Union members
      public long ServerId { get; set; }
      public string AccountName { get; set; }
      public long SetId { get; set; }
      public string SetName { get; set; }
      public string JobStatusDescription { get; set; }

      public bool FinishedSuccessfully {
         get {
            return BackupJobStatus == "BS_STOP_SUCCESS";
         }
      }

      public BackupJobReport() {

         this.LogEntries = new Collection<BackupJobReportLogEntry>();
         this.NewFiles = new Collection<BackupJobReportNewFileEntry>();
         this.UpdatedFiles = new Collection<BackupJobReportUpdatedFileEntry>();
         this.UpdatedPermissionFiles = new Collection<BackupJobReportUpdatedPermissionFileEntry>();
         this.MovedFiles = new Collection<BackupJobReportMovedFileEntry>();
         this.DeletedFiles = new Collection<BackupJobReportDeletedFileEntry>();
      }
   }

   public class BackupJobReportLogEntry {
      public string Type { get; set; }
      public DateTime TimeStamp { get; set; }
      public string Message { get; set; }
   }

   public abstract class BackupJobReportFileEntry {

      public DateTime LastModified { get; set; }
      public double FileSize { get; set; }
      public double UnzipFilesSize { get; set; }
      public string Ratio { get; set; }
   }

   public class BackupJobReportNewFileEntry : BackupJobReportFileEntry {
      public string Name { get; set; }      
   }

   public class BackupJobReportUpdatedFileEntry : BackupJobReportFileEntry {
      public string Name { get; set; }
   }

   public class BackupJobReportUpdatedPermissionFileEntry : BackupJobReportFileEntry {
      public string Name { get; set; }
   }

   public class BackupJobReportDeletedFileEntry : BackupJobReportFileEntry {
      public string Name { get; set; }
   }

   public class BackupJobReportMovedFileEntry : BackupJobReportFileEntry {
      public string FromFile { get; set; }
      public string ToFile { get; set; }
   }
}
