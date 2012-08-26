using System;
using System.IO;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.xml.xmp;
using iTextSharp.text.pdf.intern;

namespace iTextSharp.text.pdf
{   
/**
 * @see PdfWriter
 */
public class PdfAWriter : PdfWriter {

    /**
     * Use this method to get an instance of the <CODE>PdfWriter</CODE>.
     * @param	document	The <CODE>Document</CODE> that has to be written
     * @param	os	The <CODE>Stream</CODE> the writer has to write to.
     * @param conformanceLevel PDF/A conformance level of a new PDF document
     * @return	a new <CODE>PdfWriter</CODE>
     * @throws	DocumentException on error
     */
    public static PdfAWriter GetInstance(Document document, Stream os, PdfAConformanceLevel conformanceLevel)
    {
        PdfDocument pdf = new PdfDocument();
        document.AddDocListener(pdf);
        PdfAWriter writer = new PdfAWriter(pdf, os, conformanceLevel);
        pdf.AddWriter(writer);
        return writer;
    }

    /**
     * Use this method to get an instance of the <CODE>PdfWriter</CODE>.
     * @param	document	The <CODE>Document</CODE> that has to be written
     * @param	os	The <CODE>Stream</CODE> the writer has to write to.
     * @param listener A <CODE>DocListener</CODE> to pass to the PdfDocument.
     * @param conformanceLevel PDF/A conformance level of a new PDF document
     * @return	a new <CODE>PdfWriter</CODE>
     * @throws	DocumentException on error
     */
    public static PdfAWriter GetInstance(Document document, Stream os, IDocListener listener, PdfAConformanceLevel conformanceLevel){
        PdfDocument pdf = new PdfDocument();
        pdf.AddDocListener(listener);
        document.AddDocListener(pdf);
        PdfAWriter writer = new PdfAWriter(pdf, os, conformanceLevel);
        pdf.AddWriter(writer);
        return writer;
    }

    /**
     *
     * @param writer
     * @param conformanceLevel
     */
    static public void SetPdfVersion(PdfWriter writer, PdfAConformanceLevel conformanceLevel) {
        switch (conformanceLevel) {
            case PdfAConformanceLevel.PDF_A_1A:
            case PdfAConformanceLevel.PDF_A_1B:
                writer.PdfVersion = PdfWriter.VERSION_1_4;
                break;
            case PdfAConformanceLevel.PDF_A_2A:
            case PdfAConformanceLevel.PDF_A_2B:
            case PdfAConformanceLevel.PDF_A_2U:
                writer.PdfVersion = PdfWriter.VERSION_1_7;
                break;
            case PdfAConformanceLevel.PDF_A_3A:
            case PdfAConformanceLevel.PDF_A_3B:
            case PdfAConformanceLevel.PDF_A_3U:
                writer.PdfVersion = PdfWriter.VERSION_1_7;
                break;
            default:
                writer.PdfVersion = PdfWriter.VERSION_1_4;
                break;
        }
    }

    /**
     * @see PdfWriter#setOutputIntents(String, String, String, String, ICC_Profile)
     */
    override public void SetOutputIntents(String outputConditionIdentifier, String outputCondition, String registryName, String info, ICC_Profile colorProfile){
        base.SetOutputIntents(outputConditionIdentifier, outputCondition, registryName, info, colorProfile);
        PdfArray a = extraCatalog.GetAsArray(PdfName.OUTPUTINTENTS);
        if (a != null) {
            PdfDictionary d = a.GetAsDict(0);
            if (d != null) {
                d.Put(PdfName.S, PdfName.GTS_PDFA1);
            }
        }
    }

    /**
     * Always throws an exception since PDF/X conformance level cannot be set for PDF/A conformant documents.
     * @param pdfx
     */
    public void SetPDFXConformance(int pdfx) {
        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("pdfx.conformance.cannot.be.set.for.PdfAWriter.instance"));
    }

    /**
     * @see com.itextpdf.text.pdf.PdfWriter#isPdfIso()
     */
    override public bool IsPdfIso() {
        return pdfIsoConformance.IsPdfIso();
    }

    /**
     * @param conformanceLevel PDF/A conformance level of a new PDF document
     */
    internal protected PdfAWriter(PdfAConformanceLevel conformanceLevel) : base()
    {
        ((IPdfAConformance)pdfIsoConformance).SetConformanceLevel(conformanceLevel);
        SetPdfVersion(this, conformanceLevel);
    }

    /**
     * Constructs a <CODE>PdfAWriter</CODE>.
     * <P>
     * Remark: a PdfAWriter can only be constructed by calling the method <CODE>getInstance(Document document, Stream os, PdfAconformanceLevel conformanceLevel)</CODE>.
     * @param document the <CODE>PdfDocument</CODE> that has to be written
     * @param os the <CODE>Stream</CODE> the writer has to write to
     * @param conformanceLevel PDF/A conformance level of a new PDF document
     */
    internal protected PdfAWriter(PdfDocument document, Stream os, PdfAConformanceLevel conformanceLevel)
        : base(document, os)
    {
        ((IPdfAConformance)pdfIsoConformance).SetConformanceLevel(conformanceLevel);
        SetPdfVersion(this, conformanceLevel);
    }

    /**
     * @see com.itextpdf.text.pdf.PdfWriter#getTtfUnicodeWriter()
     */
    override protected TtfUnicodeWriter GetTtfUnicodeWriter() {
        if (ttfUnicodeWriter == null)
            ttfUnicodeWriter = new PdfATtfUnicodeWriter(this);
        return ttfUnicodeWriter;
    }

    /**
     * @see PdfWriter#getXmpWriter(java.io.MemoryStream, com.itextpdf.text.pdf.PdfDocument.PdfInfo)
     */
    override protected XmpWriter GetXmpWriter(MemoryStream baos, PdfDocument.PdfInfo info)
    {
        if (xmpWriter == null)
            xmpWriter = new PdfAXmpWriter(baos, info, ((IPdfAConformance)pdfIsoConformance).GetConformanceLevel());
        return xmpWriter;
    }

    /**
     * @see PdfWriter#checkPdfIsoConformance(int, Object)
     */
    override protected void CheckPdfIsoConformance(int key, Object obj1)
    {
        PdfAConformanceImp.CheckPdfAConformance(this, key, obj1);
    }

    /**
     * @see com.itextpdf.text.pdf.PdfWriter#getPdfIsoConformance()
     */
    override public IPdfIsoConformance GetPdfIsoConformance()
    {
        return new PdfAConformanceImp();
    }
}

}
