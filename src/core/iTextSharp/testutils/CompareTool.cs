using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.util;
/*
 * $Id: CompareTool.cs 318 2012-02-27 22:46:07Z eugenemark $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
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
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more inFormation, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.util.collections;
using iTextSharp.text.io;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;

namespace iTextSharp.testutils {

public class CompareTool {
    protected class ObjectPath {
        protected RefKey baseCmpObject;
        protected RefKey baseOutObject;
        protected Stack<PathItem> path = new Stack<PathItem>();
        protected Stack<Pair<RefKey>> indirects = new Stack<Pair<RefKey>>();

        public ObjectPath() {
        }

        public ObjectPath(RefKey baseCmpObject, RefKey baseOutObject) {
            this.baseCmpObject = baseCmpObject;
            this.baseOutObject = baseOutObject;
        }

        private ObjectPath(RefKey baseCmpObject, RefKey baseOutObject, Stack<PathItem> path) {
            this.baseCmpObject = baseCmpObject;
            this.baseOutObject = baseOutObject;
            this.path = path;
        }

        protected class Pair<T> {
            private T first;
            private T second;

            public Pair(T first, T second) {
                this.first = first;
                this.second = second;
            }

            public override bool Equals(object obj) {
                return (obj is Pair<T> && first.Equals(((Pair<T>) obj).first) &&
                        second.Equals(((Pair<T>) obj).second));
            }

            public override int GetHashCode() {
                return first.GetHashCode()*31 + second.GetHashCode();
            }
        }

        protected abstract class PathItem {
            public abstract XmlNode ToXmlNode(XmlDocument document);
        }

        private class DictPathItem : PathItem {

            String key;

            public DictPathItem(String key) {
                this.key = key;
            }

            public override String ToString() {
                return "Dict key: " + key;
            }

            public override int GetHashCode() {
                return key.GetHashCode();
            }

            public override bool Equals(Object obj) {
                return obj is DictPathItem && key.Equals(((DictPathItem) obj).key);
            }

            public override XmlNode ToXmlNode(XmlDocument document) {
                XmlNode element = document.CreateElement("dictKey");
                element.AppendChild(document.CreateTextNode(key));
                return element;
            }
        }

        private class ArrayPathItem : PathItem {

            int index;

            public ArrayPathItem(int index) {
                this.index = index;
            }

            public override String ToString() {
                return "Array index: " + index.ToString();
            }

            public override int GetHashCode() {
                return index;
            }

            public override bool Equals(Object obj) {
                return obj is ArrayPathItem && index == ((ArrayPathItem) obj).index;
            }

            public override XmlNode ToXmlNode(XmlDocument document) {
                XmlNode element = document.CreateElement("arrayIndex");
                element.AppendChild(document.CreateTextNode(index.ToString()));
                return element;
            }
        }

        private class OffsetPathItem : PathItem {

            private int offset;

            public OffsetPathItem(int offset) {
                this.offset = offset;
            }

            public override String ToString() {
                return "Offset: " + offset;
            }

            public override int GetHashCode() {
                return offset;
            }

            public override bool Equals(Object obj) {
                return obj is OffsetPathItem && offset == ((OffsetPathItem) obj).offset;
            }

            public override XmlNode ToXmlNode(XmlDocument document) {
                XmlNode element = document.CreateElement("offset");
                element.AppendChild(document.CreateTextNode(offset.ToString()));

                return element;
            }
        }

        public ObjectPath ResetDirectPath(RefKey baseCmpObject, RefKey baseOutObject) {
            ObjectPath newPath = new ObjectPath(baseCmpObject, baseOutObject);
            newPath.indirects = new Stack<Pair<RefKey>>(new Stack<Pair<RefKey>>(indirects));
            newPath.indirects.Push(new Pair<RefKey>(baseCmpObject, baseOutObject));
            return newPath;
        }

        public bool IsComparing(RefKey baseCmpObject, RefKey baseOutObject) {
            return indirects.Contains(new Pair<RefKey>(baseCmpObject, baseOutObject));
        }

        public void PushArrayItemToPath(int index) {
            path.Push(new ArrayPathItem(index));
        }

        public void PushDictItemToPath(String key) {
            path.Push(new DictPathItem(key));
        }

        public void PushOffsetToPath(int offset) {
            path.Push(new OffsetPathItem(offset));
        }

        public void Pop() {
            path.Pop();
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();

            foreach (PathItem pathItem in path) {
                sb.Insert(0, "\n" + pathItem.ToString());
            }

            sb.Insert(0, String.Format("Base cmp object: {0} obj. Base out object: {1} obj", baseCmpObject, baseOutObject));

            return sb.ToString();
        }

        public override int GetHashCode() {
            int code1 = baseCmpObject != null ? baseCmpObject.GetHashCode() : 1;
            int code2 = baseOutObject != null ? baseOutObject.GetHashCode() : 1;
            int hashCode = code1 * 31 + code2;

            foreach (PathItem pathItem in path) {
                hashCode *= 31;
                hashCode += pathItem.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(Object obj) {
            return obj is ObjectPath && baseCmpObject.Equals(((ObjectPath) obj).baseCmpObject) && baseOutObject.Equals(((ObjectPath) obj).baseOutObject) &&
                    Util.AreEqual(path, ((ObjectPath) obj).path);
        }

        public Object Clone() {
            return new ObjectPath(baseCmpObject, baseOutObject, new Stack<PathItem>(new Stack<PathItem>(path)));
        }

        public XmlNode ToXmlNode(XmlDocument document) {
            XmlElement baseNode = document.CreateElement("base");
            baseNode.SetAttribute("cmp", baseCmpObject.ToString() + " obj");
            baseNode.SetAttribute("out", baseOutObject.ToString() + " obj");

            XmlElement element = document.CreateElement("path");

            foreach (PathItem pathItem in path) {
                element.PrependChild(pathItem.ToXmlNode(document));
            }

            element.PrependChild(baseNode);

            return element;
        }
    }

    protected class CompareResult {
        protected Dictionary<ObjectPath, String> differences = new Dictionary<ObjectPath, String>();
        protected int messageLimit = 1;

        public CompareResult(int messageLimit) {
            this.messageLimit = messageLimit;
        }

        public bool isOk() {
            return differences.Count == 0;
        }

        public int GetErrorCount() {
            return differences.Count;
        }

        public bool IsMessageLimitReached() {
            return differences.Count >= messageLimit;
        }

        public String GetReport() {
            StringBuilder sb = new StringBuilder();
            bool firstEntry = true;

            foreach (KeyValuePair<ObjectPath, String> entry in differences) {
                if (!firstEntry) {
                    sb.Append("-----------------------------").Append("\n");
                }

                ObjectPath diffPath = entry.Key;
                sb.Append(entry.Value).Append("\n").Append(diffPath.ToString()).Append("\n");
                firstEntry = false;
            }

            return sb.ToString();
        }

        public void AddError(ObjectPath path, String message) {
            if (differences.Count < messageLimit && !differences.ContainsKey(path)) {
                differences[((ObjectPath) path.Clone())] = message;
            }
        }

        public void WriteReportToXml(Stream stream) {
            XmlDocument xmlReport = new XmlDocument();
            XmlElement errors = xmlReport.CreateElement("errors");
            errors.SetAttribute("count", differences.Count.ToString());

            XmlElement root = xmlReport.CreateElement("report");
            root.AppendChild(errors);

            foreach (KeyValuePair<ObjectPath, String> entry in differences) {
                XmlNode errorNode = xmlReport.CreateElement("error");
                XmlNode message = xmlReport.CreateElement("message");
                message.AppendChild(xmlReport.CreateTextNode(entry.Value));
                XmlNode path = entry.Key.ToXmlNode(xmlReport);
                errorNode.AppendChild(message);
                errorNode.AppendChild(path);
                errors.AppendChild(errorNode);
            }

            xmlReport.AppendChild(root);
            xmlReport.PreserveWhitespace = true;

            using (XmlTextWriter writer = new XmlTextWriter(stream, null)) {
                writer.Formatting = Formatting.Indented;
                xmlReport.Save(writer);
            }
        }
    }

    private String gsExec;
    private String compareExec;
    private const String gsParams = " -dNOPAUSE -dBATCH -sDEVICE=png16m -r150 -sOutputFile=<outputfile> <inputfile>";
    private const String compareParams = " \"<image1>\" \"<image2>\" \"<difference>\"";

    private const String cannotOpenTarGetDirectory = "Cannot open tarGet directory for <filename>.";
    private const String gsFailed = "GhostScript failed for <filename>.";
    private const String unexpectedNumberOfPages = "Unexpected number of pages for <filename>.";
    private const String differentPages = "File <filename> differs on page <pagenumber>.";
    private const String undefinedGsPath = "Path to GhostScript is not specified. Please use -DgsExec=<path_to_ghostscript> (e.g. -DgsExec=\"C:/Program Files/gs/gs9.14/bin/gswin32c.exe\")";

    private const String ignoredAreasPrefix = "ignored_areas_";

    private String cmpPdf;
    private String cmpPdfName;
    private String cmpImage;
    private String outPdf;
    private String outPdfName;
    private String outImage;

    private IList<PdfDictionary> outPages;
    private IList<RefKey> outPagesRef;
    private IList<PdfDictionary> cmpPages;
    private IList<RefKey> cmpPagesRef;

    private int compareByContentErrorsLimit = 1;
    private bool generateCompareByContentXmlReport = false;
    private String xmlReportName = "report";
    private double floatComparisonError = 0;
    // if false, the error will be relative
    private bool absoluteError = true;

    public CompareTool() {
        gsExec = Environment.GetEnvironmentVariable("gsExec");
        compareExec = Environment.GetEnvironmentVariable("compareExec");
    }

    private String Compare(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        return Compare(outPath, differenceImagePrefix, ignoredAreas, null);
    }

    private String Compare(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas, IList<int> equalPages) {
        if (gsExec == null)
            return undefinedGsPath;
        if (!File.Exists(gsExec)) {
            return gsExec + " does not exist";
        }

        try {
            DirectoryInfo tarGetDir;
            FileSystemInfo[] allImageFiles;
            FileSystemInfo[] imageFiles;
            FileSystemInfo[] cmpImageFiles;
            if (Directory.Exists(outPath)) {
                tarGetDir = new DirectoryInfo(outPath);
                allImageFiles = tarGetDir.GetFileSystemInfos("*.png");
                imageFiles = Array.FindAll(allImageFiles, PngPredicate);
                foreach (FileSystemInfo fileSystemInfo in imageFiles) {
                    fileSystemInfo.Delete();
                }

                cmpImageFiles = Array.FindAll(allImageFiles, CmpPngPredicate);
                foreach (FileSystemInfo fileSystemInfo in cmpImageFiles) {
                    fileSystemInfo.Delete();
                }
            } else
                tarGetDir = Directory.CreateDirectory(outPath);

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
                    allImageFiles = tarGetDir.GetFileSystemInfos("*.png");
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
            return cannotOpenTarGetDirectory.Replace("<filename>", outPdf);
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

    /**
     * Sets the maximum errors count which will be returned as the result of the comparison.
     * @param compareByContentMaxErrorCount the errors count.
     * @return Returns this.
     */
    public virtual CompareTool SetCompareByContentErrorsLimit(int compareByContentMaxErrorCount) {
        this.compareByContentErrorsLimit = compareByContentMaxErrorCount;
        return this;
    }

    public virtual void SetGenerateCompareByContentXmlReport(bool generateCompareByContentXmlReport) {
        this.generateCompareByContentXmlReport = generateCompareByContentXmlReport;
    }

    /**
     * Sets the absolute error parameter which will be used in floating point numbers comparison.
     * @param error the epsilon new value.
     * @return Returns this.
     */
    public CompareTool SetFloatAbsoluteError(float error) {
        this.floatComparisonError = error;
        this.absoluteError = true;
        return this;
    }

    /**
     * Sets the relative error parameter which will be used in floating point numbers comparison.
     * @param error the epsilon new value.
     * @return Returns this.
     */
    public CompareTool SetFloatRelativeError(float error) {
        this.floatComparisonError = error;
        this.absoluteError = false;
        return this;
    }

    public void SetXmlReportName(String reportName) {
        this.xmlReportName = reportName;
    }

    public String GetXmlReportName() {
        return this.xmlReportName;
    }

    private String CompareByContent(String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        Console.Write("[itext] INFO  Comparing by content..........");
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

        CompareResult compareResult = new CompareResult(compareByContentErrorsLimit);
        IList<int> equalPages = new List<int>(cmpPages.Count);
        for (int i = 0; i < cmpPages.Count; i++) {
            ObjectPath currentPath = new ObjectPath(cmpPagesRef[i], outPagesRef[i]);
            if (CompareDictionariesExtended(outPages[i], cmpPages[i], currentPath, compareResult))
                equalPages.Add(i);
        }

        PdfObject outStructTree = outReader.Catalog.Get(PdfName.STRUCTTREEROOT);
        PdfObject cmpStructTree = cmpReader.Catalog.Get(PdfName.STRUCTTREEROOT);
        RefKey outStructTreeRef = outStructTree == null ? null : new RefKey((PdfIndirectReference) outStructTree);
        RefKey cmpStructTreeRef = cmpStructTree == null ? null : new RefKey((PdfIndirectReference) cmpStructTree);
        CompareObjects(outStructTree, cmpStructTree, new ObjectPath(outStructTreeRef, cmpStructTreeRef), compareResult);

        PdfObject outOcProperties = outReader.Catalog.Get(PdfName.OCPROPERTIES);
        PdfObject cmpOcProperties = cmpReader.Catalog.Get(PdfName.OCPROPERTIES);
        RefKey outOcPropertiesRef = outOcProperties is PdfIndirectReference ? new RefKey((PdfIndirectReference)outOcProperties) : null;
        RefKey cmpOcPropertiesRef = cmpOcProperties is PdfIndirectReference ? new RefKey((PdfIndirectReference)cmpOcProperties) : null;
        CompareObjects(outOcProperties, cmpOcProperties, new ObjectPath(outOcPropertiesRef, cmpOcPropertiesRef), compareResult);


        outReader.Close();
        cmpReader.Close();

        if (generateCompareByContentXmlReport) {
            try {
                compareResult.WriteReportToXml(new FileStream(outPath + "/" +xmlReportName+ ".xml", FileMode.Create));
            }
            catch (Exception exc) { }
        }


        if (equalPages.Count == cmpPages.Count && compareResult.isOk()) {
            Console.WriteLine("OK");
            Console.Out.Flush();
            return null;
        } else {
            Console.WriteLine("Fail");
            Console.Out.Flush();
            String compareByContentReport = "Compare by content report:\n" + compareResult.GetReport();
            Console.WriteLine(compareByContentReport);
            Console.Out.Flush();
            String message = Compare(outPath, differenceImagePrefix, ignoredAreas, equalPages);
            if (message == null || message.Length == 0)
                return "Compare by content fails. No visual differences";
            return message;
        }
    }

    public virtual String CompareByContent(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix, IDictionary<int, IList<Rectangle>> ignoredAreas) {
        Init(outPdf, cmpPdf);
        return CompareByContent(outPath, differenceImagePrefix, ignoredAreas);
    }

    public virtual String CompareByContent(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix) {
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

    private bool CompareObjects(PdfObject outObj, PdfObject cmpObj, ObjectPath currentPath, CompareResult compareResult) {
        PdfObject outDirectObj = PdfReader.GetPdfObject(outObj);
        PdfObject cmpDirectObj = PdfReader.GetPdfObject(cmpObj);

        if (cmpDirectObj == null && outDirectObj == null) {
            return true;
        }

        if (outDirectObj == null) {
            compareResult.AddError(currentPath, "Expected object was not found.");
            return false;
        } else if (cmpDirectObj == null) {
            compareResult.AddError(currentPath, "Found object which was not expected to be found.");
            return false;
        } else if (cmpDirectObj.Type != outDirectObj.Type) {
            compareResult.AddError(currentPath, String.Format("Types do not match. Expected: {0}. Found: {1}.", cmpDirectObj.GetType().Name, outDirectObj.GetType().Name));
            return false;
        }

        if (cmpObj.IsIndirect() && outObj.IsIndirect()) {
            if (currentPath.IsComparing(new RefKey((PdfIndirectReference)cmpObj), new RefKey((PdfIndirectReference)outObj)))
                return true;
            currentPath = currentPath.ResetDirectPath(new RefKey((PdfIndirectReference)cmpObj), new RefKey((PdfIndirectReference)outObj));
        }

        if (cmpDirectObj.IsDictionary() && ((PdfDictionary)cmpDirectObj).IsPage()) {
            if (!outDirectObj.IsDictionary() || !((PdfDictionary)outDirectObj).IsPage()) {
                if (compareResult != null && currentPath != null)
                    compareResult.AddError(currentPath, "Expected a page. Found not a page.");
                return false;
            }
            RefKey cmpRefKey = new RefKey((PRIndirectReference)cmpObj);
            RefKey outRefKey = new RefKey((PRIndirectReference)outObj);
            // References to the same page
            if (cmpPagesRef.Contains(cmpRefKey) && cmpPagesRef.IndexOf(cmpRefKey) == outPagesRef.IndexOf(outRefKey))
                return true;
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format("The dictionaries refer to different pages. Expected page number: {0}. Found: {1}",
                        cmpPagesRef.IndexOf(cmpRefKey), outPagesRef.IndexOf(outRefKey)));
            return false;
        }

        if (cmpDirectObj.IsDictionary()) {
            if (!CompareDictionariesExtended((PdfDictionary)outDirectObj, (PdfDictionary)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsStream()) {
            if (!CompareStreamsExtended((PRStream)outDirectObj, (PRStream)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsArray()) {
            if (!CompareArraysExtended((PdfArray)outDirectObj, (PdfArray)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsName()) {
            if (!CompareNamesExtended((PdfName)outDirectObj, (PdfName)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsNumber()) {
            if (!CompareNumbersExtended((PdfNumber)outDirectObj, (PdfNumber)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsString()) {
            if (!CompareStringsExtended((PdfString)outDirectObj, (PdfString)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj.IsBoolean()) {
            if (!CompareBooleansExtended((PdfBoolean)outDirectObj, (PdfBoolean)cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (cmpDirectObj is PdfLiteral) {
            if (!CompareLiteralsExtended((PdfLiteral) outDirectObj, (PdfLiteral) cmpDirectObj, currentPath, compareResult))
                return false;
        } else if (outDirectObj.IsNull() && cmpDirectObj.IsNull()) {
        } else {
            throw new InvalidOperationException();
        }
        return true;
    }

    public bool CompareDictionaries(PdfDictionary outDict, PdfDictionary cmpDict) {
        return CompareDictionariesExtended(outDict, cmpDict, null, null);
    }

    private bool CompareDictionariesExtended(PdfDictionary outDict, PdfDictionary cmpDict, ObjectPath currentPath, CompareResult compareResult) {
        if (cmpDict != null && outDict == null || outDict != null && cmpDict == null) {
            compareResult.AddError(currentPath, "One of the dictionaries is null, the other is not.");
            return false;
        }

        bool dictsAreSame = true;
        // Iterate through the union of the keys of the cmp and out dictionaries!
        HashSet2<PdfName> mergedKeys = new HashSet2<PdfName>(cmpDict.Keys);
        mergedKeys.AddAll(outDict.Keys);

        foreach (PdfName key in mergedKeys) {
            if (key.CompareTo(PdfName.PARENT) == 0 || key.CompareTo(PdfName.P) == 0) continue;
            if (outDict.IsStream() && cmpDict.IsStream() && (key.Equals(PdfName.FILTER) || key.Equals(PdfName.LENGTH))) continue;
            if (key.CompareTo(PdfName.BASEFONT) == 0 || key.CompareTo(PdfName.FONTNAME) == 0) {
                PdfObject cmpObj = cmpDict.GetDirectObject(key);
                if (cmpObj.IsName() && cmpObj.ToString().IndexOf('+') > 0) {
                    PdfObject outObj = outDict.GetDirectObject(key);
                    if (!outObj.IsName() || outObj.ToString().IndexOf('+') == -1) {
                        if (compareResult != null && currentPath != null)
                            compareResult.AddError(currentPath, String.Format("PdfDictionary {0} entry: Expected: {1}. Found: {2}", key.ToString(), cmpObj.ToString(), outObj.ToString()));
                        dictsAreSame = false;
                    }
                    String cmpName = cmpObj.ToString().Substring(cmpObj.ToString().IndexOf('+'));
                    String outName = outObj.ToString().Substring(outObj.ToString().IndexOf('+'));
                    if (!cmpName.Equals(outName)) {
                        if (compareResult != null && currentPath != null)
                            compareResult.AddError(currentPath, String.Format("PdfDictionary {0} entry: Expected: {1}. Found: {2}", key.ToString(), cmpObj.ToString(), outObj.ToString()));
                        dictsAreSame = false;
                    }
                    continue;
                }
            }

            if (floatComparisonError != 0 && cmpDict.IsPage() && outDict.IsPage() && key.Equals(PdfName.CONTENTS)) {
                if (!CompareContentStreamsByParsingExtended(outDict.GetDirectObject(key), cmpDict.GetDirectObject(key),
                        (PdfDictionary)outDict.GetDirectObject(PdfName.RESOURCES), (PdfDictionary)cmpDict.GetDirectObject(PdfName.RESOURCES),
                        currentPath, compareResult)) {
                    dictsAreSame = false;
                }
                continue;
            }

            if (currentPath != null)
                currentPath.PushDictItemToPath(key.ToString());
            dictsAreSame = CompareObjects(outDict.Get(key), cmpDict.Get(key), currentPath, compareResult) && dictsAreSame;
            if (currentPath != null)
                currentPath.Pop();
            if (!dictsAreSame && (currentPath == null || compareResult == null || compareResult.IsMessageLimitReached()))
                return false;
        }

        return dictsAreSame;
    }

    public bool ÑompareContentStreamsByParsing(PdfObject outObj, PdfObject cmpObj) {
        return CompareContentStreamsByParsingExtended(outObj, cmpObj, null, null, null, null);
    }

    public bool ÑompareContentStreamsByParsing(PdfObject outObj, PdfObject cmpObj, PdfDictionary outResources, PdfDictionary cmpResources) {
        return CompareContentStreamsByParsingExtended(outObj, cmpObj, outResources, cmpResources, null, null);
    }

    private bool CompareContentStreamsByParsingExtended(PdfObject outObj, PdfObject cmpObj, PdfDictionary outResources, PdfDictionary cmpResources, ObjectPath currentPath, CompareResult compareResult) {
        if (outObj.Type != outObj.Type) {
            compareResult.AddError(currentPath, String.Format(
                    "PdfObject. Types are different. Expected: {0}. Found: {1}", cmpObj.Type, outObj.Type));
            return false;
        }

        if (outObj.IsArray()) {
            PdfArray outArr = (PdfArray) outObj;
            PdfArray cmpArr = (PdfArray) cmpObj;
            if (cmpArr.Size != outArr.Size) {
                compareResult.AddError(currentPath, String.Format("PdfArray. Sizes are different. Expected: {0}. Found: {1}", cmpArr.Size, outArr.Size));
                return false;
            }
            for (int i = 0; i < cmpArr.Size; i++) {
                if (!CompareContentStreamsByParsingExtended(outArr.GetPdfObject(i), cmpArr.GetPdfObject(i), outResources, cmpResources, currentPath, compareResult)) {
                    return false;
                }
            }
        }

        PRTokeniser cmpTokeniser = new PRTokeniser(new RandomAccessFileOrArray(
                new RandomAccessSourceFactory().CreateSource(ContentByteUtils.GetContentBytesFromContentObject(cmpObj))));
        PRTokeniser outTokeniser = new PRTokeniser(new RandomAccessFileOrArray(
                new RandomAccessSourceFactory().CreateSource(ContentByteUtils.GetContentBytesFromContentObject(outObj))));

        PdfContentParser cmpPs = new PdfContentParser(cmpTokeniser);
        PdfContentParser outPs = new PdfContentParser(outTokeniser);

        List<PdfObject> cmpOperands = new List<PdfObject>();
        List<PdfObject> outOperands = new List<PdfObject>();

        while (cmpPs.Parse(cmpOperands).Count > 0) {
            outPs.Parse(outOperands);
            if (cmpOperands.Count != outOperands.Count) {
                compareResult.AddError(currentPath, String.Format(
                        "PdfObject. Different commands lengths. Expected: {0}. Found: {1}", cmpOperands.Count, outOperands.Count));
                return false;
            }
            if (cmpOperands.Count == 1 && CompareLiterals((PdfLiteral) cmpOperands[0], new PdfLiteral("BI")) && CompareLiterals((PdfLiteral) outOperands[0], new PdfLiteral("BI"))) {
                PRStream cmpStr = (PRStream) cmpObj;
                PRStream outStr = (PRStream) outObj;
                if (null != outStr.GetDirectObject(PdfName.RESOURCES) && null != cmpStr.GetDirectObject(PdfName.RESOURCES)) {
                    outResources = (PdfDictionary) outStr.GetDirectObject(PdfName.RESOURCES);
                    cmpResources = (PdfDictionary) cmpStr.GetDirectObject(PdfName.RESOURCES);
                }
                if (!ÑompareInlineImagesExtended(outPs, cmpPs, outResources, cmpResources, currentPath, compareResult)) {
                    return false;
                }
                continue;
            }
            for (int i = 0; i < cmpOperands.Count; i++) {
                if (!CompareObjects(outOperands[i], cmpOperands[i], currentPath, compareResult)) {
                    return false;
                }
            }
        }
        return true;
    }

    private bool ÑompareInlineImagesExtended(PdfContentParser outPs, PdfContentParser cmpPs, PdfDictionary outDict, PdfDictionary cmpDict, ObjectPath currentPath, CompareResult compareResult) {
        InlineImageInfo cmpInfo = InlineImageUtils.ParseInlineImage(cmpPs, cmpDict);
        InlineImageInfo outInfo = InlineImageUtils.ParseInlineImage(outPs, outDict);
        return CompareObjects(outInfo.ImageDictionary, cmpInfo.ImageDictionary, currentPath, compareResult) &&
               Util.ArraysAreEqual(outInfo.Samples, cmpInfo.Samples);
    }

    public bool CompareStreams(PRStream outStream, PRStream cmpStream) {
        return CompareStreamsExtended(outStream, cmpStream, null, null);
    }

    private bool CompareStreamsExtended(PRStream outStream, PRStream cmpStream, ObjectPath currentPath, CompareResult compareResult) {
        bool decodeStreams = PdfName.FLATEDECODE.Equals(outStream.Get(PdfName.FILTER));
        byte[] outStreamBytes = PdfReader.GetStreamBytesRaw(outStream);
        byte[] cmpStreamBytes = PdfReader.GetStreamBytesRaw(cmpStream);
        if (decodeStreams) {
            outStreamBytes = PdfReader.DecodeBytes(outStreamBytes, outStream);
            cmpStreamBytes = PdfReader.DecodeBytes(cmpStreamBytes, cmpStream);
        }
        if (floatComparisonError != 0 &&
            PdfName.XOBJECT.Equals(cmpStream.GetDirectObject(PdfName.TYPE)) &&
            PdfName.XOBJECT.Equals(outStream.GetDirectObject(PdfName.TYPE)) &&
            PdfName.FORM.Equals(cmpStream.GetDirectObject(PdfName.SUBTYPE)) &&
            PdfName.FORM.Equals(outStream.GetDirectObject(PdfName.SUBTYPE))) {
            return
                CompareContentStreamsByParsingExtended(outStream, cmpStream, outStream.GetAsDict(PdfName.RESOURCES),
                    cmpStream.GetAsDict(PdfName.RESOURCES), currentPath, compareResult) &&
                CompareDictionariesExtended(outStream, cmpStream, currentPath, compareResult);
        } else {
            if (Util.ArraysAreEqual(outStreamBytes, cmpStreamBytes)) {
                return CompareDictionariesExtended(outStream, cmpStream, currentPath, compareResult);
            } else {
                if (cmpStreamBytes.Length != outStreamBytes.Length) {
                    if (compareResult != null && currentPath != null) {
                        compareResult.AddError(currentPath,
                            String.Format("PRStream. Lengths are different. Expected: {0}. Found: {1}",
                                cmpStreamBytes.Length, outStreamBytes.Length));
                    }
                } else {
                    for (int i = 0; i < cmpStreamBytes.Length; i++) {
                        if (cmpStreamBytes[i] != outStreamBytes[i]) {
                            int l = Math.Max(0, i - 10);
                            int r = Math.Min(cmpStreamBytes.Length, i + 10);
                            if (compareResult != null && currentPath != null) {
                                currentPath.PushOffsetToPath(i);
                                compareResult.AddError(currentPath,
                                    String.Format(
                                        "PRStream. The bytes differ at index {0}. Expected: {1} ({2}). Found: {3} ({4})",
                                        i, Encoding.UTF8.GetString(new byte[] {cmpStreamBytes[i]}),
                                        Encoding.UTF8.GetString(cmpStreamBytes, l, r - l).Replace("\n", "\\n"),
                                        Encoding.UTF8.GetString(new byte[] { outStreamBytes[i] }),
                                        Encoding.UTF8.GetString(outStreamBytes, l, r - l).Replace("\n", "\\n")));
                                currentPath.Pop();
                            }
                        }
                    }
                }
                return false;
            }
        }
    }

    public bool CompareArrays(PdfArray outArray, PdfArray cmpArray) {
        return CompareArraysExtended(outArray, cmpArray, null, null);
    }

    private bool CompareArraysExtended(PdfArray outArray, PdfArray cmpArray, ObjectPath currentPath, CompareResult compareResult) {
        if (outArray == null) {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, "Found null. Expected PdfArray.");
            return false;
        } else if (outArray.Size != cmpArray.Size) {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format("PdfArrays. Lengths are different. Expected: {0}. Found: {1}.", cmpArray.Size, outArray.Size));
            return false;
        }
        bool arraysAreEqual = true;
        for (int i = 0; i < cmpArray.Size; i++) {
            if (currentPath != null)
                currentPath.PushArrayItemToPath(i);
            arraysAreEqual = CompareObjects(outArray.GetPdfObject(i), cmpArray.GetPdfObject(i), currentPath, compareResult) && arraysAreEqual;
            if (currentPath != null)
                currentPath.Pop();
            if (!arraysAreEqual && (currentPath == null || compareResult == null || compareResult.IsMessageLimitReached()))
                return false;
        }

        return arraysAreEqual;
    }

    public bool CompareNames(PdfName outName, PdfName cmpName) {
        return cmpName.CompareTo(outName) == 0;
    }

    private bool CompareNamesExtended(PdfName outName, PdfName cmpName, ObjectPath currentPath, CompareResult compareResult) {
        if (cmpName.CompareTo(outName) == 0) {
            return true;
        } else {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format("PdfName. Expected: {0}. Found: {1}", cmpName.ToString(), outName.ToString()));
            return false;
        }
    }

    public bool CompareNumbers(PdfNumber outNumber, PdfNumber cmpNumber) {
        double difference = Math.Abs(outNumber.DoubleValue - cmpNumber.DoubleValue);
        if (!absoluteError && cmpNumber.DoubleValue != 0)
        {
            difference /= cmpNumber.DoubleValue;
        }
        return difference <= floatComparisonError;
    }

    private bool CompareNumbersExtended(PdfNumber outNumber, PdfNumber cmpNumber, ObjectPath currentPath, CompareResult compareResult) {
        if (CompareNumbers(outNumber, cmpNumber)) {
            return true;
        } else {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format("PdfNumber. Expected: {0}. Found: {1}", cmpNumber, outNumber));
            return false;
        }
    }

    public bool CompareStrings(PdfString outString, PdfString cmpString) {
        return Util.ArraysAreEqual(cmpString.GetBytes(), outString.GetBytes());
    }

    private bool CompareStringsExtended(PdfString outString, PdfString cmpString, ObjectPath currentPath, CompareResult compareResult) {
        if (Util.ArraysAreEqual(cmpString.GetBytes(), outString.GetBytes())) {
            return true;
        } else {
            String cmpStr = cmpString.ToUnicodeString();
            String outStr = outString.ToUnicodeString();
            if (cmpStr.Length != outStr.Length) {
                if (compareResult != null && currentPath != null)
                    compareResult.AddError(currentPath, String.Format("PdfString. Lengths are different. Expected: {0}. Found: {1}", cmpStr.Length, outStr.Length));
            } else {
                for (int i = 0; i < cmpStr.Length; i++) {
                    if (cmpStr[i] != outStr[i]) {
                        int l = Math.Max(0, i - 10);
                        int r = Math.Min(cmpStr.Length, i + 10);

                        if (compareResult != null && currentPath != null) {
                            currentPath.PushOffsetToPath(i);
                            compareResult.AddError(currentPath, String.Format("PdfString. Characters differ at position {0}. Expected: {1} ({2}). Found: {3} ({4}).",
                                    i, cmpStr[i], cmpStr.Substring(l, r).Replace("\n", "\\n"),
                                    outStr[i], outStr.Substring(l, r).Replace("\n", "\\n")));
                            currentPath.Pop();
                        }

                        break;
                    }
                }
            }
            return false;
        }
    }

    public bool CompareLiterals(PdfLiteral outLiteral, PdfLiteral cmpLiteral) {
        return Util.ArraysAreEqual(cmpLiteral.GetBytes(), outLiteral.GetBytes());
    }

    private bool CompareLiteralsExtended(PdfLiteral outLiteral, PdfLiteral cmpLiteral, ObjectPath currentPath,
        CompareResult compareResult) {
        if (CompareLiterals(outLiteral, cmpLiteral)) {
            return true;
        } else {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format(
                    "PdfLiteral. Expected: {0}. Found: {1}", cmpLiteral, outLiteral));
            return false;
        }
    }

    public bool CompareBooleans(PdfBoolean outBoolean, PdfBoolean cmpBoolean) {
        return Util.ArraysAreEqual(cmpBoolean.GetBytes(), outBoolean.GetBytes());
    }

    private bool CompareBooleansExtended(PdfBoolean outBoolean, PdfBoolean cmpBoolean, ObjectPath currentPath, CompareResult compareResult) {
        if (cmpBoolean.BooleanValue == outBoolean.BooleanValue) {
            return true;
        } else {
            if (compareResult != null && currentPath != null)
                compareResult.AddError(currentPath, String.Format("PdfBoolean. Expected: {0}. Found: {1}.", cmpBoolean.BooleanValue, outBoolean.BooleanValue));
            return false;
        }
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

    public virtual String CompareDocumentInfo(String outPdf, String cmpPdf) {
        Console.Write("[itext] INFO  Comparing document info.......");
        String message = null;
        PdfReader outReader = new PdfReader(outPdf);
        PdfReader cmpReader = new PdfReader(cmpPdf);
        String[] cmpInfo = ConvertInfo(cmpReader.Info);
        String[] outInfo = ConvertInfo(outReader.Info);
        for (int i = 0; i < cmpInfo.Length; ++i) {
            if (!cmpInfo[i].Equals(outInfo[i])) {
                message = "Document info fail";
                break;
            }
        }
        outReader.Close();
        cmpReader.Close();

        if (message == null)
            Console.WriteLine("OK");
        else
            Console.WriteLine("Fail");
        Console.Out.Flush();

        return message;
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
        Console.Write("[itext] INFO  Comparing link annotations....");
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
        if (message == null)
            Console.WriteLine("OK");
        else
            Console.WriteLine("Fail");
        Console.Out.Flush();
        return message;
    }

    private String[] ConvertInfo(IDictionary<String, String> info) {
        String[] convertedInfo = new String[] {"", "", "", ""};
        foreach (String key in info.Keys) {
            if (Util.EqualsIgnoreCase(Meta.TITLE, key)) {
                convertedInfo[0] = info[key];
            } else if (Util.EqualsIgnoreCase(Meta.AUTHOR, key)) {
                convertedInfo[1] = info[key];
            } else if (Util.EqualsIgnoreCase(Meta.SUBJECT, key)) {
                convertedInfo[2] = info[key];
            } else if (Util.EqualsIgnoreCase(Meta.KEYWORDS, key)) {
                convertedInfo[3] = info[key];
            }
        }
        return convertedInfo;
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
