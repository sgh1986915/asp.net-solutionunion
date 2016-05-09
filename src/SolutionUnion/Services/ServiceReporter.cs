using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class ServiceReporter {

      public IQueryable<ServiceReportItem> GetServices(long userId) {

         var repo = Repository<User>.GetInstance();

         return
            from u in repo.CreateQuery()
            where u.Id == userId
            from s in u.Services
            select new ServiceReportItem { 
               Description = s.Description,
               Id = s.Id,
               Price = s.Price
            };
      }
   }
}
