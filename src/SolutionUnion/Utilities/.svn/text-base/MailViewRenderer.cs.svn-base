using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;

namespace SolutionUnion {
   
   static class MailViewRenderer {

      public static string Render(string viewName, object model, ControllerContext context) {

         string viewLocation = "Notifications/en/{0}";

         ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(context, String.Format(viewLocation, viewName));

         if (viewResult.View == null)
            throw new InvalidOperationException();

         using (StringWriter output = new StringWriter()) {

            ViewContext viewContext = new ViewContext(
               context,
               viewResult.View,
               new ViewDataDictionary(model),
               new TempDataDictionary(),
               output
            );

            viewResult.View.Render(viewContext, output);

            return output.ToString();
         }
      }
   }
}
