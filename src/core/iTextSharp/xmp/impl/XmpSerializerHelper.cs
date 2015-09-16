//Copyright (c) 2006, Adobe Systems Incorporated
//All rights reserved.
//
//        Redistribution and use in source and binary forms, with or without
//        modification, are permitted provided that the following conditions are met:
//        1. Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//        2. Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        3. All advertising materials mentioning features or use of this software
//        must display the following acknowledgement:
//        This product includes software developed by the Adobe Systems Incorporated.
//        4. Neither the name of the Adobe Systems Incorporated nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
//
//        THIS SOFTWARE IS PROVIDED BY ADOBE SYSTEMS INCORPORATED ''AS IS'' AND ANY
//        EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//        DISCLAIMED. IN NO EVENT SHALL ADOBE SYSTEMS INCORPORATED BE LIABLE FOR ANY
//        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//        http://www.adobe.com/devnet/xmp/library/eula-xmp-library-java.html

using System;
using System.IO;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.text.xml.xmp;
using iTextSharp.xmp.options;

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// Serializes the <code>XMPMeta</code>-object to an <code>OutputStream</code> according to the
    /// <code>SerializeOptions</code>. 
    /// 
    /// @since   11.07.2006
    /// </summary>
    public class XmpSerializerHelper {
        /// <summary>
        /// Static method to Serialize the metadata object. For each serialisation, a new XMPSerializer
        /// instance is created, either XMPSerializerRDF or XMPSerializerPlain so thats its possible to 
        /// serialialize the same XMPMeta objects in two threads.
        /// </summary>
        /// <param name="xmp"> a metadata implementation object </param>
        /// <param name="out"> the output stream to Serialize to </param>
        /// <param name="options"> serialization options, can be <code>null</code> for default. </param>
        /// <exception cref="XmpException"> </exception>
        public static void Serialize(XmpMetaImpl xmp, Stream @out, SerializeOptions options) {
            options = options ?? new SerializeOptions();

            // sort the internal data model on demand
            if (options.Sort) {
                xmp.Sort();
            }
            (new XmpSerializerRdf()).Serialize(xmp, @out, options);
        }


        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into a string.
        /// <em>Note:</em> Encoding is forced to UTF-16 when serializing to a
        /// string to ensure the correctness of &quot;exact packet size&quot;.
        /// </summary>
        /// <param name="xmp"> a metadata implementation object </param>
        /// <param name="options"> Options to control the serialization (see
        ///            <seealso cref="SerializeOptions"/>). </param>
        /// <returns> Returns a string containing the serialized RDF. </returns>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static string SerializeToString(XmpMetaImpl xmp, SerializeOptions options) {
            // forces the encoding to be UTF-16 to get the correct string length
            options = options ?? new SerializeOptions();
            options.EncodeUtf16Be = true;

            MemoryStream @out = new MemoryStream(2048);
            Serialize(xmp, @out, options);

            try {
                return new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(options.Encoding)).GetString(@out.GetBuffer());
            }
            catch (Exception) {
                // cannot happen as UTF-8/16LE/BE is required to be implemented in
                // Java
                return GetString(@out.GetBuffer());
            }
        }


        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into a byte buffer.
        /// </summary>
        /// <param name="xmp"> a metadata implementation object </param>
        /// <param name="options"> Options to control the serialization (see <seealso cref="SerializeOptions"/>). </param>
        /// <returns> Returns a byte buffer containing the serialized RDF. </returns>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static byte[] SerializeToBuffer(XmpMetaImpl xmp, SerializeOptions options) {
            MemoryStream @out = new MemoryStream(2048);
            Serialize(xmp, @out, options);
            return @out.GetBuffer();
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
