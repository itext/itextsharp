/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class KubrickCollection : IWriter {
// ===========================================================================
    /** An image file used in this example */
    public readonly String IMG_BOX =  Path.Combine(
      Utility.ResourceImage, "kubrick_box.jpg"
    );
    /** An image file used in this example */
    public readonly String IMG_KUBRICK =  Path.Combine(
      Utility.ResourceImage, "kubrick.jpg"
    );       
    /** The name of a field in the collection schema. */
    public const String TYPE_FIELD = "TYPE";
    /** A caption of a field in the collection schema. */
    public const String TYPE_CAPTION = "File type";
    /** The name of a field in the collection schema. */
    public const String FILE_FIELD = "FILE";
    /** A caption of a field in the collection schema. */
    public const String FILE_CAPTION = "File name";
    /** Sort order for the keys in the collection. */
    public String[] KEYS = { TYPE_FIELD, FILE_FIELD };
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        
        
        // step 4
        PdfCollection collection = new PdfCollection(PdfCollection.HIDDEN);
        PdfCollectionSchema schema = _collectionSchema();
        collection.Schema = schema;
        PdfCollectionSort sort = new PdfCollectionSort(KEYS);
        collection.Sort = sort;
        writer.Collection = collection;

        PdfCollectionItem collectionitem = new PdfCollectionItem(schema);
        PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
          writer, IMG_KUBRICK, "kubrick.jpg", null
        );
        fs.AddDescription("Stanley Kubrick", false);
        collectionitem.AddItem(TYPE_FIELD, "JPEG");
        fs.AddCollectionItem(collectionitem);
        writer.AddFileAttachment(fs);
        
        Image img = Image.GetInstance(IMG_BOX);
        document.Add(img);
        List list = new List(List.UNORDERED, 20);
        PdfDestination dest = new PdfDestination(PdfDestination.FIT);
        dest.AddFirst(new PdfNumber(1));
        PdfTargetDictionary intermediate;
        PdfTargetDictionary target;
        Chunk chunk;
        ListItem item;
        PdfAction action = null;

        IEnumerable<Movie> box = PojoFactory.GetMovies(1)
          .Concat(PojoFactory.GetMovies(4))
        ;
        StringBuilder sb = new StringBuilder();
        foreach (Movie movie in box) {
          if (movie.Year > 1960) {
            sb.AppendLine(String.Format(
              "{0};{1};{2}", movie.MovieTitle, movie.Year, movie.Duration
            ));
            item = new ListItem(movie.MovieTitle);
            if (!"0278736".Equals(movie.Imdb)) {
              target = new PdfTargetDictionary(true);
              target.EmbeddedFileName = movie.Title;
              intermediate = new PdfTargetDictionary(true);
              intermediate.FileAttachmentPage = 1;
              intermediate.FileAttachmentIndex = 1;
              intermediate.AdditionalPath = target;
              action = PdfAction.GotoEmbedded(null, intermediate, dest, true);
              chunk = new Chunk(" (see info)");
              chunk.SetAction(action);
              item.Add(chunk);
            }
            list.Add(item);
          }
        }
        document.Add(list);
        
        fs = PdfFileSpecification.FileEmbedded(
          writer, null, "kubrick.txt", 
          Encoding.UTF8.GetBytes(sb.ToString())
        );
        fs.AddDescription("Kubrick box: the movies", false);
        collectionitem.AddItem(TYPE_FIELD, "TXT");
        fs.AddCollectionItem(collectionitem);
        writer.AddFileAttachment(fs);

        PdfPTable table = new PdfPTable(1);
        table.SpacingAfter = 10;
        PdfPCell cell = new PdfPCell(new Phrase("All movies by Kubrick"));
        cell.Border = PdfPCell.NO_BORDER;
        fs = PdfFileSpecification.FileEmbedded(
          writer, null, KubrickMovies.FILENAME, 
          Utility.PdfBytes(new KubrickMovies())
          //new KubrickMovies().createPdf()
        );
        collectionitem.AddItem(TYPE_FIELD, "PDF");
        fs.AddCollectionItem(collectionitem);
        target = new PdfTargetDictionary(true);
        target.FileAttachmentPagename = "movies";
        target.FileAttachmentName = "The movies of Stanley Kubrick";
        cell.CellEvent = new PdfActionEvent(
          writer, PdfAction.GotoEmbedded(null, target, dest, true)
        );
        cell.CellEvent = new FileAttachmentEvent(
          writer, fs, "The movies of Stanley Kubrick"
        );
        cell.CellEvent = new LocalDestinationEvent(writer, "movies");
        table.AddCell(cell);
        writer.AddFileAttachment(fs);

        cell = new PdfPCell(new Phrase("Kubrick DVDs"));
        cell.Border = PdfPCell.NO_BORDER;
        fs = PdfFileSpecification.FileEmbedded(
          writer, null, 
          KubrickDvds.RESULT, new KubrickDvds().CreatePdf()
        );
        collectionitem.AddItem(TYPE_FIELD, "PDF");
        fs.AddCollectionItem(collectionitem);
        cell.CellEvent = new FileAttachmentEvent(writer, fs, "Kubrick DVDs");
        table.AddCell(cell);
        writer.AddFileAttachment(fs);
        
        cell = new PdfPCell(new Phrase("Kubrick documentary"));
        cell.Border = PdfPCell.NO_BORDER;
        fs = PdfFileSpecification.FileEmbedded(
          writer, null, 
          KubrickDocumentary.RESULT, new KubrickDocumentary().CreatePdf()
        );
        collectionitem.AddItem(TYPE_FIELD, "PDF");
        fs.AddCollectionItem(collectionitem);
        cell.CellEvent = new FileAttachmentEvent(
          writer, fs, "Kubrick Documentary"
        );
        table.AddCell(cell);
        writer.AddFileAttachment(fs);

        document.NewPage();
        document.Add(table);
      }
    }
// ---------------------------------------------------------------------------     
    /**
     * Creates a Collection schema that can be used to organize the movies
     * of Stanley Kubrick in a collection: year, title, duration, 
     * DVD acquisition, filesize (filename is present, but hidden).
     * @return a collection schema
     */
    private PdfCollectionSchema _collectionSchema() {
      PdfCollectionSchema schema = new PdfCollectionSchema();
      
      PdfCollectionField type = new PdfCollectionField(
        TYPE_CAPTION, PdfCollectionField.TEXT
      );
      type.Order = 0;
      schema.AddField(TYPE_FIELD, type);
      
      PdfCollectionField filename = new PdfCollectionField(
        FILE_FIELD, PdfCollectionField.FILENAME
      );
      filename.Order = 1;
      schema.AddField(FILE_FIELD, filename);
      
      return schema;
    }
// ===========================================================================
  }
}