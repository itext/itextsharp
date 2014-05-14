using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
/*
 * $Id: CompareTool.cs 318 2012-02-27 22:46:07Z eugenemark $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2014 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.testutils {

public class CompareTool {

    private String gsExec;
    private String compareExec;
    private const String gsParams = " -dNOPAUSE -dBATCH -sDEVICE=png16m -r150 -sOutputFile=<outputfile> <inputfile>";
    private const String compareParams = " <image1> <image2> <difference>";

    private const String cannotOpenTargetDirectory = "Cannot open target directory for <filename>.";
    private const String gsFailed = "GhostScript failed for <filename>.";
    private const String unexpectedNumberOfPages = "Unexpected number of pages for <filename>.";
    private const String differentPages = "File <filename> differs on page <pagenumber>.";
    private const String undefinedGsPath = "Path to GhostScript is not specified. Please use -DgsExec=<path_to_ghostscript> (e.g. -DgsExec=\"C:/Program Files/gs/gs9.14/bin/gswin32c.exe\")";

    private const String ignoredAreasPrefix = "ignored_areas_";

    private String outPdf;
    private String outPdfName;
    private String outImage;
    private String cmpPdf;
    private String cmpPdfName;
    private String cmpImage;


    public CompareTool(String outPdf, String cmpPdf) {
        gsExec = Environment.GetEnvironmentVariable("gsExec");
        compareExec = Environment.GetEnvironmentVariable("compareExec");
        Init(outPdf, cmpPdf);
    }

    virtual public String Compare(String outPath, String differenceImagePrefix) {
        return Compare(outPath, differenceImagePrefix, null);
    }

    public virtual String Compare(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        return Compare(outPath, differenceImagePrefix, ignoredAreas, null);
    }

    virtual public String Compare(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas, IList<int> equalPages) {
        if (gsExec == null)
            return undefinedGsPath;
        if (!File.Exists(gsExec)) {
            return gsExec + " does not exist";
        }

        try {
            DirectoryInfo targetDir;
            FileSystemInfo[] allImageFiles;
            FileSystemInfo[] imageFiles;
            FileSystemInfo[] cmpImageFiles;
            if (Directory.Exists(outPath)) {
                targetDir = new DirectoryInfo(outPath);
                allImageFiles = targetDir.GetFileSystemInfos("*.png");
                imageFiles = Array.FindAll(allImageFiles, PngPredicate);
                foreach (FileSystemInfo fileSystemInfo in imageFiles) {
                    fileSystemInfo.Delete();
                }

                cmpImageFiles = Array.FindAll(allImageFiles, CmpPngPredicate);
                foreach (FileSystemInfo fileSystemInfo in cmpImageFiles) {
                    fileSystemInfo.Delete();
                }
            } else
                targetDir = Directory.CreateDirectory(outPath);

            if (File.Exists(outPath + differenceImagePrefix)) {
                File.Delete(outPath + differenceImagePrefix);
            }

            if (ignoredAreas != null && ignoredAreas.Count > 0) {
                PdfReader cmpReader = new PdfReader(cmpPdf);
                PdfReader outReader = new PdfReader(outPdf);
                PdfStamper outStamper = new PdfStamper(outReader,
                    new FileStream(outPath + ignoredAreasPrefix + outPdfName, FileMode.Create));
                PdfStamper cmpStamper = new PdfStamper(cmpReader,
                    new FileStream(outPath + ignoredAreasPrefix + cmpPdfName, FileMode.Create));

                foreach (KeyValuePair<int, IList<Rectangle>> entry in ignoredAreas) {
                    int pageNumber = entry.Key;
                    IList<Rectangle> rectangles = entry.Value;

                    if (rectangles != null && rectangles.Count > 0) {
                        PdfContentByte outCB = outStamper.GetOverContent(pageNumber);
                        PdfContentByte cmpCB = cmpStamper.GetOverContent(pageNumber);

                        foreach (Rectangle rect in rectangles) {
                            rect.BackgroundColor = BaseColor.BLACK;
                            outCB.Rectangle(rect);
                            cmpCB.Rectangle(rect);
                        }
                    }
                }

                outStamper.Close();
                cmpStamper.Close();

                outReader.Close();
                cmpReader.Close();

                Init(outPath + ignoredAreasPrefix + outPdfName, outPath + ignoredAreasPrefix + cmpPdfName);
            }

            String gsParams = CompareTool.gsParams.Replace("<outputfile>", outPath + cmpImage).Replace("<inputfile>", cmpPdf);
            Process p = new Process();
            p.StartInfo.FileName = @gsExec;
            p.StartInfo.Arguments = @gsParams;
            p.StartInfo.UseShellExecute = false;   
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            String line;
            while ((line = p.StandardOutput.ReadLine()) != null) {
                Console.Out.WriteLine(line);
            }
            p.StandardOutput.Close();;
            while ((line = p.StandardError.ReadLine()) != null) {
                Console.Out.WriteLine(line);
            }
            p.StandardError.Close();
            p.WaitForExit();
            if ( p.ExitCode == 0 ) {
                gsParams = CompareTool.gsParams.Replace("<outputfile>", outPath + outImage).Replace("<inputfile>", outPdf);
                p = new Process();
                p.StartInfo.FileName = @gsExec;
                p.StartInfo.Arguments = @gsParams;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                while ((line = p.StandardOutput.ReadLine()) != null) {
                    Console.Out.WriteLine(line);
                }
                p.StandardOutput.Close();;
                while ((line = p.StandardError.ReadLine()) != null) {
                    Console.Out.WriteLine(line);
                }
                p.StandardError.Close();
                p.WaitForExit();

                if (p.ExitCode == 0) {
                    allImageFiles = targetDir.GetFileSystemInfos("*.png");
                    imageFiles = Array.FindAll(allImageFiles, PngPredicate);
                    cmpImageFiles = Array.FindAll(allImageFiles, CmpPngPredicate);
                    bool bUnexpectedNumberOfPages = imageFiles.Length != cmpImageFiles.Length;
                    int cnt = Math.Min(imageFiles.Length, cmpImageFiles.Length);
                    if (cnt < 1) {
                        return "No files for comparing!!!\nThe result or sample pdf file is not processed by GhostScript.";
                    }
                    Array.Sort(imageFiles, new ImageNameComparator());
                    Array.Sort(cmpImageFiles, new ImageNameComparator());
                    String differentPagesFail = null;
                    for (int i = 0; i < cnt; i++) {
                        if (equalPages != null && equalPages.Contains(i))
                            continue;
                        Console.Out.WriteLine("Comparing page " + (i + 1).ToString() + " (" + imageFiles[i].FullName + ")...");
                        FileStream is1 = new FileStream(imageFiles[i].FullName, FileMode.Open);
                        FileStream is2 = new FileStream(cmpImageFiles[i].FullName, FileMode.Open);
                        bool cmpResult = CompareStreams(is1, is2);
                        is1.Close();
                        is2.Close();
                        if (!cmpResult) {
                            if (File.Exists(compareExec)) {
                                String compareParams = CompareTool.compareParams.Replace("<image1>", imageFiles[i].FullName).Replace("<image2>", cmpImageFiles[i].FullName).Replace("<difference>", outPath + differenceImagePrefix + (i + 1).ToString() + ".png");
                                p = new Process();
                                p.StartInfo.FileName = @compareExec;
                                p.StartInfo.Arguments = @compareParams;
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.RedirectStandardError = true;
                                p.StartInfo.CreateNoWindow = true;
                                p.Start();

                                while ((line = p.StandardError.ReadLine()) != null) {
                                    Console.Out.WriteLine(line);
                                }
                                p.StandardError.Close();
                                p.WaitForExit();
                                if (p.ExitCode == 0) {
                                    if (differentPagesFail == null) {
                                        differentPagesFail =
                                            differentPages.Replace("<filename>", outPdf).Replace("<pagenumber>",
                                                                                                 (i + 1).ToString());
                                        differentPagesFail += "\nPlease, examine " + outPath + differenceImagePrefix + (i + 1).ToString() +
                                                              ".png for more details.";
                                    } else {
                                        differentPagesFail =
                                            "File " + outPdf + " differs.\nPlease, examine difference images for more details.";    
                                    }
                                }
                                else
                                {
                                    differentPagesFail = differentPages.Replace("<filename>", outPdf).Replace("<pagenumber>", (i + 1).ToString());
                                    Console.Out.WriteLine("Invalid compareExec variable.");
                                }
                            } else {
                                differentPagesFail =
                                            differentPages.Replace("<filename>", outPdf).Replace("<pagenumber>",
                                                                                                 (i + 1).ToString());
                                differentPagesFail += "\nYou can optionally specify path to ImageMagick compare tool (e.g. -DcompareExec=\"C:/Program Files/ImageMagick-6.5.4-2/compare.exe\") to visualize differences.";
                                break;
                            }
                        } else {
                            Console.Out.WriteLine("done.");
                        }
                    }
                    if (differentPagesFail != null) {
                        return differentPagesFail;
                    } else {
                        if (bUnexpectedNumberOfPages)
                            return unexpectedNumberOfPages.Replace("<filename>", outPdf);
                    }
                } else {
                    return gsFailed.Replace("<filename>", outPdf);
                }
            } else {
                return gsFailed.Replace("<filename>", cmpPdf);
            }
        } catch(Exception) {
            return cannotOpenTargetDirectory.Replace("<filename>", outPdf);
        }

        return null;
    }

    virtual public String Compare(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        Init(outPdf, cmpPdf);
        return Compare(outPath, differenceImagePrefix, ignoredAreas);
    }

    virtual public String Compare(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix) {
        Init(outPdf, cmpPdf);
        return Compare(outPath, differenceImagePrefix, null);
    }

    private IList<PdfDictionary> outPages;
    private IList<RefKey> outPagesRef;
    private IList<PdfDictionary> cmpPages;
    private IList<RefKey> cmpPagesRef;

    public virtual String CompareByContent(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        PdfReader outReader = new PdfReader(outPdf);
        outPages = new List<PdfDictionary>();
        outPagesRef = new List<RefKey>();
        LoadPagesFromReader(outReader, outPages, outPagesRef);

        PdfReader cmpReader = new PdfReader(cmpPdf);
        cmpPages = new List<PdfDictionary>();
        cmpPagesRef = new List<RefKey>();
        LoadPagesFromReader(cmpReader, cmpPages, cmpPagesRef);

        if (outPages.Count != cmpPages.Count)
            return Compare(outPath, differenceImagePrefix, ignoredAreas);

        IList<int> equalPages = new List<int>(cmpPages.Count);
        for (int i = 0; i < cmpPages.Count; i++) {
            if (ObjectsIsEquals(outPages[i], cmpPages[i]))
                equalPages.Add(i);
        }
        outReader.Close();
        cmpReader.Close();


        if (equalPages.Count == cmpPages.Count) {
            return null;
        } else {
            String message = Compare(outPath, differenceImagePrefix, ignoredAreas, equalPages);
            if (message == null || message.Length == 0)
                return "Compare by content fails.\nNo visual differences";
            return message;
        }
    }

    public virtual String CompareByContent(String outPath, String differenceImagePrefix) {
        return CompareByContent(outPath, differenceImagePrefix, null);
    }

    public virtual String CompareByContent(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        Init(outPdf, cmpPdf);
        return CompareByContent(outPath, differenceImagePrefix, ignoredAreas);
    }

    public virtual String compareByContent(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix) {
        return CompareByContent(outPdf, cmpPdf, outPath, differenceImagePrefix, null);
    }

    private void LoadPagesFromReader(PdfReader reader, IList<PdfDictionary> pages, IList<RefKey> pagesRef) {
        PdfObject pagesDict = reader.Catalog.Get(PdfName.PAGES);
        AddPagesFromDict(pagesDict, pages, pagesRef);
    }

    private void AddPagesFromDict(PdfObject dictRef, IList<PdfDictionary> pages, IList<RefKey> pagesRef) {
        PdfDictionary dict = (PdfDictionary)PdfReader.GetPdfObject(dictRef);
        if (dict.IsPages()) {
            PdfArray kids = dict.GetAsArray(PdfName.KIDS);
            if (kids == null) return;
            foreach (PdfObject kid in kids) {
                AddPagesFromDict(kid, pages, pagesRef);
            }
        } else if(dict.IsPage()) {
            pages.Add(dict);
            pagesRef.Add(new RefKey((PRIndirectReference)dictRef));
        }
    }

    private bool ObjectsIsEquals(PdfDictionary outDict, PdfDictionary cmpDict) {
        foreach (PdfName key in cmpDict.Keys) {
            if (key.CompareTo(PdfName.PARENT) == 0) continue;
            if (key.CompareTo(PdfName.BASEFONT) == 0 || key.CompareTo(PdfName.FONTNAME) == 0) {
                PdfObject cmpObj = cmpDict.GetDirectObject(key);
                if (cmpObj.IsName() && cmpObj.ToString().IndexOf('+') > 0) {
                    PdfObject outObj = outDict.GetDirectObject(key);
                    if (!outObj.IsName())
                        return false;
                    String cmpName = cmpObj.ToString().Substring(cmpObj.ToString().IndexOf('+'));
                    String outName = outObj.ToString().Substring(outObj.ToString().IndexOf('+'));
                    if (!cmpName.Equals(outName))
                        return false;
                    continue;
                }
            }
            if (!ObjectsIsEquals(outDict.Get(key), cmpDict.Get(key)))
                return false;
        }
        return true;
    }

    private bool ObjectsIsEquals(PdfObject outObj, PdfObject cmpObj) {
        PdfObject outDirectObj = PdfReader.GetPdfObject(outObj);
        PdfObject cmpDirectObj = PdfReader.GetPdfObject(cmpObj);

        if (outDirectObj == null || cmpDirectObj.Type != outDirectObj.Type)
            return false;
        if (cmpDirectObj.IsDictionary()) {
            PdfDictionary cmpDict = (PdfDictionary)cmpDirectObj;
            PdfDictionary outDict = (PdfDictionary)outDirectObj;
            if (cmpDict.IsPage()) {
                if (!outDict.IsPage())
                    return false;
                RefKey cmpRefKey = new RefKey((PRIndirectReference)cmpObj);
                RefKey outRefKey = new RefKey((PRIndirectReference)outObj);
                if (cmpPagesRef.Contains(cmpRefKey) && cmpPagesRef.IndexOf(cmpRefKey) == outPagesRef.IndexOf(outRefKey))
                    return true;
                return false;
            }
            if (!ObjectsIsEquals(outDict, cmpDict))
                return false;
        } else if (cmpDirectObj.IsStream()) {
            if (!ObjectsIsEquals((PRStream)outDirectObj, (PRStream)cmpDirectObj))
                return false;
        } else if (cmpDirectObj.IsArray()) {
            if (!ObjectsIsEquals((PdfArray)outDirectObj, (PdfArray)cmpDirectObj))
                return false;
        } else if (cmpDirectObj.IsName()) {
            if (!ObjectsIsEquals((PdfName)outDirectObj, (PdfName)cmpDirectObj))
                return false;
        } else if (cmpDirectObj.IsNumber()) {
            if (!ObjectsIsEquals((PdfNumber)outDirectObj, (PdfNumber)cmpDirectObj))
                return false;
        } else if (cmpDirectObj.IsString()) {
            if (!ObjectsIsEquals((PdfString)outDirectObj, (PdfString)cmpDirectObj))
                return false;
        } else if (cmpDirectObj.IsBoolean()) {
            if (!ObjectsIsEquals((PdfBoolean)outDirectObj, (PdfBoolean)cmpDirectObj))
                return false;
        } else {
            throw new InvalidOperationException();
        }
        return true;
    }

    private bool ObjectsIsEquals(PRStream outStream, PRStream cmpStream) {
        return ArraysAreEqual(PdfReader.GetStreamBytesRaw(outStream), PdfReader.GetStreamBytesRaw(cmpStream));
    }

    private static bool ArraysAreEqual<T>(T[] outBytes, T[] cmpBytes) {
        if (outBytes == null && cmpBytes == null)
            return true;
        if (outBytes == null || cmpBytes == null || outBytes.Length != cmpBytes.Length)
            return false;
        for (int ind = 0; ind < outBytes.Length; ind++)
            if (!outBytes[ind].Equals(cmpBytes[ind]))
                return false;
        return true;
    }

    private bool ObjectsIsEquals(PdfArray outArray, PdfArray cmpArray) {
        if (outArray == null || outArray.Size != cmpArray.Size)
            return false;
        for (int i = 0; i < cmpArray.Size; i++) {
            if (!ObjectsIsEquals(outArray[i], cmpArray[i]))
                return false;
        }

        return true;
    }

    private bool ObjectsIsEquals(PdfName outName, PdfName cmpName) {
        return cmpName.CompareTo(outName) == 0;
    }

    private bool ObjectsIsEquals(PdfNumber outNumber, PdfNumber cmpNumber) {
        return cmpNumber.DoubleValue == outNumber.DoubleValue;
    }

    private bool ObjectsIsEquals(PdfString outString, PdfString cmpString) {
        return ArraysAreEqual(cmpString.GetBytes(), outString.GetBytes());
    }

    private bool ObjectsIsEquals(PdfBoolean outBoolean, PdfBoolean cmpBoolean) {
        return ArraysAreEqual(cmpBoolean.GetBytes(), outBoolean.GetBytes());
    }

    private void Init(String outPdf, String cmpPdf) {
        this.outPdf = outPdf;
        this.cmpPdf = cmpPdf;
        outPdfName = Path.GetFileName(outPdf);
        cmpPdfName = Path.GetFileName(cmpPdf);
        //template for GhostScript and ImageMagic
        outImage = outPdfName + "-%03d.png";
        if (cmpPdfName.StartsWith("cmp_")) cmpImage = cmpPdfName + "-%03d.png";
        else cmpImage = "cmp_" + cmpPdfName + "-%03d.png";
    }

    private bool CompareStreams(FileStream is1, FileStream is2) {
        byte[] buffer1 = new byte[64 * 1024];
        byte[] buffer2 = new byte[64 * 1024];
        int len1;
        int len2;
        for (; ;) {
            len1 = is1.Read(buffer1, 0, 64 * 1024);
            len2 = is2.Read(buffer2, 0, 64 * 1024);
            if (len1 != len2)
                return false;

            if (len1 == -1 || len1 == 0)
                break;

            for (int i = 0; i < len1; i++)
                if (buffer1[i] != buffer2[i])
                    return false;

            if (len1 < buffer1.Length)
                break;

        }
        return true;
    }

    private bool LinksAreSame(PdfAnnotation.PdfImportedLink cmpLink, PdfAnnotation.PdfImportedLink outLink) {
        // Compare link boxes, page numbers the links refer to, and simple parameters (non-indirect, non-arrays, non-dictionaries)

        if (cmpLink.GetDestinationPage() != outLink.GetDestinationPage())
            return false;
        if (!cmpLink.GetRect().ToString().Equals(outLink.GetRect().ToString()))
            return false;

        IDictionary<PdfName, PdfObject> cmpParams = cmpLink.GetParameters();
        IDictionary<PdfName, PdfObject> outParams = outLink.GetParameters();
        if (cmpParams.Count != outParams.Count)
            return false;

        foreach (KeyValuePair<PdfName, PdfObject> cmpEntry in cmpParams) {
            PdfObject cmpObj = cmpEntry.Value;
            if (!outParams.ContainsKey(cmpEntry.Key))
                return false;
            PdfObject outObj = outParams[cmpEntry.Key];
            if (cmpObj.Type != outObj.Type)
                return false;

            switch (cmpObj.Type) {
                case PdfObject.NULL:
                case PdfObject.BOOLEAN:
                case PdfObject.NUMBER:
                case PdfObject.STRING:
                case PdfObject.NAME:
                    if (!cmpObj.ToString().Equals(outObj.ToString()))
                        return false;
                    break;
            }
        }

        return true;
    }

    virtual public String CompareLinks(String outPdf, String cmpPdf) {
        Console.Out.WriteLine("Comparing link annotations...");
        String message = null;
        PdfReader outReader = new PdfReader(outPdf);
        PdfReader cmpReader = new PdfReader(cmpPdf);
        for (int i = 0; i < outReader.NumberOfPages && i < cmpReader.NumberOfPages; i++) {
            List<PdfAnnotation.PdfImportedLink> outLinks = outReader.GetLinks(i + 1);
            List<PdfAnnotation.PdfImportedLink> cmpLinks = cmpReader.GetLinks(i + 1);
            if (cmpLinks.Count != outLinks.Count) {
                message = String.Format("Different number of links on page {0}.", i + 1);
                break;
            }
            for (int j = 0; j < cmpLinks.Count; j++) {
                if (!LinksAreSame(cmpLinks[j], outLinks[j])) {
                    message = String.Format("Different links on page {0}.\n{1}\n{2}", i + 1, cmpLinks[j].ToString(),
                        outLinks[j].ToString());
                    break;
                }
            }
        }
        outReader.Close();
        cmpReader.Close();
        return message;
    }

    private bool PngPredicate(FileSystemInfo pathname) {
        String ap = pathname.Name;
        bool b1 = ap.EndsWith(".png");
        bool b2 = ap.Contains("cmp_");
        return b1 && !b2 && ap.Contains(outPdfName);
    }

    private bool CmpPngPredicate(FileSystemInfo pathname) {
        String ap = pathname.Name;
        bool b1 = ap.EndsWith(".png");
        bool b2 = ap.Contains("cmp_");
        return b1 && b2 && ap.Contains(cmpPdfName);
    }

    class ImageNameComparator : IComparer<FileSystemInfo> {
        virtual public int Compare(FileSystemInfo f1, FileSystemInfo f2) {
            String f1Name = f1.FullName;
            String f2Name = f2.FullName;
            return f1Name.CompareTo(f2Name);
        }
    }
}
}
