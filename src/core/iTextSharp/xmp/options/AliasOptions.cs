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
    /// Options for XMPSchemaRegistryImpl#registerAlias.
    /// 
    /// @since 20.02.2006
    /// </summary>
    public sealed class AliasOptions : XmpOptions {
        /// <summary>
        /// This is a direct mapping. The actual data type does not matter. </summary>
        public const uint PROP_DIRECT = 0;

        /// <summary>
        /// The actual is an unordered array, the alias is to the first element of the array. </summary>
        public const uint PROP_ARRAY = PropertyOptions.ARRAY;

        /// <summary>
        /// The actual is an ordered array, the alias is to the first element of the array. </summary>
        public const uint PROP_ARRAY_ORDERED = PropertyOptions.ARRAY_ORDERED;

        /// <summary>
        /// The actual is an alternate array, the alias is to the first element of the array. </summary>
        public const uint PROP_ARRAY_ALTERNATE = PropertyOptions.ARRAY_ALTERNATE;

        /// <summary>
        /// The actual is an alternate text array, the alias is to the 'x-default' element of the array.
        /// </summary>
        public const uint PROP_ARRAY_ALT_TEXT = PropertyOptions.ARRAY_ALT_TEXT;


        /// <seealso cref= Options#Options() </seealso>
        public AliasOptions() {
            // EMPTY
        }


        /// <param name="options"> the options to init with </param>
        /// <exception cref="XmpException"> If options are not consistant </exception>
        public AliasOptions(uint options)
            : base(options) {
        }


        /// <returns> Returns if the alias is of the simple form. </returns>
        public bool Simple {
            get { return Options == PROP_DIRECT; }
        }


        /// <returns> Returns the option. </returns>
        public bool Array {
            get { return GetOption(PROP_ARRAY); }
            set { SetOption(PROP_ARRAY, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool ArrayOrdered {
            get { return GetOption(PROP_ARRAY_ORDERED); }
            set { SetOption(PROP_ARRAY | PROP_ARRAY_ORDERED, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool ArrayAlternate {
            get { return GetOption(PROP_ARRAY_ALTERNATE); }
            set { SetOption(PROP_ARRAY | PROP_ARRAY_ORDERED | PROP_ARRAY_ALTERNATE, value); }
        }


        /// <returns> Returns the option. </returns>
        public bool ArrayAltText {
            get { return GetOption(PROP_ARRAY_ALT_TEXT); }
            set { SetOption(PROP_ARRAY | PROP_ARRAY_ORDERED | PROP_ARRAY_ALTERNATE | PROP_ARRAY_ALT_TEXT, value); }
        }

        /// <seealso cref= Options#getValidOptions() </seealso>
        protected internal override uint ValidOptions {
            get { return PROP_DIRECT | PROP_ARRAY | PROP_ARRAY_ORDERED | PROP_ARRAY_ALTERNATE | PROP_ARRAY_ALT_TEXT; }
        }


        /// <returns> returns a <seealso cref="PropertyOptions"/>s object </returns>
        /// <exception cref="XmpException"> If the options are not consistant.  </exception>
        public PropertyOptions ToPropertyOptions() {
            return new PropertyOptions(Options);
        }


        /// <seealso cref= Options#defineOptionName(int) </seealso>
        protected internal override string DefineOptionName(uint option) {
            switch (option) {
                case PROP_DIRECT:
                    return "PROP_DIRECT";
                case PROP_ARRAY:
                    return "ARRAY";
                case PROP_ARRAY_ORDERED:
                    return "ARRAY_ORDERED";
                case PROP_ARRAY_ALTERNATE:
                    return "ARRAY_ALTERNATE";
                case PROP_ARRAY_ALT_TEXT:
                    return "ARRAY_ALT_TEXT";
                default:
                    return null;
            }
        }
    }
}
