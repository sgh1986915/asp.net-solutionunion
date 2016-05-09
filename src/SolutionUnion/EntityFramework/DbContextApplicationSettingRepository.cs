using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;

namespace SolutionUnion.EntityFramework {
   
   public class DbContextApplicationSettingRepository : DbContextRepository<ApplicationSetting> {

      readonly DbContext context;

      public DbContextApplicationSettingRepository(DbContext context) 
         : base(context) {
         
         this.context = context;
      }

      public override void SaveChanges(ApplicationSetting entity) {

         this.context.Entry(entity).State = EntityState.Modified;
         base.SaveChanges(entity);
      }

      public override void Delete(ApplicationSetting entity) {
         
         this.context.Entry(entity).State = EntityState.Deleted;
         base.Delete(entity);
      }
   }
}
