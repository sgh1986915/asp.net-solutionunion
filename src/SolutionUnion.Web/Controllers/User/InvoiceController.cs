using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers.User {

   [Authorize(Roles = UserRole.Administrator)]
   public class InvoiceController : ApplicationController {

      [HttpGetHead]
      public ActionResult Index(string az, string company, string role, string search, string sort = "Created DESC", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<InvoiceReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         var query = reporter.GetInvoices(az: az, company: company, role: role, keyword: search);

         this.ViewData.Model = query.Where(i => i.IsPaid).OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.Outstanding = query.Where(i => !i.IsPaid).OrderBy(i => i.Created).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.company = company;
         this.ViewBag.az = new SelectList(InvoiceReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.role = new SelectList(InvoiceReporter.GetRoleList(), "Key", "Value", role);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }
   }
}
