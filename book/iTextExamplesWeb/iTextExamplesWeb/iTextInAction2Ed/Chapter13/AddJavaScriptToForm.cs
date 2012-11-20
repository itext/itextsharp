/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class AddJavaScriptToForm : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string ORIGINAL = "form_without_js.pdf";
    /** The resulting PDF file. */
    public const string RESULT = "form_with_js.pdf";
    /** Path to the resources. */
    public readonly string RESOURCE = Path.Combine(
      Utility.ResourceJavaScript, "extra.js"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(RESOURCE, ""); 
    	  AddJavaScriptToForm example = new AddJavaScriptToForm();
    	  byte[] pdf = example.CreatePdf();
  	    zip.AddEntry(ORIGINAL, pdf);
  	    zip.AddEntry(RESULT, example.ManipulatePdf(pdf));         
        zip.Save(stream);             
      }
    }     
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
	  public byte[] CreatePdf() {
		  using (MemoryStream ms = new MemoryStream()) {
		    using (Document document = new Document()) {
		      PdfWriter writer = PdfWriter.GetInstance(document, ms);
		      document.Open();
       
		      BaseFont bf = BaseFont.CreateFont();
		      PdfContentByte directcontent = writer.DirectContent;
		      directcontent.BeginText();
		      directcontent.SetFontAndSize(bf, 12);
		      directcontent.ShowTextAligned(Element.ALIGN_LEFT, "Married?", 36, 770, 0);
		      directcontent.ShowTextAligned(Element.ALIGN_LEFT, "YES", 58, 750, 0);
		      directcontent.ShowTextAligned(Element.ALIGN_LEFT, "NO", 102, 750, 0);
		      directcontent.ShowTextAligned(
		        Element.ALIGN_LEFT, "Name partner?", 36, 730, 0
		      );
		      directcontent.EndText();
       
		      PdfFormField married = PdfFormField.CreateRadioButton(writer, true);
		      married.FieldName = "married";
		      married.ValueAsName = "yes";
		      Rectangle rectYes = new Rectangle(40, 766, 56, 744);
		      RadioCheckField yes = new RadioCheckField(writer, rectYes, null, "yes");
		      yes.Checked = true;
		      married.AddKid(yes.RadioField);
		      Rectangle rectNo = new Rectangle(84, 766, 100, 744);
		      RadioCheckField no = new RadioCheckField(writer, rectNo, null, "no");
		      no.Checked = false;
		      married.AddKid(no.RadioField);
		      writer.AddAnnotation(married);
       
		      Rectangle rect = new Rectangle(40, 710, 200, 726);
		      TextField partner = new TextField(writer, rect, "partner");
		      partner.BorderColor = GrayColor.GRAYBLACK;
		      partner.BorderWidth = 0.5f;
		      writer.AddAnnotation(partner.GetTextField());
		    }
        return ms.ToArray();
		  }
	  }
// ---------------------------------------------------------------------------           
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
    	PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
      	using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          stamper.Writer.AddJavaScript(File.ReadAllText(RESOURCE));
          AcroFields form = stamper.AcroFields;
          AcroFields.Item fd = form.GetFieldItem("married");
   
          PdfDictionary dictYes =
        	  (PdfDictionary) PdfReader.GetPdfObject(fd.GetWidgetRef(0));
          PdfDictionary yesAction = dictYes.GetAsDict(PdfName.AA);
          if (yesAction == null) yesAction = new PdfDictionary();
          yesAction.Put(
            new PdfName("Fo"),
        	  PdfAction.JavaScript("ReadOnly = false);", stamper.Writer)
          );
          dictYes.Put(PdfName.AA, yesAction);
   
          PdfDictionary dictNo =
        	  (PdfDictionary) PdfReader.GetPdfObject(fd.GetWidgetRef(1));
          PdfDictionary noAction = dictNo.GetAsDict(PdfName.AA);
          if (noAction == null) noAction = new PdfDictionary();
          noAction.Put(
            new PdfName("Fo"),
        	  PdfAction.JavaScript("ReadOnly = true);", stamper.Writer)
          );
          dictNo.Put(PdfName.AA, noAction);
   
		      PdfWriter writer = stamper.Writer;
		      PushbuttonField button = new PushbuttonField(
		        writer, new Rectangle(40, 690, 200, 710), "submit"
				  );
		      button.Text = "validate and submit";
		      button.Options = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
		      PdfFormField validateAndSubmit = button.Field;
		      validateAndSubmit.Action = PdfAction.JavaScript(
		        "validate();", stamper.Writer
		      );
		      stamper.AddAnnotation(validateAndSubmit, 1);
		    }
		    return ms.ToArray();
		  }
    }    
// ===========================================================================
  }
}