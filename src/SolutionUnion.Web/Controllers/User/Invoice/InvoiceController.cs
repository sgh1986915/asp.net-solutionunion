using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcCodeRouting;
using MvcUtil;

namespace SolutionUnion.Web.Controllers.User.Invoice {

   [Authorize(Roles = UserRole.Administrator)]
   public class InvoiceController : ApplicationController {

      SolutionUnion.Invoice invoice;

      [FromRoute]
      public long Id { get; set; }

      protected override void OnActionExecuting(ActionExecutingContext filterContext) {
         
         base.OnActionExecuting(filterContext);

         if (!filterContext.IsChildAction
            && filterContext.Result == null) { 

            invoice = SolutionUnion.Invoice.Find(this.Id);

            if (invoice == null)
               filterContext.Result = Error(new ErrorResult(HttpStatusCode.NotFound, "Invoice not found."));
         }
      }

      [HttpGetHead]
      public ActionResult Download() {

         string filename, contentType;

         var result = this.invoice.Download(this.Response.OutputStream, out filename, out contentType);

         if (result.IsError)
            return Error(result);

         this.Response.ContentType = contentType;
         this.Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);

         return new EmptyResult();
      }
   }
}
