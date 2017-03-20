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
using System.Text;
using iTextSharp.testutils;
using Microsoft.XmlDiffPatch;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;
using iTextSharp.text.pdf.parser;

namespace itextsharp.tests.text.pdf
{
    public class TaggedPdfCopyTest
    {
        private Document document;
        private PdfCopy copy;
        private String output;
        private string dir;


        public const String NO_PARENT_TREE = "The document does not contain ParentTree";
        public const String NO_CLASS_MAP = "The document does not contain ClassMap";
        public const String NO_ROLE_MAP = "The document does not contain RoleMap";
        public const String NO_STRUCT_TREE_ROOT = "No StructTreeRoot found";
        public const String NO_ID_TREE = "The document does not contain ID Tree";
        public const String EMPTY_ID_TREE = "The document's ID Tree is empty";

        public const String RESOURCES = @"..\..\resources\text\pdf\TaggedPdfCopyTest\";
        public const String SOURCE4 =  RESOURCES + "pdf\\source4.pdf";
        public const String SOURCE10 = RESOURCES + "pdf\\source10.pdf";
        public const String SOURCE11 = RESOURCES + "pdf\\source11.pdf";
        public const String SOURCE12 = RESOURCES + "pdf\\source12.pdf";
        public const String SOURCE16 = RESOURCES + "pdf\\source16.pdf";
        public const String SOURCE17 = RESOURCES + "pdf\\source17.pdf";
        public const String SOURCE18 = RESOURCES + "pdf\\source18.pdf";
        public const String SOURCE19 = RESOURCES + "pdf\\source19.pdf";
        public const String SOURCE22 = RESOURCES + "pdf\\source22.pdf";
        public const String SOURCE24 = RESOURCES + "pdf\\source24.pdf";
        public const String SOURCE25 = RESOURCES + "pdf\\source25.pdf";
        public const String SOURCE25_1 = RESOURCES + "pdf\\source25_1.pdf";
        public const String SOURCE32 = RESOURCES + "pdf\\source32.pdf";
        public const String SOURCE42 = RESOURCES + "pdf\\source42.pdf";
        public const String SOURCE51 = RESOURCES + "pdf\\source51.pdf";
        public const String SOURCE52 = RESOURCES + "pdf\\source52.pdf";
        public const String SOURCE53 = RESOURCES + "pdf\\source53.pdf";
        public const String SOURCE61 = RESOURCES + "pdf\\source61.pdf";
        public const String SOURCE62 = RESOURCES + "pdf\\source62.pdf";
        public const String SOURCE63 = RESOURCES + "pdf\\source63.pdf";
        public const String SOURCE64 = RESOURCES + "pdf\\source64.pdf";
        public const String SOURCE72 = RESOURCES + "pdf\\source72.pdf";
        public const String SOURCE73 = RESOURCES + "pdf\\source73.pdf";
        public const String SOURCE81 = RESOURCES + "pdf\\source81.pdf";
        public const String DEV_805 = RESOURCES + "pdf\\dev-805.pdf";
        public const String SOURCE_CF_11 = RESOURCES + "pdf/sourceCf11.pdf";
        public const String SOURCE_CF_12 = RESOURCES + "pdf/sourceCf12.pdf";
        public const String SOURCE_CF_13 = RESOURCES + "pdf/sourceCf13.pdf";
        public const String SOURCE_CF_14 = RESOURCES + "pdf/sourceCf14.pdf";
        public const String SOURCE_CF_15 = RESOURCES + "pdf/sourceCf15.pdf";
        public const String SOURCE_CF_16 = RESOURCES + "pdf/sourceCf16.pdf";

        public const String CMP25 = RESOURCES + "pdf/cmp_out25.pdf";

        public const String OUT = TARGET + "pdf\\out";
        public const String TARGET = "TaggedPdfCopyTest\\";


        /*static {

    }*/

        [SetUp]
        virtual public void Initialize()
        {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
            Document.Compress = false;
        }

        private void InitializeDocument(String name)
        {
            output = OUT + name + ".pdf";
            document = new Document();
            copy = new PdfCopy(document, new FileStream(output, FileMode.Create));
            copy.SetTagged();
            document.Open();
        }

        [Test]
        virtual public void ClassMapConflict()
        {
            InitializeDocument("-cmc");
            PdfReader reader1 = new PdfReader(SOURCE11);
            try
            {
                copy.AddPage(copy.GetImportedPage(reader1, 76, true));
            }
            catch (BadPdfFormatException)
            {
            }
            reader1.Close();
            PdfReader reader2 = new PdfReader(SOURCE12);
            bool exceptionThrown = false;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader2, 76, true));
            }
            catch (BadPdfFormatException)
            {
                exceptionThrown = true;
            }
            reader2.Close();
            if (!exceptionThrown)
                Assert.Fail("BadPdfFormatException expected!");
        }

        [Test]
        virtual public void RoleMapConflict()
        {
            InitializeDocument("-rolemap");

            PdfReader reader1 = new PdfReader(SOURCE11);
            //PdfDictionary trailer = reader1.trailer;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader1, 76, true));
            }
            catch (BadPdfFormatException)
            {
            }
            reader1.Close();
            PdfReader reader2 = new PdfReader(SOURCE22);
            bool exceptionThrown = false;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader2, 76, true));
            }
            catch (BadPdfFormatException)
            {
                exceptionThrown = true;
            }
            reader2.Close();
            if (!exceptionThrown)
                Assert.Fail("BadPdfFormatException expected!");
        }

        [Test]
        virtual public void PdfMergeTest()
        {
            PdfDictionary CM31 = new PdfDictionary();
            PdfDictionary sElem = new PdfDictionary();
            //<</O/Layout/EndIndent 18.375/StartIndent 11.25/TextIndent -11.25/LineHeight 13>>
            //<</C/SC.7.147466/Pg 118 0 R/Type/StructElem/K 3/S/Span/Lang(en)/P 1 0 R>>
            CM31.Put(PdfName.O, new PdfName("Layout"));
            CM31.Put(new PdfName("EndIndent"), new PdfNumber(18.375));
            CM31.Put(new PdfName("StartIndent"), new PdfNumber(11.25));
            CM31.Put(new PdfName("TextIndent"), new PdfNumber(-11.25));
            CM31.Put(new PdfName("LineHeight"), new PdfNumber(13));
            sElem.Put(PdfName.C, new PdfName("SC.7.147466"));
            sElem.Put(PdfName.K, new PdfNumber(5));
            sElem.Put(PdfName.S, PdfName.SPAN);
            sElem.Put(PdfName.LANG, new PdfString("en"));


            InitializeDocument("-merge");

            int n = 14;
            PdfReader reader1 = new PdfReader(SOURCE11);
            copy.AddPage(copy.GetImportedPage(reader1, 76, true));
            copy.AddPage(copy.GetImportedPage(reader1, 83, true));
            PdfReader reader2 = new PdfReader(SOURCE32);
            copy.AddPage(copy.GetImportedPage(reader2, 69, true));
            copy.AddPage(copy.GetImportedPage(reader2, 267, true));
            document.Close();
            reader1.Close();
            reader2.Close();
            PdfReader reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 2, "Kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);

            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            int[] nums = new int[] { 44, 0, 65, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 81 };
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1), true);

            PdfDictionary ClassMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.CLASSMAP)),
                                   NO_CLASS_MAP);
            PdfDictionary currCM31 =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(ClassMap.Get(new PdfName("CM31"))),
                                   "ClassMap does not contain.\"CM31\"");
            if (!PdfStructTreeController.CompareObjects(CM31, currCM31))
                Assert.Fail("ClassMap contains incorrect \"CM31\"");

            PdfDictionary RoleMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.ROLEMAP)),
                                   NO_ROLE_MAP);
            if (!PdfName.SPAN.Equals(RoleMap.Get(new PdfName("ParagraphSpan"))))
                throw new BadPdfFormatException("RoleMap does not contain \"ParagraphSpan\"");

            reader.Close();
        }

        [Test]
        virtual public void CopyTaggedPdf0()
        {
            InitializeDocument("0");
            PdfReader reader = new PdfReader(SOURCE11);
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            document.Close();
            reader.Close();

            Assert.AreEqual(GetCommonNumsCount(SOURCE11), GetCommonNumsCount(output));

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot = (PdfDictionary)reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT);
            PdfDictionary ClassMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.CLASSMAP)),
                                   NO_CLASS_MAP);
            if (ClassMap.Size != 109) Assert.Fail("ClassMap incorrect");
            String[] CMs = new String[]
                {
                    "CM84", "CM81", "CM80", "CM87", "CM88", "CM9", "CM94", "CM95", "CM96",
                    "CM97", "CM90", "CM91", "CM92", "CM93", "CM98", "CM99", "CM16", "CM17", "CM14", "CM15",
                    "CM12", "CM13", "CM10", "CM19", "CM20", "CM21", "CM22", "CM23", "CM24", "CM25", "CM26",
                    "CM27", "CM28", "CM29", "CM100", "CM101", "CM102", "CM103", "CM105", "CM106", "CM30",
                    "CM31", "CM34", "CM35", "CM32", "CM33", "CM38", "CM39", "CM36", "CM118", "CM117", "CM49",
                    "CM48", "CM116", "CM115", "CM47", "CM114", "CM46", "CM113", "CM45", "CM112", "CM44", "CM43",
                    "CM111", "CM42", "CM110", "CM41", "CM108", "CM109", "CM127", "CM126", "CM58", "CM129",
                    "CM128", "CM55", "CM123", "CM54", "CM125", "CM57", "CM56", "CM51", "CM50", "CM53", "CM120",
                    "CM52", "CM119", "CM68", "CM136", "CM135", "CM67", "CM133", "CM139", "CM60", "CM132", "CM64",
                    "CM63", "CM62", "CM61", "CM145", "CM76", "CM78", "CM1", "CM2", "CM71", "CM70", "CM73",
                    "CM141", "CM72", "CM74"
                };
            for (int i = 0; i < CMs.Length; ++i)
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(ClassMap.Get(new PdfName(CMs[i]))),
                                   "ClassMap does not contain \"" + CMs[i] + "\"");

            PdfDictionary RoleMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.ROLEMAP)),
                                   NO_ROLE_MAP);
            if (!PdfName.SPAN.Equals(RoleMap.Get(new PdfName("ParagraphSpan"))))
                throw new BadPdfFormatException("RoleMap does not contain \"ParagraphSpan\".");

            //if (reader.eofPos != 3378440L) Assert.Fail("Invalid size of pdf.");
            reader.Close();
            CompareResults("0");
        }

        [Test]
        virtual public void CopyTaggedPdf1()
        {
            InitializeDocument("1");
            PdfReader reader = new PdfReader(SOURCE32);
            copy.AddPage(copy.GetImportedPage(reader, 5, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, 22, "Nums");
            VerifyArraySize(PdfStructTreeController.GetDirectObject(array[1]), 61, "Nums of page 1");
            reader.Close();
            CompareResults("1");
        }

        [Test]
        virtual public void CopyTaggedPdf2()
        {
            InitializeDocument("2");
            PdfReader reader = new PdfReader(SOURCE16);
            copy.AddPage(copy.GetImportedPage(reader, 2, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, 2, "Nums");
            VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(1)), 7, "Nums of page 1");
            reader.Close();
            CompareResults("2");
        }

        [Test]
        virtual public void CopyTaggedPdf3()
        {
            InitializeDocument("3");
            PdfReader reader = new PdfReader(SOURCE10);
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] { 16, 87, 128, 74, 74, 74, 26 };
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("3");
        }

        [Test]
        virtual public void CopyTaggedPdf4()
        {
            InitializeDocument("4");
            PdfReader reader = new PdfReader(SOURCE10);
            int n = reader.NumberOfPages;
            for (int i = n; i > 0; --i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 7, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] { 26, 74, 74, 74, 128, 87, 16 };
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("4");
        }

        [Test]
        virtual public void CopyTaggedPdf5()
        {
            InitializeDocument("5");
            PdfReader reader = new PdfReader(SOURCE10);
            int n = 3;
            copy.AddPage(copy.GetImportedPage(reader, 1, true));
            copy.AddPage(copy.GetImportedPage(reader, 3, true));
            copy.AddPage(copy.GetImportedPage(reader, 7, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] { 16, 128, 26 };
            for (int i = 0; i < n; ++i)
                //nums[i] = ((PdfArray)PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2+1))).size();
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("5");
        }

        [Test]
        virtual public void CopyTaggedPdf6()
        {
            InitializeDocument("6");
            PdfReader reader = new PdfReader(SOURCE11);
            int n = 12;
            copy.AddPage(copy.GetImportedPage(reader, 1, true));
            copy.AddPage(copy.GetImportedPage(reader, 25, true));
            copy.AddPage(copy.GetImportedPage(reader, 7, true));
            copy.AddPage(copy.GetImportedPage(reader, 48, true));
            copy.AddPage(copy.GetImportedPage(reader, 50, true));
            copy.AddPage(copy.GetImportedPage(reader, 2, true));
            copy.AddPage(copy.GetImportedPage(reader, 8, true));
            copy.AddPage(copy.GetImportedPage(reader, 90, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 6, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] { 5, 0, 33, 12, 0, 48, 35, 182, 0, 0, 17, 37 };
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1), true);

            PdfDictionary ClassMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.CLASSMAP)),
                                   NO_CLASS_MAP);
            if (ClassMap.Size != 27) Assert.Fail("ClassMap incorrect");
            String[] CMs = new String[]
                {
                    "CM118", "CM117", "CM133", "CM47", "CM46", "CM114", "CM43", "CM110", "CM21", "CM22", "CM26", "CM27",
                    "CM145", "CM128", "CM29", "CM56", "CM1", "CM2", "CM72", "CM16", "CM34", "CM17", "CM14", "CM15",
                    "CM119", "CM12", "CM13"
                };
            for (int i = 0; i < CMs.Length; ++i)
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(ClassMap.Get(new PdfName(CMs[i]))),
                                   "ClassMap.does.not.contain.\"" + CMs[i] + "\"");

            PdfDictionary RoleMap =
                VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.ROLEMAP)),
                                   NO_ROLE_MAP);
            if (!PdfName.SPAN.Equals(RoleMap.Get(new PdfName("ParagraphSpan"))))
                throw new BadPdfFormatException("RoleMap does not contain \"ParagraphSpan\".");
            //if (reader.eofPos != 249068) Assert.Fail("Invalid size of pdf.");

            reader.Close();
            CompareResults("6");
        }

        [Test]
        virtual public void CopyTaggedPdf7()
        {
            InitializeDocument("7");
            PdfReader reader = new PdfReader(SOURCE16);
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; ++i)
            {
                copy.AddPage(copy.GetImportedPage(reader, i, true));
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            }
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            n *= 4;
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 5, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] {48, 48, 7, 7, 48, 7, 48, 7};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("7");
        }

        [Test]
        virtual public void CopyTaggedPdf8()
        {
            InitializeDocument("8");
            PdfReader reader = new PdfReader(SOURCE42);
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; ++i)
            {
                copy.AddPage(copy.GetImportedPage(reader, i, true));
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            }
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            n = 52;
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 6, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
//            int[] nums = new int[] {42, 42, 11, 11, 13, 13, 42, 11, 13, 42, 11, 13};
//            for (int i = 0; i < n; ++i)
//                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
//                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("8");
        }

        [Test]
        virtual public void CopyTaggedPdf9()
        {
            InitializeDocument("9");
            PdfReader reader4 = new PdfReader(SOURCE4);
            PdfReader reader10 = new PdfReader(SOURCE10);
            PdfReader reader32 = new PdfReader(SOURCE32);
            int n = 40;
            copy.AddPage(copy.GetImportedPage(reader4, 1, true));
            copy.AddPage(copy.GetImportedPage(reader10, 2, true));
            copy.AddPage(copy.GetImportedPage(reader10, 3, true));
            copy.AddPage(copy.GetImportedPage(reader10, 7, true));
            copy.AddPage(copy.GetImportedPage(reader32, 50, true));
            copy.AddPage(copy.GetImportedPage(reader32, 55, true));
            copy.AddPage(copy.GetImportedPage(reader4, 1, true));
            copy.AddPage(copy.GetImportedPage(reader32, 50, true));
            copy.AddPage(copy.GetImportedPage(reader32, 55, true));
            copy.AddPage(copy.GetImportedPage(reader32, 56, true));
            copy.AddPage(copy.GetImportedPage(reader32, 60, true));
            copy.AddPage(copy.GetImportedPage(reader10, 3, true));
            copy.AddPage(copy.GetImportedPage(reader10, 4, true));
            copy.AddPage(copy.GetImportedPage(reader10, 1, true));
            copy.AddPage(copy.GetImportedPage(reader32, 1, true));
            copy.AddPage(copy.GetImportedPage(reader32, 15, true));
            copy.AddPage(copy.GetImportedPage(reader32, 20, true));
            copy.AddPage(copy.GetImportedPage(reader32, 5, true));
            copy.AddPage(copy.GetImportedPage(reader4, 1, true));
            copy.AddPage(copy.GetImportedPage(reader10, 7, true));

            document.Close();
            reader4.Close();
            reader10.Close();
            reader32.Close();

            PdfReader reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 11, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, n*2, "Nums");
            int[] nums = new int[] { 7, 87, 128, 26, 135, 0, 0, 83, 7, 135, 0, 0, 0, 0, 0, 0, 83, 116, 26, 128, 74, 16, 12, 0, 0, 38, 54, 61, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 26 }; 
            for (int i = 0; i < n; ++i)
                //            nums[i] = ((PdfArray)PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2+1))).size();
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1), true);
            reader.Close();
            CompareResults("9");
        }

        [Test]
        virtual public void CopyTaggedPdf10()
        {
            //source17: StructTreeRoot has no kids - incorrect syntax of tags - try to fix in result pdf
            InitializeDocument("10");
            PdfReader reader = new PdfReader(SOURCE17);
            copy.AddPage(copy.GetImportedPage(reader, 2, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, 2, "Nums");
            VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(1)), 7, "Nums of page 1");
            reader.Close();
            CompareResults("10");
        }

        [Test]
        virtual public void CopyTaggedPdf11()
        {
            //source51: invalid nums - references to PdfDictionary, all pages has the same "NumDictionary"
            // 58 0 obj
            // <</Nums[0 2 0 R 1 2 0 R 2 2 0 R 3 2 0 R 4 2 0 R 5 2 0 R 6 2 0 R]>>
            // endobj
            //where 2 0 R is StructElement of Document
            InitializeDocument("11");
            PdfReader reader = new PdfReader(SOURCE51);
            bool exceptionThrown = false;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader, 2, true));
            }
            catch (BadPdfFormatException)
            {
                exceptionThrown = true;
            }
            //document.Close();
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("BadPdfFormatException expected!");

            //        reader = new PdfReader(output);
            //        PdfDictionary structTreeRoot = VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            //        VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            //        PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            //        VerifyIsDictionary(obj, NO_PARENT_TREE);
            //        PdfArray array = ((PdfDictionary)obj).GetAsArray(PdfName.NUMS);
            //        VerifyArraySize(array, 2, "Nums");
            //        VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(1)), 7, "Nums of page 1");
            //        reader.Close();
            //        CompareResults("11");
        }

        [Test]
        virtual public void CopyTaggedPdf12()
        {
            //source52: Nums array is empty:
            // 58 0 obj
            // <</Nums[                                                       ]>>
            // endobj
            InitializeDocument("12");
            PdfReader reader = new PdfReader(SOURCE52);
            bool exceptionThrown = false;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader, 2, true));
            }
            catch (BadPdfFormatException)
            {
                exceptionThrown = true;
            }
            //document.Close();
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("BadPdfFormatException expected!");

            //        reader = new PdfReader(output);
            //        PdfDictionary structTreeRoot = VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            //        VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            //        PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            //        VerifyIsDictionary(obj, NO_PARENT_TREE);
            //        PdfArray array = ((PdfDictionary)obj).GetAsArray(PdfName.NUMS);
            //        VerifyArraySize(array, 2, "Nums");
            //        VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(1)), 7, "Nums of page 1");
            //        reader.Close();
            //        CompareResults("12");
        }

        [Test]
        virtual public void CopyTaggedPdf13()
        {
            //source53: StructTreeRoot doesn't have kids and Nums is empty
            InitializeDocument("13");
            PdfReader reader = new PdfReader(SOURCE53);
            bool exceptionThrown = false;
            try
            {
                copy.AddPage(copy.GetImportedPage(reader, 2, true));
            }
            catch (BadPdfFormatException)
            {
                exceptionThrown = true;
            }
            //document.Close();
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("BadPdfFormatException expected!");
        }

        [Test]
        virtual public void CopyTaggedPdf14() {
            InitializeDocument("14");
            PdfReader reader = new PdfReader(SOURCE11);

            copy.AddPage(copy.GetImportedPage(reader, 5, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot = VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 1, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            PdfArray array = ((PdfDictionary)obj).GetAsArray(PdfName.NUMS);
            VerifyArraySize(array, 8, "Nums");
            VerifyArraySize(PdfStructTreeController.GetDirectObject(array[1]), 20, "Nums of page 1");
            reader.Close();
        }

        [Test]
        virtual public void CopyTaggedPdf15() {
            InitializeDocument("15");
            copy.SetMergeFields();

            PdfReader reader1 = new PdfReader(SOURCE61);
            PdfReader reader2 = new PdfReader(SOURCE62);
            copy.AddDocument(reader1);
            copy.AddDocument(reader2);
            document.Close();
            reader1.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(output);
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary structTreeRoot = catalog.GetAsDict(PdfName.STRUCTTREEROOT);
            PdfDictionary structParent = structTreeRoot.GetAsDict(PdfName.PARENTTREE);
            PdfArray nums = structParent.GetAsArray(PdfName.NUMS);
            PdfDictionary acroForm = catalog.GetAsDict(PdfName.ACROFORM);
            PdfDictionary fonts = acroForm.GetAsDict(PdfName.DR).GetAsDict(PdfName.FONT);

            Assert.AreEqual(new PdfName("Helvetica"), fonts.GetAsDict(new PdfName("Helv")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("ZapfDingbats"), fonts.GetAsDict(new PdfName("ZaDb")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("ArialMT"), fonts.GetAsDict(new PdfName("ArialMT")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("CourierStd"), fonts.GetAsDict(new PdfName("CourierStd")).GetAsName(PdfName.BASEFONT));

            Assert.AreEqual(1, nums.GetAsNumber(2).IntValue);
            Assert.AreEqual(4, nums.GetAsNumber(8).IntValue);

            Assert.AreEqual(12, nums.Size);
            Assert.AreEqual(3, acroForm.GetAsArray(PdfName.FIELDS).Size);

            reader.Close();
        }

        [Test]
        [Ignore("ignore")]
        virtual public void CopyTaggedPdf16() {
            InitializeDocument("16");
            copy.SetMergeFields();

            PdfReader reader1 = new PdfReader(SOURCE63);
            PdfReader reader2 = new PdfReader(SOURCE64);
            copy.AddDocument(reader1);
            copy.AddDocument(reader2);
            document.Close();
            reader1.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(output);
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acroForm = catalog.GetAsDict(PdfName.ACROFORM);
            PdfDictionary fonts = acroForm.GetAsDict(PdfName.DR).GetAsDict(PdfName.FONT);

            Assert.AreEqual(new PdfName("Helvetica"), fonts.GetAsDict(new PdfName("Helv")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("Courier"), fonts.GetAsDict(new PdfName("Cour")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("Times-Bold"), fonts.GetAsDict(new PdfName("TiBo")).GetAsName(PdfName.BASEFONT));
            Assert.AreEqual(new PdfName("ZapfDingbats"), fonts.GetAsDict(new PdfName("ZaDb")).GetAsName(PdfName.BASEFONT));

            reader.Close();
        }

        [Test]
        virtual public void CopyTaggedPdf17() {
            InitializeDocument("17");

            PdfReader reader1 = new PdfReader(SOURCE10);
            PdfReader reader2 = new PdfReader(SOURCE19);
            copy.AddPage(copy.GetImportedPage(reader1, 1, true));
            copy.AddPage(copy.GetImportedPage(reader2, 1, false));

            document.Close();
            reader1.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(output);
            Assert.AreEqual(2, reader.NumberOfPages);
            Assert.NotNull(reader.GetPageN(1));
            Assert.NotNull(reader.GetPageN(2));
            reader.Close();
        }

        [Test]
        virtual public void CopyTaggedPdf19() {
            InitializeDocument("19");

            PdfReader reader = new PdfReader(SOURCE18);
            copy.AddPage(copy.GetImportedPage(reader, 1, true));

            document.Close();
            reader.Close();

            reader = new PdfReader(output);

            PdfDictionary page1 = reader.GetPageN(1);
            PdfDictionary t1_0 = page1.GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.XOBJECT).GetAsStream(new PdfName("Fm0")).GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.FONT).GetAsDict(new PdfName("T1_0"));
            Assert.NotNull(t1_0);

            reader.Close();
        }

        [Test]
        virtual public void CopyTaggedPdf20() {
            InitializeDocument("20");
            copy.SetMergeFields();

            PdfReader reader2 = new PdfReader(SOURCE72);
            List<int> pagesToKeep = new List<int>();
            pagesToKeep.Add(1);
            pagesToKeep.Add(3);
            pagesToKeep.Add(5);
            copy.AddDocument(reader2, pagesToKeep);
            document.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(output);
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acroForm = catalog.GetAsDict(PdfName.ACROFORM);
            PdfArray acroFields = acroForm.GetAsArray(PdfName.FIELDS);
            Assert.IsTrue(acroFields.Size == 4);

            reader.Close();

            CompareResults("20");
        }

        [Test]
        virtual public void CopyTaggedPdf21() {
            InitializeDocument("21");
            copy.SetMergeFields();

            PdfReader reader1 = new PdfReader(SOURCE73);
            copy.AddDocument(reader1);
            document.Close();
            reader1.Close();

            PdfReader reader = new PdfReader(output);
            PdfDictionary page = reader.GetPageN(1);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xObject = resources.GetAsDict(PdfName.XOBJECT);
            PdfStream img = xObject.GetAsStream(new PdfName("Im0"));
            PdfArray decodeParms = img.GetAsArray(PdfName.DECODEPARMS);
            Assert.AreEqual(2, decodeParms.Size);
            PdfObject iref = decodeParms[0];
            Assert.IsTrue(iref is PdfIndirectReference);
            Assert.IsTrue(reader.GetPdfObjectRelease(((PdfIndirectReference) iref).Number) is PdfNull);

            reader.Close();
        }

        //Check for crash in case of structure element contains no "Pg" keys.
        [Test]
        virtual public void CopyTaggedPdf22() {
            InitializeDocument("22");

            PdfReader reader = new PdfReader(DEV_805);

            int n = reader.NumberOfPages;
            for (int page = 0; page < n;) {
                copy.AddPage(copy.GetImportedPage(reader, ++page, true));
            }

            copy.FreeReader(reader);
            document.Close();
            reader.Close();
        }

        [Test]
        public void CopyTaggedPdf23() {
            PdfReader reader = new PdfReader(SOURCE81);
            PdfDictionary structTreeRoot = VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            PdfDictionary idTree = VerifyIsDictionary(PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.IDTREE)), NO_ID_TREE);
            Assert.IsTrue(idTree.hashMap.Count > 0, EMPTY_ID_TREE);
        }

        [Test]
        [ExpectedException(typeof (BadPdfFormatException))]
        public void CopyTaggedPdf24() {
            InitializeDocument("24");
            PdfReader reader1 = new PdfReader(SOURCE24);
            copy.AddPage(copy.GetImportedPage(reader1, 17, true));
            document.Close();
            reader1.Close();
        }

        [Test]
        [Timeout(60000)]
        public void CopyTaggedPdf25() {
            InitializeDocument("25");
            PdfReader reader = new PdfReader(SOURCE25);
            PdfReader reader1 = new PdfReader(SOURCE25_1);

            copy.AddDocument(reader);
            copy.FreeReader(reader);

            copy.AddDocument(reader1);
            copy.FreeReader(reader1);

            PdfStructureTreeRoot structTreeRoot = copy.StructureTreeRoot;
            copy.Close();

            document.Close();
            reader.Close();
            reader1.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(output, CMP25, OUT, "diff_");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void CopyFields1Test() {
            InitializeDocument("CopyFields1");
            copy.SetMergeFields();

            PdfReader readerMain = new PdfReader(SOURCE_CF_14);
            PdfReader secondSourceReader = new PdfReader(SOURCE_CF_15);
            //PdfReader thirdReader = new PdfReader("./src/test/resources/com/itextpdf/text/pdf/PdfCopyTest/appearances1.pdf");

            copy.AddDocument(readerMain);
            copy.CopyDocumentFields(secondSourceReader);
            //copy.addDocument(thirdReader);

            copy.Close();
            readerMain.Close();
            secondSourceReader.Close();
            //thirdReader.close();
            /*CompareTool compareTool = new CompareTool("./target/com/itextpdf/test/pdf/PdfCopyTest/copyFields.pdf", "./src/test/resources/com/itextpdf/text/pdf/PdfCopyTest/cmp_copyFields.pdf");
        String errorMessage = compareTool.compareByContent("./target/com/itextpdf/test/pdf/PdfCopyTest/", "diff");
        if (errorMessage != null) {
            junit.framework.Assert.fail(errorMessage);
        }*/

            CompareResults("CopyFields1");
        }

        [Test]
        virtual public void CopyFields2Test() {
            InitializeDocument("CopyFields2");
            copy.SetMergeFields();

            PdfReader readerMain = new PdfReader(SOURCE_CF_11);
            PdfReader secondSourceReader = new PdfReader(SOURCE_CF_14);

            copy.AddDocument(readerMain);
            copy.CopyDocumentFields(secondSourceReader);

            copy.Close();
            readerMain.Close();
            secondSourceReader.Close();

            CompareResults("CopyFields2");
        }

        [Test]
        virtual public void CopyFields3Test() {
            InitializeDocument("CopyFields3");
            copy.SetMergeFields();

            PdfReader readerMain = new PdfReader(SOURCE_CF_12);
            PdfReader secondSourceReader = new PdfReader(SOURCE_CF_11);

            copy.AddDocument(readerMain);
            copy.CopyDocumentFields(secondSourceReader);

            copy.Close();
            readerMain.Close();
            secondSourceReader.Close();

            CompareResults("CopyFields3");
        }

        [Test]
        virtual public void CopyFields4Test() {
            InitializeDocument("CopyFields4");
            copy.SetMergeFields();

            PdfReader readerMain = new PdfReader(SOURCE_CF_13);
            PdfReader secondSourceReader = new PdfReader(SOURCE_CF_16);

            copy.AddDocument(readerMain);
            copy.CopyDocumentFields(secondSourceReader);

            copy.Close();
            readerMain.Close();
            secondSourceReader.Close();

            CompareResults("CopyFields4");
        }

        [TearDown]
        virtual public void Compress()
        {
            Document.Compress = true;
        }

        private PdfArray VerifyArraySize(PdfObject obj, int size, String message) {
            return VerifyArraySize(obj, size, message, false);
        }

        private PdfArray VerifyArraySize(PdfObject obj, int size, String message, bool ignoreIfNotArray) {
            if (!(obj is PdfArray)) {
                if (ignoreIfNotArray)
                    return null;
                Assert.Fail(message + " is not array");
            }
            if (((PdfArray) obj).Size != size)
                Assert.Fail(message + " has wrong size");
            return (PdfArray) obj;
        }

        private PdfDictionary VerifyIsDictionary(PdfObject obj, String message)
        {
            if (obj == null || !obj.IsDictionary())
                Assert.Fail(message);
            return (PdfDictionary) obj;
        }

        private void CompareResults(String name) {
            PdfReader reader = new PdfReader(OUT + name + ".pdf");
            string orig = RESOURCES + "xml\\test" + name + ".xml";
            string curr = TARGET + "xml\\test" + name + ".xml";
            FileStream xmlOut = new FileStream(curr, FileMode.Create);
            new MyTaggedPdfReaderTool().ConvertToXml(reader, xmlOut);
            xmlOut.Close();
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            Assert.True(xmldiff.Compare(orig, curr, false));
        }

        private int GetCommonNumsCount(String filename) {
            PdfReader reader = new PdfReader(filename);
            PdfDictionary structTreeRoot = reader.Catalog.GetAsDict(PdfName.STRUCTTREEROOT);
            PdfArray kids = ((PdfDictionary)PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE))).GetAsArray(PdfName.KIDS);
            int cnt = 0;
            for (int i = 0; i < kids.Size; i++) {
                PdfArray nums = kids.GetAsDict(i).GetAsArray(PdfName.NUMS);
                cnt += nums.Size;
            }
            reader.Close();
            return cnt;
        }

        private class MyTaggedPdfReaderTool : TaggedPdfReaderTool
        {
            public override void ConvertToXml(PdfReader reader, Stream os, Encoding encoding)
            {
                this.reader = reader;
                outp = new StreamWriter(os, encoding);
                outp.Write("<root>");
                // get the StructTreeRoot from the root object
                PdfDictionary catalog = reader.Catalog;
                PdfDictionary str = catalog.GetAsDict(PdfName.STRUCTTREEROOT);
                if (str == null)
                    throw new IOException("No StructTreeRoot");
                // Inspect the child or children of the StructTreeRoot
                InspectChild(str.GetDirectObject(PdfName.K));
                outp.Write("</root>");
                outp.Close();
            }
        }
    }
}
