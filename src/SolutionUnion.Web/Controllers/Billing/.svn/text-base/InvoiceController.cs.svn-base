using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers.Billing {
   
   [Authorize]
   public class InvoiceController : ApplicationController {

      [HttpGetHead]
      public ActionResult Index(string sort = "Created DESC", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<InvoiceReporter>();

         var query = reporter.GetInvoices(userId: SolutionUnion.User.CurrentUserId);

         this.ViewData.Model = query.Where(i => i.IsPaid).OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.Outstanding = query.Where(i => !i.IsPaid).OrderBy(i => i.Created).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }
   }
}
