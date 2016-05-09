using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SolutionUnion {

   [DataContract(Name = "OperationResult", Namespace = "")]
   public class ErrorResult : OperationResult {
      
      readonly Collection<SerializableValidationResult> _Errors;

      [DataMember]
      public Collection<SerializableValidationResult> Errors {
         get { return _Errors; }
      }

      public bool HasErrors { get { return Errors.Count > 0; } }

      public override string Message {
         get {
            return (from e in Errors
                    orderby e.MemberNames.Count, e.MemberNames.Contains("") descending
                    select e.ErrorMessage).FirstOrDefault() ?? "";
         }
      }

      public static string NameOf(LambdaExpression valueSelector, bool includeFirstKeySegment = false) {

         if (valueSelector == null) throw new ArgumentNullException("valueSelector");

         MemberExpression memberExpr = valueSelector.Body as MemberExpression;
         Expression currentExpr = (memberExpr != null) ? memberExpr.Expression : valueSelector.Body;
         ConstantExpression constantExpr = currentExpr as ConstantExpression;

         if (constantExpr == null) {

            List<string> keySegments = new List<string>();

            if (memberExpr != null)
               keySegments.Add(memberExpr.Member.Name);

            while (constantExpr == null) {

               while ((memberExpr = currentExpr as MemberExpression) == null) {

                  MethodCallExpression methodCall = (MethodCallExpression)currentExpr;
                  currentExpr = methodCall.Object;

                  if (methodCall.Method.Name.StartsWith("get_") && methodCall.Arguments.Count == 1 && methodCall.Arguments[0].Type == typeof(Int32)) {

                     int index = Expression.Lambda<Func<int>>((MemberExpression)methodCall.Arguments[0]).Compile()();

                     keySegments.Add(String.Concat("[", index.ToString(CultureInfo.InvariantCulture), "]"));

                  } else {
                     throw new ArgumentException("valueSelector");
                  }
               }

               currentExpr = memberExpr.Expression;
               constantExpr = currentExpr as ConstantExpression;

               keySegments.Add(memberExpr.Member.Name);
            }

            keySegments.Reverse();

            if (!includeFirstKeySegment)
               keySegments.RemoveAt(0);

            return String.Join(".", keySegments.ToArray()).Replace(".[", "[");

         } else if (memberExpr != null) {
            return memberExpr.Member.Name;

         } else {
            throw new ArgumentException("valueSelector");
         }
      }

      public ErrorResult() 
         : this(HttpStatusCode.BadRequest) { }

      public ErrorResult(HttpStatusCode statusCode)
         : this(statusCode, new ValidationResult[0]) { }

      public ErrorResult(string errorMessage)
         : this(HttpStatusCode.BadRequest, new ValidationResult[1] { new ValidationResult(errorMessage) }) { }

      public ErrorResult(HttpStatusCode statusCode, string errorMessage)
         : this(statusCode, new ValidationResult[1] { new ValidationResult(errorMessage) }) { }

      public ErrorResult(HttpStatusCode statusCode, IEnumerable<ValidationResult> validation) 
         : base(statusCode) {

         this._Errors = new Collection<SerializableValidationResult>();

         AddValidationResults(validation);
      }

      public void AddValidationResults(IEnumerable<ValidationResult> validation) {

         if (validation == null) throw new ArgumentNullException("results");

         foreach (var item in validation) 
            AddValidationResult(item);
      }

      public void AddValidationResult(ValidationResult validation) {

         if (validation == null) throw new ArgumentNullException("result");

         this.Errors.Add(new SerializableValidationResult(validation.ErrorMessage, validation.MemberNames));
      }

      public bool Assert(bool condition, string message, object value = null, string key = null) {

         if (message == null) throw new ArgumentNullException("message");

         if (!condition)
            Errors.Add(new SerializableValidationResult(String.Format(CultureInfo.InvariantCulture, message, value, key), new[] { key }));

         return condition;
      }

      public bool Assert(OperationResult otherResult) {

         if (otherResult == null) throw new ArgumentNullException("otherResult");

         bool isError = otherResult.IsError;

         if (isError) {

            ErrorResult errorResult = otherResult as ErrorResult;

            if (errorResult != null) {

               foreach (var error in errorResult.Errors)
                  this.Errors.Add(error);
            
            } else {
               Assert(!isError, otherResult.Message);
            }
         }

         return !isError;
      }

      public bool Assert<TMember>(bool condition, string message, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {

         if (valueSelector == null) throw new ArgumentNullException("valueSelector");

         if (!condition) {

            if (message == null) throw new ArgumentNullException("message");

            string fullKey = NameOf(valueSelector, includeFirstKeySegment);
            string key = fullKey.Split('.').Last();
            object value = valueSelector.Compile().Invoke();

            MemberExpression memberExpr = valueSelector.Body as MemberExpression;

            if (memberExpr != null) { 
               
               DisplayNameAttribute displayNameAttr = 
                  Attribute.GetCustomAttribute(memberExpr.Member, typeof(DisplayNameAttribute)) as DisplayNameAttribute;

               if (displayNameAttr != null)
                  key = displayNameAttr.DisplayName ?? key;
            }

            this.Errors.Add(new SerializableValidationResult(String.Format(CultureInfo.InvariantCulture, message, value, key), new[] { fullKey }));
         }

         return condition;
      }

      public bool Assert<TMember>(OperationResult otherResult, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {

         if (otherResult == null) throw new ArgumentNullException("otherResult");

         return Assert<TMember>(!otherResult.IsError, otherResult.Message, valueSelector, includeFirstKeySegment);
      }

      public bool Not(bool condition, string message, object value = null, string key = null) {
         return !Assert(condition, message, value, key);
      }

      public bool Not(OperationResult otherResult) {
         return !Assert(otherResult);
      }

      public bool Not<TMember>(bool condition, string message, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {
         return !Assert<TMember>(condition, message, valueSelector, includeFirstKeySegment);
      }

      public bool Not<TMember>(OperationResult otherResult, Expression<Func<TMember>> valueSelector, bool includeFirstKeySegment = false) {
         return !Assert<TMember>(otherResult, valueSelector, includeFirstKeySegment);
      }

      public bool Valid(object obj) {

         var validation = new List<ValidationResult>();

         bool valid = Validator.TryValidateObject(obj, new ValidationContext(obj, null, null), validation, validateAllProperties: true);

         AddValidationResults(validation);

         return valid;
      }

      public bool NotValid(object obj) {
         return !Valid(obj);
      }

      // TODO: Add Valid/NotValid overloads for arrays (MemberNames prefixed with [i])

      public bool ValidProperty<TProperty>(Expression<Func<TProperty>> propertySelector) {

         if (propertySelector == null) throw new ArgumentNullException("propertySelector");

         MemberExpression memberExpr = (MemberExpression)propertySelector.Body;
         string propertyName = memberExpr.Member.Name;
         TProperty propertyValue = propertySelector.Compile().Invoke();
         object instance = Expression.Lambda(memberExpr.Expression).Compile().DynamicInvoke();

         var validation = new List<ValidationResult>();

         bool valid = Validator.TryValidateProperty(propertyValue, new ValidationContext(instance, null, null) { MemberName = propertyName }, validation);

         AddValidationResults(validation);

         return valid;
      }

      public bool NotValidProperty<TProperty>(Expression<Func<TProperty>> propertySelector) {
         return !ValidProperty<TProperty>(propertySelector);
      }
   }

   [DataContract(Name = "Error", Namespace = "")]
   public class SerializableValidationResult {

      string _ErrorMessage;
      readonly MemberNamesCollection _MemberNames;

      [DataMember(Name = "Message")]
      public string ErrorMessage {
         get { return _ErrorMessage; }
         set { _ErrorMessage = value; } 
      }

      [DataMember]
      public MemberNamesCollection MemberNames { 
         get { return _MemberNames; } 
      }

      public SerializableValidationResult()
         : this("") { }

      public SerializableValidationResult(string errorMessage)
         : this(errorMessage, new string[0]) { }

      public SerializableValidationResult(string errorMessage, IEnumerable<string> memberNames) {

         if (memberNames == null) throw new ArgumentNullException("memberNames");

         this.ErrorMessage = errorMessage;
         this._MemberNames = new MemberNamesCollection(memberNames.ToList());
      }
   }

   [CollectionDataContract(Namespace = "", ItemName = "Name")]
   public class MemberNamesCollection : Collection<string> {

      public MemberNamesCollection(IList<string> memberNames)
         : base(memberNames) { }
   }
}
