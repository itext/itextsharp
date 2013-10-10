namespace iTextSharp.text.pdf.parser {
    internal class GlyphTextRenderListener : GlyphRenderListener, ITextExtractionStrategy {
        private ITextExtractionStrategy deleg;

        public GlyphTextRenderListener(ITextExtractionStrategy deleg) : base(deleg) {
            this.deleg = deleg;
        }

        public string GetResultantText() {
            return deleg.GetResultantText();
        }
    }
}