/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class Calculator : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "calculator.pdf";
    /** Path to the resource. */
    public const string RESOURCE = "calculator.js";
// ---------------------------------------------------------------------------            
    /** The font that will be used in the appearances. */
    public BaseFont bf;
    /** Position of the digits */
    Rectangle[] digits = new Rectangle[10];
    /** Position of the operators. */
    Rectangle plus, minus, mult, div, equals;
    /** Position of the other annotations */
    Rectangle clearEntry, clear, result, move;
// ---------------------------------------------------------------------------                
    /**
     * Initializes the font
     */
    public void InitializeFont() {
      bf = BaseFont.CreateFont();
    }
// --------------------------------------------------------------------------- 
    protected string jsString;
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Calculator calc = new Calculator();
        calc.InitializeFont();
        calc.InitializeRectangles();
        zip.AddEntry(RESULT, calc.CreatePdf());
        zip.AddEntry(RESOURCE, calc.jsString);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Initializes the rectangles for the calculator keys.
     */
    public void InitializeRectangles() {
      digits[0] = CreateRectangle(3, 1, 1, 1);
      digits[1] = CreateRectangle(1, 3, 1, 1);
      digits[2] = CreateRectangle(3, 3, 1, 1);
      digits[3] = CreateRectangle(5, 3, 1, 1);
      digits[4] = CreateRectangle(1, 5, 1, 1);
      digits[5] = CreateRectangle(3, 5, 1, 1);
      digits[6] = CreateRectangle(5, 5, 1, 1);
      digits[7] = CreateRectangle(1, 7, 1, 1);
      digits[8] = CreateRectangle(3, 7, 1, 1);
      digits[9] = CreateRectangle(5, 7, 1, 1);
      plus = CreateRectangle(7, 7, 1, 1);
      minus = CreateRectangle(9, 7, 1, 1);
      mult = CreateRectangle(7, 5, 1, 1);
      div = CreateRectangle(9, 5, 1, 1);
      equals = CreateRectangle(7, 1, 3, 1);
      clearEntry = CreateRectangle(7, 9, 1, 1);
      clear = CreateRectangle(9, 9, 1, 1);
      result = CreateRectangle(1, 9, 5, 1);
      move = CreateRectangle(8, 3, 1, 1);
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document for PdfReader.
     * @param stream the stream to create new PDF document
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document(new Rectangle(360, 360))) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);      
          // step 3
          document.Open();
          jsString = File.ReadAllText(
            Path.Combine(Utility.ResourceJavaScript, RESOURCE)
          );
          writer.AddJavaScript(jsString);
          // step 4
          // add the keys for the digits
          for (int i = 0; i < 10; i++) {
            AddPushButton(
              writer, digits[i],
              i.ToString(), "this.augment(" + i + ")"
            );
          }
          // add the keys for the operators
          AddPushButton(writer, plus, "+", "this.register('+')");
          AddPushButton(writer, minus, "-", "this.register('-')");
          AddPushButton(writer, mult, "x", "this.register('*')");
          AddPushButton(writer, div, ":", "this.register('/')");
          AddPushButton(writer, equals, "=", "this.calculateResult()");
          // add the other keys
          AddPushButton(writer, clearEntry, "CE", "this.reset(false)");
          AddPushButton(writer, clear, "C", "this.reset(true)");
          AddTextField(writer, result, "result");
          AddTextField(writer, move, "move");
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Add a text field.
     * @param writer the PdfWriter
     * @param rect the position of the text field
     * @param name the name of the text field
     */
    public void AddTextField(PdfWriter writer, Rectangle rect, String name) {
      PdfFormField field = PdfFormField.CreateTextField(
        writer, false, false, 0
      );
      field.FieldName = name;
      field.SetWidget(rect, PdfAnnotation.HIGHLIGHT_NONE);
      field.Quadding = PdfFormField.Q_RIGHT;
      field.SetFieldFlags(PdfFormField.FF_READ_ONLY);
      writer.AddAnnotation(field);
    }
// ---------------------------------------------------------------------------    
    /**
     * Create a pushbutton for a key
     * @param writer the PdfWriter
     * @param rect the position of the key
     * @param btn the label for the key
     * @param script the script to be executed when the button is pushed
     */
    public void AddPushButton(PdfWriter writer, Rectangle rect,
      String btn, String script)
    {
      float w = rect.Width;
      float h = rect.Height;
      PdfFormField pushbutton = PdfFormField.CreatePushButton(writer);
      pushbutton.FieldName = "btn_" + btn;
      pushbutton.SetWidget(rect, PdfAnnotation.HIGHLIGHT_PUSH);
      PdfContentByte cb = writer.DirectContent;
      pushbutton.SetAppearance(
        PdfAnnotation.APPEARANCE_NORMAL,
        CreateAppearance(cb, btn, BaseColor.GRAY, w, h)
      );
      pushbutton.SetAppearance(
        PdfAnnotation.APPEARANCE_ROLLOVER,
        CreateAppearance(cb, btn, BaseColor.RED, w, h)
      );
      pushbutton.SetAppearance(
        PdfAnnotation.APPEARANCE_DOWN,
        CreateAppearance(cb, btn, BaseColor.BLUE, w, h)
      );
      pushbutton.SetAdditionalActions(
        PdfName.U,
        PdfAction.JavaScript(script, writer)
      );
      pushbutton.SetAdditionalActions(
        PdfName.E, PdfAction.JavaScript(
          "this.showMove('" + btn + "');", writer
        )
      );
      pushbutton.SetAdditionalActions(
        PdfName.X, PdfAction.JavaScript(
          "this.showMove(' ');", writer
        )
      );
      writer.AddAnnotation(pushbutton);
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates an appearance for a key
     * @param cb the canvas
     * @param btn the label for the key
     * @param color the color of the key
     * @param w the width
     * @param h the height
     * @return an appearance
     */
    public PdfAppearance CreateAppearance(
      PdfContentByte cb, String btn, BaseColor color, float w, float h)
    {
      PdfAppearance app = cb.CreateAppearance(w, h);
      app.SetColorFill(color);
      app.Rectangle(2, 2, w - 4, h - 4);
      app.Fill();
      app.BeginText();
      app.SetColorFill(BaseColor.BLACK);
      app.SetFontAndSize(bf, h / 2);
      app.ShowTextAligned(Element.ALIGN_CENTER, btn, w / 2, h / 4, 0);
      app.EndText();
      return app;
    }
// ---------------------------------------------------------------------------    
    /**
     * Create a rectangle object for a key.
     * @param column column of the key on the key pad
     * @param row row of the key on the key pad
     * @param width width of the key
     * @param height height of the key
     * @return a rectangle defining the position of a key.
     */
    public Rectangle CreateRectangle(
      int column, int row, int width, int height) 
    {
      column = column * 36 - 18;
      row = row * 36 - 18;
      return new Rectangle(
        column, row,
        column + width * 36, row + height * 36
      );
    }
// ===========================================================================
  }
}