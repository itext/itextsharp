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
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class PdfImageObjectTest
    {
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\PdfImageObjectTest\";

        private void TestFile(String filename, int page, String objectid)
        {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, filename);
            try
            {
                PdfDictionary resources = pdfReader.GetPageResources(page);
                PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
                PdfIndirectReference objRef = xobjects.GetAsIndirectObject(new PdfName(objectid));
                if (objRef == null)
                    throw new NullReferenceException("Reference " + objectid + " not found - Available keys are " + xobjects.Keys);
                PRStream stream = (PRStream)PdfReader.GetPdfObject(objRef);
                PdfDictionary colorSpaceDic = resources != null ? resources.GetAsDict(PdfName.COLORSPACE) : null;
                PdfImageObject img = new PdfImageObject(stream, colorSpaceDic);
                byte[] result = img.GetImageAsBytes();
                Assert.NotNull(result);
                int zeroCount = 0;
                foreach (byte b in result)
                {
                    if (b == 0) zeroCount++;
                }
                Assert.IsTrue(zeroCount > 0);
            }
            finally
            {
                pdfReader.Close();
            }
        }

        [Test]
        virtual public void TestMultiStageFilters()
        {
            TestFile("multistagefilter1.pdf", 1, "Obj13");
        }

        [Test]
        virtual public void TestAscii85Filters()
        {
            TestFile("ASCII85_RunLengthDecode.pdf", 1, "Im9");
        }

        [Test]
        virtual public void TestCcittFilters()
        {
            TestFile("ccittfaxdecode.pdf", 1, "background0");
        }

        [Test]
        virtual public void TestFlateDecodeFilters()
        {
            TestFile("flatedecode_runlengthdecode.pdf", 1, "Im9");
        }

        [Test]
        virtual public void TestDctDecodeFilters()
        {
            TestFile("dctdecode.pdf", 1, "im1");
        }

        [Test]
        virtual public void Testjbig2Filters()
        {
            TestFile("jbig2decode.pdf", 1, "2");
        }
    }
}
