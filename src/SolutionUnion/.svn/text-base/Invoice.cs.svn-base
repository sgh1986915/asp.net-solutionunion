using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SolutionUnion.Resources.Validation;
using System.Collections.ObjectModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.Hosting;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using SolutionUnion.Repositories;
using SolutionUnion.Payment;

namespace SolutionUnion {

   public class Invoice {

      public long Id { get; set; }
      public long UserId { get; set; }
      public decimal Discount { get; set; }
      public decimal Subtotal { get; set; }
      public decimal Total { get; set; }
      public bool IsPaid { get; set; }
      public string TransactionId { get; set; }
      public string CreditCardLastFour { get; set; }
      public string Error { get; set; }
      public DateTime? PaidDate { get; set; }
      public DateTime Created { get; set; }

      public virtual User User { get; set; }
      public virtual InvoicePdf InvoicePdf { get; set; }

      public virtual ICollection<InvoiceItemLine> InvoiceItemLines { get; private set; }

      public static Invoice Find(long id) {

         if (!User.IsAuthenticated)
            throw new InvalidOperationException();

         var repo = Repository<Invoice>.GetInstance();

         var query = repo.CreateQuery().Where(i => i.Id == id);

         if (!User.CurrentPrincipal.IsInRole(UserRole.Administrator))
            query = query.Where(i => i.UserId == User.CurrentUserId);

         return query.SingleOrDefault();
      }

      public Invoice() {
         this.InvoiceItemLines = new Collection<InvoiceItemLine>();
      }

      public OperationResult Pay(long? creditCardId = null) {

         if (this.IsPaid)
            return new ErrorResult("Invoice already paid.");

         CreditCard card = null;

         if (creditCardId.HasValue) {

            card = this.User.CreditCardsQuery.SingleOrDefault(c => c.Id == creditCardId.Value);

            if (card == null)
               return new ErrorResult(String.Format(CultureInfo.InvariantCulture, "Couldn't find credit card {0}.", creditCardId.Value));

         } else {
            
            card = this.User.CreditCardsQuery.SingleOrDefault(c => c.IsDefault);

            if (card == null)
               return new ErrorResult("Couldn't find a default credit card.");
         }

         var payment = ServiceLocator.Current.GetInstance<PaymentProcessor>();

         var result = payment.Pay(this, card);

         var repo = Repository<Invoice>.GetInstance();

         if (result.IsError) {

            this.Error = result.Message;

            repo.SaveChanges(this);

            return new InvoicePayError(
               new InvoicePayErrorCreditCardInfo(card.Id, card.LastFour)
               , (from c in this.User.CreditCardsQuery
                  where c.Id != card.Id
                  select new { c.Id, c.LastFour })
                  .ToArray()
                  .Select(c => new InvoicePayErrorCreditCardInfo(c.Id, c.LastFour))
                  .ToArray()
               , String.Format("Couldn't pay invoice with card ending in {0}, payment processor said: {1}", card.LastFour, this.Error));
         }

         var resultOK = (PaymentResult)result;

         this.IsPaid = true;
         this.TransactionId = resultOK.TransactionId;
         this.CreditCardLastFour = card.LastFour;
         this.PaidDate = DateTime.Now;
         this.Error = null;
         this.InvoicePdf = CreatePdf();

         repo.SaveChanges(this);

         return new SuccessfulResult("Invoice paid successfully.");
      }

      public OperationResult Download(Stream output, out string filename, out string contentType) {

         filename = String.Format(CultureInfo.InvariantCulture, "invoice-{0:000}.pdf", this.Id);
         contentType = "application/pdf";

         if (this.InvoicePdf == null) {

            this.InvoicePdf = CreatePdf();

            var repo = Repository<Invoice>.GetInstance();

            repo.SaveChanges(this);
         }

         byte[] buffer = this.InvoicePdf.Pdf;

         output.Write(buffer, 0, buffer.Length);

         return new SuccessfulResult();
      }

      InvoicePdf CreatePdf() {

         using (var memStream = new MemoryStream()) {

            BuildPdf(memStream);

            return new InvoicePdf {
               Pdf = memStream.ToArray()
            };
         }
      }

      void BuildPdf(Stream destination) {

         using (Document doc = new Document()) {

            PdfWriter writer = PdfWriter.GetInstance(doc, destination);

            doc.Open();

            Rectangle page = doc.PageSize;
            PdfPTable head = new PdfPTable(1);
            head.TotalWidth = page.Width;
            Phrase phrase = new Phrase(
              this.Created.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + " GMT",
              new Font(Font.FontFamily.COURIER, 8)
            );
            PdfPCell c = new PdfPCell(phrase);
            c.Border = Rectangle.NO_BORDER;
            c.VerticalAlignment = Element.ALIGN_TOP;
            c.HorizontalAlignment = Element.ALIGN_CENTER;
            head.AddCell(c);
            head.WriteSelectedRows(
               // first/last row; -1 writes all rows
              0, -1,
               // left offset
              0,
               // ** bottom** yPos of the table
              page.Height - doc.TopMargin + head.TotalHeight + 20,
              writer.DirectContent
            );

            /* 
             * add image to document
             */
            Image gif = Image.GetInstance(
               HostingEnvironment.MapPath("~/Content/images/header-forgot-pass.gif")
            );
            gif.Alignment = Image.MIDDLE_ALIGN;
            // downsize the image by specified percentage        
            gif.ScalePercent(50f);
            doc.Add(gif);

            string[] col = { "No.", "Description", "Amount" };
            PdfPTable table = new PdfPTable(3);
            /*
            * default table width => 80%
            */
            table.WidthPercentage = 100;
            // then set the column's __relative__ widths
            table.SetWidths(new Single[] { 1, 5, 4 });
            /*
            * by default tables 'collapse' on surrounding elements,
            * so you need to explicitly add spacing
            */
            table.SpacingBefore = 10;

            for (int i = 0; i < col.Length; ++i) {
               PdfPCell cell = new PdfPCell(new Phrase(col[i]));
               cell.BackgroundColor = new BaseColor(204, 204, 204);
               table.AddCell(cell);
            }

            int index = 0;

            foreach (var item in this.InvoiceItemLines) {
               index++;
               table.AddCell(index.ToString(CultureInfo.InvariantCulture));
               table.AddCell(item.Description);
               table.AddCell(item.Amount.ToString("c"));
            }

            doc.Add(table);
         }
      }
   }
}
