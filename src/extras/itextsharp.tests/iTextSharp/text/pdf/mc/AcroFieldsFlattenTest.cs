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
using System;
using System.IO;
using iTextSharp.testutils;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.mc;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.xml;
using Path = System.IO.Path;

namespace itextsharp.tests.iTextSharp.text.pdf.mc {
    class AcroFieldsFlattenTest {
        public const String CMP_FOLDER = @"..\..\resources\text\pdf\mc\";
        public const String OUT_FOLDER = @"AcroFieldsFlattenTest\";

        [SetUp]
        virtual public void Initialize() {
            DirectoryInfo dir = new DirectoryInfo(OUT_FOLDER);
            if (dir.Exists) {
                foreach (FileInfo file in dir.GetFiles()) {
                    file.Delete();
                }
            } else {
                Directory.CreateDirectory(OUT_FOLDER);
            }
        }

        [Test]
        virtual public void FieldFieldsAndFlattenTest() {
            String acroFormFileName = "SF2809.pdf";
            String filledAcroFormFileName = "SF2809_filled.pdf";
            String flattenAcroFormFileName = "SF2809_alt.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + acroFormFileName);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUT_FOLDER + filledAcroFormFileName, FileMode.Create));
            AcroFields form = stamper.AcroFields;
            foreach (String key in form.Fields.Keys) {
                form.SetField(key, key);
            }
            stamper.Close();

            LoggerFactory.GetInstance().SetLogger(new SysoLogger());
            reader = new PdfReader(OUT_FOLDER + filledAcroFormFileName);
            MCFieldFlattener flattener = new MCFieldFlattener();
            flattener.Process(reader, new FileStream(OUT_FOLDER + flattenAcroFormFileName, FileMode.Create));
            //Compare(OUT_FOLDER + flattenAcroFormFileName, CMP_FOLDER + flattenAcroFormFileName);
        }

        private void Compare(String outPdf, String cmpPdf) {
            CompareTool ct = new CompareTool();
            Assert.IsNull(ct.Compare(outPdf, cmpPdf, OUT_FOLDER, "difference"));

            String outXml = Path.GetFileNameWithoutExtension(outPdf);
            String cmpXml = Path.GetFileNameWithoutExtension(cmpPdf);

            outXml = OUT_FOLDER + outXml.Replace(".pdf", "") + ".xml";
            cmpXml = OUT_FOLDER + "cmp_" + cmpXml.Replace("cmp_", "").Replace(".pdf", "") + ".xml";
            
            PdfReader reader = new PdfReader(outPdf);
            new MyTaggedPdfReaderTool().ConvertToXml(reader, new FileStream(outXml, FileMode.Create));
            reader.Close();

            reader = new PdfReader(outPdf);
            new MyTaggedPdfReaderTool().ConvertToXml(reader, new FileStream(cmpXml, FileMode.Create));
            reader.Close();

            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            Assert.True(xmldiff.Compare(cmpXml, outXml, false));
        }

        #region Nested type: MyMarkedContentRenderFilter

        private class MyMarkedContentRenderFilter : MarkedContentRenderFilter {
            private readonly int mcid;

            public MyMarkedContentRenderFilter(int mcid)
                : base(mcid) {
                this.mcid = mcid;
            }

            public override bool AllowText(TextRenderInfo renderInfo) {
                return renderInfo.HasMcid(mcid, true);
            }
        }

        #endregion

        #region Nested type: MyTaggedPdfReaderTool

        private class MyTaggedPdfReaderTool : TaggedPdfReaderTool {
            public override void ParseTag(String tag, PdfObject obj, PdfDictionary page) {
                if (obj is PdfNumber) {
                    PdfNumber mcid = (PdfNumber)obj;
                    RenderFilter filter = new AcroFieldsFlattenTest.MyMarkedContentRenderFilter(mcid.IntValue);
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    FilteredTextRenderListener listener = new FilteredTextRenderListener(
                        strategy, filter);
                    PdfContentStreamProcessor processor = new PdfContentStreamProcessor(
                        listener);
                    processor.ProcessContent(PdfReader.GetPageContent(page), page
                                                                                 .GetAsDict(PdfName.RESOURCES));
                    outp.Write(XMLUtil.EscapeXML(listener.GetResultantText(), true));
                } else {
                    base.ParseTag(tag, obj, page);
                }
            }

            public override void InspectChildDictionary(PdfDictionary k) {
                InspectChildDictionary(k, true);
            }
        }

        #endregion
    }
}
