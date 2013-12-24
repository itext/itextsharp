using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.text
{
    public class TabSplitCharacter : ISplitCharacter
    {
        public static readonly ISplitCharacter TAB = new TabSplitCharacter();

        virtual public bool IsSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck)
        {
            return true;
        }
    }
}
