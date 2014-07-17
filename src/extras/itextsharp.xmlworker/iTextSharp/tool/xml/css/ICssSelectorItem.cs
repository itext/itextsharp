namespace iTextSharp.tool.xml.css {
    public interface ICssSelectorItem {
        bool Matches(Tag t);

        char Separator { get; }

        int Specificity { get; }
    }
}
