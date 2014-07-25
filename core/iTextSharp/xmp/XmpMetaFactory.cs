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
using System.Runtime.CompilerServices;
using iTextSharp.xmp.impl;
using iTextSharp.xmp.options;

namespace iTextSharp.xmp {
    /// <summary>
    /// Creates <code>XMPMeta</code>-instances from an <code>InputStream</code>
    /// 
    /// @since 30.01.2006
    /// </summary>
    public static class XmpMetaFactory {
        /// <summary>
        /// The singleton instance of the <code>XMPSchemaRegistry</code>.
        /// </summary>
        private static IXmpSchemaRegistry _schema = new XmpSchemaRegistryImpl();

        /// <summary>
        /// cache for version info
        /// </summary>
        private static IXmpVersionInfo _versionInfo;

        /// <returns> Returns the singleton instance of the <code>XMPSchemaRegistry</code>. </returns>
        public static IXmpSchemaRegistry SchemaRegistry {
            get { return _schema; }
        }

        /// <returns> Returns an empty <code>XMPMeta</code>-object. </returns>
        public static IXmpMeta Create() {
            return new XmpMetaImpl();
        }

        /// <summary>
        /// Parsing with default options.
        /// </summary>
        /// <param name="in"> an <code>InputStream</code> </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        /// <seealso cref= XMPMetaFactory#parse(InputStream, ParseOptions) </seealso>
        public static IXmpMeta Parse(Stream @in) {
            return Parse(@in, null);
        }

        /// <summary>
        /// These functions support parsing serialized RDF into an XMP object, and serailizing an XMP
        /// object into RDF. The input for parsing may be any valid Unicode
        /// encoding. ISO Latin-1 is also recognized, but its use is strongly discouraged. Serialization
        /// is always as UTF-8.
        /// <p/>
        /// <code>parseFromBuffer()</code> parses RDF from an <code>InputStream</code>. The encoding
        /// is recognized automatically.
        /// </summary>
        /// <param name="in">      an <code>InputStream</code> </param>
        /// <param name="options"> Options controlling the parsing.<br>
        ///                The available options are:
        ///                <ul>
        ///                <li> XMP_REQUIRE_XMPMETA - The &lt;x:xmpmeta&gt; XML element is required around
        ///                <tt>&lt;rdf:RDF&gt;</tt>.
        ///                <li> XMP_STRICT_ALIASING - Do not reconcile alias differences, throw an exception.
        ///                </ul>
        ///                <em>Note:</em>The XMP_STRICT_ALIASING option is not yet implemented. </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        public static IXmpMeta Parse(Stream @in, ParseOptions options) {
            return XmpMetaParser.Parse(@in, options);
        }

        /// <summary>
        /// Parsing with default options.
        /// </summary>
        /// <param name="packet"> a String contain an XMP-file. </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        /// <seealso cref= XMPMetaFactory#parse(InputStream) </seealso>
        public static IXmpMeta ParseFromString(string packet) {
            return ParseFromString(packet, null);
        }

        /// <summary>
        /// Creates an <code>XMPMeta</code>-object from a string.
        /// </summary>
        /// <param name="packet">  a String contain an XMP-file. </param>
        /// <param name="options"> Options controlling the parsing. </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        /// <seealso cref= XMPMetaFactory#parseFromString(String, ParseOptions) </seealso>
        public static IXmpMeta ParseFromString(string packet, ParseOptions options) {
            return XmpMetaParser.Parse(packet, options);
        }

        /// <summary>
        /// Parsing with default options.
        /// </summary>
        /// <param name="buffer"> a String contain an XMP-file. </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        /// <seealso cref= XMPMetaFactory#parseFromBuffer(byte[], ParseOptions) </seealso>
        public static IXmpMeta ParseFromBuffer(byte[] buffer) {
            return ParseFromBuffer(buffer, null);
        }

        /// <summary>
        /// Creates an <code>XMPMeta</code>-object from a byte-buffer.
        /// </summary>
        /// <param name="buffer">  a String contain an XMP-file. </param>
        /// <param name="options"> Options controlling the parsing. </param>
        /// <returns> Returns the <code>XMPMeta</code>-object created from the input. </returns>
        /// <exception cref="XmpException"> If the file is not well-formed XML or if the parsing fails. </exception>
        /// <seealso cref= XMPMetaFactory#parse(InputStream, ParseOptions) </seealso>
        public static IXmpMeta ParseFromBuffer(byte[] buffer, ParseOptions options) {
            return XmpMetaParser.Parse(buffer, options);
        }

        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into an <code>OutputStream</code>
        /// with default options.
        /// </summary>
        /// <param name="xmp"> a metadata object </param>
        /// <param name="out"> an <code>OutputStream</code> to write the serialized RDF to. </param>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static void Serialize(IXmpMeta xmp, Stream @out) {
            Serialize(xmp, @out, null);
        }

        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into an <code>OutputStream</code>.
        /// </summary>
        /// <param name="xmp">     a metadata object </param>
        /// <param name="options"> Options to control the serialization (see <seealso cref="SerializeOptions"/>). </param>
        /// <param name="out">     an <code>OutputStream</code> to write the serialized RDF to. </param>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static void Serialize(IXmpMeta xmp, Stream @out, SerializeOptions options) {
            AssertImplementation(xmp);
            XmpSerializerHelper.Serialize((XmpMetaImpl) xmp, @out, options);
        }

        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into a byte buffer.
        /// </summary>
        /// <param name="xmp">     a metadata object </param>
        /// <param name="options"> Options to control the serialization (see <seealso cref="SerializeOptions"/>). </param>
        /// <returns> Returns a byte buffer containing the serialized RDF. </returns>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static byte[] SerializeToBuffer(IXmpMeta xmp, SerializeOptions options) {
            AssertImplementation(xmp);
            return XmpSerializerHelper.SerializeToBuffer((XmpMetaImpl) xmp, options);
        }

        /// <summary>
        /// Serializes an <code>XMPMeta</code>-object as RDF into a string. <em>Note:</em> Encoding
        /// is ignored when serializing to a string.
        /// </summary>
        /// <param name="xmp">     a metadata object </param>
        /// <param name="options"> Options to control the serialization (see <seealso cref="SerializeOptions"/>). </param>
        /// <returns> Returns a string containing the serialized RDF. </returns>
        /// <exception cref="XmpException"> on serializsation errors. </exception>
        public static string SerializeToString(IXmpMeta xmp, SerializeOptions options) {
            AssertImplementation(xmp);
            return XmpSerializerHelper.SerializeToString((XmpMetaImpl) xmp, options);
        }

        /// <param name="xmp"> Asserts that xmp is compatible to <code>XMPMetaImpl</code>.s </param>
        private static void AssertImplementation(IXmpMeta xmp) {
            if (!(xmp is XmpMetaImpl)) {
                throw new NotSupportedException("The serializing service works only" +
                                                "with the XMPMeta implementation of this library");
            }
        }

        /// <summary>
        /// Resets the _schema registry to its original state (creates a new one).
        /// Be careful this might break all existing XMPMeta-objects and should be used
        /// only for testing purpurses.
        /// </summary>
        public static void Reset() {
            _schema = new XmpSchemaRegistryImpl();
        }

        /// <summary>
        /// Obtain version information. The XMPVersionInfo singleton is created the first time
        /// its requested.
        /// </summary>
        /// <returns> Returns the version information. </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IXmpVersionInfo GetVersionInfo() {
            if (_versionInfo == null) {
                try {
                    _versionInfo = new XmpVersionInfoImpl();
                }
                catch (Exception e) {
                    // EMTPY, severe error would be detected during the tests
                    Console.WriteLine(e);
                }
            }
            return _versionInfo;
        }

        #region Nested type: XmpVersionInfoImpl

        private class XmpVersionInfoImpl : IXmpVersionInfo {
            private const int major = 5;
            private const int minor = 1;
            private const int micro = 0;
            private const int engBuild = 3;
            private const bool debug = false;

            // Adobe XMP Core 5.0-jc001 DEBUG-<branch>.<changelist>, 2009 Jan 28 15:22:38-CET
            private const string message = "Adobe XMP Core 5.1.0-jc003";

            #region XmpVersionInfo Members

            virtual public int Major {
                get { return major; }
            }

            virtual public int Minor {
                get { return minor; }
            }

            virtual public int Micro {
                get { return micro; }
            }

            virtual public bool Debug {
                get { return debug; }
            }

            virtual public int Build {
                get { return engBuild; }
            }

            virtual public string Message {
                get { return message; }
            }

            #endregion

            public override string ToString() {
                return message;
            }
        }

        #endregion
    }
}
