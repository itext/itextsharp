using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.examples {
    internal class XMLWorkerHTMLExample : Setup {
        /**
	 * This method shows you how to setup the processing yourself. This is how
	 * it's done in the {@link XMLWorkerHelper}
	 *
	 * @throws IOException if something with IO went wrong.
	 * @throws DocumentException if something with the document goes wrong.
	 */
        private const string TARGET = @"";
        private const string RESOURCES = @"..\..\resources\";

        [Test]
        virtual public void SetupDefaultProcessingYourself() {
            Document doc = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc,
                new FileStream(TARGET + "columbus2.pdf", FileMode.Create));
            doc.Open();
            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext,
                new PdfWriterPipeline(doc, writer)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(worker);
            p.Parse(File.OpenRead(RESOURCES + @"\examples\columbus.html"));
            doc.Close();
        }

        private class CustomImageProvider : AbstractImageProvider {
            public override string GetImageRootPath() {
                return "http://www.gutenberg.org/dirs/1/8/0/6/18066/18066-h/";
            }
        }

        /**
	 * Define an ImageRoot. You'll see that the document columbus3.pdf now has
	 * images.
	 *
	 * @throws IOException if something with IO went wrong.
	 * @throws DocumentException if something with the document goes wrong.
	 */

        [Test]
        virtual public void AddingAnImageRoot() {
            Document doc = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc,
                new FileStream(TARGET + "columbus3.pdf", FileMode.Create));
            doc.Open();
            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetImageProvider(new CustomImageProvider()).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext,
                new PdfWriterPipeline(doc, writer)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(worker);
            p.Parse(File.OpenRead(RESOURCES + @"\examples\columbus.html"));
            doc.Close();
        }


        private class CustomLinkProvider : ILinkProvider {
            virtual public string GetLinkRoot() {
                return "http://www.gutenberg.org/dirs/1/8/0/6/18066/18066-h/";
            }
        }

        /**
	 * Define a LinProvider. You'll see that the document columbus3.pdf now
	 * links that point to the right url.
	 *
	 * @throws IOException if something with IO went wrong.
	 * @throws DocumentException if something with the document goes wrong.
	 */

        [Test]
        virtual public void AddingALinkProvider() {
            Document doc = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(TARGET + "columbus3.pdf", FileMode.Create));
            doc.Open();
            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetLinkProvider(new CustomLinkProvider()).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext,
                new PdfWriterPipeline(doc, writer)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(worker);
            p.Parse(File.OpenRead(RESOURCES + @"\examples\columbus.html"));
            doc.Close();
        }
    }
}
