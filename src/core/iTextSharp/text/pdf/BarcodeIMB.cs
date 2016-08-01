using System;
using System.Text.RegularExpressions;
using iTextSharp.text.error_messages;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2014 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf
{
    /** Implements the USPS Intelligent Mail barcode, the 4 state barcode used to replace PLANET and POSTNET barcodes.
     * 
     * The default paramaters are
     * <pre>
     * n = 72f / 22f; // distance between bars
     * x = 0.02f * 72f; // bar width
     * barHeight = 0.125f * 72f; // height of the full bars
     * codeType = IMB;
     * </pre>
     * 
     * @author Jareth Hein
     */
    public class BarcodeIMB : Barcode
    {

    #region US Postal Service derived encoding routines
    /*
    Code in this region adapted from code taken from https://ribbs.usps.gov/onecodesolution/download.cfm downloaded 
    on Apr 17, 2011.  It contained the following license.
 
       * * *    Intelligent Mail - Notice of License

    For Intelligent Mail barcode, IMb, Encoder Software and Fonts

    By using any portion of the Intelligent Mail barcode Encoder software (herein "Software"), Fonts, and/or the
    accompanying Documentation, you are agreeing to the terms of this License agreement.

    This License is effective as of the day you download or otherwise acquire the Software, Documentation, and/or
    Fonts.  It remains effective so long as you have a copy of the Software, Documentation, and/or Fonts or, if
    shorter, for the life of the longest Copyright in the Software, Documentation, and/or Fonts.

    The Postal Service grants you, based upon its rights in the Software, Documentation, and Fonts, a world-wide,
    revocable, license to use them in the mailing industry without paying the Postal Service a royalty or other fee,
    including the rights to reproduce, display, merge, sell, distribute, sublicense, and translate them; and the right
    to create derivative works, which is not limited to the right to modify, improve, and to incorporate them into
    larger works, and to translate, edit, and the like the software in whole or in part. provided that, the conditions
    set forth below are met.


    The Conditions:  

        The license granted in this Agreement is conditioned upon the licensee maintaining the following
        requirements:

        1.  Redistribution of the Software, Documentation, and/or Font must contain (a) the copyright notice set 
            forth below, (b) this list of conditions, and (c) the disclaimer noted below. 

        2.  Unless you have the specific prior written permission of the Postal Service, you must refrain from using 
            the Postal Service?s name or trademarks to endorse or promote products or services derived from or 
            containing or using this Software and/or Fonts, except for factual statements presented in the body of 
            text that merely reference your use of the US Postal Service?s? Software and/or Fonts.

        3.  If you distribute the Software and/or Fonts to others, you cannot charge them a price for the Software 
            and/or Fonts, except you may charge others for:

            a.  any product that they are apart of, for example, for your product that includes the Software and/or 
                Fonts.
            b.  your services performed using the Software and/or Fonts.
            c.  your versions of the Software and/or Fonts, if your versions were created by making modifications or 
                improvements to the Software and/or Fonts (respectively) for a legitimate purpose in furtherance of 
                your or another?s business.  

    The US Postal Service reserves the right to change this License in any manner within reason at its discretion. 

    Copyright Notice: ? US Postal Service 2009

    Disclaimer: 

        This software is provided by the United States Postal Service "as is" and any expressed or implied warranties,
        including, but not limited to, the implied warranties of merchantability and fitness for a particular purpose
        are disclaimed. In no event shall the US Postal Service be liable for any direct, indirect, incidental, special,
        exemplary, or consequential damages (including, but not limited to, procurement of substitute goods or services;
        loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether
        in contract, strict liability, or tort (including negligence or otherwise) arising in any way out of the use of
        this software, even if advised of the possibility of such damage.


       * * * 
       */
#if SELF_TEST
        private static bool EncoderSelfTestedFlag;
#endif
        private const int TABLE_2_OF_13_SIZE = 78;
        private const int TABLE_5_OF_13_SIZE = 1287;

        public enum IMBResult
        {
            USPS_FSB_ENCODER_API_SUCCESS,
            USPS_FSB_ENCODER_API_SELFTEST_FAILED,
            USPS_FSB_ENCODER_API_BAR_STRING_IS_NULL,
            USPS_FSB_ENCODER_API_BYTE_CONVERSION_FAILED,
            USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED,
            USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED,
            USPS_FSB_ENCODER_API_CHARACTER_RANGE_ERROR,
            USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL,
            USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL,
            USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH,
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA,
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2,
            USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH,
            USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA
        }


        /*******************************************************************************
        **
        ** Internal Types
        **
        *******************************************************************************/

        private struct NumberRecordType
        {
            public int Base;
            public int Number;
        }




        /*******************************************************************************
        **
        ** Local Variables
        **
        *******************************************************************************/
        private static readonly int[] BarTopCharacterIndexArray = { 4, 0, 2, 6, 3, 5, 1, 9, 8, 7, 1, 2, 0, 6, 4, 8, 2, 9, 5, 3, 0, 1, 3, 7, 4, 6, 8, 9, 2, 0, 5, 1, 9, 4, 3, 8, 6, 7, 1, 2, 4, 3, 9, 5, 7, 8, 3, 0, 2, 1, 4, 0, 9, 1, 7, 0, 2, 4, 6, 3, 7, 1, 9, 5, 8 };
        private static readonly int[] BarBottomCharacterIndexArray = { 7, 1, 9, 5, 8, 0, 2, 4, 6, 3, 5, 8, 9, 7, 3, 0, 6, 1, 7, 4, 6, 8, 9, 2, 5, 1, 7, 5, 4, 3, 8, 7, 6, 0, 2, 5, 4, 9, 3, 0, 1, 6, 8, 2, 0, 4, 5, 9, 6, 7, 5, 2, 6, 3, 8, 5, 1, 9, 8, 7, 4, 0, 2, 6, 3 };
        private static readonly int[] BarTopCharacterShiftArray = { 3, 0, 8, 11, 1, 12, 8, 11, 10, 6, 4, 12, 2, 7, 9, 6, 7, 9, 2, 8, 4, 0, 12, 7, 10, 9, 0, 7, 10, 5, 7, 9, 6, 8, 2, 12, 1, 4, 2, 0, 1, 5, 4, 6, 12, 1, 0, 9, 4, 7, 5, 10, 2, 6, 9, 11, 2, 12, 6, 7, 5, 11, 0, 3, 2 };
        private static readonly int[] BarBottomCharacterShiftArray = { 2, 10, 12, 5, 9, 1, 5, 4, 3, 9, 11, 5, 10, 1, 6, 3, 4, 1, 10, 0, 2, 11, 8, 6, 1, 12, 3, 8, 6, 4, 4, 11, 0, 6, 1, 9, 11, 5, 3, 7, 3, 10, 7, 11, 8, 2, 10, 3, 5, 8, 0, 3, 12, 11, 8, 4, 5, 1, 3, 0, 7, 12, 9, 8, 10 };


        private static Boolean Table2of13InitializedFlag = false;
        private static Boolean Table5of13InitializedFlag = false;

        private static int[] Table2of13 = new int[TABLE_2_OF_13_SIZE];
        private static int[] Table5of13 = new int[TABLE_5_OF_13_SIZE];





        /*******************************************************************************
        **
        ** Internal Functions
        **
        *******************************************************************************/

        /*******************************************************************************
        ** MultiplyBytesByShort
        *******************************************************************************/
        private static Boolean MultiplyBytesByShort(byte[] ByteArrayPtr, ushort Multiplicand)
        {
            int ByteIndex;
            UInt32 Carry32, Temp32;


            /* Check for obviously incorrect inputs */
            if (ByteArrayPtr == null)
                return false;
            if (ByteArrayPtr.Length < 1)
                return false;

            /* Do groups of two bytes */
            Carry32 = 0;
            for (ByteIndex = ByteArrayPtr.Length - 1; ByteIndex > 0; ByteIndex -= 2)
            {
                /* Fill two low bytes of four byte variable with packed data */
                Temp32 = (UInt32)ByteArrayPtr[ByteIndex];
                Temp32 |= (UInt32)ByteArrayPtr[ByteIndex - 1] << 8;

                /* 0x0000???? * 0x0000???? = 0xCCCCRRRR (C-carry data, R-result data) */
                Temp32 *= (UInt32)Multiplicand;
                Temp32 += Carry32;

                /* The two low bytes (result) go back into the packed data */
                ByteArrayPtr[ByteIndex] = (byte)Temp32;
                ByteArrayPtr[ByteIndex - 1] = (byte)(Temp32 >> 8);

                /* The two high bytes will be the carry data for the next pass */
                Carry32 = Temp32 >> 16;
            }

            /* Single byte left over? */
            if (ByteIndex == 0)
            {
                Temp32 = (UInt32)ByteArrayPtr[0];
                Temp32 *= (UInt32)Multiplicand;
                Temp32 += Carry32;

                /* The low byte goes back into the packed data */
                ByteArrayPtr[0] = (byte)(Temp32 & 0xFF);
            }

            return true;
        }

        /*******************************************************************************
        ** DivideBytesByShort
        *******************************************************************************/
        private static Boolean
        DivideBytesByShort(byte[] ByteArrayPtr, ushort Divisor, ref UInt32 RemainderPtr)
        {
            int ByteIndex;
            UInt32 Temp32, Remainder32;


            /* Check for obviously incorrect inputs */
            if (ByteArrayPtr == null)
                return false;
            if (ByteArrayPtr.Length < 2)
                return false;
            if (Divisor == 0)
                return false;

            /* If we do not have an even number of bytes, do the first byte separately */
            if ((ByteArrayPtr.Length % 2) == 1)
            {
                Temp32 = (UInt32)ByteArrayPtr[0];
                Remainder32 = Temp32 % Divisor;
                Temp32 /= Divisor;

                ByteArrayPtr[0] = (byte)Temp32;
                ByteIndex = 1;
            }
            else
            {
                Remainder32 = 0;
                ByteIndex = 0;
            }

            /* Now that we have an even number of bytes left, go from left to right in 
            groups of two */
            for (; ByteIndex < ByteArrayPtr.Length; ByteIndex += 2)
            {
                /* Build up a slice consisting of the previous slices remainder in the 
                    top 16 and data in the low 16 */
                Temp32 = Remainder32 << 16;
                Temp32 |= (UInt32)ByteArrayPtr[ByteIndex] << 8;
                Temp32 |= (UInt32)ByteArrayPtr[ByteIndex + 1];

                Remainder32 = Temp32 % Divisor;
                Temp32 /= Divisor;

                /* Replace slice of dividend with slice of quotient */
                ByteArrayPtr[ByteIndex] = (byte)(Temp32 >> 8);
                ByteArrayPtr[ByteIndex + 1] = (byte)Temp32;
            }

            RemainderPtr = Remainder32;
            return true;
        }

        /*******************************************************************************
        ** AddShortToBytes
        *******************************************************************************/
        private static Boolean
        AddShortToBytes(byte[] ByteArrayPtr, UInt32 Addend)
        {
            int ByteIndex;
            UInt32 Temp32;
            Boolean Carry32;


            /* Check for obviously incorrect inputs */
            if (ByteArrayPtr == null)
                return false;
            if (ByteArrayPtr.Length < 1)
                return false;

            /* Fill two low bytes of four byte variable with packed data */
            Temp32 = (UInt32)ByteArrayPtr[ByteArrayPtr.Length - 1];
            Temp32 |= (UInt32)ByteArrayPtr[ByteArrayPtr.Length - 2] << 8;

            /* Add a two byte value into the four byte variable */
            Temp32 += (UInt32)Addend;

            /* The two low bytes go back into the packed area */
            ByteArrayPtr[ByteArrayPtr.Length - 1] = (byte)Temp32;
            ByteArrayPtr[ByteArrayPtr.Length - 2] = (byte)(Temp32 >> 8);

            /* A single bit carry may be generated */
            Carry32 = Temp32 > 0xFFFF;

            /* Propagate carry up */
            for (ByteIndex = ByteArrayPtr.Length - 3; Carry32 && (ByteIndex > 0); ByteIndex--)
            {
                Temp32 = (Carry32 ? (UInt32)1 : (UInt32)0) + (UInt32)ByteArrayPtr[ByteIndex];

                ByteArrayPtr[ByteIndex] = (byte)Temp32;

                Carry32 = Temp32 > 0xFF;
            }

            return true;
        }

        /*******************************************************************************
        ** ConvertFromBytesToMultiBase
        *******************************************************************************/
        private static Boolean ConvertFromBytesToMultiBase(byte[] ByteArrayPtr, NumberRecordType[] NumberArrayPtr)
        {
            UInt32 Remainder = 0;
            int NumberIndex;

            for (NumberIndex = NumberArrayPtr.Length - 1; NumberIndex >= 0; NumberIndex--)
            {
                if (DivideBytesByShort(ByteArrayPtr,
                                       (ushort)NumberArrayPtr[NumberIndex].Base,
                                       ref Remainder) != true)
                    return false;
                NumberArrayPtr[NumberIndex].Number = (int)Remainder;
            }

            return true;
        }

        /*******************************************************************************
        ** ConvertFromMultiBaseToBytes
        *******************************************************************************/
        private static Boolean ConvertFromMultiBaseToBytes(NumberRecordType[] NumberArrayPtr, byte[] ByteArrayPtr)
        {
            int NumberIndex, i;

            for (i = 0; i < ByteArrayPtr.Length; i++)
            {
                Buffer.SetByte(ByteArrayPtr, i, (byte)0);
            }

            for (NumberIndex = 0; NumberIndex < NumberArrayPtr.Length; NumberIndex++)
            {
                if (MultiplyBytesByShort(ByteArrayPtr,
                                           (ushort)NumberArrayPtr[NumberIndex].Base) != true)
                    return false;
                if (AddShortToBytes(ByteArrayPtr,
                                      (ushort)NumberArrayPtr[NumberIndex].Number) != true)
                    return false;
            }

            return true;
        }

        /*******************************************************************************
        ** ReverseShort
        *******************************************************************************/
        private static ushort ReverseShort(ushort Input)
        {
            ushort Reverse = 0;
            int Index;

            for (Index = 0; Index < 16; Index++)
            {
                Reverse <<= 1;
                Reverse |= (ushort)(Input & 1);
                Input >>= 1;
            }

            return Reverse;
        }


        /*******************************************************************************
        ** InitializeNof13Table
        *******************************************************************************/
        private static Boolean InitializeNof13Table(ref int[] TableNof13, int N)
        {
            ushort Count, Reverse;
            int LUT_LowerIndex, LUT_UpperIndex;
            int BitCount;
            int BitIndex;
            Boolean SymmetricFlag;


            /* Count up to 2^13 and find all those values that have N bits on */
            LUT_LowerIndex = 0;
            LUT_UpperIndex = TableNof13.Length - 1;

            for (Count = 0; Count < 8192; Count++)
            {
                BitCount = 0;
                for (BitIndex = 0; BitIndex < 13; BitIndex++)
                    BitCount += (((Count & (1 << BitIndex)) != 0) ? 1 : 0);

                /* If we don't have the right number of bits on, go on to the next value */
                if (BitCount != N)
                    continue;

                Reverse = (ushort)(ReverseShort(Count) >> 3);

                SymmetricFlag = Count == Reverse;

                /* If the reverse is less than count, we have already visited this pair before */
                if (Reverse < Count)
                {
                    continue;
                }

                if (SymmetricFlag)
                {
                    TableNof13[LUT_UpperIndex] = Count;
                    LUT_UpperIndex -= 1;
                }
                else
                {
                    TableNof13[LUT_LowerIndex] = Count;
                    LUT_LowerIndex += 1;
                    TableNof13[LUT_LowerIndex] = Reverse;
                    LUT_LowerIndex += 1;
                }
            }

            /* We better have the exact correct number of table entries */
            if (LUT_LowerIndex != (LUT_UpperIndex + 1))
                return false;

            return true;
        }

        /*******************************************************************************
        ** GetNof13Table
        *******************************************************************************/
        private static Boolean GetNof13Table(int N)
        {
            switch (N)
            {
                case 2:
                    if (Table2of13InitializedFlag == false)
                        if (InitializeNof13Table(ref Table2of13, 2) != true)
                            return false;
                    Table2of13InitializedFlag = true;
                    return true;
                case 5:
                    if (Table5of13InitializedFlag == false)
                        if (InitializeNof13Table(ref Table5of13, 5) != true)
                            return false;
                    Table5of13InitializedFlag = true;
                    return true;
                default:
                    return false;
            }
        }

        /*******************************************************************************
        ** GenerateCRC11FrameCheckSequence
        *******************************************************************************/
        private static ushort GenerateCRC11FrameCheckSequence(byte[] ByteArray)
        {
            ushort GeneratorPolynomial = 0x0F35;
            ushort FrameCheckSequence = 0x07FF;
            ushort Data;
            int ByteIndex, Bit;


            /* Do most significant byte skipping most significant bit */
            Data = (ushort)(ByteArray[0] << 5);
            for (Bit = 2; Bit < 8; Bit++)
            {
                if (((FrameCheckSequence ^ Data) & 0x400) > 0)
                    FrameCheckSequence = (ushort)((FrameCheckSequence << 1) ^ GeneratorPolynomial);
                else
                    FrameCheckSequence = (ushort)(FrameCheckSequence << 1);
                FrameCheckSequence &= 0x7FF;
                Data <<= 1;
            }

            /* Do rest of the bytes */
            for (ByteIndex = 1; ByteIndex < 13; ByteIndex++)
            {
                Data = (ushort)(ByteArray[ByteIndex] << 3);
                for (Bit = 0; Bit < 8; Bit++)
                {
                    if (((FrameCheckSequence ^ Data) & 0x0400) > 0)
                        FrameCheckSequence = (ushort)((FrameCheckSequence << 1) ^ GeneratorPolynomial);
                    else
                        FrameCheckSequence = (ushort)(FrameCheckSequence << 1);
                    FrameCheckSequence &= 0x7FF;
                    Data <<= 1;
                }
            }

            return FrameCheckSequence;
        }


        /*******************************************************************************
        ** Internal Encode
        *******************************************************************************/
        private static IMBResult
        intEncode(String TrackingString, String RoutingString, ref String BarString)
        {
            byte[] ByteArray = new byte[13];
            NumberRecordType[] CodewordArray = new NumberRecordType[10];
            NumberRecordType[] CharacterArray = new NumberRecordType[10];
            int DigitIndex;
            int CodewordIndex;
            int CharacterIndex;
            int BarIndex;
            int[] BarTopArray = new int[65];
            int[] BarBottomArray = new int[65];
            UInt32 FrameCheckSequence11BitValue;
            NumberRecordType[] ZipArray = new NumberRecordType[12];
            NumberRecordType[] AddArray = new NumberRecordType[12];



            /* Reset NumberRecord arrays before use */
            for (DigitIndex = 0; DigitIndex < 12; DigitIndex++)
            {
                ZipArray[DigitIndex].Base = 10;
                ZipArray[DigitIndex].Number = 0;
                AddArray[DigitIndex].Base = 10;
                AddArray[DigitIndex].Number = 0;
            }

            /* Fill out NumberRecord arrays according to routing length */
            switch (RoutingString.Length)
            {
                case 0:
                    /* Do nothing, ZipArray and AddArray already set to zeros */
                    break;
                case 5:
                    for (DigitIndex = 0; DigitIndex < 5; DigitIndex++)
                        ZipArray[DigitIndex + 7].Number = (int)(RoutingString[DigitIndex] - '0');
                    AddArray[11].Number = 1; /*          1 */
                    break;
                case 9:
                    for (DigitIndex = 0; DigitIndex < 9; DigitIndex++)
                        ZipArray[DigitIndex + 3].Number = (int)(RoutingString[DigitIndex] - '0');
                    AddArray[11].Number = 1; /*          1 */
                    AddArray[6].Number = 1; /*     100000 */
                    break;
                case 11:
                    for (DigitIndex = 0; DigitIndex < 11; DigitIndex++)
                        ZipArray[DigitIndex + 1].Number = (int)(RoutingString[DigitIndex] - '0');
                    AddArray[11].Number = 1; /*          1 */
                    AddArray[6].Number = 1; /*     100000 */
                    AddArray[2].Number = 1; /* 1000000000 */
                    break;
                default:
                    return IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH;
            }

            /* Add AddArray to ZipArray */
            for (DigitIndex = 11; ; DigitIndex--)
            {
                ZipArray[DigitIndex].Number += AddArray[DigitIndex].Number;

                if (DigitIndex <= 0)
                    break;

                if (ZipArray[DigitIndex].Number >= 10)
                {
                    ZipArray[DigitIndex].Number -= 10;
                    ZipArray[DigitIndex - 1].Number += 1;
                }
            }


            /* Convert from 12 digits of base 10 to 13 bytes (only needs rightmost 37 bits at this point) */
            if (ConvertFromMultiBaseToBytes(ZipArray, ByteArray) != true)
                return IMBResult.USPS_FSB_ENCODER_API_BYTE_CONVERSION_FAILED;



            /* Put tracking data into Byte Array */
            MultiplyBytesByShort(ByteArray, (ushort)10);
            AddShortToBytes(ByteArray, (ushort)(TrackingString[0] - '0'));
            MultiplyBytesByShort(ByteArray, (ushort)5);
            AddShortToBytes(ByteArray, (ushort)(TrackingString[1] - '0'));
            for (DigitIndex = 2; DigitIndex < 20; DigitIndex++)
            {
                MultiplyBytesByShort(ByteArray, (ushort)10);
                AddShortToBytes(ByteArray, (ushort)(TrackingString[DigitIndex] - '0'));
            }


            /* Generate a CRC FCS character on the 102 bit value */
            FrameCheckSequence11BitValue = GenerateCRC11FrameCheckSequence(ByteArray);


            /* Get the 5 of 13 table we need */
            if (GetNof13Table(5) != true)
                return IMBResult.USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED;
            /* Get the 2 of 13 table we need */
            if (GetNof13Table(2) != true)
                return IMBResult.USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED;

            /* Convert to base that allows 5 or 2 of 13 representation. */
            for (CodewordIndex = 0; CodewordIndex < 10; CodewordIndex++)
            {
                CodewordArray[CodewordIndex].Base = TABLE_5_OF_13_SIZE + TABLE_2_OF_13_SIZE;
                CodewordArray[CodewordIndex].Number = 0;
            }
            CodewordArray[0].Base = 659;
            CodewordArray[9].Base = 636;
            if (ConvertFromBytesToMultiBase(ByteArray, CodewordArray) != true)
                return IMBResult.USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED;


            if (CodewordArray[0].Number >= 659)
                return IMBResult.USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED;
            if (CodewordArray[9].Number >= 636)
                return IMBResult.USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED;

            /* Put orientation information into the rightmost codeword */
            CodewordArray[9].Number = CodewordArray[9].Number * 2;


            /* Put the leftmost FCS bit into the leftmost codeword */
            if ((FrameCheckSequence11BitValue >> 10) > 0)
                CodewordArray[0].Number += 659;


            /* Convert from codewords to 13 bit characters */
            for (CharacterIndex = 0; CharacterIndex < 10; CharacterIndex++)
            {
                if (CodewordArray[CharacterIndex].Number >= TABLE_5_OF_13_SIZE + TABLE_2_OF_13_SIZE)
                    return IMBResult.USPS_FSB_ENCODER_API_CHARACTER_RANGE_ERROR;
                else if (CodewordArray[CharacterIndex].Number >= TABLE_5_OF_13_SIZE)
                {
                    CharacterArray[CharacterIndex].Base = 8192;
                    CharacterArray[CharacterIndex].Number = Table2of13[CodewordArray[CharacterIndex].Number - TABLE_5_OF_13_SIZE];
                }
                else
                {
                    CharacterArray[CharacterIndex].Base = 8192;
                    CharacterArray[CharacterIndex].Number = Table5of13[CodewordArray[CharacterIndex].Number];
                }
            }


            /* Insert the FCS into the data by the following process:        */
            /*   for each character get the corresponding bit of the FCS     */
            /*     note that character 0 is the leftmost character while its */
            /*     corresponding FCS bit (0) is the rightmost in the FCS     */
            /*     if the bit value is:                                      */
            /*       0 - then leave the character as 5 of 13                 */
            /*       1 - reverse all bits values in the character which      */
            /*           makes it 8 of 13                                    */
            for (CharacterIndex = 0; CharacterIndex < 10; CharacterIndex++)
                if ((FrameCheckSequence11BitValue & (1 << CharacterIndex)) > 0)
                    CharacterArray[CharacterIndex].Number = ~CharacterArray[CharacterIndex].Number & 0x1FFF;


            /* Map 13 bit characters to their positions within the barcode */
            for (BarIndex = 0; BarIndex < 65; BarIndex++)
            {
                BarTopArray[BarIndex] = (CharacterArray[BarTopCharacterIndexArray[BarIndex]].Number >> BarTopCharacterShiftArray[BarIndex]) & 1;
                BarBottomArray[BarIndex] = (CharacterArray[BarBottomCharacterIndexArray[BarIndex]].Number >> BarBottomCharacterShiftArray[BarIndex]) & 1;
            }

            /* Convert the barcode to a string of characters representing the 4-state bars */

            char[] BarArr = new char[65];
            for (BarIndex = 0; BarIndex < 65; BarIndex++)
                if (BarTopArray[BarIndex] == 0)
                    if (BarBottomArray[BarIndex] == 0)
                        BarArr[BarIndex] = 'T';
                    else
                        BarArr[BarIndex] = 'D';
                else
                    if (BarBottomArray[BarIndex] == 0)
                        BarArr[BarIndex] = 'A';
                    else
                        BarArr[BarIndex] = 'F';
            BarString = new String(BarArr);

            return IMBResult.USPS_FSB_ENCODER_API_SUCCESS;
        }


        public static IMBResult
        Encoder(String TrackString, String RouteString, ref String BarString)
        {
#if SELF_TEST
  char SelfTestBarString[65+1];
#endif
            int TrackIndex, RouteIndex;

#if SELF_TEST
  /* Check for proper operation */
  if ( EncoderSelfTestedFlag != true )
  {
    /* The following four tests are taken from the Specification Document */

    if ( Encode( "01234567094987654321", "", SelfTestBarString ) != USPS_FSB_ENCODER_API_SUCCESS )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    if ( strncmp( SelfTestBarString, "ATTFATTDTTADTAATTDTDTATTDAFDDFADFDFTFFFFFTATFAAAATDFFTDAADFTFDTDT", 65 ) != 0 )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    
    if ( Encode( "01234567094987654321", "01234", SelfTestBarString ) != USPS_FSB_ENCODER_API_SUCCESS )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    if ( strncmp( SelfTestBarString, "DTTAFADDTTFTDTFTFDTDDADADAFADFATDDFTAAAFDTTADFAAATDFDTDFADDDTDFFT", 65 ) != 0 )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    
    if ( Encode( "01234567094987654321", "012345678", SelfTestBarString ) != USPS_FSB_ENCODER_API_SUCCESS )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    if ( strncmp( SelfTestBarString, "ADFTTAFDTTTTFATTADTAAATFTFTATDAAAFDDADATATDTDTTDFDTDATADADTDFFTFA", 65 ) != 0 )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    
    if ( Encode( "01234567094987654321", "01234567891", SelfTestBarString ) != USPS_FSB_ENCODER_API_SUCCESS )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    if ( strncmp( SelfTestBarString, "AADTFFDFTDADTAADAATFDTDDAAADDTDTTDAFADADDDTFFFDDTTTADFAAADFTDAADA", 65 ) != 0 )
      return USPS_FSB_ENCODER_API_SELFTEST_FAILED;
    
    EncoderSelfTestedFlag = true;
  }
#endif

            /* Check the parameters */
            if (TrackString == null)
                return IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL;
            if (RouteString == null)
                return IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL;
            if (BarString == null)
                return IMBResult.USPS_FSB_ENCODER_API_BAR_STRING_IS_NULL;

            /* Track String must be 20 ASCII digits with 2nd digit from left limited to 0-4 */
            /*   Put the data in a temporary area to make sure strlen will not wander into invalid memory */
            if (TrackString.Length != 20)
                return IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH;
            for (TrackIndex = 0; TrackIndex < 20; TrackIndex++)
                if ((TrackString[TrackIndex] < '0') || (TrackString[TrackIndex] > '9'))
                    return IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA;
            if ((TrackString[1] < '0') || (TrackString[1] > '4'))
                return IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2;

            /* Route String must be 0, 5, 9, or 11 ASCII digits */
            switch (RouteString.Length)
            {
                case 0:
                case 5:
                case 9:
                case 11:
                    /* Length is OK */
                    break;
                default:
                    return IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH;
            }
            for (RouteIndex = 0; RouteIndex < RouteString.Length; RouteIndex++)
                if ((RouteString[RouteIndex] < '0') || (RouteString[RouteIndex] > '9'))
                    return IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA;

            /* Now that the inputs check out OK, send it to the encoder */
            return intEncode(TrackString, RouteString, ref BarString);
        }

        #endregion //USPS derived routines

        /** Creates new BarcodeIMB */
        public BarcodeIMB()
        {
            n = 72f / 22f; // distance between bars
            x = 0.02f * 72f; // bar width
            barHeight = 0.125f * 72f; // height of the full bars
            codeType = IMB;
        }

        /** Places the barcode in a <CODE>PdfContentByte</CODE>. The
          * barcode is always placed at coodinates (0, 0). Use the
          * translation matrix to move it elsewhere.<p>
          * The bars will be painted with the specified barColor, if specified, else with the current fill color.
          * @param cb the <CODE>PdfContentByte</CODE> where the barcode will be placed
          * @param barColor the color of the bars. It can be <CODE>null</CODE>
          * @param textColor the color of the text (not used). It can be <CODE>null</CODE>
          * @return the dimensions the barcode occupies
          */
        public override Rectangle PlaceBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor)
        {
            if (barColor != null)
                cb.SetColorFill(barColor);
            float startX = 0;
            for (int k = 0; k < Code.Length; ++k)
            {
                char data = Code[k];
                float barLen = 0, barOffset = 0;
                switch (data)
                {
                    case 'F':
                        barLen = barHeight;
                        barOffset = 0;
                        break;
                    case 'A':
                        barLen = 0.082f * 72f;
                        barOffset = 0.043f * 72f;
                        break;
                    case 'T':
                        barLen = 0.039f * 72f;
                        barOffset = 0.043f * 72f;
                        break;
                    case 'D':
                        barLen = 0.082f * 72f;
                        barOffset = 0;
                        break;
                }
                cb.Rectangle(startX, barOffset, x - inkSpreading, barLen);
                startX += n;
            }
            cb.Fill();

            return this.BarcodeSize;
        }

        /** Gets the maximum area that the barcode and the text, if
          * any, will occupy. The lower left corner is always (0, 0).
          * @return the size the barcode occupies.
          */
        public override Rectangle BarcodeSize
        {
            get
            {
                int len = code.Length;
                float width = x + n;
                float fullWidth = (len + 2) * width;
                float fullHeight = barHeight;
                return new Rectangle(fullWidth, fullHeight);
            }
        }

        /** Sets the code to generate a barcode for
         * @param code The code of the IMB
         */
        public override string Code
        {
            set
            {
                if (value.Length > 0)
                {
                    /* make sure it has a legal value... an IMB barcode can have only the characters FATD, and must be 65 chars long */
                    Regex legalCodeRegex = new Regex(@"^[FATD]{65}$");
                    if (!legalCodeRegex.IsMatch(value))
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.badly.formed.string.1", value));
                    }
                }
                base.Code = value;
            }
        }

        /** Encode and set an IMB code.
         * @param TrackString is the trackable part of the barcode: 20 ASCII chars, two digit barcodetype (second digit must be 0-4, usually 00) + 3 digit service code + mailerID + serialnumber of mailpiece
         * @param RouteString is the routing part of the barcode: Must be 0, 5, 9 or 11 digits [5 or 9 digit ZIP code [+ DP]]
         * @param BarString is the encoded string to be set into the barcode
         * @return is the state of the encoding operation
          */
        public void EncodeAndSetIMB(string Track, string Route)
        {
            string bCode = "";
            IMBResult res = Encoder(Track, Route, ref bCode);
            switch (res)
            {
                case IMBResult.USPS_FSB_ENCODER_API_SUCCESS:
                    this.Code = bCode;
                    return;
                case IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.null.track.string"));
                case IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.null.route.string"));
                case IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.track.string.bad.length.1", Track));
                case IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.track.string.bad.data.1", Track));
                case IMBResult.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.track.string.bad.2nd.digit.1", Track));
                case IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.route.string.bad.length.1", Route));
                case IMBResult.USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.route.string.bad.data.1", Route));
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("imb.encoding.failure"));
            }
        }

#if DRAWING
        /** Draw the barcode, and return it as an image
         * @param foreground Color of the bars.
         * @param background Color of the background the bars will be drawn on
         * @return a System.Drawing.Image containing the drawn IMB
         */
        public override System.Drawing.Image CreateDrawingImage(System.Drawing.Color foreground, System.Drawing.Color background)
        {
            int len = code.Length;
            int width = 3;
            int fullWidth = len * (width * 2);
            int height = (int)barHeight;
            int barX = 0;
            int barY = 0;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fullWidth, height);
            for (int i = 0; i < len; i++)
            {
                char data = code[i];
                int w = i * (width * 2);
                for (int wi = 0; wi < width; wi++)
                {
                    barX = w + wi;
                    for (int part = 0; part < 3; part++)
                    {
                        System.Drawing.Color c = background;
                        switch (part)
                        {
                            case 0:
                                if (data == 'F' || data == 'A') c = foreground;
                                break;
                            case 1:
                                c = foreground;
                                break;
                            case 2:
                                if (data == 'F' || data == 'D') c = foreground;
                                break;
                        }
                        barY = (part * (int)size);
                        for (int h = 0; h < size; h++, barY++)
                        {
                            bmp.SetPixel(barX, barY, c);
                        }
                    }
                }
                barX += width;
                for (int wi = 0; wi < width; wi++)
                {
                    for (barY = 0; barY < (3 * size); barY++)
                    {
                        bmp.SetPixel(barX + wi, barY, background);
                    }
                }
            }
            return bmp;
        }
#endif
    }
}
