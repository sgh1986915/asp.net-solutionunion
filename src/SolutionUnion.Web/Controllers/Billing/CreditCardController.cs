using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcCodeRouting;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers.Billing {
   
   [Authorize]
   public class CreditCardController : ApplicationController {

      [HttpGetHead]
      public ActionResult Index(string sort = "Id", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<CreditCardReporter>();

         var query = reporter.GetCreditCards();

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpGetHead]
      public ActionResult Add() {

         var input = new CreditCardEdit();

         SetAddViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Add(CreditCardEdit input) {

         SetAddViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.User.CurrentUser.AddCreditCard(input);

         if (result.IsError)
            return ViewWithErrors(result);

         if (this.Request.IsAjaxRequest()) { 
            var resultOK = (CreditCardCreatedResult)result;

            this.Response.StatusCode = (int)result.StatusCode;

            return Json(new { resultOK.Id });
         }

         this.TempData["Message"] = result.Message;

         return RedirectToAction(null);
      }

      public void SetAddViewData(CreditCardEdit input) {

         this.ViewData.Model = input;
         this.ViewBag.Message = this.TempData["Message"] as string;
         this.ViewData["ExpiryMonth"] = new SelectList(CreditCardEdit.GetMonths(), "Key", "Value", input.ExpiryMonth);
         this.ViewData["ExpiryYear"] = new SelectList(CreditCardEdit.GetYears(), "Key", "Value", input.ExpiryYear);
         this.ViewData["Country"] = new SelectList(CreditCardEdit.GetCountries(), "Key", "Value", input.Country);
         this.ViewData["State"] = new SelectList(CreditCardEdit.GetStates(input.Country), "Key", "Value", input.State);
      }

      [HttpGetHead]
      public ActionResult Edit_States([FromRoute]string p0) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         return Json(CreditCardEdit.GetStates(p0), JsonRequestBehavior.AllowGet);
      }
   }
}
