using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using System.Net;
using System.Linq.Dynamic;
using MvcCodeRouting;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers {
   
   [Authorize(Roles = UserRole.Administrator + "," + UserRole.Reseller)]
   public class ReportingController : ApplicationController {

      public ActionResult Index(DateTime? date
         , bool all = false
         , bool portalLog = false
         , string portalLogSort = "Created DESC"
         , bool failedJobs = true
         , string failedJobsSort = "JobDate DESC"
         , bool successfulJobs = true
         , string successfulJobsSort = "JobDate DESC"
         , long? accountId = null
         , string search = null) {

         if (date == null)
            date = DateTime.Today;

         var service = ServiceLocator.Current.GetInstance<PortalReporter>();

         var result = service.GetReport(date.Value, portalLog, failedJobs, successfulJobs, accountId, search);

         if (result.IsError)
            return Error(result);

         var resultOK = (PortalReport)result;

         resultOK.PortalLog = resultOK.PortalLog.AsQueryable().OrderBy(portalLogSort).ToList();
         resultOK.FailedJobs = resultOK.FailedJobs.AsQueryable().OrderBy(failedJobsSort).ToList();
         resultOK.SuccessfulJobs = resultOK.SuccessfulJobs.AsQueryable().OrderBy(successfulJobsSort).ToList();

         this.ViewData.Model = result;
         this.ViewBag.date = new SelectList(PortalReporter.GetDates(), "Key", "Value", (date != null) ? date.Value.ToString("yyyy-MM-dd") : null);
         this.ViewBag.all = all;
         this.ViewBag.portalLog = portalLog;
         this.ViewBag.portalLogSort = portalLogSort;
         this.ViewBag.failedJobs = failedJobs;
         this.ViewBag.failedJobsSort = failedJobsSort;
         this.ViewBag.successfulJobs = successfulJobs;
         this.ViewBag.successfulJobsSort = successfulJobsSort;
         this.ViewBag.search = search;

         if (this.Request.IsAjaxRequest()) 
            return PartialView();

         return View();
      }

      public ActionResult BackupJobReport([FromRoute]long id) {

         var summary = SolutionUnion.BackupJobReportSummary.Find(id);

         if (summary == null)
            return Error(HttpStatusCode.NotFound, "BackupJobReportSummary Not Found.");

         var result = summary.GetBackupJobReport();

         if (result.IsError)
            return Error(result);

         this.ViewData.Model = result;

         return View();
      }
   }
}