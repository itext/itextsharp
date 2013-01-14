/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class MemoryTests : IWriter {

    /** The resulting report. */
    public readonly String RESULT0 = "test_results.txt";

    /** A resulting PDF file. */
    public readonly String RESULT1
        = "pdfptable_without_memory_management.pdf";
    /** A resulting PDF file. */
    public readonly String RESULT2
        = "pdfptable_with_memory_management.pdf";
    
    private bool test;
    private long memory_use;
    private long initial_memory_use = 0l;
    private long maximum_memory_use = 0l;
    
    /** Path to the resources. */
    public readonly String RESOURCE = Utility.ResourceDirectory+"/posters/{0}.jpg";
    /** The different epochs. */
    public readonly String[] EPOCH = new [] { "Forties", "Fifties", "Sixties", "Seventies", "Eighties", "Nineties", "Twenty-first Century" };
    
    /** The fonts for the title. */
    public readonly Font[] FONT  = new []
    {
        new Font(Font.FontFamily.HELVETICA, 24),
        new Font(Font.FontFamily.HELVETICA, 18),
        new Font(Font.FontFamily.HELVETICA, 14),
        new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)
    };
    
    
    /**
     * Creates a PDF with a table
     * @param writer the writer to our report file
     * @param filename the PDF that will be created
     * @throws IOException
     * @throws DocumentException
     * @throws SQLException
     */
    private void CreatePdfWithPdfPTable(StreamWriter writer, Stream doc) {
        // Create a connection to the database
    	//DatabaseConnection connection = new HsqldbConnection("filmfestival"); 
    	// step 1
        Document document = new Document();
        // step 2
        PdfWriter.GetInstance(document, doc);
        // step 3
        document.Open();
        // step 4
        // Create a table with 2 columns
        PdfPTable table = new PdfPTable(new float[]{1, 7});
        // Mark the table as not complete
        if (test) table.Complete = false;
        table.WidthPercentage = 100;
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        List list;
        PdfPCell cell;
        int count = 0;
        // add information about a movie
        foreach (Movie movie in movies) {
            table.SpacingBefore = 5;
            // add a movie poster
            cell = new PdfPCell(Image.GetInstance(String.Format(RESOURCE, movie.Imdb)), true);
            cell.Border = PdfPCell.NO_BORDER;
            table.AddCell(cell);
            // add movie information
            cell = new PdfPCell();
            Paragraph p = new Paragraph(movie.Title, FilmFonts.BOLD);
            p.Alignment = Element.ALIGN_CENTER;
            p.SpacingBefore = 5;
            p.SpacingAfter = 5;
            cell.AddElement(p);
            cell.Border = PdfPCell.NO_BORDER;
            if (movie.OriginalTitle != null) {
                p = new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC);
                p.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(p);
            }
            list = PojoToElementFactory.GetDirectorList(movie);
            list.IndentationLeft = 30;
            cell.AddElement(list);
            p = new Paragraph(String.Format("Year: %d", movie.Year), FilmFonts.NORMAL);
            p.IndentationLeft = 15;
            p.Leading = 24;
            cell.AddElement(p);
            p = new Paragraph(String.Format("Run length: %d", movie.Duration), FilmFonts.NORMAL);
            p.Leading = 14;
            p.IndentationLeft = 30;
            cell.AddElement(p);
            list = PojoToElementFactory.GetCountryList(movie);
            list.IndentationLeft = 40;
            cell.AddElement(list);
            table.AddCell(cell);
            // insert a checkpoint every 10 movies
            if (count++ % 10 == 0) {
            	// add the incomplete table to the document
                if (test)
                    document.Add(table);
                Checkpoint(writer);
            }
        }
        // Mark the table as complete
        if (test) table.Complete = true;
        // add the table to the document
        document.Add(table);
        // insert a last checkpoint
        Checkpoint(writer);
        // step 5
        document.Close();
    }
    
    /**
     * Writes a checkpoint to the report file.
     * @param writer the writer to our report file
     */
    private void Checkpoint(StreamWriter writer) {
        memory_use = GetMemoryUse();
        maximum_memory_use = Math.Max(maximum_memory_use, memory_use);
        Println(writer, "memory use: ", memory_use);
    }
    
    /**
     * Resets the maximum memory that is in use
     * @param writer the writer to our report file
     */
    private void ResetMaximum(StreamWriter writer) {
        Println(writer, "maximum: ", maximum_memory_use);
        Println(writer, "total used: ", maximum_memory_use - initial_memory_use);
        maximum_memory_use = 0l;
        GarbageCollect();
        initial_memory_use = GetMemoryUse();
        Println(writer, "initial memory use: ", initial_memory_use);
    }

    /**
     * Writes a line to our report file
     * @param writer the writer to our report file
     * @param message the message to write
     */
    private void Println(StreamWriter writer, String message) {
        try {
            writer.WriteLine(message);
            writer.WriteLine("\n");
            writer.Flush();
        } catch (IOException e) {
            Console.WriteLine(e.StackTrace);
        }
    }
    
    /**
     * Writes a line to our report file
     * @param writer the writer to our report file
     * @param message the message to write
     * @param l a memory value
     */
    private void Println(StreamWriter writer, String message, long l) {
        try {
            writer.WriteLine(message + l);
            writer.WriteLine("\n");
            writer.Flush();
        } catch (IOException e) {
            Console.WriteLine(e.StackTrace);
        }
    }
    
    /**
     * Returns the current memory use.
     * 
     * @return the current memory use
     */
    private static long GetMemoryUse() {
        return (GC.GetTotalMemory(true));
    }
    
    /**
     * Makes sure all garbage is cleared from the memory.
     */
    private static void GarbageCollect() {
        try {
            GC.Collect();
            Thread.Sleep(200);
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
            GC.Collect();
            Thread.Sleep(200);
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
        } catch (Exception ex) {
            Console.WriteLine(ex.StackTrace);
        }
    }

    /**
     * Main method.
     * @param args no arguments needed
     * @throws DocumentException 
     * @throws IOException
     * @throws SQLException
     */
    public static void Main(String[] args) {
        
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
                        ResetMaximum(sw);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            // step 1
                            test = false;
                            Println(sw, RESULT1);
                            // PDF without memory management
                            CreatePdfWithPdfPTable(sw, ms);
                            ResetMaximum(sw);
                            zip.AddEntry(RESULT1, ms.ToArray());
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            // step 2
                            test = true;
                            Println(sw, RESULT2);
                            // PDF with memory management
                            CreatePdfWithPdfPTable(sw, ms);
                            ResetMaximum(sw);
                            zip.AddEntry(RESULT2, ms.ToArray());
                        }
                        // step 3
                        zip.AddEntry(RESULT0, resultStream.ToArray());
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