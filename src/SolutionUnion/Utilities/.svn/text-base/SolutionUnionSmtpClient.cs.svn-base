using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace SolutionUnion {
   
   public class SolutionUnionSmtpClient : SmtpClient {

      public SolutionUnionSmtpClient() {

         if (this.EnableSsl && this.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
            this.EnableSsl = false;
      }
   }
}
