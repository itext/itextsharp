/**
 * @see PdfName constants for AFRelationship key of Filespec dictionary.
 * An AFRelationship value represents the relationship of this object to the source that points to it. Predefined values are Source, Data, Alternative, Supplement or Unspecified. Other values may be used to represent other types of relationships, but should follow the rules for second-class names (ISO 32000‑1, Annex E).
 */
namespace iTextSharp.text.pdf {
    public sealed class AFRelationshipValue {
        /**
         * Source shall be used if this file specification is the original source material for the associated content.
         */
        public static readonly PdfName Source = new PdfName("Source");
        /**
         * Data shall be used if this file specification represents information used to derive a visual presentation, such as for a table or a graph.
         */
        public static readonly PdfName Data = new PdfName("Data");
        /**
         * Alternative shall be used if this file specification is an alternative representation of content, for example audio.
         */
        public static readonly PdfName Alternative = new PdfName("Alternative");
        /**
         * Supplement shall be used if this file specification represents a supplemental representation of the original source or data that may be more easily consumable (e.g. A MathML version of an equation).
         * NOTE 1 Supplement is to be used only when the file is not the source or an alternative.
         */
        public static readonly PdfName Supplement = new PdfName("Supplement");
        /**
         * Unspecified shall be used when the relationship is not known or cannot be described using one of the other values.
         * NOTE 2 Unspecified is to be used only when no other value correctly reflects the relationship.
         */
        public static readonly PdfName Unspecified = new PdfName("Unspecified");
    }
}
