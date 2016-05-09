using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MvcCodeRouting;
using MvcUtil;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers {

   [Authorize(Roles = UserRole.Administrator)]
   public class UserController : ApplicationController {
      
      [HttpGetHead]
      public ActionResult Add() {

         UserCreateInput input = new UserCreateInput();

         SetAddViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Add(UserCreateInput input) {

         SetAddViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.User.Create(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("Add");
      }

      [HttpGetHead]
      public ActionResult Add_EmailAvailability(string email) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.User.ValidateNewUserEmail(email);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      public ActionResult Add_CompanyNameAvailability(string companyName) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.User.ValidateNewUserCompanyName(companyName);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      public ActionResult Add_States([FromRoute]string p0) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         return Json(CreditCardEdit.GetStates(p0), JsonRequestBehavior.AllowGet);
      }

      void SetAddViewData(UserCreateInput input) {

         SetEditViewData((UserEdit)input);

         this.ViewData["CreditCard.ExpiryMonth"] = new SelectList(CreditCardEdit.GetMonths(), "Key", "Value", input.CreditCard.ExpiryMonth);
         this.ViewData["CreditCard.ExpiryYear"] = new SelectList(CreditCardEdit.GetYears(), "Key", "Value", input.CreditCard.ExpiryYear);
         this.ViewData["CreditCard.Country"] = new SelectList(CreditCardEdit.GetCountries(), "Key", "Value", input.CreditCard.Country);
         this.ViewData["CreditCard.State"] = new SelectList(CreditCardEdit.GetStates(input.CreditCard.Country), "Key", "Value", input.CreditCard.State);
      }

      [HttpGetHead]
      public ActionResult Edit([FromRoute]long id) {
         
         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Error(HttpStatusCode.NotFound, "User Not Found.");

         UserEdit input = user.Edit();

         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Edit([FromRoute]long id, UserEdit input) {

         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Error(HttpStatusCode.NotFound, "User Not Found.");

         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = user.Update(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("Edit");
      }

      [HttpPost]
      public ActionResult Edit_ComputeDiscount([FromRoute]long id, decimal percentageDiscount) {

         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Json(new ErrorResult(HttpStatusCode.NotFound, "User Not Found."));

         decimal discount = user.ComputeDiscount(percentageDiscount);

         return Json(new { Value = discount, FormattedValue = discount.ToString("c") });
      }

      void SetEditViewData(UserEdit input) {

         this.ViewData.Model = input;
         this.ViewBag.Message = this.TempData["Message"] as string;
      }

      [HttpGetHead]
      public ActionResult Services([FromRoute]long id) {

         if (!this.Request.IsAjaxRequest() 
            && !this.ControllerContext.IsChildAction)
            return HttpNotFound();

         var service = ServiceLocator.Current.GetInstance<ServiceReporter>();

         this.ViewData.Model = service.GetServices(id);

         return PartialView();
      }

      [HttpGetHead]
      public ActionResult AddService([FromRoute]long id) {

         this.ViewData.Model = new ServiceEdit();

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpPost]
      public ActionResult AddService([FromRoute]long id, ServiceEdit[] input) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Json(new ErrorResult(HttpStatusCode.NotFound, "User Not Found."));

         if (!this.ModelState.IsValid)
            return Json(new ErrorResult(HttpStatusCode.BadRequest, this.ModelState.Values.Where(m => m.Errors.Count > 0).First().Errors.First().ErrorMessage));

         var result = user.AddServices(input);

         return Json(result);
      }

      [HttpGetHead]
      public ActionResult EditService([FromRoute]long id, [FromRoute]long serviceId) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Error(HttpStatusCode.NotFound, "User Not Found.");

         ServiceEdit input;
         var result = user.EditService(serviceId, out input);

         if (result.IsError)
            return Error(result);

         this.ViewData.Model = input;

         return PartialView();
      }

      [HttpPost]
      public ActionResult EditService([FromRoute]long id, [FromRoute]long serviceId, ServiceEdit input) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var user = SolutionUnion.User.Find(id);

         if (user == null) 
            return Json(new ErrorResult(HttpStatusCode.NotFound, "User Not Found."));

         if (!this.ModelState.IsValid)
            return Json(new ErrorResult(HttpStatusCode.BadRequest, this.ModelState.Values.Where(m => m.Errors.Count > 0).First().Errors.First().ErrorMessage));

         var result = user.UpdateService(serviceId, input);

         return Json(result);
      }

      [HttpPost]
      public ActionResult DeleteService([FromRoute]long id, [FromRoute]long serviceId) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var user = SolutionUnion.User.Find(id);

         if (user == null)
            return Json(new ErrorResult(HttpStatusCode.NotFound, "User Not Found."));

         var result = user.DeleteService(serviceId);

         return Json(result);
      }

      [HttpGetHead]
      public ActionResult DefaultSettings() {

         this.ViewData.Model = ApplicationSetting.Instance.EditUserDefaults();
         this.ViewBag.Message = this.TempData["Message"] as string;

         return View();
      }

      [HttpPost]
      public ActionResult DefaultSettings(ApplicationSettingUserDefaultsEdit input) {

         this.ViewData.Model = input;

         if (!this.ModelState.IsValid)
            return View();

         var result = ApplicationSetting.Instance.UpdateUserDefaults(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("DefaultSettings");
      }

      public ActionResult List(string az, string company, string role, string search, string sort = "CompanyName", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<UserReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         var query = reporter.GetUsers(az: az, company: company, role: role, keyword: search);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.company = company;
         this.ViewBag.az = new SelectList(UserReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.role = new SelectList(UserReporter.GetRoleList(), "Key", "Value", role);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpPost]
      public ActionResult Activate([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Activate();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Suspend([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Suspend();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Lock([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Lock();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Unlock([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Unlock();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpGetHead]
      public ActionResult ChangePassword([FromRoute]long id) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         return PartialView();
      }

      [HttpPost]
      public ActionResult ChangePassword([FromRoute]long id, UserChangePasswordInput input) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = user.ChangePassword(input);

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult MarkAsDeleted([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.MarkAsDeleted();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Restore([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Restore();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Delete([FromRoute]long id) {

         var user = SolutionUnion.User.Find(id);

         if (user == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid User information.");
         }

         var result = user.Delete();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      public ActionResult Invoices() {
         return View();
      }

      public ActionResult MarkedAsDeleted(string az, string company, string role, string search, string sort = "CompanyName", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<UserReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         var query = reporter.GetMarkedAsDeleted(az: az, company: company, role: role, keyword: search);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.company = company;
         this.ViewBag.az = new SelectList(UserReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.role = new SelectList(UserReporter.GetRoleList(), "Key", "Value", role);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }
   }
}
