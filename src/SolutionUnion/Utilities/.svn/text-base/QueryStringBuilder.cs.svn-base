using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Text;

namespace SolutionUnion {
   
   static class QueryStringBuilder {
      
      public static string ToQueryString(this NameValueCollection qs) {

         StringBuilder sb = new StringBuilder();

         for (int i = 0; i < qs.AllKeys.Length; i++) {
            string key = qs.AllKeys[i];
            string[] values = qs.GetValues(key) ?? new string[0];

            for (int j = 0; j < values.Length; j++) {
               if (sb.Length > 0) sb.Append("&");
               sb.Append(key).Append("=").Append(HttpUtility.UrlEncode(values[j]));
            }
         }

         return sb.ToString();
      }
   }
}
