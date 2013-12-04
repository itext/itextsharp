using System;

namespace iTextSharp.text.pdf
{
    public class PdfPHeaderCell : PdfPCell
    {
        // static member variables for the different styles

        /** this is a possible style. */
        public const int NONE = 0;

        /** this is a possible style. */
        public const int ROW = 1;

        /** this is a possible style. */
        public const int COLUMN = 2;

        /** this is a possible style. */
        public const int BOTH = 3;


        protected int scope = NONE;
        protected String name = null;

        public PdfPHeaderCell() : base()
        {
            role = PdfName.TH;
        }

        public PdfPHeaderCell(PdfPHeaderCell headerCell) : base(headerCell)
        {
            role = headerCell.role;
            scope = headerCell.scope;
            name = headerCell.name;
        }

        public int Scope
        {
            get { return scope; }
            set { scope = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override PdfName Role
        {
            get { return role; }
            set { role = value; }
        }
    }
}