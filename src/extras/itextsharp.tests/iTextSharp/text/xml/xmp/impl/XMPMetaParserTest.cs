/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2022 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.IO;
using System.Xml;
using iTextSharp.xmp;
using iTextSharp.xmp.impl;
using NUnit.Framework;

namespace iText.Kernel.XMP.Impl
{
    
    using XMPMeta = IXmpMeta;
    
    [TestFixture]
    public class XMPMetaParserTest
    {
        private const String XMP_WITH_XXE = "<?xpacket begin=\"\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n"
                                            + "<!DOCTYPE foo [ <!ENTITY xxe SYSTEM \"../../../resources/text/xmp/impl/xxe-data.txt\" > ]>\n"
                                            + "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\">\n"
                                            + "    <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n"
                                            + "        <rdf:Description rdf:about=\"\" xmlns:pdfaid=\"http://www.aiim.org/pdfa/ns/id/\">\n"
                                            + "            <pdfaid:part>&xxe;1</pdfaid:part>\n"
                                            + "            <pdfaid:conformance>B</pdfaid:conformance>\n"
                                            + "        </rdf:Description>\n" + "    </rdf:RDF>\n"
                                            + "</x:xmpmeta>\n" + "<?xpacket end=\"r\"?>";
        
        [Test]
        public virtual void XxeTestFromString()
        {
            XmlException ex = Assert.Throws<XmlException>(delegate {
                XmpMetaParser.Parse(XMP_WITH_XXE, null);
            });
            // do not check the expected message as it differs depending on .net version
        }
        
        [Test]
        public virtual void XxeTestFromByteBuffer()
        {
            XmpException ex = Assert.Throws<XmpException>(delegate {
                XmpMetaParser.Parse(System.Text.Encoding.UTF8.GetBytes(XMP_WITH_XXE), null);
            });

            Assert.AreEqual("Unsupported Encoding", ex.Message);
        }

        [Test]
        public virtual void XxeTestFromInputStream()
        {
            Stream inputStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XMP_WITH_XXE));
            XmpException ex = Assert.Throws<XmpException>(delegate {
                XmpMetaParser.Parse(inputStream, null);
            });

            Assert.AreEqual("Unsupported Encoding", ex.Message);
        }
    }
}
