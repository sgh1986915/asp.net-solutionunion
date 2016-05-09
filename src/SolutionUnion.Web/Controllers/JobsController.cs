using System;
using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers {
   
   public class JobsController : ApplicationController {

      protected override void OnAuthorization(AuthorizationContext filterContext) {

         if (!this.Request.IsLocal) {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            filterContext.Result = Content("");
         }
      }
      
      [HttpPost]
      public ActionResult ImportNewAccounts() {

         var service = ServiceLocator.Current.GetInstance<NewAccountImporter>();
         var result = service.Import();

         this.Response.StatusCode = (int)result.StatusCode;

         return Content(result.Message);
      }

      [HttpPost]
      public ActionResult Daily() {

         var service = ServiceLocator.Current.GetInstance<DatabaseMaintenanceService>();
         service.Run();

         return new EmptyResult();
      }

      [HttpPost]
      public ActionResult ActionItemReport() {

         var service = ServiceLocator.Current.GetInstance<ActionItemReporter>();
         var result = service.Run(this);

         this.Response.StatusCode = (int)result.StatusCode;

         return Content(result.Message);
      }

      [HttpPost]
      public ActionResult SynchronizeAccounts() {

         var service = ServiceLocator.Current.GetInstance<AccountSynchronizer>();
         var result = service.SynchronizeAll();

         this.Response.StatusCode = (int)result.StatusCode;

         return Content(result.Message);
      }

      [HttpPost]
      public ActionResult SynchronizeAccountsSuperFrequent() {

         var service = ServiceLocator.Current.GetInstance<AccountSynchronizer>();
         var result = service.SynchronizeAllSuperFrequent();

         this.Response.StatusCode = (int)result.StatusCode;

         return Content(result.Message);
      }
   }
}