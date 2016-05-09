using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcCodeRouting;
using MvcUtil;

namespace SolutionUnion.Web.Controllers.Billing.CreditCard {
   
   [Authorize]
   public class CreditCardController : ApplicationController {

      [FromRoute]
      public long Id { get; set; }

      [HttpGetHead]
      public ActionResult Edit() {

         CreditCardEdit input;

         var result = SolutionUnion.User.CurrentUser.EditCreditCard(this.Id, out input);

         if (result.IsError)
            return Error(result);

         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Edit(CreditCardEdit input) {

         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.User.CurrentUser.UpdateCreditCard(this.Id, input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction(null);
      }

      public void SetEditViewData(CreditCardEdit input) {

         this.ViewData.Model = input;
         this.ViewBag.Message = this.TempData["Message"] as string;
         this.ViewData["ExpiryMonth"] = new SelectList(CreditCardEdit.GetMonths(), "Key", "Value", input.ExpiryMonth);
         this.ViewData["ExpiryYear"] = new SelectList(CreditCardEdit.GetYears(), "Key", "Value", input.ExpiryYear);
         this.ViewData["Country"] = new SelectList(CreditCardEdit.GetCountries(), "Key", "Value", input.Country);
         this.ViewData["State"] = new SelectList(CreditCardEdit.GetStates(input.Country), "Key", "Value", input.State);
      }

      [HttpPost]
      public ActionResult MakeDefault() {

         var result = SolutionUnion.User.CurrentUser.MakeCreditCardDefault(this.Id);

         return Json(result);
      }

      [HttpPost]
      public ActionResult Delete() {

         var result = SolutionUnion.User.CurrentUser.DeleteCreditCard(this.Id);

         return Json(result);
      }
   }
}
