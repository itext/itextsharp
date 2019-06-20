/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
