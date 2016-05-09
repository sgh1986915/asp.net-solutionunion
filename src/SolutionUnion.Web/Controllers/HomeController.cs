using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using MvcCodeRouting;
using MvcUtil;
using SolutionUnion.Resources.Validation;
using SolutionUnion.Services;

namespace SolutionUnion.Web.Controllers {
   
   public class HomeController : ApplicationController {

      [HttpGetHead]
      public ActionResult SignUp2(string i) {

         var actionUrl = this.Url.Action(System.Reflection.MethodBase.GetCurrentMethod().Name);

         if (!this.Request.Url.AbsolutePath.Equals(actionUrl, StringComparison.Ordinal))
            return Redirect(actionUrl);

         UserSignUpInput input;
         var result = SolutionUnion.User.SignUp(invitationCipher: i, input: out input);

         if (result.IsError)
            return Error(result);

         SetSignUpViewData(input);
         
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult SignUp2(string i, UserSignUpInput input) {

         SetSignUpViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.User.SignUp(i, input, this);

         if (result.IsError)
            return ViewWithErrors(result);

         var resultOK = (UserSignUpResult)result;

         return RedirectToAction("SignUpPayment", new { id = resultOK.SignUpId });
      }

      [HttpGetHead]
      public ActionResult SignUp_EmailAvailability(string email) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.User.ValidateNewUserEmail(email);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      public ActionResult SignUp_CompanyNameAvailability(string companyName) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         var result = SolutionUnion.User.ValidateNewUserCompanyName(companyName);

         object data = (result.IsError) ? result.Message : (object)true;

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      void SetSignUpViewData(UserSignUpInput input) {

         this.ViewData.Model = input;
         this.ViewBag.Role = new SelectList(UserSignUpInput.GetRoles(), "Key", "Value", input.Role);
      }

      [HttpGetHead]
      public ActionResult SignUpPayment([FromRoute]long id) {

         UserSignUpPaymentInput input;
         
         var result = SolutionUnion.User.SignUpPayment(id, this.Request.Url.GetLeftPart(UriPartial.Authority) + this.Url.Action("SignUpPaymentConfirm"), out input);

         if (result.IsError)
            return Error(result);

         this.ViewData.Model = input;

         string message = this.TempData["Message"] as string;

         if (!String.IsNullOrEmpty(message))
            this.ModelState.AddModelError("", message);

         this.ViewData[input.Metadata.ExpiryMonthField] = new SelectList(UserSignUpPaymentInput.GetMonths(), "Key", "Value");
         this.ViewData[input.Metadata.ExpiryYearField] = new SelectList(UserSignUpPaymentInput.GetYears(), "Key", "Value");
         this.ViewData[input.Metadata.CountryField] = new SelectList(UserSignUpPaymentInput.GetCountries(), "Key", "Value");
         this.ViewData[input.Metadata.StateField] = new SelectList(UserSignUpPaymentInput.GetStates(input.Country), "Key", "Value");

         return View();
      }

      [HttpGetHead]
      public ActionResult SignUpPayment_States([FromRoute]string p0) {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         return Json(UserSignUpPaymentInput.GetStates(p0), JsonRequestBehavior.AllowGet);
      }

      [HttpGet]
      public ActionResult SignUpPaymentConfirm() {

         var result = SolutionUnion.User.SignUpPayment(this.Request.Url.Query, this);

         if (result.IsError) {
            var error = result as UserSignUpPaymentError;

            if (error != null) {
               this.TempData["Message"] = error.Message;
               return RedirectToAction("SignUpPayment", new { id = error.SignUpId } );
            }

            return Error(result);
         }

         var resultOK = (UserSignUpPaymentResult)result;

         var loginResult = resultOK.Login();

         if (!loginResult.IsError)
            SetAuthCookie((UserLoginResult)loginResult, rememberMe: false);

         return RedirectToAction("");
      }

      [HttpGetHead]
      public ActionResult Login() {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Login(UserLoginInput input, bool rememberMe = false, string returnUrl = null) {

         this.ViewData.Model = input;

         if (!this.ModelState.IsValid)
            return View();

         var user = SolutionUnion.User.Find(input.Email);

         if (Not(user != null, ValidationRes.User_EmailPassNotMatch))
            return View();

         var result = user.Login(input.Password, rememberMe);

         if (result.IsError)
            return ViewWithErrors(result);

         SetAuthCookie((UserLoginResult)result, rememberMe);

         string location = (returnUrl.HasValue() && this.Url.IsLocalUrl(returnUrl)) ?
            returnUrl : this.Url.Action("");

         return Redirect(location);
      }

      void SetAuthCookie(UserLoginResult result, bool rememberMe) {
         FormsAuthentication.SetAuthCookie(result.Username, rememberMe);
      }

      [HttpGet]
      public ActionResult SignOut() {

         FormsAuthentication.SignOut();

         return RedirectToAction("");
      }

      [HttpGetHead]
      public ActionResult ForgotPassword() {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult ForgotPassword(UserRecoverPasswordInput input) {

         this.ViewData.Model = input;

         if (!this.ModelState.IsValid)
            return View();

         var user = SolutionUnion.User.Find(input.Email);

         if (Not(user != null, ValidationRes.UserController_UserNotExist, () => input.Email))
            return View();

         var result = user.RecoverPassword(this);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["PostForgotPassword"] = result;

         return RedirectToAction("PasswordSent");
      }

      [HttpGetHead]
      public ActionResult PasswordSent() {

         OperationResult result = this.TempData["PostForgotPassword"] as OperationResult;

         if (result == null)
            return RedirectToAction("");

         return View(result);
      }

      [Authorize]
      public ActionResult Index() {

         var service = ServiceLocator.Current.GetInstance<DashboardReporter>();

         this.ViewData.Model = service.GetReport();

         return View();
      }

      [Authorize]
      public ActionResult _Usage(DateTime? startDate) {

         var service = ServiceLocator.Current.GetInstance<DashboardReporter>();

         var report = service.GetDailyUsage(startDate);

         const string dateFormat = "yyyy-MM-dd";

         var data = new {
            StartDate = report.StartDate.ToString(dateFormat),
            EndDate = report.EndDate.ToString(dateFormat),
            ProUsage = report.ProUsage.Select(i => new object[2] { i.Date.ToString(dateFormat), i.Usage }).ToArray(),
            LiteUsage = report.LiteUsage.Select(i => new object[2] { i.Date.ToString(dateFormat), i.Usage }).ToArray(),
            Unit = report.Unit,
            Previous = Url.Action("_Usage", new { startDate = report.StartDate.AddMonths(-1).ToString(dateFormat) }),
            Next = Url.Action("_Usage", new { startDate = report.StartDate.AddMonths(1).ToString(dateFormat) })
         };

         return Json(data, JsonRequestBehavior.AllowGet);
      }

      [HttpGetHead]
      [Authorize]
      public ActionResult Profile() {

         var input = SolutionUnion.User.CurrentUser.EditProfile();

         SetProfileViewData(input);

         return View();
      }

      [HttpPost]
      [Authorize]
      public ActionResult Profile(UserProfile input) {

         SetProfileViewData(input);

         if (!this.ModelState.IsValid)
            return View();

         var result = SolutionUnion.User.CurrentUser.UpdateProfile(input);

         if (result.IsError)
            return ViewWithErrors(result);

         this.TempData["Message"] = result.Message;

         return RedirectToAction("Profile");
      }

      void SetProfileViewData(UserProfile input) {

         this.ViewData.Model = input;
         this.ViewBag.Message = this.TempData["Message"] as string;
      }

      [HttpGetHead]
      [Authorize]
      public ActionResult ChangePassword() {

         if (!this.Request.IsAjaxRequest())
            return HttpNotFound();

         return PartialView();
      }

      [HttpPost]
      [Authorize]
      public ActionResult ChangePassword(UserProfileChangePassword input) {

         var result = SolutionUnion.User.CurrentUser.ProfileChangePassword(input);

         this.Response.StatusCode = (int)result.StatusCode;

         return Json(result.Message);
      }
   }
}