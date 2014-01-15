using System;
using System.IO;

namespace iTextSharp.testutils {
    public abstract class ITextTest {
        public virtual void RunTest() {
            Console.WriteLine("Starting test.");
            String outPdf = GetOutPdf();
            if (outPdf == null || outPdf.Length == 0)
                throw new InvalidOperationException("outPdf cannot be empty!");
            MakePdf(outPdf);
            AssertPdf(outPdf);
            ComparePdf(outPdf, GetCmpPdf());
            Console.WriteLine("Test complete.");
        }

        protected abstract void MakePdf(String outPdf);

        /**
         * Gets the name of the resultant PDF file.
         * This name will be passed to <code>makePdf</code>, <code>assertPdf</code> and <code>comparePdf</code> methods.
         * @return
         */
        protected abstract String GetOutPdf();

        protected virtual void AssertPdf(String outPdf) {
        }

        protected virtual void ComparePdf(String outPdf, String cmpPdf) {
        }

        /**
         * Gets the name of the compare PDF file.
         * This name will be passed to <code>comparePdf</code> method.
         * @return
         */
        protected virtual String GetCmpPdf() {
            return "";
        }

        protected virtual void DeleteDirectory(string path) {
            if (path == null)
                return;
            if (Directory.Exists(path)) {
                foreach (string d in Directory.GetDirectories(path)) {
                    DeleteDirectory(d);
                    Directory.Delete(d);
                }
                foreach (string f in Directory.GetFiles(path)) {
                    File.Delete(f);
                }
                Directory.Delete(path);
            }
        }

        protected virtual void DeleteFiles(String path) {
            if (path != null && Directory.Exists(path)) {
                foreach (String f in Directory.GetFiles(path)) {
                    File.Delete(f);
                }
            }
        }
    }
}
