using System.Collections.Generic;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Implementation of the RenderListener interface that generates a list
    /// of MyItem objects while parsing a PDF page.
    /// </summary>
    public class MyRenderListenerSimple : IRenderListener
    {
        /// <summary>
        /// The resulting list of items after parsing.
        /// </summary>
        List<MyItem> items = new List<MyItem>();

        /// <summary>
        /// Getter for the items that were encountered.
        /// </summary>
        public List<MyItem> Items
        {
            get { return items; }
        }

        /// <see cref="IRenderListener.BeginTextBlock"/>
        public void BeginTextBlock()
        {
        }

        /// <see cref="IRenderListener.RenderText"/>
        public void RenderText(TextRenderInfo renderInfo)
        {
            items.Add(new TextItemSimple(renderInfo));
        }

        /// <see cref="IRenderListener.EndTextBlock"/>
        public void EndTextBlock()
        {
        }

        /// <see cref="IRenderListener.RenderImage"/>
        public void RenderImage(ImageRenderInfo renderInfo)
        {
            items.Add(new ImageItem(renderInfo));
        }
    }
}
