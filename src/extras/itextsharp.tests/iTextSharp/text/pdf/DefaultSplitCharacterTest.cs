using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    class DefaultSplitCharacterTest {
        private string[] INPUT_TEXT = new string[] { "tha111-is one that should-be-splitted-right-herel-2018-12-18", "anddate format2 01-01-1920" };

        [Test]
        public void HypenInsideDateTest() {
            Assert.False(IsPsplitCharacter(21, INPUT_TEXT[1]));
        }

        [Test]
        public void HypenBeforeDateTest() {
            Assert.True(IsPsplitCharacter(49, INPUT_TEXT[0]));
        }

        [Test]
        public void HypenInsideTextTest() {
            Assert.True(IsPsplitCharacter(6, INPUT_TEXT[0]));
        }

        internal bool IsPsplitCharacter(int current, string text) {
            return new DefaultSplitCharacter().IsSplitCharacter(75, current, text.Length + 1, text.ToCharArray(), null);
        }
    }
}
