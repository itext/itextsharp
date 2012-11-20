/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ObjectData : IWriter {
// ===========================================================================
    /** SQL statement to get selected directors */
    public const string SELECTDIRECTORS =
      @"SELECT DISTINCT d.id, d.name, d.given_name, count(*) AS c 
      FROM film_director d, film_movie_director md
      WHERE d.id = md.director_id AND d.id < 8
      GROUP BY d.id, d.name, d.given_name ORDER BY id";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.SetTagged();
        writer.UserProperties = true;        
        // step 3
        document.Open();
        // step 4
        PdfStructureTreeRoot tree = writer.StructureTreeRoot;
        PdfStructureElement top = new PdfStructureElement(
          tree, new PdfName("Directors")
        );
        
        Dictionary<int, PdfStructureElement> directors = 
          new Dictionary<int, PdfStructureElement>();

        int id;
        Director director;
        PdfStructureElement e;
        DbProviderFactory dbp = AdoDB.Provider;
        using (var c = dbp.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SELECTDIRECTORS;
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                id = Convert.ToInt32(r["id"]);
                director = PojoFactory.GetDirector(r);
                e = new PdfStructureElement(top, new PdfName("director" + id));
                PdfDictionary userproperties = new PdfDictionary();
                userproperties.Put(PdfName.O, PdfName.USERPROPERTIES);
                PdfArray properties = new PdfArray();
                PdfDictionary property1 = new PdfDictionary();
                property1.Put(PdfName.N, new PdfString("Name"));
                property1.Put(PdfName.V, new PdfString(director.Name));            
                properties.Add(property1);
                PdfDictionary property2 = new PdfDictionary();
                property2.Put(PdfName.N, new PdfString("Given name"));
                property2.Put(PdfName.V, new PdfString(director.GivenName));            
                properties.Add(property2);
                PdfDictionary property3 = new PdfDictionary();
                property3.Put(PdfName.N, new PdfString("Posters"));
                property3.Put(PdfName.V, new PdfNumber(Convert.ToInt32(r["c"]))); 
                properties.Add(property3);
                userproperties.Put(PdfName.P, properties);
                e.Put(PdfName.A, userproperties);
                directors.Add(id, e);
              }
            }
          }
        }

        Dictionary<Movie, int> map = new Dictionary<Movie, int>();
        for (int i = 1; i < 8; i++) {
          foreach (Movie movie in PojoFactory.GetMovies(i)) {
            map.Add(movie, i);
          }
        }
        
        PdfContentByte canvas = writer.DirectContent;
        Image img;
        float x = 11.5f;
        float y = 769.7f;
        string RESOURCE = Utility.ResourcePosters;
        foreach (var entry in map.Keys) {
          img = Image.GetInstance(Path.Combine(RESOURCE, entry.Imdb + ".jpg"));
          img.ScaleToFit(1000, 60);
          img.SetAbsolutePosition(x + (45 - img.ScaledWidth) / 2, y);
          canvas.BeginMarkedContentSequence(directors[map[entry]]);
          canvas.AddImage(img);
          canvas.EndMarkedContentSequence();
          x += 48;
          if (x > 578) {
            x = 11.5f;
            y -= 84.2f;
          }
        }
      }
    }
// ===========================================================================
  }
}