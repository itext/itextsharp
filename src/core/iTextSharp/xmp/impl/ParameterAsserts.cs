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
    /// @since   11.08.2006
    /// </summary>
    internal class ParameterAsserts : XmpConst {
        /// <summary>
        /// private constructor
        /// </summary>
        private ParameterAsserts() {
            // EMPTY
        }


        /// <summary>
        /// Asserts that an array name is set. </summary>
        /// <param name="arrayName"> an array name </param>
        /// <exception cref="XmpException"> Array name is null or empty </exception>
        public static void AssertArrayName(string arrayName) {
            if (string.IsNullOrEmpty(arrayName)) {
                throw new XmpException("Empty array name", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that a property name is set. </summary>
        /// <param name="propName"> a property name or path </param>
        /// <exception cref="XmpException"> Property name is null or empty </exception>
        public static void AssertPropName(string propName) {
            if (string.IsNullOrEmpty(propName)) {
                throw new XmpException("Empty property name", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that a schema namespace is set. </summary>
        /// <param name="schemaNs"> a schema namespace </param>
        /// <exception cref="XmpException"> Schema is null or empty </exception>
        public static void AssertSchemaNs(string schemaNs) {
            if (string.IsNullOrEmpty(schemaNs)) {
                throw new XmpException("Empty schema namespace URI", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that a prefix is set. </summary>
        /// <param name="prefix"> a prefix </param>
        /// <exception cref="XmpException"> Prefix is null or empty </exception>
        public static void AssertPrefix(string prefix) {
            if (string.IsNullOrEmpty(prefix)) {
                throw new XmpException("Empty prefix", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that a specific language is set. </summary>
        /// <param name="specificLang"> a specific lang </param>
        /// <exception cref="XmpException"> Specific language is null or empty </exception>
        public static void AssertSpecificLang(string specificLang) {
            if (string.IsNullOrEmpty(specificLang)) {
                throw new XmpException("Empty specific language", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that a struct name is set. </summary>
        /// <param name="structName"> a struct name </param>
        /// <exception cref="XmpException"> Struct name is null or empty </exception>
        public static void AssertStructName(string structName) {
            if (string.IsNullOrEmpty(structName)) {
                throw new XmpException("Empty array name", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that any string parameter is set. </summary>
        /// <param name="param"> any string parameter </param>
        /// <exception cref="XmpException"> Thrown if the parameter is null or has length 0. </exception>
        public static void AssertNotNull(object param) {
            if (param == null) {
                throw new XmpException("Parameter must not be null", XmpError.BADPARAM);
            }
            if ((param is string) && ((string) param).Length == 0) {
                throw new XmpException("Parameter must not be null or empty", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Asserts that the xmp object is of this implemention
        /// (<seealso cref="XmpMetaImpl"/>). </summary>
        /// <param name="xmp"> the XMP object </param>
        /// <exception cref="XmpException"> A wrong implentaion is used. </exception>
        public static void AssertImplementation(IXmpMeta xmp) {
            if (xmp == null) {
                throw new XmpException("Parameter must not be null", XmpError.BADPARAM);
            }
            if (!(xmp is XmpMetaImpl)) {
                throw new XmpException("The XMPMeta-object is not compatible with this implementation",
                                       XmpError.BADPARAM);
            }
        }
    }
}
