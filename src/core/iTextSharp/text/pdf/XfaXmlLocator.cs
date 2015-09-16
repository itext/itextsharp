using System.Xml;
using iTextSharp.text.pdf.security;
/*
 * $Id: XfaXmlLocator.java 5830 2013-05-31 09:29:15Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Pavel Alay, Bruno Lowagie, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
namespace iTextSharp.text.pdf
{
    /**
     * Helps to locate xml stream inside PDF document with Xfa form.
     */
    public class XfaXmlLocator : IXmlLocator
    {
        public XfaXmlLocator(PdfStamper stamper) {
            this.stamper = stamper;
            CreateXfaForm();
        }

        private PdfStamper stamper;
        private XfaForm xfaForm;
        private string encoding;

        virtual protected void CreateXfaForm() {
            xfaForm = new XfaForm(stamper.Reader);
        }

        /**
         * Gets Document to sign
         */
        virtual public XmlDocument GetDocument() {
            return xfaForm.DomDocument;
        }

        /**
         * Save document as single XML stream in AcroForm.
         * @param document signed document
         * @throws IOException
         * @throws DocumentException
         */
        virtual public void SetDocument(XmlDocument document) {
            byte[] outerXml = System.Text.Encoding.UTF8.GetBytes(document.OuterXml);
            //Create PdfStream
            PdfIndirectReference iref = stamper.Writer.
                    AddToBody(new PdfStream(outerXml)).IndirectReference;
            stamper.Reader.AcroForm.Put(PdfName.XFA, iref);
        }

        virtual public string GetEncoding() {
            return encoding;
        }

        virtual public void SetEncoding(string encoding) {
            this.encoding = encoding;
        }
    }
}
