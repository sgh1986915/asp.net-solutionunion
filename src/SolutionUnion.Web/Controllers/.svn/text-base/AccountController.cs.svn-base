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
   
   [Authorize(Roles = UserRole.Administrator + "," + UserRole.Reseller)]
   public class AccountController : ApplicationController {

      [HttpGetHead]
      public ActionResult List(string az, string company, string search, AccountType? type, AccountStatus? status, string sort = "AccountName", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<AccountReporter>();

         if (String.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            search = null;

         var query = reporter.GetAccounts(az, company, search, type, status);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.company = company;
         this.ViewBag.az = new SelectList(AccountReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.type = EnumList.Create(typeof(AccountType), type);
         this.ViewBag.status = EnumList.Create(typeof(AccountStatus), status);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpGetHead]
      [Authorize(Roles = UserRole.Administrator)]
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
      [Authorize(Roles = UserRole.Administrator)]
      public ActionResult AssignUser([FromRoute]long id, long userId) {

         var account = SolutionUnion.Account.Find(id);

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

         var account = SolutionUnion.Account.Find(id);

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

         var account = SolutionUnion.Account.Find(id);

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

         var account = SolutionUnion.Account.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Suspend();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult MarkAsDeleted([FromRoute]long id) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.MarkAsDeleted();

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
      public ActionResult ChangePassword([FromRoute]long id, AccountChangePasswordInput input) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.ChangePassword(input);

         this.Response.StatusCode = (int)result.StatusCode;
         
         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Restore([FromRoute]long id) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Restore();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Delete([FromRoute]long id) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Account information.");
         }

         var result = account.Delete();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpGetHead]
      public ActionResult MarkedAsDeleted(string az, string search, long? serverId, AccountStatus? status, string sort = "AccountName", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<AccountReporter>();

         var query = reporter.GetMarkedAsDeleted(az, serverId, search, status);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.az = new SelectList(AccountReporter.GetAlphabetList(), "Key", "Value", az);
         this.ViewBag.serverId = new SelectList(AccountReporter.GetServerList(), "Key", "Value", serverId);
         this.ViewBag.status = EnumList.Create(typeof(AccountStatus), status);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpGetHead]
      public ActionResult Add() {

         AccountEdit input = Account.New();

         this.ViewData.Model = input;
         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Add(AccountEdit input) {

         this.ViewData.Model = input;
         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.Account.Create(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("Add");
      }

      [HttpGetHead]
      public ActionResult Edit([FromRoute]long id) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null)
            return Error(HttpStatusCode.NotFound, "Account Not Found.");

         AccountEdit input = account.Edit();

         this.ViewData.Model = input;
         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Edit([FromRoute]long id, AccountEdit input) {

         var account = SolutionUnion.Account.Find(id);

         if (account == null)
            return Error(HttpStatusCode.NotFound, "Account Not Found.");

         this.ViewData.Model = input;
         SetEditViewData(input);

         this.ModelState.Remove("Password");
         this.ModelState.Remove("ConfirmPassword");
         this.ModelState.Remove("AccountName");

         if (!this.ModelState.IsValid)
            return View();

         var result = account.Update(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("Edit");
      }

      [HttpGetHead]
      public ActionResult Updated() {
         return View();
      }

      public ActionResult _Servers(bool isAcb) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         ServerType serverType = (isAcb) ? ServerType.Lite : ServerType.Pro;

         var input = Account.New(serverType);

         return Json(new { Servers = input.GetServerList(), MinVaultSize = input.VaultSize }, JsonRequestBehavior.AllowGet);
      }

      public ActionResult _AccountAvailability(string accountName, bool isAcb, long? serverId) {

         var serverType = (isAcb) ? ServerType.Lite : ServerType.Pro;
         var result = SolutionUnion.Account.ValidateNewAccountName(accountName, serverType, serverId);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      public ActionResult _VaultSize(decimal vaultSize, bool isAcb) {

         var serverType = (isAcb) ? ServerType.Lite : ServerType.Pro;
         var result = SolutionUnion.Account.ValidateNewAccountVaultSize(vaultSize, serverType);

         object data = result.IsError ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      void SetEditViewData(AccountEdit input) {

         this.ViewBag.Message = this.TempData["Message"] as string;
         this.ViewBag.AccountType = EnumList.Create(typeof(AccountType), input.AccountType);
         this.ViewBag.TimeZone = new SelectList(TimeZones.Zones.ToDictionary(s => s, s => s), "Key", "Value", input.TimeZone);
         this.ViewBag.ServerId = input.GetServerList();
      }

      [HttpGetHead]
      public ActionResult DailyUsage([FromRoute]long id, DateTime? month, int start = 0) {

         var reporter = ServiceLocator.Current.GetInstance<DailyUsageReporter>();

         const int pageSize = 10;

         var query = reporter.GetReport(id, month);

         this.ViewData.Model = query.Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.start = start;
         this.ViewBag.month = new SelectList(DailyUsageReporter.GetMonths(), "Key", "Value", (month != null) ? month.Value.ToString("yyyy-MM-dd") : null);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }
   }
}