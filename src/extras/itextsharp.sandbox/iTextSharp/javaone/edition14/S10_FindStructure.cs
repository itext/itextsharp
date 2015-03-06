using System.Collections.Generic;
using System.IO;
using iTextSharp.javaone.edition14.part4.helper;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    ///  In this second example that parses a PDF to discover its structure,
    /// we'll highlight all the structures that are encountered.
    /// </summary>
    public class S10_FindStructure : S09_FindLines
    {
        /// <summary>
        /// The resulting PDF after parsing for structure.
        /// </summary>
        public static readonly string DEST = "results/javaone/edition2014/10_structure.pdf";

        /// <summary>
        /// Reads the first page of a document of which the top margin is 48pt heigh
        /// and highlights structures.
        /// </summary>
        /// <param name="args">No arguments needed</param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            S10_FindStructure app = new S10_FindStructure();
            PdfReader reader = new PdfReader(SRC);
            List<MyItem> items = app.GetContentItems(reader, 1, 48);
            items.Sort();
            List<MyItem> lines = app.GetLines(items);
            List<MyItem> structures = app.GetStructures(lines);
            app.Highlight(structures, reader, 1, DEST);
        }

        /// <summary>
        /// Combines lines into structures
        /// </summary>
        /// <param name="lines">lines a list of lines</param>
        /// <returns>list of structures</returns>
        public List<MyItem> GetStructures(List<MyItem> lines)
        {
            List<MyItem> structures = new List<MyItem>();
            List<MyItem> structure = new List<MyItem>();
            foreach (Line line in lines)
            {
                if (structure.Count == 0)
                {
                    structure.Add(line);
                    continue;
                }
                if (AreInSameStructure((Line) structure[structure.Count - 1], line))
                {
                    structure.Add(line);
                }
                else
                {
                    structures.Add(new Structure(structure));
                    structure = new List<MyItem>();
                    structure.Add(line);
                }
            }
            if (structure.Count != 0)
                structures.Add(new Structure(structure));
            return structures;
        }

        static bool AreInSameStructure(Line i1, Line i2)
        {
            if (!i1.Color.Equals(i2.Color))
                return false;
            else if (i2.Rectangle.Left - i1.Rectangle.Left >= MyItem.ItemPositionTolerance)
                return false;
            return true;
        }
    }
}
