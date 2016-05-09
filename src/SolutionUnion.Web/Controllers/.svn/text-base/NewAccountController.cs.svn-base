using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcCodeRouting;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers {

   [Authorize(Roles = UserRole.Administrator)]
   public class NewAccountController : ApplicationController {

      [HttpGetHead]
      public ActionResult List(string az, string search, long? serverId, string sort = "AccountName", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<NewAccountReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         var query = reporter.GetNewAccounts(az, search, serverId);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.az = new SelectList(NewAccountReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.serverId = new SelectList(NewAccountReporter.GetServerList(), "Key", "Value", serverId);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpGetHead]
      public ActionResult AssignUser([FromRoute]long id, string az, string type, string search, int start = 0) {

         var reporter = ServiceLocator.Current.GetInstance<UserReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         const int pageSize = 10;

         var query = reporter.GetUsers(az: az, role: type, keyword: search);

         this.ViewData.Model = query.OrderBy(u => u.UserId).Skip(start).Take(pageSize).ToList();
         this.ViewBag.AccountID = id;
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();
         this.ViewBag.az = new SelectList(UserReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.type = new SelectList(UserReporter.GetRoleList(), "Key", "Value", az);
         this.ViewBag.search = search;
         this.ViewBag.start = start;

         return PartialView();
      }

      [HttpPost]
      public ActionResult AssignUser([FromRoute]long id, long userId) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.AssignUser(userId);

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Convert([FromRoute]long id) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Convert();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Activate([FromRoute]long id) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Activate();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Suspend([FromRoute]long id) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Suspend();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Delete([FromRoute]long id) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Delete();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpGetHead]
      public ActionResult BackupJobReport([FromRoute]long id) {

         var account = SolutionUnion.NewAccount.Find(id);

         if (account == null) 
            return Error(HttpStatusCode.NotFound, "Account Not Found.");

         var result = account.GetBackupJobReport();

         if (result.IsError)
            return Error(result);

         this.ViewData.Model = result;

         return View();
      }

      [HttpGetHead]
      public ActionResult BackupJobReportSample() {

         var result = new Ahsay.BackupJobReport {
            AccountName = "foo",
            ServerId = 5,
            SetId = 168546543,
            SetName = "yyy",
            JobStatusDescription = "Completed",
            BackupJobStatus = "OK",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddMinutes(-30),
            ID = "654654",
            NumOfDeletedFiles = 50,
            NumOfMovedFiles = 40,
            NumOfNewFiles = 10,
            NumOfUpdatedFiles = 3,
            NumOfUpdatedPermissionFiles = 0,
            TotalDeletedFilesSize = 2545645435454654,
            TotalMovedFilesSize = 46354355435435,
            TotalNewFilesSize = 3543545454,
            TotalUpdatedFilesSize = 343434334,
            TotalUpdatedPermissionFileSize = 0,
            LogEntries = { 
               new Ahsay.BackupJobReportLogEntry {
                  Message = "AA!!",
                  Type = "Info",
                  TimeStamp = DateTime.Now.AddHours(-1)
               },
               new Ahsay.BackupJobReportLogEntry {
                  Message = "AAQQQ!!",
                  Type = "Warning",
                  TimeStamp = DateTime.Now.AddHours(-1).AddMinutes(-5)
               }
            }
         };

         foreach (var i in Enumerable.Range(0, (int)result.NumOfDeletedFiles)) {
            result.DeletedFiles.Add(
               new Ahsay.BackupJobReportDeletedFileEntry {
                  FileSize = i * 1024 * 2,
                  LastModified = DateTime.Now.AddMinutes(-1 * i),
                  Name = i.ToString() + "file",
                  Ratio = "XX",
                  UnzipFilesSize = i * 1024
               }
            );
         }

         foreach (var i in Enumerable.Range(0, (int)result.NumOfMovedFiles)) {
            result.MovedFiles.Add(
               new Ahsay.BackupJobReportMovedFileEntry {
                  FileSize = i * 1024 * 2,
                  LastModified = DateTime.Now.AddMinutes(-1 * i),
                  FromFile = i.ToString() + "file",
                  ToFile = i.ToString() + "file.bak",
                  Ratio = "XX",
                  UnzipFilesSize = i * 1024
               }
            );
         }

         foreach (var i in Enumerable.Range(0, (int)result.NumOfNewFiles)) {
            result.NewFiles.Add(
               new Ahsay.BackupJobReportNewFileEntry {
                  FileSize = i * 1024 * 2,
                  LastModified = DateTime.Now.AddMinutes(-1 * i),
                  Name = i.ToString() + "file",
                  Ratio = "XX",
                  UnzipFilesSize = i * 1024
               }
            );
         }

         foreach (var i in Enumerable.Range(0, (int)result.NumOfUpdatedFiles)) {
            result.UpdatedFiles.Add(
               new Ahsay.BackupJobReportUpdatedFileEntry {
                  FileSize = i * 1024 * 2,
                  LastModified = DateTime.Now.AddMinutes(-1 * i),
                  Name = i.ToString() + "file",
                  Ratio = "XX",
                  UnzipFilesSize = i * 1024
               }
            );
         }

         this.ViewData.Model = result;

         return View("BackupJobReport");
      }
   }
}