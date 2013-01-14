/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class MemoryInfo : IWriter {

    /** The resulting PDF file. */
    public static readonly String RESULT
        = "memory_info.txt";
    public static readonly String RESULT0
        = "MovieTemplates.pdf";
    
    /**
     * Do a full read of a PDF file
     * @param writer a writer to a report file
     * @param filename the file to read
     * @throws IOException
     */
    public static void fullRead(StreamWriter writer, byte[] file){
        long before = GC.GetTotalMemory(true);
        PdfReader reader = new PdfReader(file);
        int num = reader.NumberOfPages;
        writer.WriteLine(String.Format("Number of pages: {0}", num));
        writer.WriteLine(String.Format("Memory used by full read: {0}",
                    GC.GetTotalMemory(true) - before));
        writer.Flush();
        reader.Close();
    }
    
    /**
     * Do a partial read of a PDF file
     * @param writer a writer to a report file
     * @param filename the file to read
     * @throws IOException
     */
    public static void partialRead(StreamWriter writer, byte[] file) {
        long before = GC.GetTotalMemory(true);
        PdfReader reader = new PdfReader(
                new RandomAccessFileOrArray(file), null);
        int num = reader.NumberOfPages;
        writer.WriteLine(String.Format("Number of pages: {0}", num));
        writer.WriteLine(String.Format("Memory used by partial read: {0}",
                    GC.GetTotalMemory(true) - before));
        writer.Flush();
        reader.Close();
    }
      

    /**
     * Makes sure all garbage is cleared from the memory.
     */
    private static void GarbageCollect()
    {
        try
        {
            GC.Collect();
            Thread.Sleep(200);
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
            GC.Collect();
            Thread.Sleep(200);
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
    }
// ===========================================================================
    public void Write(Stream stream) {
        try
        {
            using (ZipFile zip = new ZipFile())
            {
                using (MemoryStream resultStream = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(resultStream))
                    {
                        using (MemoryStream movies = new MemoryStream())
                        {
                            // step 1
                            MovieTemplates mt = new MovieTemplates();
                            mt.Write(movies);
                            zip.AddEntry(RESULT0, movies.ToArray());

                            // step 2
                            GarbageCollect();
                            // Do a full read
                            fullRead(sw, movies.ToArray());
                            // Do a partial read
                            partialRead(sw, movies.ToArray());
                            zip.AddEntry(RESULT, resultStream.ToArray());
                        }
                    }
                }
                zip.Save(stream);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
    }    
// ===========================================================================
  }
}