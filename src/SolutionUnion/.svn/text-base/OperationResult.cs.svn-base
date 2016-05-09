using System;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace SolutionUnion {

   [DataContract(Namespace = "")]
   public abstract class OperationResult {

      [IgnoreDataMember]
      public HttpStatusCode StatusCode { get; set; }

      public virtual string Message { get { return ""; } }

      public bool IsError {
         get { return (int)StatusCode >= 400; }
      }

      protected OperationResult(HttpStatusCode statusCode) {
         this.StatusCode = statusCode;
      }
   }

   public static class OperationResultExtensions {

      public static T WithStatus<T>(this T result, HttpStatusCode statusCode) where T : OperationResult {

         result.StatusCode = statusCode;

         return result;
      }
   }
}

