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
using System.Reflection;
using iTextSharp.testutils;
using iTextSharp.text.log;
using NUnit.Framework;

namespace iTextSharp.sandbox
{
    [TestFixture]
    public abstract class GenericTest
    {
        /// <summary>
        /// The logger class
        /// </summary>
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof (GenericTest));

        /// <summary>
        /// The class file for the example we're going to test.
        /// </summary>
        protected Type type;
        protected string sampleName;
        protected bool compareRenders = false;
        
        /// <summary>
        /// An error message
        /// </summary>
        private string errorMessage;
        /// <summary>
        /// A prefix that is part of the error message.
        /// </summary>
        private string differenceImagePrefix = "difference";

        /// <summary>
        /// Should be overriden to obtain dlls for testing.
        /// TestCaseData should have 2 args of Type and bool types
        /// </summary>
        /// <returns>Returns TestCaseData enumerable</returns>
        public abstract IEnumerable<TestCaseData> Data();

        /// <summary>
        /// Tests the example.
        /// If SRC and DEST are defined, the example manipulates a PDF;
        /// if only DEST is defined, the example creates a PDF.
        /// </summary>
        [Test]
        [TestCaseSource("Data")]
        [Timeout(120000)]
        public void Test(Type type, bool compareRenders)
        {
            this.type = type;
            this.sampleName = type.Name;
            this.compareRenders = compareRenders;
            if (this.GetType().Name == typeof (GenericTest).Name)
                return;
            LOGGER.Info("Starting test " + sampleName + ".");
            // Getting the destination PDF file (must be there!)
            String dest = GetDest();
            if (string.IsNullOrEmpty(dest))
                throw new Exception("DEST cannot be empty!"); 
            // Getting the source PDF file
            String src = GetSrc();
            // if there is none, just create a PDF
            if (string.IsNullOrEmpty(src))
            {
                CreatePdf(dest);
            }
                // if there is one, manipulate the PDF
            else
            {
                ManipulatePdf(src, dest);
            }
            // Do some further tests on the PDF
            AssertPdf(dest);
            // Compare the destination PDF with a reference PDF
            Console.WriteLine(dest + "\n" + GetCmpPdf());
            ComparePdf(dest, GetCmpPdf());
            LOGGER.Info("Test complete.");
        }

        /// <summary>
        /// Creates a PDF by invoking the CreatePdf() method in the
        /// original sample class.
        /// </summary>
        /// <param name="dest">the resulting PDF</param>
        protected void CreatePdf(String dest)
        {
            LOGGER.Info("Creating PDF.");
            MethodInfo method = type.GetMethod("CreatePdf", new Type[] {typeof(string)});
            object[] objs = {dest};
            method.Invoke(Activator.CreateInstance(type), objs);
        }

        /// <summary>
        /// Manupulates a PDF by invoking the ManipulatePdf() method in the
        /// original sample class.
        /// </summary>
        /// <param name="src">the source PDF</param>
        /// <param name="dest">the resulting PDF</param>
        protected void ManipulatePdf(String src, String dest)
        {
            LOGGER.Info("Manipulating PDF.");
            MethodInfo method = type.GetMethod("ManipulatePdf", new Type[] {typeof (string), typeof (string)});
            method.Invoke(Activator.CreateInstance(type), new object[] {src, dest});
        }

        /// <summary>
        /// * Gets the path to the source PDF from the sample class.
        /// </summary>
        /// <returns>a path to a source PDF</returns>
        protected string GetSrc()
        {
            return GetStringField("SRC");
        }

        /// <summary>
        /// Gets the path to the resulting PDF from the sample class;
        /// this method also creates directories if necessary.
        /// </summary>
        /// <returns>a path to a resulting PDF</returns>
        protected string GetDest()
        {
            string dest = GetStringField("DEST");
            if (dest != null)
            {
                DirectoryInfo dir = new FileInfo(dest).Directory;
                if (dir != null)
                    dir.Create();
            }
            return dest;
        }

        /// <summary>
        /// Returns a string value that is stored as a static variable
        /// inside an example class.
        /// </summary>
        /// <param name="name">the name of the variable</param>
        /// <returns>the value of the variable</returns>
        protected string GetStringField(string name)
        {
            try
            {
                FieldInfo field = type.GetField(name);
                if (field == null)
                    return null;
                Object obj = field.GetValue(null);
                return obj as String;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Compares two PDF files using iText's CompareTool.
        /// </summary>
        /// <param name="dest">the PDF that resulted from the test</param>
        /// <param name="cmp">the reference PDF</param>
        protected void ComparePdf(string dest, string cmp)
        {
            if (string.IsNullOrEmpty(cmp)) 
                return;
            CompareTool compareTool = new CompareTool();
            string outPath = "./target/" + new DirectoryInfo(dest).Parent; 
            new DirectoryInfo(outPath).Create();
            if (compareRenders)
            {
                AddError(compareTool.Compare(dest, cmp, outPath, differenceImagePrefix));
                AddError(compareTool.CompareLinks(dest, cmp));
            }
            else
            {
                AddError(compareTool.CompareByContent(dest, cmp, outPath, differenceImagePrefix));
            }
            AddError(compareTool.CompareDocumentInfo(dest, cmp));


            if (errorMessage != null) 
                Assert.Fail(errorMessage);
        }

        /// <summary>
        /// Perform other tests on the resulting PDF.
        /// </summary>
        /// <param name="dest">the resulting PDF</param>
        protected void AssertPdf(string dest) {}

        /// <summary>
        /// Every test needs to know where to find its reference file.
        /// </summary>
        /// <returns></returns>
        protected string GetCmpPdf()
        {
            string tmp = GetDest();
            if (tmp == null)
                return null;
            int i = tmp.LastIndexOf("/");
            string path = "../../cmpfiles/" + tmp.Substring(8, (i + 1) - 8) + "cmp_" + tmp.Substring(i + 1);
            return path;
        }

        /// <summary>
        /// Helper method to construct error messages.
        /// </summary>
        /// <param name="error">part of an error message.</param>
        private void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (errorMessage == null)
                    errorMessage = "";
                else
                    errorMessage += "\n";

                errorMessage += error;
            }
        }
    }
}
