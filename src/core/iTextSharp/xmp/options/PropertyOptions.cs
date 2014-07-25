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
    using XMPError = XmpError;
    using XmpException = XmpException;


    /// <summary>
    /// The property flags are used when properties are fetched from the <code>XMPMeta</code>-object
    /// and provide more detailed information about the property.
    /// 
    /// @since   03.07.2006
    /// </summary>
    public sealed class PropertyOptions : XmpOptions {
        public const uint NO_OPTIONS = 0x00000000;
        public const uint URI = 0x00000002;
        public const uint HAS_QUALIFIERS = 0x00000010;
        public const uint QUALIFIER = 0x00000020;
        public const uint HAS_LANGUAGE = 0x00000040;
        public const uint HAS_TYPE = 0x00000080;
        public const uint STRUCT = 0x00000100;
        public const uint ARRAY = 0x00000200;
        public const uint ARRAY_ORDERED = 0x00000400;
        public const uint ARRAY_ALTERNATE = 0x00000800;
        public const uint ARRAY_ALT_TEXT = 0x00001000;
        public const uint SCHEMA_NODE = 0x80000000;

        /// <summary>
        /// may be used in the future </summary>
        public const uint DELETE_EXISTING = 0x20000000;
        /// <summary>
        /// Updated by iText. Indicates if the property should be writted as a separate node
        /// </summary>
        public const int SEPARATE_NODE = 0x40000000;


        /// <summary>
        /// Default constructor
        /// </summary>
        public PropertyOptions() {
            // reveal default constructor
        }


        /// <summary>
        /// Intialization constructor
        /// </summary>
        /// <param name="options"> the initialization options </param>
        /// <exception cref="XmpException"> If the options are not valid  </exception>
        public PropertyOptions(uint options)
            : base(options) {
        }


        /// <returns> Return whether the property value is a URI. It is serialized to RDF using the
        ///         <tt>rdf:resource</tt> attribute. Not mandatory for URIs, but considered RDF-savvy. </returns>
        public bool Uri {
            get { return GetOption(URI); }
            set { SetOption(URI, value); }
        }


        /// <returns> Return whether the property has qualifiers. These could be an <tt>xml:lang</tt>
        ///         attribute, an <tt>rdf:type</tt> property, or a general qualifier. See the
        ///         introductory discussion of qualified properties for more information. </returns>
        public bool HasQualifiers {
            get { return GetOption(HAS_QUALIFIERS); }
            set { SetOption(HAS_QUALIFIERS, value); }
        }


        /// <returns> Return whether this property is a qualifier for some other property. Note that if the
        ///         qualifier itself has a structured value, this flag is only set for the top node of
        ///         the qualifier's subtree. Qualifiers may have arbitrary structure, and may even have
        ///         qualifiers. </returns>
        public bool Qualifier {
            get { return GetOption(QUALIFIER); }
            set { SetOption(QUALIFIER, value); }
        }


        /// <returns> Return whether this property has an <tt>xml:lang</tt> qualifier. </returns>
        public bool HasLanguage {
            get { return GetOption(HAS_LANGUAGE); }
            set { SetOption(HAS_LANGUAGE, value); }
        }


        /// <returns> Return whether this property has an <tt>rdf:type</tt> qualifier. </returns>
        public bool HasType {
            get { return GetOption(HAS_TYPE); }
            set { SetOption(HAS_TYPE, value); }
        }


        /// <returns> Return whether this property contains nested fields. </returns>
        public bool Struct {
            get { return GetOption(STRUCT); }
            set { SetOption(STRUCT, value); }
        }


        /// <returns> Return whether this property is an array. By itself this indicates a general
        ///         unordered array. It is serialized using an <tt>rdf:Bag</tt> container. </returns>
        public bool Array {
            get { return GetOption(ARRAY); }
            set { SetOption(ARRAY, value); }
        }


        /// <returns> Return whether this property is an ordered array. Appears in conjunction with
        ///         getPropValueIsArray(). It is serialized using an <tt>rdf:Seq</tt> container. </returns>
        public bool ArrayOrdered {
            get { return GetOption(ARRAY_ORDERED); }
            set { SetOption(ARRAY_ORDERED, value); }
        }


        /// <returns> Return whether this property is an alternative array. Appears in conjunction with
        ///         getPropValueIsArray(). It is serialized using an <tt>rdf:Alt</tt> container. </returns>
        public bool ArrayAlternate {
            get { return GetOption(ARRAY_ALTERNATE); }
            set { SetOption(ARRAY_ALTERNATE, value); }
        }


        /// <returns> Return whether this property is an alt-text array. Appears in conjunction with
        ///         getPropArrayIsAlternate(). It is serialized using an <tt>rdf:Alt</tt> container.
        ///         Each array element is a simple property with an <tt>xml:lang</tt> attribute. </returns>
        public bool ArrayAltText {
            get { return GetOption(ARRAY_ALT_TEXT); }
            set { SetOption(ARRAY_ALT_TEXT, value); }
        }


        /// <param name="value"> the value to set </param>
        /// <returns> Returns this to enable cascaded options. </returns>
        /// <returns> Returns whether the SCHEMA_NODE option is set. </returns>
        public bool SchemaNode {
            get { return GetOption(SCHEMA_NODE); }
            set { SetOption(SCHEMA_NODE, value); }
        }


        //-------------------------------------------------------------------------- convenience methods

        /// <returns> Returns whether the property is of composite type - an array or a struct. </returns>
        public bool CompositeProperty {
            get { return (Options & (ARRAY | STRUCT)) > 0; }
        }


        /// <returns> Returns whether the property is of composite type - an array or a struct. </returns>
        public bool Simple {
            get { return (Options & (ARRAY | STRUCT)) == 0; }
        }


        /// <returns> Returns true if only array options are set. </returns>
        public bool OnlyArrayOptions {
            get { return (Options & ~(ARRAY | ARRAY_ORDERED | ARRAY_ALTERNATE | ARRAY_ALT_TEXT)) == 0; }
        }


        /// <seealso cref= Options#getValidOptions() </seealso>
        protected internal override uint ValidOptions {
            get {
                return URI | HAS_QUALIFIERS | QUALIFIER | HAS_LANGUAGE | HAS_TYPE | STRUCT | ARRAY | ARRAY_ORDERED |
                       ARRAY_ALTERNATE | ARRAY_ALT_TEXT | SCHEMA_NODE | SEPARATE_NODE;
            }
        }

        /// <summary>
        /// Compares two options set for array compatibility.
        /// </summary>
        /// <param name="options"> other options </param>
        /// <returns> Returns true if the array options of the sets are equal. </returns>
        public bool EqualArrayTypes(PropertyOptions options) {
            return Array == options.Array && ArrayOrdered == options.ArrayOrdered &&
                   ArrayAlternate == options.ArrayAlternate && ArrayAltText == options.ArrayAltText;
        }


        /// <summary>
        /// Merges the set options of a another options object with this.
        /// If the other options set is null, this objects stays the same. </summary>
        /// <param name="options"> other options </param>
        /// <exception cref="XmpException"> If illegal options are provided  </exception>
        public void MergeWith(PropertyOptions options) {
            if (options != null) {
                Options = Options | options.Options;
            }
        }


        /// <seealso cref= Options#defineOptionName(int) </seealso>
        protected internal override string DefineOptionName(uint option) {
            switch (option) {
                case URI:
                    return "URI";
                case HAS_QUALIFIERS:
                    return "HAS_QUALIFIER";
                case QUALIFIER:
                    return "QUALIFIER";
                case HAS_LANGUAGE:
                    return "HAS_LANGUAGE";
                case HAS_TYPE:
                    return "HAS_TYPE";
                case STRUCT:
                    return "STRUCT";
                case ARRAY:
                    return "ARRAY";
                case ARRAY_ORDERED:
                    return "ARRAY_ORDERED";
                case ARRAY_ALTERNATE:
                    return "ARRAY_ALTERNATE";
                case ARRAY_ALT_TEXT:
                    return "ARRAY_ALT_TEXT";
                case SCHEMA_NODE:
                    return "SCHEMA_NODE";
                default:
                    return null;
            }
        }


        /// <summary>
        /// Checks that a node not a struct and array at the same time;
        /// and URI cannot be a struct.
        /// </summary>
        /// <param name="options"> the bitmask to check. </param>
        /// <exception cref="XmpException"> Thrown if the options are not consistent. </exception>
        protected internal override void AssertConsistency(uint options) {
            if ((options & STRUCT) > 0 && (options & ARRAY) > 0) {
                throw new XmpException("IsStruct and IsArray options are mutually exclusive",
                                       XmpError.BADOPTIONS);
            }
            if ((options & URI) > 0 && (options & (ARRAY | STRUCT)) > 0) {
                throw new XmpException("Structs and arrays can't have \"value\" options", XmpError.BADOPTIONS);
            }
        }
    }
}
