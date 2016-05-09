using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

namespace SolutionUnion {

   [DataContract(Name = "OperationResult", Namespace = "")]
   public class SuccessfulResult : OperationResult {

      readonly string _Message;

      public override string Message { get { return _Message; } }

      public SuccessfulResult() 
         : this(HttpStatusCode.OK) { }

      public SuccessfulResult(HttpStatusCode statusCode)
         : base(statusCode) { }

      public SuccessfulResult(string message)
         : this() {

         _Message = message;
      }

      public SuccessfulResult(HttpStatusCode statusCode, string message) 
         : this(statusCode) {

         _Message = message;
      }
   }
}
