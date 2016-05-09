using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Globalization;
using System.Net;
using System.Web.Routing;
using MvcCodeRouting;

namespace SolutionUnion.Web {
   
   public abstract class ApplicationController : Controller {

      protected override void Initialize(RequestContext requestContext) {
         
         base.Initialize(requestContext);

         this.BindRouteProperties();
      }

      protected override void OnActionExecuting(ActionExecutingContext filterContext) {

         if (filterContext.IsChildAction)
            return;

         var session = SolutionUnion.Session.Current;

         if (session != null) 
            session.Update();
      }

      // Validation helpers

      protected bool Assert(bool condition, string message, object value = null, string key = null) {

         if (message == null) throw new ArgumentNullException("message");

         if (!condition)
            this.ModelState.AddModelError(key ?? "", String.Format(CultureInfo.InvariantCulture, message, value, key));

         return condition;
      }

      protected bool Assert<TMember>(bool condition, string message, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {

         if (valueSelector == null) throw new ArgumentNullException("valueSelector");

         if (!condition) {

            string fullKey = ErrorResult.NameOf(valueSelector, includeFirstKeySegment);
            string key = fullKey.Split('.').Last();
            object value = valueSelector.Compile().Invoke();

            this.ModelState.AddModelError(fullKey, String.Format(CultureInfo.InvariantCulture, message, value, key));
         }

         return condition;
      }

      protected bool Not(bool condition, string message, object value = null, string key = null) {
         return !Assert(condition, message, value, key);
      }

      protected bool Not<TMember>(bool condition, string message, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {
         return !Assert<TMember>(condition, message, valueSelector, includeFirstKeySegment);
      }

      protected ViewResult ViewWithErrors(OperationResult result) {

         ErrorResult errorResult = (ErrorResult)result;

         AddErrorsToModelState(errorResult);
         
         this.Response.StatusCode = (int)errorResult.StatusCode;
         
         return View();
      }

      void AddErrorsToModelState(ErrorResult errorResult) {

         foreach (var item in errorResult.Errors) {
            IList<string> memberNames = item.MemberNames.ToArray();

            if (memberNames.Count > 0) {

               for (int i = 0; i < memberNames.Count; i++)
                  this.ModelState.AddModelError(memberNames[i] ?? "", item.ErrorMessage);

            } else {
               this.ModelState.AddModelError("", item.ErrorMessage);
            }
         }
      }

      protected HttpException CreateHttpException(OperationResult result) {

         ErrorResult errorResult = (ErrorResult)result;

         string errorMessage = null;

         var val = errorResult.Errors.FirstOrDefault(v => v.MemberNames.Count == 0);

         if (val != null)
            errorMessage = val.ErrorMessage;

         throw new HttpException((int)errorResult.StatusCode, errorMessage ?? "");
      }

      // Result helpers

      protected ActionResult Error(HttpStatusCode statusCode, string errorMessage) {
         return Error(new ErrorResult(statusCode, errorMessage));
      }

      protected ActionResult Error(OperationResult result) {

         this.Response.StatusCode = (int)result.StatusCode;

         string view = (this.User.Identity.IsAuthenticated) ? "Error" : "PublicError";

         return View(view, result);
      }

      protected ActionResult Json(OperationResult result) {

         this.Response.StatusCode = (int)result.StatusCode;
         return Json(result.Message, JsonRequestBehavior.AllowGet);
      }
   }
}