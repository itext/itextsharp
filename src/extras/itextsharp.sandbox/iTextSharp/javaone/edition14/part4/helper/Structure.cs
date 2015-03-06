using System.Collections.Generic;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// MyItem implementation that gets its coordinates and color from a list
    /// of lines that belong to the same structure.
    /// </summary>
    public class Structure : Line
    {
        /// <summary>
        /// Creates a Structure object based on a list of lines that belong
        /// together in the same structure.
        /// </summary>
        /// <param name="items">a list of MyItem objects</param>
        public Structure(List<MyItem> items) : base(items) { }
    }
}
