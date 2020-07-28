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
                                            + "<!DOCTYPE foo [ <!ENTITY xxe SYSTEM \"../../resources/text/xmp/impl/xxe-data.txt\" > ]>\n"
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