using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SolutionUnion {
   
   public class Contact {

      internal event PropertyChangedEventHandler PropertyChanged;

      string _Name, _Email;

      public long Id { get; set; }
      public string ListId { get; set; }
      public long AccountId { get; set; }
      
      public string Name {
         get { return _Name; }
         set {
            if (_Name != value) {
               _Name = value;
               SendPropertyChanged("Name");
            }
         }
      }

      public string Email {
         get { return _Email; }
         set {
            if (_Email != value) {
               _Email = value;
               SendPropertyChanged("Email");
            }
         }
      }
      
      public bool IsDefault { get; set; }
      public DateTime Created { get; set; }

      void SendPropertyChanged(string propertyName) {

         var propChanged = this.PropertyChanged;

         if (propChanged != null)
            propChanged(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
