using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolutionUnion.Repositories;

namespace SolutionUnion.Services {
   
   public class InvitationReporter {

      readonly Repository<Invitation> repo;

      public InvitationReporter(Repository<Invitation> repo) {
         this.repo = repo;
      }

      public IQueryable<InvitationReportItem> GetInvitations(string keyword = null) {

         var query = this.repo.CreateQuery();

         if (keyword.HasValue())
            query = query.Where(i => i.Email.Contains(keyword) || i.User.FirstName.Contains(keyword) || i.User.LastName.Contains(keyword));

         return
            from i in query
            select new InvitationReportItem {
               Id = i.Id,
               DateSent = i.DateSent,
               Email = i.Email,
               UserFirstName = i.User.FirstName,
               UserLastName = i.User.LastName
            };
      }
   }
}
