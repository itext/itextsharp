using System;
using System.Collections.Generic;
using System.Text;

namespace iTextSharp.text.pdf
{
	public class PdfBody : Rectangle, IElement
	{
        public PdfBody(Rectangle rectangle) : base(rectangle)
        {
        }

        public override bool Process(IElementListener listener)
        {
            return false;
        }

        public override int Type
        {
            get { return Element.BODY; }
        }

        public override bool IsContent()
        {
            return false;
        }

        public override bool IsNestable()
        {
            return false;
        }

        public override IList<Chunk> Chunks
        {
            get {return null;}
        }
	}
}
