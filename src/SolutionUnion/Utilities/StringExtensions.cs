using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SolutionUnion {
   
   public static class StringExtensions {

      static readonly Regex EmailPattern = new Regex(@"\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");

      public static bool HasValue(this string value) {
         return !String.IsNullOrEmpty(value);
      }

      public static bool IsEmail(this string value) {
         return value.HasValue() && EmailPattern.IsMatch(value);
      }
   }
}
