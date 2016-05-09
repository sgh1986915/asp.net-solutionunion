using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace SolutionUnion {

   public class DisplayNameLocalizedAttribute : DisplayNameAttribute {

      readonly ResourceManager resourceMng;
      readonly string resourceKey;

      public override string DisplayName {
         get {
            if (resourceMng != null)
               return resourceMng.GetString(resourceKey);

            return null;
         }
      }

      public DisplayNameLocalizedAttribute(string resourceKey, Type resourceType) {

         this.resourceKey = resourceKey;

         PropertyInfo resMngProp = resourceType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(p => p.PropertyType == typeof(ResourceManager));

         if (resMngProp != null)
            resourceMng = (ResourceManager)resMngProp.GetValue(null, null);
      } 
   }
}
