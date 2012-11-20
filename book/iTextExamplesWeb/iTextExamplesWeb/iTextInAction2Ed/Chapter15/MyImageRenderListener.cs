/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class MyImageRenderListener : IRenderListener {
// ===========================================================================
    /** the byte array of the extracted images */
    private List<byte[]> _myImages;
    public List<byte[]> MyImages {
      get { return _myImages; }
    }
    /** the file names of the extracted images */
    private List<string> _imageNames;
    public List<string> ImageNames { 
      get { return _imageNames; }
    } 
// ---------------------------------------------------------------------------
    /**
     * Creates a RenderListener that will look for images.
     */
    public MyImageRenderListener() {
      _myImages = new List<byte[]>();
      _imageNames = new List<string>();
    }
// ---------------------------------------------------------------------------
    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#beginTextBlock()
     */
    public void BeginTextBlock() { }
// ---------------------------------------------------------------------------     
    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#endTextBlock()
     */
    public void EndTextBlock() { }
// ---------------------------------------------------------------------------     
    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#renderImage(
     *     com.itextpdf.text.pdf.parser.ImageRenderInfo)
     */
    public void RenderImage(ImageRenderInfo renderInfo) {
      try {
        PdfImageObject image = renderInfo.GetImage();
        if (image == null 
/*
 * do not attempt to parse => jbig2 decoder not fully implemented.
 * THE JAVA EXAMPLE INCORRECTLY CREATES A CORRUPT JBIG2 IMAGE
 * BECAUSE THERE IS NO EXPLICIT CHECK. I POSTED TWICE TO THE MAILING
 * LIST, SINCE VERSION 5.1.3 BUT THE ERROR HAS NOT BEEN CORRECTED.
 */
          || image.GetImageBytesType() == PdfImageObject.ImageBytesType.JBIG2
        ) 
        return;
        
        _imageNames.Add(string.Format(
          "Image{0}.{1}", 
          renderInfo.GetRef().Number, image.GetFileType()
        ) );
        _myImages.Add(image.GetImageAsBytes());
      }
      catch {
// pass through any other unsupported image types
      }
    }
 // ---------------------------------------------------------------------------     
   /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#renderText(
     *     com.itextpdf.text.pdf.parser.TextRenderInfo)
     */
    public void RenderText(TextRenderInfo renderInfo) { }
// ===========================================================================
  }
}