using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using MvcCodeRouting;
using MvcUtil;
using MvcUtil.Unity;

namespace SolutionUnion.Web {
   
   public class MvcApplication : System.Web.HttpApplication {

      static readonly IUnityContainer Container = new UnityContainer();
      static readonly IServiceLocator ServiceLoc = new UnityServiceLocator(Container);

      void Application_Start() {

         try {
            ProtectConfigurationSections();

            ServiceLocator.SetLocatorProvider(() => ServiceLoc);
            ControllerBuilder.Current.SetControllerFactory(new UnityControllerFactory(Container));
            RegisterImplementations(Container);

            DefaultModelBinder.ResourceClassKey = "ModelBindingRes";
            MvcHandler.DisableMvcResponseHeader = true;

            RegisterRoutes(RouteTable.Routes);
            ViewEngines.Engines.EnableCodeRouting();

         } catch {
            HttpRuntime.UnloadAppDomain();
            throw;
         }
      }
      
      void ProtectConfigurationSections() {

         var config = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);

         ConfigurationSection connStringsSection = config.GetSection("connectionStrings");

         bool hasChanges = false;

         if (connStringsSection != null
            && !connStringsSection.SectionInformation.IsProtected) {

            connStringsSection.SectionInformation.ProtectSection(ProtectedConfiguration.DefaultProvider);
            hasChanges = true;
         }

         ConfigurationSection smtpSection = config.GetSection("system.net/mailSettings/smtp");

         if (smtpSection != null
            && !smtpSection.SectionInformation.IsProtected) {

            smtpSection.SectionInformation.ProtectSection(ProtectedConfiguration.DefaultProvider);
            hasChanges = true;
         }

         ConfigurationSection braintreeSection = config.GetSection(Configuration.BraintreeConfigurationSection.SectionName);

         if (braintreeSection != null
            && !braintreeSection.SectionInformation.IsProtected) {

            braintreeSection.SectionInformation.ProtectSection(ProtectedConfiguration.DefaultProvider);
            hasChanges = true;
         }

         if (hasChanges)
            config.Save();
      }

      void RegisterImplementations(IUnityContainer container) {

         string connectionName = "SolutionUnion";

         container
            .RegisterType(typeof(Session), new PerRequestLifetimeManager(), new InjectionFactory(c => SolutionUnion.Session.GetCurrent()))
            .RegisterType<EntityFramework.SolutionUnionContext>(new PerRequestLifetimeManager(), new InjectionConstructor(connectionName))
            .RegisterType<System.Data.Entity.DbContext, EntityFramework.SolutionUnionContext>()
            .RegisterType<Repositories.RoleRepository, EntityFramework.DbContextRoleRepository>()
            .RegisterType<Repositories.Repository<ApplicationSetting>, EntityFramework.DbContextApplicationSettingRepository>()
            .RegisterType<Repositories.Repository<ApplicationSettingsStoragePrice>, EntityFramework.DbContextApplicationSettingsStoragePriceRepository>()
            .RegisterType(typeof(Repositories.Repository<>), typeof(EntityFramework.DbContextRepository<>))

            .RegisterType<System.Net.Mail.SmtpClient, SolutionUnionSmtpClient>()
            .RegisterType<Payment.PaymentProcessor, Payment.BraintreePaymentProcessor>()
            ;
      }

      void RegisterRoutes(RouteCollection routes) {
         
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
         routes.IgnoreRoute("favicon.ico");

         routes.MapCodeRoutes(
            rootNamespace: typeof(Controllers.HomeController).Namespace
         );
      }

      void Application_Error() {

         HttpException httpEx = this.Server.GetLastError() as HttpException;
         HttpResponse response = this.Context.Response;

         if (httpEx != null) {

            int statusCode = httpEx.GetHttpCode();

            if (statusCode != 0)
               response.StatusCode = statusCode;
         }

         if (response.StatusCode < 400)
            response.StatusCode = 500;
      }
   }
}