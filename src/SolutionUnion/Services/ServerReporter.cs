using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class ServerReporter {

      readonly Repository<Server> repo;

      public ServerReporter(Repository<Server> repo) {
         this.repo = repo;
      }

      public IQueryable<ServerReportItem> GetServers(ServerType? type = null, string keyword = null) {

         var query = this.repo.CreateQuery();

         if (type.HasValue) {
            bool isAcb = type.Value == ServerType.Lite;
            query = query.Where(s => s.IsAcb == isAcb);
         }

         if (keyword.HasValue()) {
            query = query.Where(s => s.Description.StartsWith(keyword));
         }

         return
            from s in query
            select new ServerReportItem { 
               Description = s.Description,
               Id = s.Id,
               IsAcb = s.IsAcb,
               IsDefault = s.IsDefault,
               OrIpUrl = s.OrIpUrl
            };
      }
   }
}
