using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities.Collections;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.text.xml.xmp;
using iTextSharp.xmp.options;

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

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// Serializes the <code>XMPMeta</code>-object using the standard RDF serialization format. 
    /// The output is written to an <code>OutputStream</code> 
    /// according to the <code>SerializeOptions</code>. 
    /// 
    /// @since   11.07.2006
    /// </summary>
    public class XmpSerializerRdf {
        /// <summary>
        /// default padding </summary>
        private const int DEFAULT_PAD = 2048;

        private const string RDF_XMPMETA_END = "</x:xmpmeta>";

        private const string RDF_RDF_END = "</rdf:RDF>";

        private const string RDF_SCHEMA_START = "<rdf:Description rdf:about=";
        private const string RDF_SCHEMA_END = "</rdf:Description>";
        private const string RDF_STRUCT_START = "<rdf:Description";
        private const string RDF_STRUCT_END = "</rdf:Description>";
        private const string RDF_EMPTY_STRUCT = "<rdf:Description/>";
        private static readonly string PACKET_HEADER = "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>";

        /// <summary>
        /// The w/r is missing inbetween </summary>
        private static readonly string PACKET_TRAILER = "<?xpacket end=\"";

        private static readonly string PACKET_TRAILER2 = "\"?>";
        private static readonly string RDF_XMPMETA_START = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"";

        private static readonly string RDF_RDF_START =
            "<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">";

        /// <summary>
        /// a set of all rdf attribute qualifier </summary>
        internal static readonly ISet RDF_ATTR_QUALIFIER =
            new HashSet(new string[] {XmpConst.XML_LANG, "rdf:resource", "rdf:ID", "rdf:bagID", "rdf:nodeID"});

        /// <summary>
        /// the stored serialization options </summary>
        private SerializeOptions _options;

        /// <summary>
        /// the output stream to Serialize to </summary>
        private CountOutputStream _outputStream;

        /// <summary>
        /// the padding in the XMP Packet, or the length of the complete packet in
        ///  case of option <em>exactPacketLength</em>. 
        /// </summary>
        private int _padding;

        /// <summary>
        /// the size of one unicode char, for UTF-8 set to 1 
        ///  (Note: only valid for ASCII chars lower than 0x80),
        ///  set to 2 in case of UTF-16 
        /// </summary>
        private int _unicodeSize = 1; // UTF-8

        /// <summary>
        /// this writer is used to do the actual serialization </summary>
        private StreamWriter _writer;

        /// <summary>
        /// the metadata object to be serialized. </summary>
        private XmpMetaImpl _xmp;


        /// <summary>
        /// The actual serialization.
        /// </summary>
        /// <param name="xmp"> the metadata object to be serialized </param>
        /// <param name="out"> outputStream the output stream to Serialize to </param>
        /// <param name="options"> the serialization options
        /// </param>
        /// <exception cref="XmpException"> If case of wrong options or any other serialization error. </exception>
        public virtual void Serialize(IXmpMeta xmp, Stream @out, SerializeOptions options) {
            try {
                _outputStream = new CountOutputStream(@out);
                _xmp = (XmpMetaImpl) xmp;
                _options = options;
                _padding = options.Padding;

                _writer = new StreamWriter(_outputStream, new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(options.Encoding)));

                CheckOptionsConsistence();

                // serializes the whole packet, but don't write the tail yet 
                // and flush to make sure that the written bytes are calculated correctly
                string tailStr = SerializeAsRdf();
                _writer.Flush();

                // adds padding
                AddPadding(tailStr.Length);

                // writes the tail
                Write(tailStr);
                _writer.Flush();

                _outputStream.Close();
            }
            catch (IOException) {
                throw new XmpException("Error writing to the OutputStream", XmpError.UNKNOWN);
            }
        }


        /// <summary>
        /// Calculates the padding according to the options and write it to the stream. </summary>
        /// <param name="tailLength"> the length of the tail string </param>
        /// <exception cref="XmpException"> thrown if packet size is to small to fit the padding </exception>
        /// <exception cref="IOException"> forwards writer errors </exception>
        private void AddPadding(int tailLength) {
            if (_options.ExactPacketLength) {
                // the string length is equal to the length of the UTF-8 encoding
                int minSize = _outputStream.BytesWritten + tailLength*_unicodeSize;
                if (minSize > _padding) {
                    throw new XmpException("Can't fit into specified packet size", XmpError.BADSERIALIZE);
                }
                _padding -= minSize; // Now the actual amount of padding to add.
            }

            // fix rest of the padding according to Unicode unit size.
            _padding /= _unicodeSize;

            int newlineLen = _options.Newline.Length;
            if (_padding >= newlineLen) {
                _padding -= newlineLen; // Write this newline last.
                while (_padding >= (100 + newlineLen)) {
                    WriteChars(100, ' ');
                    WriteNewline();
                    _padding -= (100 + newlineLen);
                }
                WriteChars(_padding, ' ');
                WriteNewline();
            }
            else {
                WriteChars(_padding, ' ');
            }
        }


        /// <summary>
        /// Checks if the supplied options are consistent. </summary>
        /// <exception cref="XmpException"> Thrown if options are conflicting </exception>
        protected internal virtual void CheckOptionsConsistence() {
            if (_options.EncodeUtf16Be | _options.EncodeUtf16Le) {
                _unicodeSize = 2;
            }

            if (_options.ExactPacketLength) {
                if (_options.OmitPacketWrapper | _options.IncludeThumbnailPad) {
                    throw new XmpException("Inconsistent options for exact size Serialize", XmpError.BADOPTIONS);
                }
                if ((_options.Padding & (_unicodeSize - 1)) != 0) {
                    throw new XmpException("Exact size must be a multiple of the Unicode element",
                                           XmpError.BADOPTIONS);
                }
            }
            else if (_options.ReadOnlyPacket) {
                if (_options.OmitPacketWrapper | _options.IncludeThumbnailPad) {
                    throw new XmpException("Inconsistent options for read-only packet", XmpError.BADOPTIONS);
                }
                _padding = 0;
            }
            else if (_options.OmitPacketWrapper) {
                if (_options.IncludeThumbnailPad) {
                    throw new XmpException("Inconsistent options for non-packet Serialize", XmpError.BADOPTIONS);
                }
                _padding = 0;
            }
            else {
                if (_padding == 0) {
                    _padding = DEFAULT_PAD*_unicodeSize;
                }

                if (_options.IncludeThumbnailPad) {
                    if (!_xmp.DoesPropertyExist(XmpConst.NS_XMP, "Thumbnails")) {
                        _padding += 10000*_unicodeSize;
                    }
                }
            }
        }


        /// <summary>
        /// Writes the (optional) packet header and the outer rdf-tags. </summary>
        /// <returns> Returns the packet end processing instraction to be written after the padding. </returns>
        /// <exception cref="IOException"> Forwarded writer exceptions. </exception>
        /// <exception cref="XmpException">  </exception>
        private string SerializeAsRdf() {
            int level = 0;

            // Write the packet header PI.
            if (!_options.OmitPacketWrapper) {
                WriteIndent(level);
                Write(PACKET_HEADER);
                WriteNewline();
            }

            // Write the x:xmpmeta element's start tag.
            if (!_options.OmitXmpMetaElement) {
                WriteIndent(level);
                Write(RDF_XMPMETA_START);
                // Note: this flag can only be set by unit tests
                if (!_options.OmitVersionAttribute) {
                    Write(XmpMetaFactory.GetVersionInfo().Message);
                }
                Write("\">");
                WriteNewline();
                level++;
            }

            // Write the rdf:RDF start tag.
            WriteIndent(level);
            Write(RDF_RDF_START);
            WriteNewline();

            // Write all of the properties.
            if (_options.UseCanonicalFormat) {
                SerializeCanonicalRdfSchemas(level);
            }
            else {
                SerializeCompactRdfSchemas(level);
            }

            // Write the rdf:RDF end tag.
            WriteIndent(level);
            Write(RDF_RDF_END);
            WriteNewline();

            // Write the xmpmeta end tag.
            if (!_options.OmitXmpMetaElement) {
                level--;
                WriteIndent(level);
                Write(RDF_XMPMETA_END);
                WriteNewline();
            }
            // Write the packet trailer PI into the tail string as UTF-8.
            string tailStr = "";
            if (!_options.OmitPacketWrapper) {
                for (level = _options.BaseIndent; level > 0; level--) {
                    tailStr += _options.Indent;
                }

                tailStr += PACKET_TRAILER;
                tailStr += _options.ReadOnlyPacket ? 'r' : 'w';
                tailStr += PACKET_TRAILER2;
            }

            return tailStr;
        }


        /// <summary>
        /// Serializes the metadata in pretty-printed manner. </summary>
        /// <param name="level"> indent level </param>
        /// <exception cref="IOException"> Forwarded writer exceptions </exception>
        /// <exception cref="XmpException">  </exception>
        private void SerializeCanonicalRdfSchemas(int level) {
            if (_xmp.Root.ChildrenLength > 0) {
                StartOuterRdfDescription(_xmp.Root, level);

                for (IEnumerator it = _xmp.Root.IterateChildren(); it.MoveNext();) {
                    XmpNode currSchema = (XmpNode) it.Current;
                    SerializeCanonicalRdfSchema(currSchema, level);
                }

                EndOuterRdfDescription(level);
            }
            else {
                WriteIndent(level + 1);
                Write(RDF_SCHEMA_START); // Special case an empty XMP object.
                WriteTreeName();
                Write("/>");
                WriteNewline();
            }
        }


        /// <exception cref="IOException"> </exception>
        private void WriteTreeName() {
            Write('"');
            string name = _xmp.Root.Name;
            if (name != null) {
                AppendNodeValue(name, true);
            }
            Write('"');
        }


        /// <summary>
        /// Serializes the metadata in compact manner. </summary>
        /// <param name="level"> indent level to start with </param>
        /// <exception cref="IOException"> Forwarded writer exceptions </exception>
        /// <exception cref="XmpException">  </exception>
        private void SerializeCompactRdfSchemas(int level) {
            // Begin the rdf:Description start tag.
            WriteIndent(level + 1);
            Write(RDF_SCHEMA_START);
            WriteTreeName();

            // Write all necessary xmlns attributes.
            ISet usedPrefixes = new HashSet();
            usedPrefixes.Add("xml");
            usedPrefixes.Add("rdf");

            for (IEnumerator it = _xmp.Root.IterateChildren(); it.MoveNext();) {
                XmpNode schema = (XmpNode) it.Current;
                DeclareUsedNamespaces(schema, usedPrefixes, level + 3);
            }

            // Write the top level "attrProps" and close the rdf:Description start tag.
            bool allAreAttrs = true;
            for (IEnumerator it = _xmp.Root.IterateChildren(); it.MoveNext();) {
                XmpNode schema = (XmpNode) it.Current;
                allAreAttrs &= SerializeCompactRdfAttrProps(schema, level + 2);
            }

            if (!allAreAttrs) {
                Write('>');
                WriteNewline();
            }
            else {
                Write("/>");
                WriteNewline();
                return; // ! Done if all properties in all schema are written as attributes.
            }

            // Write the remaining properties for each schema.
            for (IEnumerator it = _xmp.Root.IterateChildren(); it.MoveNext();) {
                XmpNode schema = (XmpNode) it.Current;
                SerializeCompactRdfElementProps(schema, level + 2);
            }

            // Write the rdf:Description end tag.
            WriteIndent(level + 1);
            Write(RDF_SCHEMA_END);
            WriteNewline();
        }


        /// <summary>
        /// Write each of the parent's simple unqualified properties as an attribute. Returns true if all
        /// of the properties are written as attributes.
        /// </summary>
        /// <param name="parentNode"> the parent property node </param>
        /// <param name="indent"> the current indent level </param>
        /// <returns> Returns true if all properties can be rendered as RDF attribute. </returns>
        /// <exception cref="IOException"> </exception>
        private bool SerializeCompactRdfAttrProps(XmpNode parentNode, int indent) {
            bool allAreAttrs = true;

            for (IEnumerator it = parentNode.IterateChildren(); it.MoveNext();) {
                XmpNode prop = (XmpNode) it.Current;

                if (prop != null && canBeRDFAttrProp(prop)) {
                    WriteNewline();
                    WriteIndent(indent);
                    Write(prop.Name);
                    Write("=\"");
                    AppendNodeValue(prop.Value, true);
                    Write('\"');
                }
                else {
                    allAreAttrs = false;
                }
            }
            return allAreAttrs;
        }


        /// <summary>
        /// Recursively handles the "value" for a node that must be written as an RDF
        /// property element. It does not matter if it is a top level property, a
        /// field of a struct, or an item of an array. The indent is that for the
        /// property element. The patterns bwlow ignore attribute qualifiers such as
        /// xml:lang, they don't affect the output form.
        /// 
        /// <blockquote>
        /// 
        /// <pre>
        ///  	&lt;ns:UnqualifiedStructProperty-1
        ///  		... The fields as attributes, if all are simple and unqualified
        ///  	/&gt;
        ///  
        ///  	&lt;ns:UnqualifiedStructProperty-2 rdf:parseType=&quot;Resource&quot;&gt;
        ///  		... The fields as elements, if none are simple and unqualified
        ///  	&lt;/ns:UnqualifiedStructProperty-2&gt;
        ///  
        ///  	&lt;ns:UnqualifiedStructProperty-3&gt;
        ///  		&lt;rdf:Description
        ///  			... The simple and unqualified fields as attributes
        ///  		&gt;
        ///  			... The compound or qualified fields as elements
        ///  		&lt;/rdf:Description&gt;
        ///  	&lt;/ns:UnqualifiedStructProperty-3&gt;
        ///  
        ///  	&lt;ns:UnqualifiedArrayProperty&gt;
        ///  		&lt;rdf:Bag&gt; or Seq or Alt
        ///  			... Array items as rdf:li elements, same forms as top level properties
        ///  		&lt;/rdf:Bag&gt;
        ///  	&lt;/ns:UnqualifiedArrayProperty&gt;
        ///  
        ///  	&lt;ns:QualifiedProperty rdf:parseType=&quot;Resource&quot;&gt;
        ///  		&lt;rdf:value&gt; ... Property &quot;value&quot; 
        ///  			following the unqualified forms ... &lt;/rdf:value&gt;
        ///  		... Qualifiers looking like named struct fields
        ///  	&lt;/ns:QualifiedProperty&gt;
        /// </pre>
        /// 
        /// </blockquote>
        /// 
        /// *** Consider numbered array items, but has compatibility problems. ***
        /// Consider qualified form with rdf:Description and attributes.
        /// </summary>
        /// <param name="parentNode"> the parent node </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards writer exceptions </exception>
        /// <exception cref="XmpException"> If qualifier and element fields are mixed. </exception>
        private void SerializeCompactRdfElementProps(XmpNode parentNode, int indent) {
            for (IEnumerator it = parentNode.IterateChildren(); it.MoveNext();) {
                XmpNode node = (XmpNode) it.Current;
                if (node == null)
                    continue;
                if (canBeRDFAttrProp(node)) {
                    continue;
                }

                bool emitEndTag = true;
                bool indentEndTag = true;

                // Determine the XML element name, write the name part of the start tag. Look over the
                // qualifiers to decide on "normal" versus "rdf:value" form. Emit the attribute
                // qualifiers at the same time.
                string elemName = node.Name;
                if (XmpConst.ARRAY_ITEM_NAME.Equals(elemName)) {
                    elemName = "rdf:li";
                }

                WriteIndent(indent);
                Write('<');
                Write(elemName);

                bool hasGeneralQualifiers = false;
                bool hasRdfResourceQual = false;

                for (IEnumerator iq = node.IterateQualifier(); iq.MoveNext();) {
                    XmpNode qualifier = (XmpNode) iq.Current;
                    if (qualifier == null)
                        continue;
                    if (!RDF_ATTR_QUALIFIER.Contains(qualifier.Name)) {
                        hasGeneralQualifiers = true;
                    }
                    else {
                        hasRdfResourceQual = "rdf:resource".Equals(qualifier.Name);
                        Write(' ');
                        Write(qualifier.Name);
                        Write("=\"");
                        AppendNodeValue(qualifier.Value, true);
                        Write('"');
                    }
                }


                // Process the property according to the standard patterns.
                if (hasGeneralQualifiers) {
                    SerializeCompactRdfGeneralQualifier(indent, node);
                }
                else {
                    // This node has only attribute qualifiers. Emit as a property element.
                    if (!node.Options.CompositeProperty) {
                        object[] result = SerializeCompactRdfSimpleProp(node);
                        emitEndTag = (bool) ((bool?) result[0]);
                        indentEndTag = (bool) ((bool?) result[1]);
                    }
                    else if (node.Options.Array) {
                        SerializeCompactRdfArrayProp(node, indent);
                    }
                    else {
                        emitEndTag = SerializeCompactRdfStructProp(node, indent, hasRdfResourceQual);
                    }
                }

                // Emit the property element end tag.
                if (emitEndTag) {
                    if (indentEndTag) {
                        WriteIndent(indent);
                    }
                    Write("</");
                    Write(elemName);
                    Write('>');
                    WriteNewline();
                }
            }
        }


        /// <summary>
        /// Serializes a simple property.
        /// </summary>
        /// <param name="node"> an XMPNode </param>
        /// <returns> Returns an array containing the flags emitEndTag and indentEndTag. </returns>
        /// <exception cref="IOException"> Forwards the writer exceptions. </exception>
        private object[] SerializeCompactRdfSimpleProp(XmpNode node) {
            // This is a simple property.
            bool? emitEndTag = true;
            bool? indentEndTag = true;

            if (node.Options.Uri) {
                Write(" rdf:resource=\"");
                AppendNodeValue(node.Value, true);
                Write("\"/>");
                WriteNewline();
                emitEndTag = false;
            }
            else if (string.IsNullOrEmpty(node.Value)) {
                Write("/>");
                WriteNewline();
                emitEndTag = false;
            }
            else {
                Write('>');
                AppendNodeValue(node.Value, false);
                indentEndTag = false;
            }

            return new object[] {emitEndTag, indentEndTag};
        }


        /// <summary>
        /// Serializes an array property.
        /// </summary>
        /// <param name="node"> an XMPNode </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards the writer exceptions. </exception>
        /// <exception cref="XmpException"> If qualifier and element fields are mixed. </exception>
        private void SerializeCompactRdfArrayProp(XmpNode node, int indent) {
            // This is an array.
            Write('>');
            WriteNewline();
            EmitRdfArrayTag(node, true, indent + 1);

            if (node.Options.ArrayAltText) {
                XmpNodeUtils.NormalizeLangArray(node);
            }

            SerializeCompactRdfElementProps(node, indent + 2);

            EmitRdfArrayTag(node, false, indent + 1);
        }


        /// <summary>
        /// Serializes a struct property.
        /// </summary>
        /// <param name="node"> an XMPNode </param>
        /// <param name="indent"> the current indent level </param>
        /// <param name="hasRdfResourceQual"> Flag if the element has resource qualifier </param>
        /// <returns> Returns true if an end flag shall be emitted. </returns>
        /// <exception cref="IOException"> Forwards the writer exceptions. </exception>
        /// <exception cref="XmpException"> If qualifier and element fields are mixed. </exception>
        private bool SerializeCompactRdfStructProp(XmpNode node, int indent, bool hasRdfResourceQual) {
            // This must be a struct.
            bool hasAttrFields = false;
            bool hasElemFields = false;
            bool emitEndTag = true;

            for (IEnumerator ic = node.IterateChildren(); ic.MoveNext();) {
                XmpNode field = (XmpNode) ic.Current;
                if (field == null)
                    continue;
                if (canBeRDFAttrProp(field)) {
                    hasAttrFields = true;
                }
                else {
                    hasElemFields = true;
                }

                if (hasAttrFields && hasElemFields) {
                    break; // No sense looking further.
                }
            }

            if (hasRdfResourceQual && hasElemFields) {
                throw new XmpException("Can't mix rdf:resource qualifier and element fields", XmpError.BADRDF);
            }

            if (!node.HasChildren()) {
                // Catch an empty struct as a special case. The case
                // below would emit an empty
                // XML element, which gets reparsed as a simple property
                // with an empty value.
                Write(" rdf:parseType=\"Resource\"/>");
                WriteNewline();
                emitEndTag = false;
            }
            else if (!hasElemFields) {
                // All fields can be attributes, use the
                // emptyPropertyElt form.
                SerializeCompactRdfAttrProps(node, indent + 1);
                Write("/>");
                WriteNewline();
                emitEndTag = false;
            }
            else if (!hasAttrFields) {
                // All fields must be elements, use the
                // parseTypeResourcePropertyElt form.
                Write(" rdf:parseType=\"Resource\">");
                WriteNewline();
                SerializeCompactRdfElementProps(node, indent + 1);
            }
            else {
                // Have a mix of attributes and elements, use an inner rdf:Description.
                Write('>');
                WriteNewline();
                WriteIndent(indent + 1);
                Write(RDF_STRUCT_START);
                SerializeCompactRdfAttrProps(node, indent + 2);
                Write(">");
                WriteNewline();
                SerializeCompactRdfElementProps(node, indent + 1);
                WriteIndent(indent + 1);
                Write(RDF_STRUCT_END);
                WriteNewline();
            }
            return emitEndTag;
        }


        /// <summary>
        /// Serializes the general qualifier. </summary>
        /// <param name="node"> the root node of the subtree </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards all writer exceptions. </exception>
        /// <exception cref="XmpException"> If qualifier and element fields are mixed. </exception>
        private void SerializeCompactRdfGeneralQualifier(int indent, XmpNode node) {
            // The node has general qualifiers, ones that can't be
            // attributes on a property element.
            // Emit using the qualified property pseudo-struct form. The
            // value is output by a call
            // to SerializePrettyRDFProperty with emitAsRDFValue set.
            Write(" rdf:parseType=\"Resource\">");
            WriteNewline();

            SerializeCanonicalRdfProperty(node, false, true, indent + 1);

            for (IEnumerator iq = node.IterateQualifier(); iq.MoveNext();) {
                XmpNode qualifier = (XmpNode) iq.Current;
                if (qualifier == null)
                    continue;
                SerializeCanonicalRdfProperty(qualifier, false, false, indent + 1);
            }
        }


        /// <summary>
        /// Serializes one schema with all contained properties in pretty-printed
        /// manner.<br> 
        /// Each schema's properties are written to a single
        /// rdf:Description element. All of the necessary namespaces are declared in
        /// the rdf:Description element. The baseIndent is the base level for the
        /// entire serialization, that of the x:xmpmeta element. An xml:lang
        /// qualifier is written as an attribute of the property start tag, not by
        /// itself forcing the qualified property form.
        /// 
        /// <blockquote>
        /// 
        /// <pre>
        ///  	 &lt;rdf:Description rdf:about=&quot;TreeName&quot; xmlns:ns=&quot;URI&quot; ... &gt;
        ///  
        ///  	 	... The actual properties of the schema, see SerializePrettyRDFProperty
        ///  
        ///  	 	&lt;!-- ns1:Alias is aliased to ns2:Actual --&gt;  ... If alias comments are wanted
        ///  
        ///  	 &lt;/rdf:Description&gt;
        /// </pre>
        /// 
        /// </blockquote>
        /// </summary>
        /// <param name="schemaNode"> a schema node </param>
        /// <param name="level"> </param>
        /// <exception cref="IOException"> Forwarded writer exceptions </exception>
        /// <exception cref="XmpException">  </exception>
        private void SerializeCanonicalRdfSchema(XmpNode schemaNode, int level) {
            // Write each of the schema's actual properties.
            for (IEnumerator it = schemaNode.IterateChildren(); it.MoveNext();) {
                XmpNode propNode = (XmpNode) it.Current;
                if (propNode == null)
                    continue;
                SerializeCanonicalRdfProperty(propNode, _options.UseCanonicalFormat, false, level + 2);
            }
        }


        /// <summary>
        /// Writes all used namespaces of the subtree in node to the output. 
        /// The subtree is recursivly traversed. </summary>
        /// <param name="node"> the root node of the subtree </param>
        /// <param name="usedPrefixes"> a set containing currently used prefixes </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards all writer exceptions. </exception>
        private void DeclareUsedNamespaces(XmpNode node, ISet usedPrefixes, int indent) {
            if (node.Options.SchemaNode) {
                // The schema node name is the URI, the value is the prefix.
                string prefix = node.Value.Substring(0, node.Value.Length - 1);
                DeclareNamespace(prefix, node.Name, usedPrefixes, indent);
            }
            else if (node.Options.Struct) {
                for (IEnumerator it = node.IterateChildren(); it.MoveNext();) {
                    XmpNode field = (XmpNode) it.Current;
                    if (field == null)
                        continue;
                    DeclareNamespace(field.Name, null, usedPrefixes, indent);
                }
            }

            for (IEnumerator it = node.IterateChildren(); it.MoveNext();) {
                XmpNode child = (XmpNode) it.Current;
                if (child == null)
                    continue;
                DeclareUsedNamespaces(child, usedPrefixes, indent);
            }

            for (IEnumerator it = node.IterateQualifier(); it.MoveNext();) {
                XmpNode qualifier = (XmpNode) it.Current;
                if (qualifier == null)
                    continue;
                DeclareNamespace(qualifier.Name, null, usedPrefixes, indent);
                DeclareUsedNamespaces(qualifier, usedPrefixes, indent);
            }
        }


        /// <summary>
        /// Writes one namespace declaration to the output. </summary>
        /// <param name="prefix"> a namespace prefix (without colon) or a complete qname (when namespace == null) </param>
        /// <param name="namespace"> the a namespace </param>
        /// <param name="usedPrefixes"> a set containing currently used prefixes </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards all writer exceptions. </exception>
        private void DeclareNamespace(string prefix, string @namespace, ISet usedPrefixes, int indent) {
            if (@namespace == null) {
                // prefix contains qname, extract prefix and lookup namespace with prefix
                QName qname = new QName(prefix);
                if (qname.HasPrefix()) {
                    prefix = qname.Prefix;
                    // add colon for lookup
                    @namespace = XmpMetaFactory.SchemaRegistry.GetNamespaceUri(prefix + ":");
                    // prefix w/o colon
                    DeclareNamespace(prefix, @namespace, usedPrefixes, indent);
                }
                else {
                    return;
                }
            }

            if (!usedPrefixes.Contains(prefix)) {
                WriteNewline();
                WriteIndent(indent);
                Write("xmlns:");
                Write(prefix);
                Write("=\"");
                Write(@namespace);
                Write('"');
                usedPrefixes.Add(prefix);
            }
        }


        /// <summary>
        /// Start the outer rdf:Description element, including all needed xmlns attributes.
        /// Leave the element open so that the compact form can add property attributes.
        /// </summary>
        /// <exception cref="IOException"> If the writing to   </exception>
        private void StartOuterRdfDescription(XmpNode schemaNode, int level) {
            WriteIndent(level + 1);
            Write(RDF_SCHEMA_START);
            WriteTreeName();

            ISet usedPrefixes = new HashSet();
            usedPrefixes.Add("xml");
            usedPrefixes.Add("rdf");

            DeclareUsedNamespaces(schemaNode, usedPrefixes, level + 3);

            Write('>');
            WriteNewline();
        }


        /// <summary>
        ///  Write the </rdf:Description> end tag.
        /// </summary>
        private void EndOuterRdfDescription(int level) {
            WriteIndent(level + 1);
            Write(RDF_SCHEMA_END);
            WriteNewline();
        }


        /// <summary>
        /// Recursively handles the "value" for a node. It does not matter if it is a
        /// top level property, a field of a struct, or an item of an array. The
        /// indent is that for the property element. An xml:lang qualifier is written
        /// as an attribute of the property start tag, not by itself forcing the
        /// qualified property form. The patterns below mostly ignore attribute
        /// qualifiers like xml:lang. Except for the one struct case, attribute
        /// qualifiers don't affect the output form.
        /// 
        /// <blockquote>
        /// 
        /// <pre>
        /// 	&lt;ns:UnqualifiedSimpleProperty&gt;value&lt;/ns:UnqualifiedSimpleProperty&gt;
        /// 
        /// 	&lt;ns:UnqualifiedStructProperty&gt; (If no rdf:resource qualifier)
        /// 		&lt;rdf:Description&gt;
        /// 			... Fields, same forms as top level properties
        /// 		&lt;/rdf:Description&gt;
        /// 	&lt;/ns:UnqualifiedStructProperty&gt;
        /// 
        /// 	&lt;ns:ResourceStructProperty rdf:resource=&quot;URI&quot;
        /// 		... Fields as attributes
        /// 	&gt;
        /// 
        /// 	&lt;ns:UnqualifiedArrayProperty&gt;
        /// 		&lt;rdf:Bag&gt; or Seq or Alt
        /// 			... Array items as rdf:li elements, same forms as top level properties
        /// 		&lt;/rdf:Bag&gt;
        /// 	&lt;/ns:UnqualifiedArrayProperty&gt;
        /// 
        /// 	&lt;ns:QualifiedProperty&gt;
        /// 		&lt;rdf:Description&gt;
        /// 			&lt;rdf:value&gt; ... Property &quot;value&quot; following the unqualified 
        /// 				forms ... &lt;/rdf:value&gt;
        /// 			... Qualifiers looking like named struct fields
        /// 		&lt;/rdf:Description&gt;
        /// 	&lt;/ns:QualifiedProperty&gt;
        /// </pre>
        /// 
        /// </blockquote>
        /// </summary>
        /// <param name="node"> the property node </param>
        /// <param name="emitAsRdfValue"> property shall be rendered as attribute rather than tag </param>
        /// <param name="useCanonicalRdf"> use canonical form with inner description tag or 
        /// 		  the compact form with rdf:ParseType=&quot;resource&quot; attribute. </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> Forwards all writer exceptions. </exception>
        /// <exception cref="XmpException"> If &quot;rdf:resource&quot; and general qualifiers are mixed. </exception>
        private void SerializeCanonicalRdfProperty(XmpNode node, bool useCanonicalRdf, bool emitAsRdfValue, int indent) {
            bool emitEndTag = true;
            bool indentEndTag = true;

            // Determine the XML element name. Open the start tag with the name and
            // attribute qualifiers.

            string elemName = node.Name;
            if (emitAsRdfValue) {
                elemName = "rdf:value";
            }
            else if (XmpConst.ARRAY_ITEM_NAME.Equals(elemName)) {
                elemName = "rdf:li";
            }

            WriteIndent(indent);
            Write('<');
            Write(elemName);

            bool hasGeneralQualifiers = false;
            bool hasRdfResourceQual = false;

            for (IEnumerator it = node.IterateQualifier(); it.MoveNext();) {
                XmpNode qualifier = (XmpNode) it.Current;
                if (qualifier != null) {
                    if (!RDF_ATTR_QUALIFIER.Contains(qualifier.Name)) {
                        hasGeneralQualifiers = true;
                    }
                    else {
                        hasRdfResourceQual = "rdf:resource".Equals(qualifier.Name);
                        if (!emitAsRdfValue) {
                            Write(' ');
                            Write(qualifier.Name);
                            Write("=\"");
                            AppendNodeValue(qualifier.Value, true);
                            Write('"');
                        }
                    }
                }
            }

            // Process the property according to the standard patterns.

            if (hasGeneralQualifiers && !emitAsRdfValue) {
                // This node has general, non-attribute, qualifiers. Emit using the
                // qualified property form.
                // ! The value is output by a recursive call ON THE SAME NODE with
                // emitAsRDFValue set.

                if (hasRdfResourceQual) {
                    throw new XmpException("Can't mix rdf:resource and general qualifiers", XmpError.BADRDF);
                }

                // Change serialization to canonical format with inner rdf:Description-tag
                // depending on option
                if (useCanonicalRdf) {
                    Write(">");
                    WriteNewline();

                    indent++;
                    WriteIndent(indent);
                    Write(RDF_STRUCT_START);
                    Write(">");
                }
                else {
                    Write(" rdf:parseType=\"Resource\">");
                }
                WriteNewline();

                SerializeCanonicalRdfProperty(node, useCanonicalRdf, true, indent + 1);

                for (IEnumerator it = node.IterateQualifier(); it.MoveNext();) {
                    XmpNode qualifier = (XmpNode) it.Current;
                    if (qualifier != null && !RDF_ATTR_QUALIFIER.Contains(qualifier.Name)) {
                        SerializeCanonicalRdfProperty(qualifier, useCanonicalRdf, false, indent + 1);
                    }
                }

                if (useCanonicalRdf) {
                    WriteIndent(indent);
                    Write(RDF_STRUCT_END);
                    WriteNewline();
                    indent--;
                }
            }
            else {
                // This node has no general qualifiers. Emit using an unqualified form.

                if (!node.Options.CompositeProperty) {
                    // This is a simple property.

                    if (node.Options.Uri) {
                        Write(" rdf:resource=\"");
                        AppendNodeValue(node.Value, true);
                        Write("\"/>");
                        WriteNewline();
                        emitEndTag = false;
                    }
                    else if (node.Value == null || "".Equals(node.Value)) {
                        Write("/>");
                        WriteNewline();
                        emitEndTag = false;
                    }
                    else {
                        Write('>');
                        AppendNodeValue(node.Value, false);
                        indentEndTag = false;
                    }
                }
                else if (node.Options.Array) {
                    // This is an array.
                    Write('>');
                    WriteNewline();
                    EmitRdfArrayTag(node, true, indent + 1);
                    if (node.Options.ArrayAltText) {
                        XmpNodeUtils.NormalizeLangArray(node);
                    }
                    for (IEnumerator it = node.IterateChildren(); it.MoveNext();) {
                        XmpNode child = (XmpNode) it.Current;
                        SerializeCanonicalRdfProperty(child, useCanonicalRdf, false, indent + 2);
                    }
                    EmitRdfArrayTag(node, false, indent + 1);
                }
                else if (!hasRdfResourceQual) {
                    // This is a "normal" struct, use the rdf:parseType="Resource" form.
                    if (!node.HasChildren()) {
                        // Change serialization to canonical format with inner rdf:Description-tag
                        // if option is set
                        if (useCanonicalRdf) {
                            Write(">");
                            WriteNewline();
                            WriteIndent(indent + 1);
                            Write(RDF_EMPTY_STRUCT);
                        }
                        else {
                            Write(" rdf:parseType=\"Resource\"/>");
                            emitEndTag = false;
                        }
                        WriteNewline();
                    }
                    else {
                        // Change serialization to canonical format with inner rdf:Description-tag
                        // if option is set
                        if (useCanonicalRdf) {
                            Write(">");
                            WriteNewline();
                            indent++;
                            WriteIndent(indent);
                            Write(RDF_STRUCT_START);
                            Write(">");
                        }
                        else {
                            Write(" rdf:parseType=\"Resource\">");
                        }
                        WriteNewline();

                        for (IEnumerator it = node.IterateChildren(); it.MoveNext();) {
                            XmpNode child = (XmpNode) it.Current;
                            SerializeCanonicalRdfProperty(child, useCanonicalRdf, false, indent + 1);
                        }

                        if (useCanonicalRdf) {
                            WriteIndent(indent);
                            Write(RDF_STRUCT_END);
                            WriteNewline();
                            indent--;
                        }
                    }
                }
                else {
                    // This is a struct with an rdf:resource attribute, use the
                    // "empty property element" form.
                    for (IEnumerator it = node.IterateChildren(); it.MoveNext();) {
                        XmpNode child = (XmpNode) it.Current;
                        if (child != null) {
                            if (!canBeRDFAttrProp(child)) {
                                throw new XmpException("Can't mix rdf:resource and complex fields",
                                                       XmpError.BADRDF);
                            }
                            WriteNewline();
                            WriteIndent(indent + 1);
                            Write(' ');
                            Write(child.Name);
                            Write("=\"");
                            AppendNodeValue(child.Value, true);
                            Write('"');
                        }
                    }
                    Write("/>");
                    WriteNewline();
                    emitEndTag = false;
                }
            }

            // Emit the property element end tag.
            if (emitEndTag) {
                if (indentEndTag) {
                    WriteIndent(indent);
                }
                Write("</");
                Write(elemName);
                Write('>');
                WriteNewline();
            }
        }


        /// <summary>
        /// Writes the array start and end tags.
        /// </summary>
        /// <param name="arrayNode"> an array node </param>
        /// <param name="isStartTag"> flag if its the start or end tag </param>
        /// <param name="indent"> the current indent level </param>
        /// <exception cref="IOException"> forwards writer exceptions </exception>
        private void EmitRdfArrayTag(XmpNode arrayNode, bool isStartTag, int indent) {
            if (isStartTag || arrayNode.HasChildren()) {
                WriteIndent(indent);
                Write(isStartTag ? "<rdf:" : "</rdf:");

                if (arrayNode.Options.ArrayAlternate) {
                    Write("Alt");
                }
                else if (arrayNode.Options.ArrayOrdered) {
                    Write("Seq");
                }
                else {
                    Write("Bag");
                }

                if (isStartTag && !arrayNode.HasChildren()) {
                    Write("/>");
                }
                else {
                    Write(">");
                }

                WriteNewline();
            }
        }


        /// <summary>
        /// Serializes the node value in XML encoding. Its used for tag bodies and
        /// attributes. <em>Note:</em> The attribute is always limited by quotes,
        /// thats why <code>&amp;apos;</code> is never serialized. <em>Note:</em>
        /// Control chars are written unescaped, but if the user uses others than tab, LF
        /// and CR the resulting XML will become invalid.
        /// </summary>
        /// <param name="value"> the value of the node </param>
        /// <param name="forAttribute"> flag if value is an attribute value </param>
        /// <exception cref="IOException"> </exception>
        private void AppendNodeValue(string value, bool forAttribute) {
            if (value == null) {
                value = "";
            }
            Write(Utils.EscapeXml(value, forAttribute, true));
        }


        /// <summary>
        /// A node can be serialized as RDF-Attribute, if it meets the following conditions:
        /// <ul>
        ///  	<li>is not array item
        /// 		<li>don't has qualifier
        /// 		<li>is no URI
        /// 		<li>is no composite property
        /// </ul> 
        /// </summary>
        /// <param name="node"> an XMPNode </param>
        /// <returns> Returns true if the node serialized as RDF-Attribute </returns>
        private bool canBeRDFAttrProp(XmpNode node) {
            return !node.HasQualifier() && !node.Options.Uri && !node.Options.CompositeProperty &&
                   !node.Options.ContainsOneOf(PropertyOptions.SEPARATE_NODE) &&
                   !XmpConst.ARRAY_ITEM_NAME.Equals(node.Name);
        }


        /// <summary>
        /// Writes indents and automatically includes the baseindend from the options. </summary>
        /// <param name="times"> number of indents to write </param>
        /// <exception cref="IOException"> forwards exception </exception>
        private void WriteIndent(int times) {
            for (int i = _options.BaseIndent + times; i > 0; i--) {
                _writer.Write(_options.Indent);
            }
        }


        /// <summary>
        /// Writes a char to the output. </summary>
        /// <param name="c"> a char </param>
        /// <exception cref="IOException"> forwards writer exceptions </exception>
        private void Write(char c) {
            _writer.Write(c);
        }


        /// <summary>
        /// Writes a String to the output. </summary>
        /// <param name="str"> a String </param>
        /// <exception cref="IOException"> forwards writer exceptions </exception>
        private void Write(string str) {
            _writer.Write(str);
        }


        /// <summary>
        /// Writes an amount of chars, mostly spaces </summary>
        /// <param name="number"> number of chars </param>
        /// <param name="c"> a char </param>
        /// <exception cref="IOException"> </exception>
        private void WriteChars(int number, char c) {
            for (; number > 0; number--) {
                _writer.Write(c);
            }
        }


        /// <summary>
        /// Writes a newline according to the options. </summary>
        /// <exception cref="IOException"> Forwards exception </exception>
        private void WriteNewline() {
            _writer.Write(_options.Newline);
        }
    }
}
