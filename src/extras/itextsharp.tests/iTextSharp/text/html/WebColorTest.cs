/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.html;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.html {
    public class WebColorTest {
        // Mix of different separator chars
        private const String RGB_PERCENT = "rgb(100%, 33%	50%,20%)";
        private const String RGB_OUT_OF_RANGE = "RGB(-100, 10%, 500)";
        private const String RGB_MISSING_COLOR_VALUES = "rgb(,,127,63)";

        /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     * Throw a bunch of equivalent colors at WebColors and ensure that the
	     * return values really are equivalent.
	     * 
	     * @throws Exception
	     */
        [Test]
        public virtual void GoodColorTests() {
            String[] colors = {
                "#00FF00", "00FF00", "#0F0", "0F0", "LIme",
                "rgb(0,255,0 )"
            };
            // TODO webColor creates colors with a zero alpha channel (save
            // "transparent"), BaseColor's 3-param constructor creates them with a
            // 0xFF alpha channel. Which is right?!
            BaseColor testCol = new BaseColor(0, 255, 0);
            foreach (String colStr in colors) {
                BaseColor curCol = WebColors.GetRGBColor(colStr);
                Assert.IsTrue(testCol.Equals(curCol), DumpColor(testCol) + "!=" + DumpColor(curCol));
            }
        }

        /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
        [Test]
        public virtual void MoreColorTest() {
            String colorStr = "#888";
            String colorStrLong = "#888888";
            Assert.AreEqual(WebColors.GetRGBColor(colorStr), WebColors.GetRGBColor(colorStrLong), "Oh Nooo colors are different");
        }

        private String DumpColor(BaseColor col) {
            StringBuilder colBuf = new StringBuilder();
            colBuf.Append("r:");
            colBuf.Append(col.R);
            colBuf.Append(" g:");
            colBuf.Append(col.G);
            colBuf.Append(" b:");
            colBuf.Append(col.B);
            colBuf.Append(" a:");
            colBuf.Append(col.A);

            return colBuf.ToString();
        }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     * 
	     * @throws Exception
	     */
        [Test]
        public void BadColorTests() {
            String[] badColors = {"", null, "#xyz", "#12345", "notAColor"};

            foreach (String curStr in badColors) {
                try {
                    // we can ignore the return value that'll never happen here
                    WebColors.GetRGBColor(curStr);

                    Assert.IsTrue(false, "getRGBColor should have thrown for: " + curStr);
                } catch (FormatException e) {
                    // Non-null bad colors will throw an illArgEx
                    Assert.IsTrue(curStr != null);
                    // good, it was supposed to throw
                } catch (NullReferenceException e) {
                    // the null color will NPE
                    Assert.IsTrue(curStr == null);
                }
            }
        }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorInPercentRed() {
		    Assert.AreEqual(255, WebColors.GetRGBColor(RGB_PERCENT).R);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorInPercentGreen() {
		    Assert.AreEqual(84, WebColors.GetRGBColor(RGB_PERCENT).G);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorInPercentBlue() {
		    Assert.AreEqual(127, WebColors.GetRGBColor(RGB_PERCENT).B);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorInPercentAlpha() {
		    Assert.AreEqual(255, WebColors.GetRGBColor(RGB_PERCENT).A);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorNegativeValue() {
		    Assert.AreEqual(0, WebColors.GetRGBColor(RGB_OUT_OF_RANGE).R);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorValueOutOfRange() {
		    Assert.AreEqual(255, WebColors.GetRGBColor(RGB_OUT_OF_RANGE).B);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorChannelsMissingRed() {
		    Assert.AreEqual(127, WebColors.GetRGBColor(RGB_MISSING_COLOR_VALUES).R);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorChannelsMissingGreen() {
		    Assert.AreEqual(63, WebColors.GetRGBColor(RGB_MISSING_COLOR_VALUES).G);
	    }

	    /**
	     * Test method for
	     * {@link com.itextpdf.text.html.WebColors#getRGBColor(java.lang.String)}.
	     */
	    [Test]
	    public virtual void TestGetRGBColorChannelsMissingBlue() {
		    Assert.AreEqual(0, WebColors.GetRGBColor(RGB_MISSING_COLOR_VALUES).B);
	    }
    }
}
