using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.examples {
    /**
 * @author itextpdf.com
 *
 */

    internal class XMLWorkerHelperExample : Setup {
        private const string TARGET = @"";
        private const string RESOURCES = @"..\..\resources\";

        /**
	 * Parse html to a PDF.
	 * With the XMLWorkerHelper this is done in no time. Create a Document and a
	 * PdfWriter. Don't forget to open the document and call
	 * <code>XMLWorkerHelper.getInstance().parseXHtml()</code>. This test takes
	 * html from <code>columbus.html</code>. This document contains &lt;img&gt;
	 * tags with relative src attributes. You'll see that the images are not
	 * added, unless they are absolute url's or file paths.
	 *
	 * @throws DocumentException
	 * @throws IOException
	 */

        [Test]
        virtual public void DefaultSetup() {
            Document doc = new Document(PageSize.A4);
            PdfWriter instance = PdfWriter.GetInstance(doc,
                new FileStream(TARGET + "columbus.pdf", FileMode.Create));
            doc.Open();
            XMLWorkerHelper.GetInstance()
                .ParseXHtml(instance, doc, File.OpenRead(RESOURCES + @"\examples\columbus.html"), Encoding.Default);
            doc.Close();
        }

        private class CustomElementHandler : IElementHandler {
            virtual public void Add(IWritable w) {
                if (w is WritableElement) {
                    IList<IElement> elements = ((WritableElement) w).Elements();
                    // do something with the lists of elements
                }
            }
        }

        /**
	 * Create lists of {@link Element} instead of a document
	 * @throws IOException
	 */

        [Test]
        virtual public void DefaultSetupWithoutDocument() {
            XMLWorkerHelper.GetInstance()
                .ParseXHtml(new CustomElementHandler(), File.OpenRead(RESOURCES + @"\examples\columbus.html"), Encoding.Default);
        }
    }
}
