using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.exceptions;
using iTextSharp.text.log;

//using System.util;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Kevin Day, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
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

namespace iTextSharp.text.pdf.parser {

    /**
     * Utility methods to help with processing of inline images
     * @since 5.0.4
     */
    public static class InlineImageUtils {
         private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(InlineImageUtils));

        /**
         * Simple class in case users need to differentiate an exception from processing
         * inline images vs other exceptions 
         * @since 5.0.4
         */
		[Serializable]
        public class InlineImageParseException : IOException {
            public InlineImageParseException(String message) : base(message) {
            }

			protected InlineImageParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        
        /**
         * Map between key abbreviations allowed in dictionary of inline images and their
         * equivalent image dictionary keys
         */
        private static IDictionary<PdfName, PdfName> inlineImageEntryAbbreviationMap;
        /**
         * Map between value abbreviations allowed in dictionary of inline images for COLORSPACE
         */
        private static IDictionary<PdfName, PdfName> inlineImageColorSpaceAbbreviationMap;
        /**
         * Map between value abbreviations allowed in dictionary of inline images for FILTER
         */
        private static IDictionary<PdfName, PdfName> inlineImageFilterAbbreviationMap;
        static InlineImageUtils() { // static initializer
            inlineImageEntryAbbreviationMap = new Dictionary<PdfName, PdfName>();

            // allowed entries - just pass these through
            inlineImageEntryAbbreviationMap[PdfName.BITSPERCOMPONENT] = PdfName.BITSPERCOMPONENT;
            inlineImageEntryAbbreviationMap[PdfName.COLORSPACE] = PdfName.COLORSPACE;
            inlineImageEntryAbbreviationMap[PdfName.DECODE] = PdfName.DECODE;
            inlineImageEntryAbbreviationMap[PdfName.DECODEPARMS] = PdfName.DECODEPARMS;
            inlineImageEntryAbbreviationMap[PdfName.FILTER] = PdfName.FILTER;
            inlineImageEntryAbbreviationMap[PdfName.HEIGHT] = PdfName.HEIGHT;
            inlineImageEntryAbbreviationMap[PdfName.IMAGEMASK] = PdfName.IMAGEMASK;
            inlineImageEntryAbbreviationMap[PdfName.INTENT] = PdfName.INTENT;
            inlineImageEntryAbbreviationMap[PdfName.INTERPOLATE] = PdfName.INTERPOLATE;
            inlineImageEntryAbbreviationMap[PdfName.WIDTH] = PdfName.WIDTH;

            // abbreviations - transform these to corresponding correct values
            inlineImageEntryAbbreviationMap[new PdfName("BPC")] = PdfName.BITSPERCOMPONENT;
            inlineImageEntryAbbreviationMap[new PdfName("CS")] = PdfName.COLORSPACE;
            inlineImageEntryAbbreviationMap[new PdfName("D")] = PdfName.DECODE;
            inlineImageEntryAbbreviationMap[new PdfName("DP")] = PdfName.DECODEPARMS;
            inlineImageEntryAbbreviationMap[new PdfName("F")] = PdfName.FILTER;
            inlineImageEntryAbbreviationMap[new PdfName("H")] = PdfName.HEIGHT;
            inlineImageEntryAbbreviationMap[new PdfName("IM")] = PdfName.IMAGEMASK;
            inlineImageEntryAbbreviationMap[new PdfName("I")] = PdfName.INTERPOLATE;
            inlineImageEntryAbbreviationMap[new PdfName("W")] = PdfName.WIDTH;

            inlineImageColorSpaceAbbreviationMap = new Dictionary<PdfName, PdfName>();
            
            inlineImageColorSpaceAbbreviationMap[new PdfName("G")] = PdfName.DEVICEGRAY;
            inlineImageColorSpaceAbbreviationMap[new PdfName("RGB")] = PdfName.DEVICERGB;
            inlineImageColorSpaceAbbreviationMap[new PdfName("CMYK")] = PdfName.DEVICECMYK;
            inlineImageColorSpaceAbbreviationMap[new PdfName("I")] = PdfName.INDEXED;

            inlineImageFilterAbbreviationMap = new Dictionary<PdfName, PdfName>();
            
            inlineImageFilterAbbreviationMap[new PdfName("AHx")] = PdfName.ASCIIHEXDECODE;
            inlineImageFilterAbbreviationMap[new PdfName("A85")] = PdfName.ASCII85DECODE;
            inlineImageFilterAbbreviationMap[new PdfName("LZW")] = PdfName.LZWDECODE;
            inlineImageFilterAbbreviationMap[new PdfName("Fl")] = PdfName.FLATEDECODE;
            inlineImageFilterAbbreviationMap[new PdfName("RL")] = PdfName.RUNLENGTHDECODE;
            inlineImageFilterAbbreviationMap[new PdfName("CCF")] = PdfName.CCITTFAXDECODE;
            inlineImageFilterAbbreviationMap[new PdfName("DCT")] = PdfName.DCTDECODE;
        }
        
        /**
         * Parses an inline image from the provided content parser.  The parser must be positioned immediately following the BI operator in the content stream.
         * The parser will be left with current position immediately following the EI operator that terminates the inline image
         * @param ps the content parser to use for reading the image. 
         * @return the parsed image
         * @throws IOException if anything goes wring with the parsing
         * @throws InlineImageParseException if parsing of the inline image failed due to issues specific to inline image processing
         */
        public static InlineImageInfo ParseInlineImage(PdfContentParser ps, PdfDictionary colorSpaceDic) {
            PdfDictionary inlineImageDictionary = ParseInlineImageDictionary(ps);
            byte[] samples = ParseInlineImageSamples(inlineImageDictionary, colorSpaceDic, ps);
            return new InlineImageInfo(samples, inlineImageDictionary);
        }
        
        /**
         * Parses the next inline image dictionary from the parser.  The parser must be positioned immediately following the EI operator.
         * The parser will be left with position immediately following the whitespace character that follows the ID operator that ends the inline image dictionary.
         * @param ps the parser to extract the embedded image information from
         * @return the dictionary for the inline image, with any abbreviations converted to regular image dictionary keys and values
         * @throws IOException if the parse fails
         */
        private static PdfDictionary ParseInlineImageDictionary(PdfContentParser ps) {
            // by the time we get to here, we have already parsed the BI operator
            PdfDictionary dictionary = new PdfDictionary();
            
            for (PdfObject key = ps.ReadPRObject(); key != null && !"ID".Equals(key.ToString()); key = ps.ReadPRObject()){
                PdfObject value = ps.ReadPRObject();

                PdfName resolvedKey;
                inlineImageEntryAbbreviationMap.TryGetValue((PdfName)key, out resolvedKey);
                if (resolvedKey == null)
                    resolvedKey = (PdfName)key;

                dictionary.Put(resolvedKey, GetAlternateValue(resolvedKey, value));
            }

            int ch = ps.GetTokeniser().Read();
            if (!PRTokeniser.IsWhitespace(ch))
                throw new IOException("Unexpected character " + ch + " found after ID in inline image");
            
            return dictionary;
        }
        
        /**
         * Transforms value abbreviations into their corresponding real value 
         * @param key the key that the value is for
         * @param value the value that might be an abbreviation
         * @return if value is an allowed abbreviation for the key, the expanded value for that abbreviation.  Otherwise, value is returned without modification 
         */
        private static PdfObject GetAlternateValue(PdfName key, PdfObject value){
            if (key.Equals(PdfName.FILTER)){
                if (value is PdfName){
                    PdfName altValue;
                    inlineImageFilterAbbreviationMap.TryGetValue((PdfName)value, out altValue);
                    if (altValue != null)
                        return altValue;
                } else if (value is PdfArray){
                    PdfArray array = ((PdfArray)value);
                    PdfArray altArray = new PdfArray();
                    int count = array.Size;
                    for (int i = 0; i < count; i++){
                        altArray.Add(GetAlternateValue(key, array[i]));
                    }
                    return altArray;
                }
            } else if (key.Equals(PdfName.COLORSPACE)){
                if (value is PdfName){
                    PdfName altValue;
                    inlineImageColorSpaceAbbreviationMap.TryGetValue((PdfName)value, out altValue);
                    if (altValue != null)
                        return altValue;
                }
            }
            
            return value;
        }
        
        /**
         * @param colorSpaceName the name of the color space. If null, a bi-tonal (black and white) color space is assumed.
         * @return the components per pixel for the specified color space
         */
        private static int GetComponentsPerPixel(PdfName colorSpaceName, PdfDictionary colorSpaceDic){
            if (colorSpaceName == null)
                return 1;
            if (colorSpaceName.Equals(PdfName.DEVICEGRAY))
                return 1;
            if (colorSpaceName.Equals(PdfName.DEVICERGB))
                return 3;
            if (colorSpaceName.Equals(PdfName.DEVICECMYK))
                return 4;
            
            if (colorSpaceDic != null){
                PdfArray colorSpace = colorSpaceDic.GetAsArray(colorSpaceName);
                if (colorSpace != null){
                    if (PdfName.INDEXED.Equals(colorSpace.GetAsName(0))){
                        return 1;
                    }
                }
                else {
                    PdfName tempName = colorSpaceDic.GetAsName(colorSpaceName);
                    if(tempName != null) {
                        return GetComponentsPerPixel(tempName, colorSpaceDic);
                    }
                }
            }

            throw new ArgumentException("Unexpected color space " + colorSpaceName);
        }
        
        /**
         * Computes the number of unfiltered bytes that each row of the image will contain.
         * If the number of bytes results in a partial terminating byte, this number is rounded up
         * per the PDF specification
         * @param imageDictionary the dictionary of the inline image
         * @return the number of bytes per row of the image
         */
        private static int ComputeBytesPerRow(PdfDictionary imageDictionary, PdfDictionary colorSpaceDic){
            PdfNumber wObj = imageDictionary.GetAsNumber(PdfName.WIDTH);
            PdfNumber bpcObj = imageDictionary.GetAsNumber(PdfName.BITSPERCOMPONENT);
            int cpp = GetComponentsPerPixel(imageDictionary.GetAsName(PdfName.COLORSPACE), colorSpaceDic);
            
            int w = wObj.IntValue;
            int bpc = bpcObj != null ? bpcObj.IntValue : 1;
            
            
            int bytesPerRow = (w * bpc * cpp + 7) / 8;
            
            return bytesPerRow;
        }
        
        /**
         * Parses the samples of the image from the underlying content parser, ignoring all filters.
         * The parser must be positioned immediately after the ID operator that ends the inline image's dictionary.
         * The parser will be left positioned immediately following the EI operator.
         * This is primarily useful if no filters have been applied. 
         * @param imageDictionary the dictionary of the inline image
         * @param ps the content parser
         * @return the samples of the image
         * @throws IOException if anything bad happens during parsing
         */
        private static byte[] ParseUnfilteredSamples(PdfDictionary imageDictionary, PdfDictionary colorSpaceDic, PdfContentParser ps) {
            // special case:  when no filter is specified, we just read the number of bits
            // per component, multiplied by the width and height.
            if (imageDictionary.Contains(PdfName.FILTER))
                throw new ArgumentException("Dictionary contains filters");
            
            PdfNumber h = imageDictionary.GetAsNumber(PdfName.HEIGHT);
            
            int bytesToRead = ComputeBytesPerRow(imageDictionary, colorSpaceDic) * h.IntValue;
            byte[] bytes = new byte[bytesToRead];
            PRTokeniser tokeniser = ps.GetTokeniser();
            
            int shouldBeWhiteSpace = tokeniser.Read(); // skip next character (which better be a whitespace character - I suppose we could check for this)
            // from the PDF spec:  Unless the image uses ASCIIHexDecode or ASCII85Decode as one of its filters, the ID operator shall be followed by a single white-space character, and the next character shall be interpreted as the first byte of image data.
            // unfortunately, we've seen some PDFs where there is no space following the ID, so we have to capture this case and handle it
            int startIndex = 0;
            if (!PRTokeniser.IsWhitespace(shouldBeWhiteSpace) || shouldBeWhiteSpace == 0){ // tokeniser treats 0 as whitespace, but for our purposes, we shouldn't)
                bytes[0] = (byte)shouldBeWhiteSpace;
                startIndex++;
            }
            for (int i = startIndex; i < bytesToRead; i++){
                int ch = tokeniser.Read();
                if (ch == -1)
                    throw new InlineImageParseException("End of content stream reached before end of image data");
                
                bytes[i] = (byte)ch;
            }
            PdfObject ei = ps.ReadPRObject();
            if(!ei.ToString().Equals("EI")) {
                // Some PDF producers seem to add another non-whitespace character after the image data.
                // Let's try to handle that case here.
                PdfObject ei2 = ps.ReadPRObject();
                if(!ei2.ToString().Equals("EI"))
                    throw new InlineImageParseException("EI not found after end of image data");
            }
            
            return bytes;
        }
        
        /**
         * Parses the samples of the image from the underlying content parser, accounting for filters
         * The parser must be positioned immediately after the ID operator that ends the inline image's dictionary.
         * The parser will be left positioned immediately following the EI operator.
         * <b>Note:</b>This implementation does not actually apply the filters at this time
         * @param imageDictionary the dictionary of the inline image
         * @param ps the content parser
         * @return the samples of the image
         * @throws IOException if anything bad happens during parsing
         */
        private static byte[] ParseInlineImageSamples(PdfDictionary imageDictionary, PdfDictionary colorSpaceDic, PdfContentParser ps) {
            // by the time we get to here, we have already parsed the ID operator
            
            if (!imageDictionary.Contains(PdfName.FILTER)){
                return ParseUnfilteredSamples(imageDictionary, colorSpaceDic, ps);
            }
            
            
            // read all content until we reach an EI operator surrounded by whitespace.
            // The following algorithm has two potential issues: what if the image stream 
            // contains <ws>EI<ws> ?
            // Plus, there are some streams that don't have the <ws> before the EI operator
            // it sounds like we would have to actually decode the content stream, which
            // I'd rather avoid right now.
            MemoryStream baos = new MemoryStream();
            MemoryStream accumulated = new MemoryStream();
            int ch;
            int found = 0;
            PRTokeniser tokeniser = ps.GetTokeniser();
            byte[] ff = null;
            
            while ((ch = tokeniser.Read()) != -1){
                if (found == 0 && PRTokeniser.IsWhitespace(ch)){
                    found++;
                    accumulated.WriteByte((byte)ch);
                } else if (found == 1 && ch == 'E'){
                    found++;
                    accumulated.WriteByte((byte)ch);
                } else if (found == 1 && PRTokeniser.IsWhitespace(ch)){
                    // this clause is needed if we have a white space character that is part of the image data
                    // followed by a whitespace character that precedes the EI operator.  In this case, we need
                    // to flush the first whitespace, then treat the current whitespace as the first potential
                    // character for the end of stream check.  Note that we don't increment 'found' here.
                    baos.Write(ff = accumulated.ToArray(), 0, ff.Length);
                    accumulated.SetLength(0);
                    accumulated.WriteByte((byte)ch);
                } else if (found == 2 && ch == 'I'){ 
                    found++;
                    accumulated.WriteByte((byte)ch);
                } else if (found == 3 && PRTokeniser.IsWhitespace(ch)){
                    byte[] tmp = baos.ToArray();
                    if (InlineImageStreamBytesAreComplete(tmp, imageDictionary)) {
                        return tmp;
                    }
                    byte[] accumulatedArr = accumulated.ToArray();
                    baos.Write(accumulatedArr, 0, accumulatedArr.Length);
                    accumulated.SetLength(0);

                    baos.WriteByte((byte) ch);
                    found = 0;

                } else {
                    baos.Write(ff = accumulated.ToArray(), 0, ff.Length);
                    accumulated.SetLength(0);
                    
                    baos.WriteByte((byte)ch);
                    found = 0;
                }
            }
            throw new InlineImageParseException("Could not find image data or EI");
        }

        private static bool InlineImageStreamBytesAreComplete(byte[] samples, PdfDictionary imageDictionary) {
            try {
                PdfReader.DecodeBytes(samples, imageDictionary, FilterHandlers.GetDefaultFilterHandlers());
                return true;
            } catch (UnsupportedPdfException e) {
                LOGGER.Warn(e.Message);
                return true;
            } catch (IOException e) {
                return false;
            } 
        }
    }
}
