using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    public class StringUtilsTest {

    [TestCase('\u0000', (byte)0x0, (byte)0x0)]
    [TestCase('\b', (byte)0x0, (byte)0x08)]
    [TestCase('a', (byte)0x0, (byte)0x61)]
    [TestCase('ة', (byte)0x06, (byte)0x29)]
    [TestCase('\ud800', (byte)0xd8, (byte)0x0)]
    [TestCase('\ud7ff', (byte)0xd7, (byte)0xff)]
    [TestCase('\udbb0', (byte)0xdb, (byte)0xb0)]
    [TestCase('\ue000', (byte)0xe0, (byte)0x0)]
    [TestCase('\ufffd', (byte)0xff, (byte)0xfd)]
    [TestCase('\uffff', (byte)0xff, (byte)0xff)]
    public void ConvertCharsToBytesTest(char input, byte c1, byte c2) {
        byte[] check = {c1, c2};
        char[] vals = {input};
        byte[] result = StringUtils.ConvertCharsToBytes(vals);

        Assert.AreEqual(check, result);
    }

    }
}
