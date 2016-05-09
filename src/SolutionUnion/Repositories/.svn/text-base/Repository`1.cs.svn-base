using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;

namespace SolutionUnion.Repositories {
   
   public abstract class Repository<TEntity> where TEntity : class {

      public static Repository<TEntity> GetInstance() {
         return ServiceLocator.Current.GetInstance<Repository<TEntity>>();
      }

      public abstract IQueryable<TEntity> CreateQuery();
      public abstract IQueryable<TAssociation> CreateAssociationQuery<TAssociation>(TEntity entity, Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) where TAssociation : class;
      public abstract IQueryable<TAssociation> CreateAssociationQuery<TAssociation>(Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) where TAssociation : class;

      public bool Exists(Expression<Func<TEntity, bool>> predicate) {
         return CreateQuery().LongCount(predicate) > 0;
      }

      public abstract TEntity Find(object id);

      public abstract void Add(TEntity entity);

      public abstract void SaveChanges(TEntity entity);

      public abstract void Delete(TEntity entity);

      // http://marcosikkens.nl/Posts/2011/2/4/entity-framework-4-0---one-to-many-relations
      public abstract void DeleteRelated<TAssociation>(TAssociation relatedEntity, Expression<Func<TEntity, ICollection<TAssociation>>> navigationProperty) where TAssociation : class;
   }
}
