using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;

namespace SolutionUnion.EntityFramework {
   
   public class DbContextApplicationSettingsStoragePriceRepository : DbContextRepository<ApplicationSettingsStoragePrice> {

      readonly DbContext context;

      public DbContextApplicationSettingsStoragePriceRepository(DbContext context) 
         : base(context) {
         
         this.context = context;
      }

      public override void SaveChanges(ApplicationSettingsStoragePrice entity) {

         this.context.Entry(entity).State = EntityState.Modified;
         base.SaveChanges(entity);
      }

      public override void Delete(ApplicationSettingsStoragePrice entity) {

         this.context.Entry(entity).State = EntityState.Deleted;
         base.Delete(entity);
      }
   }
}
