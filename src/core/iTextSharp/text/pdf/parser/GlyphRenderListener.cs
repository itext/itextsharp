namespace iTextSharp.text.pdf.parser {
    internal class GlyphRenderListener : IRenderListener {
        private IRenderListener deleg;

        public GlyphRenderListener(IRenderListener deleg) {
            this.deleg = deleg;
        }

        public void BeginTextBlock() {
            deleg.BeginTextBlock();
        }

        public void RenderText(TextRenderInfo renderInfo) {
            foreach (TextRenderInfo glyphInfo in renderInfo.GetCharacterRenderInfos())
                deleg.RenderText(glyphInfo);
        }

        public void EndTextBlock() {
            deleg.EndTextBlock();
        }

        public void RenderImage(ImageRenderInfo renderInfo) {
            deleg.RenderImage(renderInfo);
        }
    }
}