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
