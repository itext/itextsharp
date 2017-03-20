/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
