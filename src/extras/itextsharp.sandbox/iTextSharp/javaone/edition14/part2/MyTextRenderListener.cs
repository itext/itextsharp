using System;
using System.IO;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part2
{
    /// <summary>
    /// A very simple text render listener that writes snippets of text to a PrintWriter.
    /// </summary>
    public class MyTextRenderListener : IRenderListener
    {
        /// <summary>The print writer to which the information will be written.</summary>
        protected TextWriter output;

        /// <summary>
        /// Creates a RenderListener that will look for text.
        /// <param name="output"></param>
        ///  </summary>
        public MyTextRenderListener(TextWriter output)
        {
            this.output = output;
        }

        /// <summary>
        /// <see cref="IRenderListener.BeginTextBlock"/>
        /// </summary>
        public void BeginTextBlock()
        {
            output.WriteLine("<");
        }

        /// <summary>
        /// <see cref="IRenderListener.EndTextBlock"/>
        /// </summary>
        public void EndTextBlock()
        {
            output.WriteLine(">");
        }

        /// <summary>
        /// <see cref="IRenderListener.RenderImage"/>
        /// </summary>
        public void RenderImage(ImageRenderInfo renderInfo)
        {
        }

        /// <summary>
        /// <see cref="IRenderListener.RenderText"/>
        /// </summary>
        public void RenderText(TextRenderInfo renderInfo)
        {
            output.WriteLine("    <");
            Vector start = renderInfo.GetBaseline().GetStartPoint();
            output.WriteLine(String.Format("        x: {0} y: {1} length: {2} \n        Text: {3}",
                start[Vector.I1], start[Vector.I2],
                renderInfo.GetBaseline().GetLength(),
                renderInfo.GetText()));
            output.WriteLine("    >");
        }
    }
}
