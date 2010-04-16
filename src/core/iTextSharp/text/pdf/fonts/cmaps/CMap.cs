using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.error_messages;
/**
 * Copyright (c) 2005, www.fontbox.org
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of fontbox; nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * http://www.fontbox.org
 *
 */
namespace iTextSharp.text.pdf.fonts.cmaps {

    /**
     * This class represents a CMap file.
     *
     * @author Ben Litchfield (ben@benlitchfield.com)
     * @since   2.1.4
     */
    public class CMap
    {
        private IList<CodespaceRange> codeSpaceRanges = new List<CodespaceRange>();
        private IDictionary<int, String> singleByteMappings = new Dictionary<int, String>();
        private IDictionary<int, String> doubleByteMappings = new Dictionary<int, String>();

        /**
         * Creates a new instance of CMap.
         */
        public CMap()
        {
            //default constructor
        }

        /**
         * This will tell if this cmap has any one byte mappings.
         *
         * @return true If there are any one byte mappings, false otherwise.
         */
        public bool HasOneByteMappings()
        {
            return singleByteMappings.Count != 0;
        }

        /**
         * This will tell if this cmap has any two byte mappings.
         *
         * @return true If there are any two byte mappings, false otherwise.
         */
        public bool HasTwoByteMappings()
        {
            return doubleByteMappings.Count != 0;
        }

        /**
         * This will perform a lookup into the map.
         *
         * @param code The code used to lookup.
         * @param offset The offset into the byte array.
         * @param length The length of the data we are getting.
         *
         * @return The string that matches the lookup.
         */
        public String Lookup( byte[] code, int offset, int length )
        {

            String result = null;
            int key = 0;
            if ( length == 1 )
            {

                key = code[offset] & 0xff;
                singleByteMappings.TryGetValue(key, out result);
            }
            else if ( length == 2 )
            {
                int intKey = code[offset] & 0xff;
                intKey <<= 8;
                intKey += code[offset+1] & 0xff;

                doubleByteMappings.TryGetValue( intKey, out result );
            }

            return result;
        }

        /**
         * This will add a mapping.
         *
         * @param src The src to the mapping.
         * @param dest The dest to the mapping.
         *
         * @throws IOException if the src is invalid.
         */
        public void AddMapping( byte[] src, String dest )
        {
            if ( src.Length == 1 )
            {
                singleByteMappings[src[0] & 0xff] = dest ;
            }
            else if ( src.Length == 2 )
            {
                int intSrc = src[0]&0xFF;
                intSrc <<= 8;
                intSrc |= src[1]&0xFF;
                doubleByteMappings[intSrc] = dest;
            }
            else
            {
                throw new IOException(MessageLocalization.GetComposedMessage("mapping.code.should.be.1.or.two.bytes.and.not.1", src.Length));
            }
        }


        /**
         * This will add a codespace range.
         *
         * @param range A single codespace range.
         */
        public void AddCodespaceRange( CodespaceRange range )
        {
            codeSpaceRanges.Add( range );
        }

        /**
         * Getter for property codeSpaceRanges.
         *
         * @return Value of property codeSpaceRanges.
         */
        public IList<CodespaceRange> GetCodeSpaceRanges()
        {
            return codeSpaceRanges;
        }

    }
}