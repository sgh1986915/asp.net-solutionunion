using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NLog;

namespace PageInvokerService {
   
   public class Job {

      Timer timer;

      public int? Interval { get; set; }
      public int? Hour { get; set; }
      public Uri Url { get; set; }

      public Logger Log { get; set; }

      public void Initialize() {

         DateTime now = DateTime.Now;
         DateTime today = now.Date;

         TimeSpan dueTime, period;

         if (this.Hour.HasValue) {

            int scheduledHour = this.Hour.Value;

            DateTime scheduledTime = ((now.Hour >= scheduledHour) ? today.AddDays(1) : today).AddHours(scheduledHour);

            dueTime = scheduledTime.Subtract(now);
            period = TimeSpan.FromDays(1);
         
         } else {
            dueTime = TimeSpan.Zero;
            period = TimeSpan.FromMinutes(this.Interval.GetValueOrDefault());
         }

         timer = new Timer(Callback, null, dueTime, period);

         this.Log.Info(String.Format("Timer Initialized Successfully, starting in {0}", dueTime));
      }

      void Callback(object state) {

         try {
            Run();

         } catch (WebException ex) {

            var response = ex.Response as HttpWebResponse;

            if (response != null)
               this.Log.Info(response.StatusDescription);

            this.Log.Error(ex.Message);
         
         } catch (Exception ex) {

            this.Log.Error(ex.Message);
         }
      }

      public void Run() {

         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Url);
         request.Method = "POST";
         request.ContentLength = 0;

         var response = (HttpWebResponse)request.GetResponse();

         this.Log.Info(response.StatusDescription);
      }
   }
}
