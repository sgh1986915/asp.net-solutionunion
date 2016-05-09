using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using System.Text;

namespace SolutionUnion {
   
   public class Session {

      public static Session Current {
         get {
            return ServiceLocator.Current.GetInstance<Session>();
         }
      }

      public long Id { get; set; }
      public long UserId { get; set; }
      public DateTime LastVisit { get; set; }
      public bool RememberLogin { get; set; }
      public DateTime Created { get; set; }

      public virtual ICollection<Log> Logs { get; private set; }

      internal IQueryable<Log> LogsQuery {
         get {
            var repo = Repository<Session>.GetInstance();
            return repo.CreateAssociationQuery(this, s => s.Logs);
         }
      }

      public static Session GetCurrent() {
         
         if (!User.IsAuthenticated)
            return null;

         var repo = Repository<Session>.GetInstance();

         return (from s in repo.CreateQuery()
                 where s.UserId == User.CurrentUserId
                 orderby s.Created descending
                 select s).FirstOrDefault();
      }

      public static void Log(OperationResult result, Account affectedAccount = null) {

         Session session = Session.Current;

         if (session != null)
            session._Log(result, affectedAccount: affectedAccount);
      }

      public static void Log(string message, Account affectedAccount = null) { 
      
         Session session = Session.Current;

         if (session != null)
            session._Log(message, affectedAccount: affectedAccount);
      }

      void _Log(OperationResult result, Account affectedAccount = null) {
         Log(result.Message, affectedAccount: affectedAccount);
      }

      void _Log(string message, Account affectedAccount = null) {

         var repo = Repository<Session>.GetInstance();

         var log = new Log {
            Created = DateTime.Now,
            Message = message,
            SessionId = this.Id,
         };

         if (affectedAccount != null) {
            log.AccountId = affectedAccount.Id;
            log.AccountName = affectedAccount.AccountName;
         }

         this.Logs.Add(log);

         repo.SaveChanges(this);
      }

      public void Update() {

         this.LastVisit = DateTime.Now;

         var repo = Repository<Session>.GetInstance();

         repo.SaveChanges(this);
      }
   }
}
