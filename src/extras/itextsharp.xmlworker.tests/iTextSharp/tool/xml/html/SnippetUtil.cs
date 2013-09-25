using System;
using System.Collections.Generic;
using iTextSharp.text;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    [Ignore]
    internal class SnippetUtil {
        /*
	 * Convenient method for retrieving the content of an elementList.
	 */

        public static void PrintAllContent(List<IElement> elementList) {
            int i = 0;
            int j = 0;
            foreach (IElement e in elementList) {
                Console.WriteLine("element " + i + ", runtime class = " + e.Type);
                foreach (Chunk c in e.Chunks) {
                    if (Chunk.NEWLINE.Content.Equals(c.Content)) {
                        Console.WriteLine("newline");
                    }
                    else {
                        Console.WriteLine(c.Type + " " + j + ": " + c.Content);
                    }
                    ++j;
                }
                ++i;
                j = 0;
            }
        }
    }
}