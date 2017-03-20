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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    internal class MultithreadedTtfTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\MultithreadedTtfTest\";
        private const string TARGET_PATH = @"MultithreadedTtfTest\";
        private static Dictionary<int, Exception> exceptions;
        private int numberOfThreads = 100;

        [Test]
        virtual public void TtfTest() {
            Directory.CreateDirectory(TARGET_PATH);
            exceptions = new Dictionary<int, Exception>();


            MultithreadedTtfTestSettableLatch sl = new MultithreadedTtfTestSettableLatch();
            sl.Set(numberOfThreads);

            List<TtfTestRunner> runners = new List<TtfTestRunner>();
            for (int i = 1; i <= numberOfThreads; i++) {
                runners.Add(new TtfTestRunner(i, sl));
            }

            foreach (TtfTestRunner ttfTestRunner in runners) {
                Thread newThread = new Thread(ttfTestRunner.Run);
                newThread.Start();
            }

            sl.Await();


            Assert.AreEqual(0, exceptions.Count);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RegisterException(int thread, Exception exception) {
            exceptions.Add(thread, exception);
        }

        public class MultithreadedTtfTestSettableLatch {
            private readonly Object _lock;
            private int interrups;
            private int locks;

            public MultithreadedTtfTestSettableLatch() {
                _lock = new Object();
                locks = 0;
                interrups = 0;
            }

            virtual public void Set(int runs) {
                lock (_lock) {
                    try {
                        locks = runs;
                        interrups = 0;
                    }
                    finally {
                        Monitor.PulseAll(_lock);
                    }
                }
            }

            virtual public void Decrement() {
                lock (_lock) {
                    try {
                        locks--;
                    }
                    finally {
                        Monitor.PulseAll(_lock);
                    }
                }
            }


            virtual public void Await() {
                lock (_lock) {
                    while (locks > 0 && interrups != locks) {
                        Monitor.Wait(_lock);
                    }
                }
            }

            virtual public void Await(long ms) {
                DateTime max = DateTime.Now.AddMilliseconds(ms);
                lock (_lock) {
                    while (locks > 0 && interrups != locks && DateTime.Now < max) {
                        Monitor.Wait(_lock, 1);
                    }
                }
            }
        }

        private class TtfTestRunner {
            private readonly MultithreadedTtfTestSettableLatch latch;
            private readonly int threadNumber;

            public TtfTestRunner(int threadNumber, MultithreadedTtfTestSettableLatch latch) {
                this.threadNumber = threadNumber;
                this.latch = latch;
            }

            virtual public void Run() {
                try {
                    PdfReader reader =
                        new PdfReader(File.Open(TEST_RESOURCES_PATH + "test.pdf", FileMode.Open, FileAccess.Read,
                            FileShare.Read));
                    PdfStamper stamper = new PdfStamper(reader,
                        new FileStream(TARGET_PATH + "out" + threadNumber + ".pdf", FileMode.Create));
                    PdfContentByte cb = stamper.GetOverContent(1);
                    cb.BeginText();
                    BaseFont font = BaseFont.CreateFont(TEST_RESOURCES_PATH + "FreeSans.ttf",
                        threadNumber%2 == 0 ? BaseFont.IDENTITY_H : BaseFont.WINANSI, BaseFont.EMBEDDED);
                    cb.SetFontAndSize(font, 12);
                    cb.MoveText(30, 600);
                    cb.ShowText("`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./");
                    cb.EndText();
                    stamper.Close();
                    reader.Close();
                }
                catch (Exception exc) {
                    RegisterException(threadNumber, exc);
                }
                finally {
                    latch.Decrement();
                }
            }
        }
    }
}
