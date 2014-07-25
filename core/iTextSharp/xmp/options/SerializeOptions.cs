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

namespace iTextSharp.xmp.options {
    using XmpException = XmpException;


    /// <summary>
    /// Options for <seealso cref="XmpMetaFactory.SerializeToBuffer(IXmpMeta, SerializeOptions)"/>.
    /// 
    /// @since 24.01.2006
    /// </summary>
    public sealed class SerializeOptions : XmpOptions {
        /// <summary>
        /// Omit the XML packet wrapper. </summary>
        public const uint OMIT_PACKET_WRAPPER = 0x0010;

        /// <summary>
        /// Mark packet as read-only. Default is a writeable packet. </summary>
        public const uint READONLY_PACKET = 0x0020;

        /// <summary>
        /// Use a compact form of RDF.
        /// The compact form is the default serialization format (this flag is technically ignored).
        /// To Serialize to the canonical form, set the flag USE_CANONICAL_FORMAT.
        /// If both flags &quot;compact&quot; and &quot;canonical&quot; are set, canonical is used.
        /// </summary>
        public const uint USE_COMPACT_FORMAT = 0x0040;

        /// <summary>
        /// Use the canonical form of RDF if set. By default the compact form is used </summary>
        public const uint USE_CANONICAL_FORMAT = 0x0080;

        /// <summary>
        /// Include a padding allowance for a thumbnail image. If no <tt>xmp:Thumbnails</tt> property
        /// is present, the typical space for a JPEG thumbnail is used.
        /// </summary>
        public const uint INCLUDE_THUMBNAIL_PAD = 0x0100;

        /// <summary>
        /// The padding parameter provides the overall packet length. The actual amount of padding is
        /// computed. An exception is thrown if the packet exceeds this length with no padding.
        /// </summary>
        public const uint EXACT_PACKET_LENGTH = 0x0200;

        /// <summary>
        /// Omit the &lt;x:xmpmeta&bt;-tag </summary>
        public const uint OMIT_XMPMETA_ELEMENT = 0x1000;

        /// <summary>
        /// Sort the struct properties and qualifier before serializing </summary>
        public const uint SORT = 0x2000;

        // ---------------------------------------------------------------------------------------------
        // encoding bit constants

        /// <summary>
        /// Bit indicating little endian encoding, unset is big endian </summary>
        private const uint LITTLEENDIAN_BIT = 0x0001;

        /// <summary>
        /// Bit indication UTF16 encoding. </summary>
        private const uint UTF16_BIT = 0x0002;

        /// <summary>
        /// UTF8 encoding; this is the default </summary>
        public const uint ENCODE_UTF8 = 0;

        /// <summary>
        /// UTF16BE encoding </summary>
        public const uint ENCODE_UTF16BE = UTF16_BIT;

        /// <summary>
        /// UTF16LE encoding </summary>
        public const uint ENCODE_UTF16LE = UTF16_BIT | LITTLEENDIAN_BIT;

        private const uint ENCODING_MASK = UTF16_BIT | LITTLEENDIAN_BIT;

        /// <summary>
        /// The number of levels of indentation to be used for the outermost XML element in the
        /// serialized RDF. This is convenient when embedding the RDF in other text, defaults to 0.
        /// </summary>
        private int _baseIndent;

        /// <summary>
        /// The string to be used for each level of indentation in the serialized
        /// RDF. If empty it defaults to two ASCII spaces, U+0020.
        /// </summary>
        private string _indent = "  ";

        /// <summary>
        /// The string to be used as a line terminator. If empty it defaults to; linefeed, U+000A, the
        /// standard XML newline.
        /// </summary>
        private string _newline = "\n";

        /// <summary>
        /// Omits the Toolkit version attribute, not published, only used for Unit tests. </summary>
        private bool _omitVersionAttribute;

        /// <summary>
        /// The amount of padding to be added if a writeable XML packet is created. If zero is passed
        /// (the default) an appropriate amount of padding is computed.
        /// </summary>
        private int _padding = 2048;


        /// <summary>
        /// Default constructor.
        /// </summary>
        public SerializeOptions() {
            // reveal default constructor
        }


        /// <summary>
        /// Constructor using inital options </summary>
        /// <param name="options"> the inital options </param>
        /// <exception cref="XmpException"> Thrown if options are not consistant. </exception>
        public SerializeOptions(uint options)
            : base(options) {
        }


        /// <returns> Returns the option. </returns>
        public bool OmitPacketWrapper {
            get { return GetOption(OMIT_PACKET_WRAPPER); }
            set { SetOption(OMIT_PACKET_WRAPPER, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool OmitXmpMetaElement {
            get { return GetOption(OMIT_XMPMETA_ELEMENT); }
            set { SetOption(OMIT_XMPMETA_ELEMENT, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool ReadOnlyPacket {
            get { return GetOption(READONLY_PACKET); }
            set { SetOption(READONLY_PACKET, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool UseCompactFormat {
            get { return GetOption(USE_COMPACT_FORMAT); }
            set { SetOption(USE_COMPACT_FORMAT, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool UseCanonicalFormat {
            get { return GetOption(USE_CANONICAL_FORMAT); }
            set { SetOption(USE_CANONICAL_FORMAT, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool IncludeThumbnailPad {
            get { return GetOption(INCLUDE_THUMBNAIL_PAD); }
            set { SetOption(INCLUDE_THUMBNAIL_PAD, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool ExactPacketLength {
            get { return GetOption(EXACT_PACKET_LENGTH); }
            set { SetOption(EXACT_PACKET_LENGTH, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool Sort {
            get { return GetOption(SORT); }
            set { SetOption(SORT, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool EncodeUtf16Be {
            get { return (Options & ENCODING_MASK) == ENCODE_UTF16BE; }
            set {
                // clear unicode bits
                SetOption(UTF16_BIT | LITTLEENDIAN_BIT, false);
                SetOption(ENCODE_UTF16BE, value);
            }
        }


        /// <returns> Returns the option. </returns>
        public bool EncodeUtf16Le {
            get { return (Options & ENCODING_MASK) == ENCODE_UTF16LE; }
            set {
                // clear unicode bits
                SetOption(UTF16_BIT | LITTLEENDIAN_BIT, false);
                SetOption(ENCODE_UTF16LE, value);
            }
        }


        /// <returns> Returns the baseIndent. </returns>
        public int BaseIndent {
            get { return _baseIndent; }
            set { _baseIndent = value; }
        }


        /// <returns> Returns the indent. </returns>
        public string Indent {
            get { return _indent; }
            set { _indent = value; }
        }


        /// <returns> Returns the newline. </returns>
        public string Newline {
            get { return _newline; }
            set { _newline = value; }
        }


        /// <returns> Returns the padding. </returns>
        public int Padding {
            get { return _padding; }
            set { _padding = value; }
        }


        /// <returns> Returns whether the Toolkit version attribute shall be omitted.
        /// <em>Note:</em> This options can only be set by unit tests. </returns>
        public bool OmitVersionAttribute {
            get { return _omitVersionAttribute; }
        }


        /// <returns> Returns the encoding as Java encoding String.  </returns>
        public string Encoding {
            get {
                if (EncodeUtf16Be) {
                    return "UTF-16BE";
                }
                if (EncodeUtf16Le) {
                    return "UTF-16LE";
                }
                return "UTF-8";
            }
        }

        /// <seealso cref= Options#getValidOptions() </seealso>
        protected internal override uint ValidOptions {
            get {
                return OMIT_PACKET_WRAPPER | READONLY_PACKET | USE_COMPACT_FORMAT | INCLUDE_THUMBNAIL_PAD |
                       OMIT_XMPMETA_ELEMENT | EXACT_PACKET_LENGTH | SORT;
                //		USE_CANONICAL_FORMAT |
            }
        }


        /// 
        /// <returns> Returns clone of this SerializeOptions-object with the same options set. </returns>
        public object Clone() {
            try {
                SerializeOptions clone = new SerializeOptions(Options);
                clone.BaseIndent = _baseIndent;
                clone.Indent = _indent;
                clone.Newline = _newline;
                clone.Padding = _padding;
                return clone;
            }
            catch (XmpException) {
                // This cannot happen, the options are already checked in "this" object.
                return null;
            }
        }


        /// <seealso cref= Options#defineOptionName(int) </seealso>
        protected internal override string DefineOptionName(uint option) {
            switch (option) {
                case OMIT_PACKET_WRAPPER:
                    return "OMIT_PACKET_WRAPPER";
                case READONLY_PACKET:
                    return "READONLY_PACKET";
                case USE_COMPACT_FORMAT:
                    return "USE_COMPACT_FORMAT";
                    //			case USE_CANONICAL_FORMAT :		return "USE_CANONICAL_FORMAT";
                case INCLUDE_THUMBNAIL_PAD:
                    return "INCLUDE_THUMBNAIL_PAD";
                case EXACT_PACKET_LENGTH:
                    return "EXACT_PACKET_LENGTH";
                case OMIT_XMPMETA_ELEMENT:
                    return "OMIT_XMPMETA_ELEMENT";
                case SORT:
                    return "NORMALIZED";
                default:
                    return null;
            }
        }
    }
}
