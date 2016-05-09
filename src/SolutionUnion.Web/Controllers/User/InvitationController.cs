using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcCodeRouting;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers.User {

   [Authorize(Roles = UserRole.Administrator)]
   public class InvitationController : ApplicationController {

      [HttpGetHead]
      public ActionResult List(string search, string sort = "DateSent", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<InvitationReporter>();

         var query = reporter.GetInvitations(keyword: search);

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

         var input = new InvitationCreateInput();

         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Add(InvitationCreateInput input) {

         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.Invitation.Create(input, this);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction(null);
      }

      [HttpGetHead]
      public ActionResult Add_EmailAvailability(string email) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.Invitation.ValidateNewInvitationEmail(email);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      public ActionResult Add_CompanyNameAvailability(string companyName) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.Invitation.ValidateNewInvitationCompanyName(companyName);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      public ActionResult Edit([FromRoute]long id) {

         var invitation = SolutionUnion.Invitation.Find(id);

         if (invitation == null)
            return Error(HttpStatusCode.NotFound, "Invitation Not Found.");

         var input = invitation.Edit();

         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Edit([FromRoute]long id, InvitationEdit input) {

         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var invitation = SolutionUnion.Invitation.Find(id);

         if (invitation == null)
            return Error(HttpStatusCode.NotFound, "Invitation Not Found.");

         var result = invitation.Update(input, this);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction(null);
      }

      void SetEditViewData(InvitationEdit input) {

         this.ViewData.Model = input;
         this.ViewBag.Message = this.TempData["Message"] as string;
      }

      [HttpPost]
      public ActionResult Resend([FromRoute]long id) {

         var invitation = SolutionUnion.Invitation.Find(id);

         if (invitation == null) {
            this.Response.StatusCode = 404;
            return Json("Invitation Not Found.");
         }

         var result = invitation.Resend(this);

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Delete([FromRoute]long id) {

         var invitation = SolutionUnion.Invitation.Find(id);

         if (invitation == null) {
            this.Response.StatusCode = 404;
            return Json("Invitation Not Found.");
         }

         var result = invitation.Delete();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }
	}
}