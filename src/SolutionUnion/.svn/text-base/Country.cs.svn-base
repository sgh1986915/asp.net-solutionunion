using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;

namespace SolutionUnion {
   
   public class Country {

      public long Id { get; set; }
      public string Name { get; set; }
      public string Code { get; set; }

      public ICollection<State> GetStates() {

         if (this.Code == "US") {

            var repo = Repository<State>.GetInstance();

            return repo.CreateQuery().ToList();
         }

         return Enumerable.Empty<State>().ToArray();
      }
   }
}
