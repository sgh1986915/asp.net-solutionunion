using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LinqKit;
using SolutionUnion.Repositories;

namespace SolutionUnion.EntityFramework {
   
   public class DbContextRepository<TEntity> : Repository<TEntity> where TEntity : class {

      readonly DbContext context;
      readonly DbSet<TEntity> set;

      public DbContextRepository(DbContext context) {
         
         this.context = context;
         this.set = context.Set<TEntity>();
      }

      public override IQueryable<TEntity> CreateQuery() {
         return this.set.AsExpandable();
      }

      public override IQueryable<TAssociation> CreateAssociationQuery<TAssociation>(TEntity entity, Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) {
         return this.context.Entry(entity).Collection(navigationProperty).Query().AsExpandable();
      }

      public override IQueryable<TAssociation> CreateAssociationQuery<TAssociation>(Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) {
         return this.context.Set<TAssociation>().AsExpandable();
      }

      public override TEntity Find(object id) {
         return this.set.Find(id);
      }

      public override void Add(TEntity entity) {

         if (entity == null) throw new ArgumentNullException("entity");

         this.set.Add(entity);
         this.context.SaveChanges();
      }

      public override void SaveChanges(TEntity entity) {
         this.context.SaveChanges();
      }

      public override void Delete(TEntity entity) {

         if (entity == null) throw new ArgumentNullException("entity");

         this.set.Remove(entity);
         this.context.SaveChanges();
      }

      public override void DeleteRelated<TAssociation>(TAssociation relatedEntity, Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) {

         if (relatedEntity == null) throw new ArgumentNullException("relatedEntity");

         this.context.Set<TAssociation>().Remove(relatedEntity);
         this.context.SaveChanges();
      }
   }
}
