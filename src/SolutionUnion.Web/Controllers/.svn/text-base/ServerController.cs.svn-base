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
   
   [Authorize(Roles = UserRole.Administrator)]
   public class ServerController : ApplicationController {

      [HttpGetHead]
      public ActionResult List(ServerType? type, string search, string sort = "Description", int start = 0, int pageSize = 50) {

         var reporter = ServiceLocator.Current.GetInstance<ServerReporter>();

         var query = reporter.GetServers(type, search);

         this.ViewData.Model = query.OrderBy(sort).Skip(start).Take(pageSize).ToList();
         this.ViewBag.PageSize = pageSize;
         this.ViewBag.Count = query.Count();

         this.ViewBag.sort = sort;
         this.ViewBag.start = start;
         this.ViewBag.type = EnumList.Create(typeof(ServerType), type);

         if (this.Request.IsAjaxRequest())
            return PartialView();

         return View();
      }

      [HttpPost]
      public ActionResult Verify([FromRoute]long id) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Server information.");
         }

         var result = server.Verify();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult MakeDefault([FromRoute]long id) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Server information.");
         }

         var result = server.MakeDefault();

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpPost]
      public ActionResult Delete([FromRoute]long id) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Server information.");
         }

         var result = server.Delete();

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
      public ActionResult ChangePassword([FromRoute]long id, ServerChangePasswordInput input) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null) {
            this.Response.StatusCode = 404;
            return Json("Invalid Server information.");
         }

         var result = server.ChangePassword(input);

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }

      [HttpGetHead]
      public ActionResult Add() {

         ServerEdit input = new ServerEdit { Type = ServerType.Pro };

         this.ViewData.Model = input;
         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Add(ServerEdit input) {

         this.ViewData.Model = input;
         SetEditViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.Server.Create(input);

         if (result.IsError)
            return ViewWithErrors(result);

         return RedirectToAction("Added");
      }

      [HttpGetHead]
      public ActionResult Added() {
         return View();
      }

      [HttpGetHead]
      public ActionResult Edit([FromRoute]long id) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null)
            return Error(HttpStatusCode.NotFound, "Server Not Found.");

         ServerEdit input = server.Edit();

         this.ViewData.Model = input;
         SetEditViewData(input);

         return View();
      }

      [HttpPost]
      public ActionResult Edit([FromRoute]long id, ServerEdit input) {

         var server = SolutionUnion.Server.Find(id);

         if (server == null)
            return Error(HttpStatusCode.NotFound, "Server Not Found.");

         this.ViewData.Model = input;
         SetEditViewData(input);

         this.ModelState.Remove("Password");
         this.ModelState.Remove("ConfirmPassword");

         if (!this.ModelState.IsValid)
            return View();

         var result = server.Update(input);

         if (result.IsError)
            return ViewWithErrors(result);

         return RedirectToAction("Updated");
      }

      [HttpGetHead]
      public ActionResult Updated() {
         return View();
      }

      void SetEditViewData(ServerEdit input) {

      }
   }
}