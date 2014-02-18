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
    private const String undefinedGsPath = "Path to GhostScript is not specified. Please use -DgsExec=<path_to_ghostscript> (e.g. -DgsExec=\"C:/Program Files/gs/gs8.64/bin/gswin32c.exe\")";

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
        init(outPdf, cmpPdf);
    }

    virtual public String Compare(String outPath, String differenceImagePrefix) {
        return Compare(outPath, differenceImagePrefix, null);
    }

    virtual public String Compare(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
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

                init(outPath + ignoredAreasPrefix + outPdfName, outPath + ignoredAreasPrefix + cmpPdfName);
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
        init(outPdf, cmpPdf);
        return Compare(outPath, differenceImagePrefix, ignoredAreas);
    }

    virtual public String Compare(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix) {
        init(outPdf, cmpPdf);
        return Compare(outPath, differenceImagePrefix, null);
    }

    private void init(String outPdf, String cmpPdf) {
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
