using System;
using System.IO;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.mc;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.xml;

namespace itextsharp.tests.iTextSharp.text.pdf.mc {
    class AcroFieldsFlattenTest {
        public const String CMP_FOLDER = @"..\..\resources\text\pdf\mc\";
        public const String OUT_FOLDER = @"..\..\target\text\pdf\mc\";

        [SetUp]
        public void Initialize() {
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
        public void fieldFieldsAndFlattenTest() {
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
        }

        [TearDown]
        public void TearDown() {
            //Compare(OUT_FOLDER + flattenAcroFormFileName, CMP_FOLDER + flattenAcroFormFileName);    
        }

        private void Compare(String outPdf, String cmpPdf) {
            CompareTool ct = new CompareTool(outPdf, cmpPdf);
            Assert.IsNull(ct.Compare(OUT_FOLDER, "difference"));

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
