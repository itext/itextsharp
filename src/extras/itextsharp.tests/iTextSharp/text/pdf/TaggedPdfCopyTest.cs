using System;
using System.IO;
using System.Text;
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

        public const String RESOURCES = @"..\..\resources\text\pdf\TaggedPdfCopyTest\";
        public const String SOURCE4 =  RESOURCES + "pdf\\source4.pdf";
        public const String SOURCE10 = RESOURCES + "pdf\\source10.pdf";
        public const String SOURCE11 = RESOURCES + "pdf\\source11.pdf";
        public const String SOURCE12 = RESOURCES + "pdf\\source12.pdf";
        public const String SOURCE16 = RESOURCES + "pdf\\source16.pdf";
        public const String SOURCE17 = RESOURCES + "pdf\\source17.pdf";
        public const String SOURCE22 = RESOURCES + "pdf\\source22.pdf";
        public const String SOURCE32 = RESOURCES + "pdf\\source32.pdf";
        public const String SOURCE42 = RESOURCES + "pdf\\source42.pdf";
        public const String SOURCE51 = RESOURCES + "pdf\\source51.pdf";
        public const String SOURCE52 = RESOURCES + "pdf\\source52.pdf";
        public const String SOURCE53 = RESOURCES + "pdf\\source53.pdf";

        public const String OUT = TARGET + "pdf\\out";
        public const String TARGET = "TaggedPdfCopyTest\\";


        /*static {

    }*/

        [SetUp]
        public void Initialize()
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
        public void ClassMapConflict()
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
        public void RoleMapConflict()
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
        public void PdfMergeTest()
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

            int n = 4;
            PdfReader reader1 = new PdfReader(SOURCE11);
            copy.AddPage(copy.GetImportedPage(reader1, 76, true));
            copy.AddPage(copy.GetImportedPage(reader1, 83, true));
            reader1.Close();
            PdfReader reader2 = new PdfReader(SOURCE32);
            copy.AddPage(copy.GetImportedPage(reader2, 69, true));
            copy.AddPage(copy.GetImportedPage(reader2, 267, true));
            document.Close();
            reader2.Close();
            PdfReader reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 2, "Kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);

            PdfArray array = ((PdfDictionary) obj).GetAsArray(PdfName.NUMS);
            int[] nums = new int[] {30, 32, 39, 80};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

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
        public void CopyTaggedPdf0()
        {
            InitializeDocument("0");
            PdfReader reader = new PdfReader(SOURCE11);
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; ++i)
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            document.Close();
            reader.Close();

            reader = new PdfReader(output);
            PdfDictionary structTreeRoot =
                VerifyIsDictionary(reader.Catalog.GetDirectObject(PdfName.STRUCTTREEROOT), NO_STRUCT_TREE_ROOT);
            VerifyArraySize(structTreeRoot.Get(PdfName.K), 5, "Invalid count of kids in StructTreeRoot");
            PdfObject obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            VerifyIsDictionary(obj, NO_PARENT_TREE);
            obj = ((PdfDictionary) obj).Get(PdfName.KIDS);
            VerifyArraySize(obj, 3, "Invalid nums.");
            PdfObject numsDict = PdfStructTreeController.GetDirectObject(((PdfArray) obj).GetDirectObject(0));
            VerifyIsDictionary(numsDict, "nums1");
            PdfArray[] arrays = new PdfArray[3];
            arrays[0] = VerifyArraySize(((PdfDictionary) numsDict).Get(PdfName.NUMS), 128, "nums 1");
            numsDict = PdfStructTreeController.GetDirectObject(((PdfArray) obj).GetDirectObject(1));
            VerifyIsDictionary(numsDict, "nums2");
            arrays[1] = VerifyArraySize(((PdfDictionary) numsDict).Get(PdfName.NUMS), 128, "nums 2");
            numsDict = PdfStructTreeController.GetDirectObject(((PdfArray) obj).GetDirectObject(2));
            VerifyIsDictionary(numsDict, "nums3");
            arrays[2] = VerifyArraySize(((PdfDictionary) numsDict).Get(PdfName.NUMS), 4, "nums 3");
            int[] nums = new int[]
                {
                    3, 91, 42, 19, 15, 15, 9, 13, 15, 17, 18, 5, 17, 37, 24, 19, 15, 23, 8, 11,
                    17, 11, 13, 29, 18, 12, 11, 9, 14, 26, 17, 22, 30, 15, 21, 28, 22, 24, 22, 20, 21, 17, 24,
                    25, 20, 14, 32, 25, 14, 15, 24, 20, 22, 24, 21, 22, 18, 12, 23, 29, 19, 22, 21, 27, 25, 19,
                    6, 14, 18, 21, 25, 11, 26, 15, 15, 30, 23, 32, 17, 22, 18, 18, 32, 18, 16, 21, 28, 28, 10,
                    18, 13, 23, 17, 19, 24, 29, 25, 34, 26, 24, 28, 27, 21, 23, 23, 23, 10, 10, 10, 9, 16, 20,
                    16, 16, 22, 27, 14, 3, 11, 30, 11, 29, 6, 99, 117, 128, 92, 67, 132, 108
                };

            //OutputStreamWriter writer = new FileWriter(OUT0+".txt");
            int k = 0;
            for (int i = 0; i < arrays.Length; ++i)
                for (int j = 0; j < arrays[i].Size/2; ++j)
                    VerifyArraySize(PdfStructTreeController.GetDirectObject(arrays[i].GetDirectObject(j*2 + 1)), nums[k++],
                                    "Nums of page " + (i + 1));
            //writer.write(((PdfArray)(PdfStructTreeController.GetDirectObject(arrays[i].GetDirectObject(j*2+1)))).size()+", ");
            //writer.Close();

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
        public void CopyTaggedPdf1()
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
            VerifyArraySize(array, 2, "Nums");
            VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(1)), 61, "Nums of page 1");
            reader.Close();
            CompareResults("1");
        }

        [Test]
        public void CopyTaggedPdf2()
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
        public void CopyTaggedPdf3()
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
            int[] nums = new int[] {16, 87, 128, 74, 74, 74, 26};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("3");
        }

        [Test]
        public void CopyTaggedPdf4()
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
            int[] nums = new int[] {26, 74, 74, 74, 128, 87, 16};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("4");
        }

        [Test]
        public void CopyTaggedPdf5()
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
            int[] nums = new int[] {16, 128, 26};
            for (int i = 0; i < n; ++i)
                //nums[i] = ((PdfArray)PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2+1))).size();
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("5");
        }

        [Test]
        public void CopyTaggedPdf6()
        {
            InitializeDocument("6");
            PdfReader reader = new PdfReader(SOURCE11);
            int n = 8;
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
            int[] nums = new int[] {3, 18, 9, 25, 15, 91, 13, 18};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

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
        public void CopyTaggedPdf7()
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
        public void CopyTaggedPdf8()
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
            n *= 4;
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
            int[] nums = new int[] {42, 42, 11, 11, 13, 13, 42, 11, 13, 42, 11, 13};
            for (int i = 0; i < n; ++i)
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("8");
        }

        [Test]
        public void CopyTaggedPdf9()
        {
            InitializeDocument("9");
            PdfReader reader4 = new PdfReader(SOURCE4);
            PdfReader reader10 = new PdfReader(SOURCE10);
            PdfReader reader32 = new PdfReader(SOURCE32);
            int n = 20;
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
            int[] nums = new int[] {7, 87, 128, 26, 135, 83, 7, 135, 83, 116, 26, 128, 74, 16, 11, 38, 51, 61, 7, 26};
            for (int i = 0; i < n; ++i)
                //            nums[i] = ((PdfArray)PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2+1))).size();
                VerifyArraySize(PdfStructTreeController.GetDirectObject(array.GetDirectObject(i*2 + 1)), nums[i],
                                "Nums of page " + (i + 1));

            reader.Close();
            CompareResults("9");
        }

        [Test]
        public void CopyTaggedPdf10()
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
        public void CopyTaggedPdf11()
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
        public void CopyTaggedPdf12()
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
        public void CopyTaggedPdf13()
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

        [TearDown]
        public void Compress()
        {
            Document.Compress = true;
        }

        private PdfArray VerifyArraySize(PdfObject obj, int size, String message)
        {
            if (obj == null || !obj.IsArray())
                Assert.Fail(message + " is not array");
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

        private void CompareResults(String name)
        {
            PdfReader reader = new PdfReader(OUT + name + ".pdf");
            string orig = RESOURCES + "xml\\test" + name + ".xml";
            string curr = TARGET + "xml\\test" + name + ".xml";
            FileStream xmlOut = new FileStream(curr, FileMode.Create);
            new MyTaggedPdfReaderTool().ConvertToXml(reader, xmlOut);
            xmlOut.Close();
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            Assert.True(xmldiff.Compare(orig, curr, false));

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