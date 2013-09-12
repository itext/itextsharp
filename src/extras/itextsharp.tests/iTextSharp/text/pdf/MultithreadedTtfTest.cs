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
        public void TtfTest() {
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

            public void Set(int runs) {
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

            public void Decrement() {
                lock (_lock) {
                    try {
                        locks--;
                    }
                    finally {
                        Monitor.PulseAll(_lock);
                    }
                }
            }


            public void Await() {
                lock (_lock) {
                    while (locks > 0 && interrups != locks) {
                        Monitor.Wait(_lock);
                    }
                }
            }

            public void Await(long ms) {
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

            public void Run() {
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