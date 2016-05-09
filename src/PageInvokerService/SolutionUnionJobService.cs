using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Xml;
using NLog;

namespace PageInvokerService {

   public partial class SolutionUnionJobRunner : ServiceBase {

      static readonly List<Job> jobs;

      static SolutionUnionJobRunner() {
         
         jobs = new List<Job>();

         XmlDocument doc = new XmlDocument();
         doc.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "jobs.config"));

         var baseAttr = doc.DocumentElement.Attributes["xml:base"];

         Uri baseUrl = (baseAttr != null) ? new Uri(baseAttr.Value) : null;

         XmlNodeList nodes = doc.DocumentElement.SelectNodes("job");

         foreach (XmlNode node in nodes) {

            string name = node.Attributes["name"].Value;

            string urlValue = node.Attributes["url"].Value;
            Uri url = (baseUrl != null) ? new Uri(baseUrl, urlValue) : new Uri(urlValue);

            int? hour = null;
            var hourAttr = node.Attributes["hour"];

            if (hourAttr != null)
               hour = Int32.Parse(hourAttr.Value);

            int? interval = null;
            var intervalAttr = node.Attributes["interval"];

            if (intervalAttr != null)
               interval = Int32.Parse(intervalAttr.Value);

            jobs.Add(new Job {
               Hour = hour,
               Interval = interval,
               Url = url,
               Log = LogManager.GetLogger(name)
            });
         }
      }

      public SolutionUnionJobRunner() {
         InitializeComponent();
      }

      protected override void OnStart(string[] args) {

         foreach (var job in jobs)
            job.Initialize();
      }

      protected override void OnStop() { }
   }
}
