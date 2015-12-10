using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.pdfa.tests.iTextSharp.text.pdfa {
    
    [TestFixture]
    class ConcurrentWriterTest {

        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        
        [Test]
        public void test() {

            PDFAConcurrencyIssue prog = new PDFAConcurrencyIssue();

            Console.WriteLine("Creating only PDF/A documents");
            prog.RunTest(true, false);


            Console.WriteLine("Creating only normal PDF/A documents");
            prog.RunTest(false, true);


            Console.WriteLine("Creating PDF/A and normal documents");
            prog.RunTest(true, true);

            Console.WriteLine("Done");

        }

        public class PDFAConcurrencyIssue {

            public PDFAConcurrencyIssue() {
                FontFactory.Register(RESOURCES + "FreeSans.ttf");
            }


            class Archive {
                public void Run() {
                    try {
                        CreatePdfA(PdfAConformanceLevel.PDF_A_1B);
                    } catch (Exception e) {
                        Console.WriteLine(e.StackTrace);
                        throw new SystemException(e.Message);
                    }
                }

                public void CreatePdfA(PdfAConformanceLevel level) {
                    Document doc = new Document();

                    MemoryStream stream = new MemoryStream();
                    PdfAWriter writer = PdfAWriter.GetInstance(doc, stream, level);

                    doc.Open();

                    ICC_Profile icc = ICC_Profile.GetInstance(File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read));
                    writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
                    writer.CreateXmpMetadata();

                    Font font = FontFactory.GetFont("FreeSans", BaseFont.IDENTITY_H, true, 12);
                    doc.Add(new Phrase("Hello, " + level + " world!", font));
                    doc.Add(new Phrase(Guid.NewGuid().ToString(), font));

                    doc.Close();
                }
            }

            class Normal {

                public void Run() {
                    try {
                        Document doc = new Document();
                        MemoryStream stream = new MemoryStream();
                        PdfWriter writer = PdfWriter.GetInstance(doc, stream);

                        doc.Open();

                        Font font = FontFactory.GetFont("FreeSans", BaseFont.IDENTITY_H, true, 12);
                        doc.Add(new Phrase("Hello, PDF world!", font));
                        doc.Add(new Phrase(Guid.NewGuid().ToString(), font));

                        doc.Close();

                    } catch (Exception e) {
                        throw new SystemException(e.Message);
                    }
                }
            }

            public void RunTest(bool createPDFA, bool createNormal) {
                List<ManualResetEvent> events = new List<ManualResetEvent>();

                Archive a = new Archive();
                Normal n = new Normal();
                for (int i = 0; i < 1000; i++) {
                    ManualResetEvent doneEvent = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate {
                        try {
                            if (createPDFA) {
                                a.Run();
                            }
                            if (createNormal) {
                                n.Run();
                            }
                        }
                        finally {
                            doneEvent.Set();
                        }
                    });
                    events.Add(doneEvent);
                    if (events.Count == 64) {
                        WaitHandle.WaitAll(events.ToArray());
                        events.Clear();
                    }
                }

                
            }

        }
    }
}
