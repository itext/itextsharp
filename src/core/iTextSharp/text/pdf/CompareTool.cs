using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace iTextSharp.text.pdf {

public class CompareTool {

    private String gsExec;
    private String compareExec;
    private String gsParams = " -dNOPAUSE -dBATCH -sDEVICE=png16m -r150 -sOutputFile=<outputfile> <inputfile>";
    private String compareParams = " <image1> <image2> <difference>";

    static private String cannotOpenTargetDirectory = "Cannot open target directory for <filename>.";
    static private String gsFailed = "GhostScript failed for <filename>.";
    static private String unexpectedNumberOfPages = "Unexpected number of pages for <filename>.";
    static private String differentPages = "File <filename> differs on page <pagenumber>.";
    static private String undefinedGsPath = "Path to GhostScript is not specified. Please use -DgsExec=<path_to_ghostscript> (e.g. -DgsExec=\"C:/Program Files/gs/gs8.64/bin/gswin32c.exe\")";

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

    public String Compare(String outPath, String differenceImage) {
        if (gsExec == null || gsExec.Length == 0) {
            return undefinedGsPath;
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

            if (File.Exists(differenceImage)) {
                File.Delete(differenceImage);
            }

            String gsParams = this.gsParams.Replace("<outputfile>", outPath + cmpImage).Replace("<inputfile>", cmpPdf);
            Process p = new Process();
            p.StartInfo.FileName = @gsExec;
            p.StartInfo.Arguments = @gsParams;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            String line;
            while ((line = p.StandardOutput.ReadLine()) != null) {
                Console.WriteLine(line);
            }
            p.StandardOutput.Close();;
            while ((line = p.StandardError.ReadLine()) != null) {
                Console.WriteLine(line);
            }
            p.StandardError.Close();
            p.WaitForExit();
            if ( p.ExitCode == 0 ) {
                gsParams = this.gsParams.Replace("<outputfile>", outPath + outImage).Replace("<inputfile>", outPdf);
                p = new Process();
                p.StartInfo.FileName = @gsExec;
                p.StartInfo.Arguments = @gsParams;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                while ((line = p.StandardOutput.ReadLine()) != null) {
                    Console.WriteLine(line);
                }
                p.StandardOutput.Close();;
                while ((line = p.StandardError.ReadLine()) != null) {
                    Console.WriteLine(line);
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
                    for (int i = 0; i < cnt; i++) {
                        Console.WriteLine("Comparing page " + (i + 1).ToString() + " (" + imageFiles[i].FullName + ")...");
                        FileStream is1 = new FileStream(imageFiles[i].FullName, FileMode.Open);
                        FileStream is2 = new FileStream(cmpImageFiles[i].FullName, FileMode.Open);
                        bool cmpResult = CompareStreams(is1, is2);
                        is1.Close();
                        is2.Close();
                        if (!cmpResult) {
                            String differentPagesFail = differentPages.Replace("<filename>", outPdf).Replace("<pagenumber>", (i + 1).ToString());
                            if (!string.IsNullOrEmpty(compareExec)) {
                                String compareParams = this.compareParams.Replace("<image1>", imageFiles[i].FullName).Replace("<image2>", cmpImageFiles[i].FullName).Replace("<difference>", differenceImage);
                                p = new Process();
                                p.StartInfo.FileName = @compareExec;
                                p.StartInfo.Arguments = @compareParams;
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.RedirectStandardError = true;
                                p.Start();

                                while ((line = p.StandardError.ReadLine()) != null) {
                                    Console.WriteLine(line);
                                }
                                p.StandardError.Close();
                                p.WaitForExit();
                                if (p.ExitCode == 0) {
                                    differentPagesFail += "\nPlease, examine " + differenceImage + " for more details.";
                                } else {

                                }
                            } else {
                                differentPagesFail += "\nYou can optionally specify path to ImageMagick compare tool (e.g. -DcompareExec=\"C:/Program Files/ImageMagick-6.5.4-2/compare.exe\") to visualize differences.";
                            }
                            if (bUnexpectedNumberOfPages)
                                differentPagesFail = unexpectedNumberOfPages.Replace("<filename>", outPdf) + "\n" + differentPagesFail;
                            return differentPagesFail;
                        } else {
                            Console.WriteLine("done.");
                        }
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

    public String Compare(String outPdf, String cmpPdf, String outPath, String differenceImage) {
        init(outPdf, cmpPdf);
        return Compare(outPath, differenceImage);
    }

    private void init(String outPdf, String cmpPdf) {
        this.outPdf = outPdf;
        this.cmpPdf = cmpPdf;
        outPdfName = Path.GetFileNameWithoutExtension(outPdf);
        cmpPdfName = Path.GetFileNameWithoutExtension(cmpPdf);
        outImage = outPdfName + "-%03d.png";
        cmpImage = "cmp_" + outPdfName + "-%03d.png";
    }

    private bool CompareStreams(FileStream is1, FileStream is2) {
        byte[] buffer1 = new byte[64 * 1024];
        byte[] buffer2 = new byte[64 * 1024];
        int len1 = 0;
        int len2 = 0;
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
        public int Compare(FileSystemInfo f1, FileSystemInfo f2) {
            String f1Name = f1.FullName;
            String f2Name = f2.FullName;
            return f1Name.CompareTo(f2Name);
        }
    }
}
}