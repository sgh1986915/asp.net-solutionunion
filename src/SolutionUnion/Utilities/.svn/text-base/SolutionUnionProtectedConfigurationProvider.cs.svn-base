using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace SolutionUnion {

   sealed class SolutionUnionProtectedConfigurationProvider 
      : ProtectedConfigurationProvider {

      readonly Cryptographer crypto = new Cryptographer();

      public override XmlNode Encrypt(XmlNode node) {

         string encryptedData = crypto.Encrypt(node.OuterXml);

         XmlDocument doc = new XmlDocument();
         XmlElement docEl = doc.CreateElement("EncryptedData");
         docEl.InnerText = encryptedData;
         doc.AppendChild(docEl);

         return docEl;
      }

      public override XmlNode Decrypt(XmlNode encryptedNode) {

         string decryptedData = crypto.Decrypt(encryptedNode.InnerText);

         XmlDocument doc = new XmlDocument();
         doc.PreserveWhitespace = true;
         doc.LoadXml(decryptedData);

         return doc.DocumentElement;
      }
   }
}
