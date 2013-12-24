namespace iTextSharp.text.pdf.parser {
    internal class GlyphRenderListener : IRenderListener {
        private IRenderListener deleg;

        public GlyphRenderListener(IRenderListener deleg) {
            this.deleg = deleg;
        }

        virtual public void BeginTextBlock() {
            deleg.BeginTextBlock();
        }

        virtual public void RenderText(TextRenderInfo renderInfo) {
            foreach (TextRenderInfo glyphInfo in renderInfo.GetCharacterRenderInfos())
                deleg.RenderText(glyphInfo);
        }

        virtual public void EndTextBlock() {
            deleg.EndTextBlock();
        }

        virtual public void RenderImage(ImageRenderInfo renderInfo) {
            deleg.RenderImage(renderInfo);
        }
    }
}
