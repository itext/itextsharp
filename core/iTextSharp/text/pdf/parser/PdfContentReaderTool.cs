using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.util;
using iTextSharp.text.pdf;
/*
 * $Id: PdfContentReaderTool.cs 518 2013-02-20 13:16:32Z pavel-alay $
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
namespace iTextSharp.text.pdf.parser {

    /**
     * Tool that parses the content of a PDF document.
     * @since   2.1.4
     */
    public class PdfContentReaderTool {

        /**
         * Shows the detail of a dictionary.
         * This is similar to the PdfLister functionality.
         * @param dic   the dictionary of which you want the detail
         * @return  a String representation of the dictionary
         */
        public static String GetDictionaryDetail(PdfDictionary dic){
            return GetDictionaryDetail(dic, 0);
        }

        /**
         * Shows the detail of a dictionary.
         * @param dic   the dictionary of which you want the detail
         * @param depth the depth of the current dictionary (for nested dictionaries)
         * @return  a String representation of the dictionary
         */
        public static  String GetDictionaryDetail(PdfDictionary dic, int depth){
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            IList<PdfName> subDictionaries = new List<PdfName>();
            foreach (PdfName key in dic.Keys) {
                PdfObject val = dic.GetDirectObject(key);
                if (val.IsDictionary())
                    subDictionaries.Add(key);
                builder.Append(key);
                builder.Append('=');
                builder.Append(val);
                builder.Append(", ");
            }
            builder.Length = builder.Length-2;
            builder.Append(')');
            foreach (PdfName pdfSubDictionaryName in subDictionaries) {
                builder.Append('\n');
                for (int i = 0; i < depth+1; i++){
                    builder.Append('\t');
                }
                builder.Append("Subdictionary ");
                builder.Append(pdfSubDictionaryName);
                builder.Append(" = ");
                builder.Append(GetDictionaryDetail(dic.GetAsDict(pdfSubDictionaryName), depth+1));
            }
            return builder.ToString();
        }

        /**
         * Displays a summary of the entries in the XObject dictionary for the stream
         * @param resourceDic the resource dictionary for the stream
         * @return a string with the summary of the entries
         * @throws IOException
         * @since 5.0.2
         */
        public static String GetXObjectDetail(PdfDictionary resourceDic) {
            StringBuilder sb = new StringBuilder();
            
            PdfDictionary xobjects = resourceDic.GetAsDict(PdfName.XOBJECT);
            if (xobjects == null)
                return "No XObjects";
            foreach (PdfName entryName in xobjects.Keys) {
                PdfStream xobjectStream = xobjects.GetAsStream(entryName);
                
                sb.Append("------ " + entryName + " - subtype = " + xobjectStream.Get(PdfName.SUBTYPE) + " = " + xobjectStream.GetAsNumber(PdfName.LENGTH) + " bytes ------\n");
                
                if (!xobjectStream.Get(PdfName.SUBTYPE).Equals(PdfName.IMAGE)){
                
                    byte[] contentBytes = ContentByteUtils.GetContentBytesFromContentObject(xobjectStream);

                    foreach (byte b in contentBytes) {
                        sb.Append((char)b);
                    }
        
                    sb.Append("------ " + entryName + " - subtype = " + xobjectStream.Get(PdfName.SUBTYPE) + "End of Content" + "------\n");
                }
            }
           
            return sb.ToString();
        }
        
        /**
         * Writes information about a specific page from PdfReader to the specified output stream.
         * @since 2.1.5
         * @param reader    the PdfReader to read the page content from
         * @param pageNum   the page number to read
         * @param out       the output stream to send the content to
         * @throws IOException
         */
        public static void ListContentStreamForPage(PdfReader reader, int pageNum, TextWriter outp) {
            outp.WriteLine("==============Page " + pageNum + "====================");
            outp.WriteLine("- - - - - Dictionary - - - - - -");
            PdfDictionary pageDictionary = reader.GetPageN(pageNum);
            outp.WriteLine(GetDictionaryDetail(pageDictionary));

            outp.WriteLine("- - - - - XObject Summary - - - - - -");
            outp.WriteLine(GetXObjectDetail(pageDictionary.GetAsDict(PdfName.RESOURCES)));
            
            outp.WriteLine("- - - - - Content Stream - - - - - -");
            RandomAccessFileOrArray f = reader.SafeFile;

            byte[] contentBytes = reader.GetPageContent(pageNum, f);
            f.Close();

            outp.Flush();

            foreach (byte b in contentBytes) {
                outp.Write((char)b);
            }

            outp.Flush();
            
            outp.WriteLine("- - - - - Text Extraction - - - - - -");
            String extractedText = PdfTextExtractor.GetTextFromPage(reader, pageNum, new LocationTextExtractionStrategy());
            if (extractedText.Length != 0)
                outp.WriteLine(extractedText);
            else
                outp.WriteLine("No text found on page " + pageNum);

            outp.WriteLine();

        }

        /**
         * Writes information about each page in a PDF file to the specified output stream.
         * @since 2.1.5
         * @param pdfFile   a File instance referring to a PDF file
         * @param out       the output stream to send the content to
         * @throws IOException
         */
        public static void ListContentStream(string pdfFile, TextWriter outp) {
            PdfReader reader = new PdfReader(pdfFile);

            int maxPageNum = reader.NumberOfPages;

            for (int pageNum = 1; pageNum <= maxPageNum; pageNum++){
                ListContentStreamForPage(reader, pageNum, outp);
            }

        }

        /**
         * Writes information about the specified page in a PDF file to the specified output stream.
         * @since 2.1.5
         * @param pdfFile   a File instance referring to a PDF file
         * @param pageNum   the page number to read
         * @param out       the output stream to send the content to
         * @throws IOException
         */
        public static void ListContentStream(string pdfFile, int pageNum, TextWriter outp) {
            PdfReader reader = new PdfReader(pdfFile);

            ListContentStreamForPage(reader, pageNum, outp);
        }

        /**
         * Writes information about each page in a PDF file to the specified file, or System.out.
         * @param args
         */
        public static void Main(String[] args) {
            try{
                if (args.Length < 1 || args.Length > 3){
                    Console.Out.WriteLine("Usage:  PdfContentReaderTool <pdf file> [<output file>|stdout] [<page num>]");
                    return;
                }

                TextWriter writer = Console.Out;
                if (args.Length >= 2){
                    if (!Util.EqualsIgnoreCase(args[1], "stdout")) {
                        Console.Out.WriteLine("Writing PDF content to " + args[1]);
                        writer = new StreamWriter(args[1]);
                    }
                }

                int pageNum = -1;
                if (args.Length >= 3){
                    pageNum = int.Parse(args[2]);
                }

                if (pageNum == -1){
                    ListContentStream(args[0], writer);
                } else {
                    ListContentStream(args[0], pageNum, writer);
                }
                writer.Flush();

                if (args.Length >= 2){
                    writer.Close();
                    Console.Out.WriteLine("Finished writing content to " + args[1]);
                }
            } catch (Exception e){
                Console.Out.WriteLine(e.ToString());
            }
        }
    }
}