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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using iTextSharp.xmp.options;
using iTextSharp.xmp.properties;

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// The schema registry handles the namespaces, aliases and global options for the XMP Toolkit. There
    /// is only one single instance used by the toolkit.
    /// 
    /// @since 27.01.2006
    /// </summary>
    public sealed class XmpSchemaRegistryImpl : XmpConst, IXmpSchemaRegistry {
        /// <summary>
        /// a map of all registered aliases. 
        ///  The map is a relationship from a qname to an <code>XMPAliasInfo</code>-object. 
        /// </summary>
        private readonly IDictionary _aliasMap = new Hashtable();

        /// <summary>
        /// a map from a namespace URI to its registered prefix </summary>
        private readonly IDictionary _namespaceToPrefixMap = new Hashtable();

        /// <summary>
        /// a map from a prefix to the associated namespace URI </summary>
        private readonly IDictionary _prefixToNamespaceMap = new Hashtable();

        /// <summary>
        /// The pattern that must not be contained in simple properties </summary>
        private readonly Regex _regex = new Regex("[/*?\\[\\]]");


        /// <summary>
        /// Performs the initialisation of the registry with the default namespaces, aliases and global
        /// options.
        /// </summary>
        public XmpSchemaRegistryImpl() {
            try {
                RegisterStandardNamespaces();
                RegisterStandardAliases();
            }
            catch (XmpException) {
                throw new Exception("The XMPSchemaRegistry cannot be initialized!");
            }
        }


        // ---------------------------------------------------------------------------------------------
        // Namespace Functions

        #region XmpSchemaRegistry Members

        /// <seealso cref= XMPSchemaRegistry#registerNamespace(String, String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string RegisterNamespace(string namespaceUri, string suggestedPrefix) {
            ParameterAsserts.AssertSchemaNs(namespaceUri);
            ParameterAsserts.AssertPrefix(suggestedPrefix);

            if (suggestedPrefix[suggestedPrefix.Length - 1] != ':') {
                suggestedPrefix += ':';
            }

            if (!Utils.IsXmlNameNs(suggestedPrefix.Substring(0, suggestedPrefix.Length - 1))) {
                throw new XmpException("The prefix is a bad XML name", XmpError.BADXML);
            }

            string registeredPrefix = (string) _namespaceToPrefixMap[namespaceUri];
            string registeredNs = (string) _prefixToNamespaceMap[suggestedPrefix];
            if (registeredPrefix != null) {
                // Return the actual prefix
                return registeredPrefix;
            }
            if (registeredNs != null) {
                // the namespace is new, but the prefix is already engaged,
                // we generate a new prefix out of the suggested
                string generatedPrefix = suggestedPrefix;
                for (int i = 1; _prefixToNamespaceMap.Contains(generatedPrefix); i++) {
                    generatedPrefix = suggestedPrefix.Substring(0, suggestedPrefix.Length - 1) + "_" + i + "_:";
                }
                suggestedPrefix = generatedPrefix;
            }
            _prefixToNamespaceMap[suggestedPrefix] = namespaceUri;
            _namespaceToPrefixMap[namespaceUri] = suggestedPrefix;

            // Return the suggested prefix
            return suggestedPrefix;
        }


        /// <seealso cref= XMPSchemaRegistry#deleteNamespace(String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeleteNamespace(string namespaceUri) {
            string prefixToDelete = GetNamespacePrefix(namespaceUri);
            if (prefixToDelete != null) {
                _namespaceToPrefixMap.Remove(namespaceUri);
                _prefixToNamespaceMap.Remove(prefixToDelete);
            }
        }


        /// <seealso cref= XMPSchemaRegistry#getNamespacePrefix(String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetNamespacePrefix(string namespaceUri) {
            return (string) _namespaceToPrefixMap[namespaceUri];
        }


        /// <seealso cref= XMPSchemaRegistry#getNamespaceURI(String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetNamespaceUri(string namespacePrefix) {
            if (namespacePrefix != null && !namespacePrefix.EndsWith(":")) {
                namespacePrefix += ":";
            }
            return (string) _prefixToNamespaceMap[namespacePrefix];
        }


        // ---------------------------------------------------------------------------------------------
        // Alias Functions


        /// <seealso cref= XMPSchemaRegistry#resolveAlias(String, String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo ResolveAlias(string aliasNs, string aliasProp) {
            string aliasPrefix = GetNamespacePrefix(aliasNs);
            if (aliasPrefix == null) {
                return null;
            }

            return (IXmpAliasInfo) _aliasMap[aliasPrefix + aliasProp];
        }


        /// <seealso cref= XMPSchemaRegistry#findAlias(java.lang.String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo FindAlias(string qname) {
            return (IXmpAliasInfo) _aliasMap[qname];
        }


        /// <seealso cref= XMPSchemaRegistry#findAliases(String) </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo[] FindAliases(string aliasNs) {
            string prefix = GetNamespacePrefix(aliasNs);
            IList result = new ArrayList();
            if (prefix != null) {
                for (IEnumerator it = _aliasMap.Keys.GetEnumerator(); it.MoveNext();) {
                    string qname = (string) it.Current;
                    if (qname != null && qname.StartsWith(prefix)) {
                        result.Add(FindAlias(qname));
                    }
                }
            }
            IXmpAliasInfo[] array = new IXmpAliasInfo[result.Count];
            result.CopyTo(array, 0);
            return array;
        }

        /// <seealso cref= XMPSchemaRegistry#getNamespaces() </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetNamespaces() {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_namespaceToPrefixMap));
        }


        /// <seealso cref= XMPSchemaRegistry#getPrefixes() </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetPrefixes() {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_prefixToNamespaceMap));
        }

        /// <seealso cref= XMPSchemaRegistry#getAliases() </seealso>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetAliases() {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_aliasMap));
        }

        #endregion

        /// <summary>
        /// Register the standard namespaces of schemas and types that are included in the XMP
        /// Specification and some other Adobe private namespaces.
        /// Note: This method is not lock because only called by the constructor.
        /// </summary>
        /// <exception cref="XmpException"> Forwards processing exceptions </exception>
        private void RegisterStandardNamespaces() {
            // register standard namespaces
            RegisterNamespace(NS_XML, "xml");
            RegisterNamespace(NS_RDF, "rdf");
            RegisterNamespace(NS_DC, "dc");
            RegisterNamespace(NS_IPTCCORE, "Iptc4xmpCore");
            RegisterNamespace(NS_IPTCEXT, "Iptc4xmpExt");
            RegisterNamespace(NS_DICOM, "DICOM");
            RegisterNamespace(NS_PLUS, "plus");

            // register Adobe standard namespaces
            RegisterNamespace(NS_X, "x");
            RegisterNamespace(NS_IX, "iX");

            RegisterNamespace(NS_XMP, "xmp");
            RegisterNamespace(NS_XMP_RIGHTS, "xmpRights");
            RegisterNamespace(NS_XMP_MM, "xmpMM");
            RegisterNamespace(NS_XMP_BJ, "xmpBJ");
            RegisterNamespace(NS_XMP_NOTE, "xmpNote");

            RegisterNamespace(NS_PDF, "pdf");
            RegisterNamespace(NS_PDFX, "pdfx");
            RegisterNamespace(NS_PDFX_ID, "pdfxid");
            RegisterNamespace(NS_PDFA_SCHEMA, "pdfaSchema");
            RegisterNamespace(NS_PDFA_PROPERTY, "pdfaProperty");
            RegisterNamespace(NS_PDFA_TYPE, "pdfaType");
            RegisterNamespace(NS_PDFA_FIELD, "pdfaField");
            RegisterNamespace(NS_PDFA_ID, "pdfaid");
            RegisterNamespace(NS_PDFUA_ID, "pdfuaid");
            RegisterNamespace(NS_PDFA_EXTENSION, "pdfaExtension");
            RegisterNamespace(NS_PHOTOSHOP, "photoshop");
            RegisterNamespace(NS_PSALBUM, "album");
            RegisterNamespace(NS_EXIF, "exif");
            RegisterNamespace(NS_EXIFX, "exifEX");
            RegisterNamespace(NS_EXIF_AUX, "aux");
            RegisterNamespace(NS_TIFF, "tiff");
            RegisterNamespace(NS_PNG, "png");
            RegisterNamespace(NS_JPEG, "jpeg");
            RegisterNamespace(NS_JP2K, "jp2k");
            RegisterNamespace(NS_CAMERARAW, "crs");
            RegisterNamespace(NS_ADOBESTOCKPHOTO, "bmsp");
            RegisterNamespace(NS_CREATOR_ATOM, "creatorAtom");
            RegisterNamespace(NS_ASF, "asf");
            RegisterNamespace(NS_WAV, "wav");
            RegisterNamespace(NS_BWF, "bext");
            RegisterNamespace(NS_RIFFINFO, "riffinfo");
            RegisterNamespace(NS_SCRIPT, "xmpScript");
            RegisterNamespace(NS_TXMP, "txmp");
            RegisterNamespace(NS_SWF, "swf");

            // register Adobe private namespaces
            RegisterNamespace(NS_DM, "xmpDM");
            RegisterNamespace(NS_TRANSIENT, "xmpx");

            // register Adobe standard type namespaces
            RegisterNamespace(TYPE_TEXT, "xmpT");
            RegisterNamespace(TYPE_PAGEDFILE, "xmpTPg");
            RegisterNamespace(TYPE_GRAPHICS, "xmpG");
            RegisterNamespace(TYPE_IMAGE, "xmpGImg");
            RegisterNamespace(TYPE_FONT, "stFnt");
            RegisterNamespace(TYPE_DIMENSIONS, "stDim");
            RegisterNamespace(TYPE_RESOURCEEVENT, "stEvt");
            RegisterNamespace(TYPE_RESOURCEREF, "stRef");
            RegisterNamespace(TYPE_ST_VERSION, "stVer");
            RegisterNamespace(TYPE_ST_JOB, "stJob");
            RegisterNamespace(TYPE_MANIFESTITEM, "stMfs");
            RegisterNamespace(TYPE_IDENTIFIERQUAL, "xmpidq");
        }


        /// <summary>
        /// Associates an alias name with an actual name.
        /// <p>
        /// Define a alias mapping from one namespace/property to another. Both
        /// property names must be simple names. An alias can be a direct mapping,
        /// where the alias and actual have the same data type. It is also possible
        /// to map a simple alias to an item in an array. This can either be to the
        /// first item in the array, or to the 'x-default' item in an alt-text array.
        /// Multiple alias names may map to the same actual, as long as the forms
        /// match. It is a no-op to reregister an alias in an identical fashion.
        /// Note: This method is not locking because only called by RegisterStandardAliases 
        /// which is only called by the constructor.
        /// Note2: The method is only package-private so that it can be tested with unittests 
        /// </summary>
        /// <param name="aliasNs">
        ///            The namespace URI for the alias. Must not be null or the empty
        ///            string. </param>
        /// <param name="aliasProp">
        ///            The name of the alias. Must be a simple name, not null or the
        ///            empty string and not a general path expression. </param>
        /// <param name="actualNs">
        ///            The namespace URI for the actual. Must not be null or the
        ///            empty string. </param>
        /// <param name="actualProp">
        ///            The name of the actual. Must be a simple name, not null or the
        ///            empty string and not a general path expression. </param>
        /// <param name="aliasForm">
        ///            Provides options for aliases for simple aliases to array
        ///            items. This is needed to know what kind of array to create if
        ///            set for the first time via the simple alias. Pass
        ///            <code>XMP_NoOptions</code>, the default value, for all
        ///            direct aliases regardless of whether the actual data type is
        ///            an array or not (see <seealso cref="AliasOptions"/>). </param>
        /// <exception cref="XmpException">
        ///             for inconsistant aliases. </exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RegisterAlias(string aliasNs, string aliasProp, string actualNs, string actualProp,
                                   AliasOptions aliasForm) {
            ParameterAsserts.AssertSchemaNs(aliasNs);
            ParameterAsserts.AssertPropName(aliasProp);
            ParameterAsserts.AssertSchemaNs(actualNs);
            ParameterAsserts.AssertPropName(actualProp);

            // Fix the alias options
            AliasOptions aliasOpts = aliasForm != null
                                         ? new AliasOptions(
                                               XmpNodeUtils.VerifySetOptions(aliasForm.ToPropertyOptions(), null).
                                                   Options)
                                         : new AliasOptions();
            if (_regex.IsMatch(aliasProp) || _regex.IsMatch(actualProp)) {
                throw new XmpException("Alias and actual property names must be simple", XmpError.BADXPATH);
            }

            // check if both namespaces are registered
            string aliasPrefix = GetNamespacePrefix(aliasNs);
            string actualPrefix = GetNamespacePrefix(actualNs);
            if (aliasPrefix == null) {
                throw new XmpException("Alias namespace is not registered", XmpError.BADSCHEMA);
            }
            if (actualPrefix == null) {
                throw new XmpException("Actual namespace is not registered", XmpError.BADSCHEMA);
            }

            string key = aliasPrefix + aliasProp;

            // check if alias is already existing
            if (_aliasMap.Contains(key)) {
                throw new XmpException("Alias is already existing", XmpError.BADPARAM);
            }
            if (_aliasMap.Contains(actualPrefix + actualProp)) {
                throw new XmpException("Actual property is already an alias, use the base property",
                                       XmpError.BADPARAM);
            }

            IXmpAliasInfo aliasInfo = new XmpAliasInfoImpl(actualNs, actualPrefix, actualProp, aliasOpts);

            _aliasMap[key] = aliasInfo;
        }


        /// <summary>
        /// Register the standard aliases.
        /// Note: This method is not lock because only called by the constructor.
        /// </summary>
        /// <exception cref="XmpException"> If the registrations of at least one alias fails. </exception>
        private void RegisterStandardAliases() {
            AliasOptions aliasToArrayOrdered = new AliasOptions();
            aliasToArrayOrdered.ArrayOrdered = true;

            AliasOptions aliasToArrayAltText = new AliasOptions();
            aliasToArrayAltText.ArrayAltText = true;


            // Aliases from XMP to DC.
            RegisterAlias(NS_XMP, "Author", NS_DC, "creator", aliasToArrayOrdered);
            RegisterAlias(NS_XMP, "Authors", NS_DC, "creator", null);
            RegisterAlias(NS_XMP, "Description", NS_DC, "description", null);
            RegisterAlias(NS_XMP, "Format", NS_DC, "format", null);
            RegisterAlias(NS_XMP, "Keywords", NS_DC, "subject", null);
            RegisterAlias(NS_XMP, "Locale", NS_DC, "language", null);
            RegisterAlias(NS_XMP, "Title", NS_DC, "title", null);
            RegisterAlias(NS_XMP_RIGHTS, "Copyright", NS_DC, "rights", null);

            // Aliases from PDF to DC and XMP.
            RegisterAlias(NS_PDF, "Author", NS_DC, "creator", aliasToArrayOrdered);
            RegisterAlias(NS_PDF, "BaseURL", NS_XMP, "BaseURL", null);
            RegisterAlias(NS_PDF, "CreationDate", NS_XMP, "CreateDate", null);
            RegisterAlias(NS_PDF, "Creator", NS_XMP, "CreatorTool", null);
            RegisterAlias(NS_PDF, "ModDate", NS_XMP, "ModifyDate", null);
            RegisterAlias(NS_PDF, "Subject", NS_DC, "description", aliasToArrayAltText);
            RegisterAlias(NS_PDF, "Title", NS_DC, "title", aliasToArrayAltText);

            // Aliases from PHOTOSHOP to DC and XMP.
            RegisterAlias(NS_PHOTOSHOP, "Author", NS_DC, "creator", aliasToArrayOrdered);
            RegisterAlias(NS_PHOTOSHOP, "Caption", NS_DC, "description", aliasToArrayAltText);
            RegisterAlias(NS_PHOTOSHOP, "Copyright", NS_DC, "rights", aliasToArrayAltText);
            RegisterAlias(NS_PHOTOSHOP, "Keywords", NS_DC, "subject", null);
            RegisterAlias(NS_PHOTOSHOP, "Marked", NS_XMP_RIGHTS, "Marked", null);
            RegisterAlias(NS_PHOTOSHOP, "Title", NS_DC, "title", aliasToArrayAltText);
            RegisterAlias(NS_PHOTOSHOP, "WebStatement", NS_XMP_RIGHTS, "WebStatement", null);

            // Aliases from TIFF and EXIF to DC and XMP.
            RegisterAlias(NS_TIFF, "Artist", NS_DC, "creator", aliasToArrayOrdered);
            RegisterAlias(NS_TIFF, "Copyright", NS_DC, "rights", null);
            RegisterAlias(NS_TIFF, "DateTime", NS_XMP, "ModifyDate", null);
            RegisterAlias(NS_TIFF, "ImageDescription", NS_DC, "description", null);
            RegisterAlias(NS_TIFF, "Software", NS_XMP, "CreatorTool", null);

            // Aliases from PNG (Acrobat ImageCapture) to DC and XMP.
            RegisterAlias(NS_PNG, "Author", NS_DC, "creator", aliasToArrayOrdered);
            RegisterAlias(NS_PNG, "Copyright", NS_DC, "rights", aliasToArrayAltText);
            RegisterAlias(NS_PNG, "CreationTime", NS_XMP, "CreateDate", null);
            RegisterAlias(NS_PNG, "Description", NS_DC, "description", aliasToArrayAltText);
            RegisterAlias(NS_PNG, "ModificationTime", NS_XMP, "ModifyDate", null);
            RegisterAlias(NS_PNG, "Software", NS_XMP, "CreatorTool", null);
            RegisterAlias(NS_PNG, "Title", NS_DC, "title", aliasToArrayAltText);
        }

        #region Nested type: XmpAliasInfoImpl

        private class XmpAliasInfoImpl : IXmpAliasInfo {
            private readonly AliasOptions _aliasForm;
            private readonly string _namespace;
            private readonly string _prefix;
            private readonly string _propName;

            public XmpAliasInfoImpl(string @namespace, string prefix, string propName, AliasOptions aliasForm) {
                _namespace = @namespace;
                _prefix = prefix;
                _propName = propName;
                _aliasForm = aliasForm;
            }

            #region IXmpAliasInfo Members

            public String Namespace {
                get { return _namespace; }
            }

            public String Prefix {
                get { return _prefix; }
            }

            public String PropName {
                get { return _propName; }
            }

            public AliasOptions AliasForm {
                get { return _aliasForm; }
            }

            #endregion

            public override String ToString() {
                return Prefix + PropName + " NS(" + Namespace + "), FORM ("
                       + AliasForm + ")";
            }
        }

        #endregion
    }

    public class ReadOnlyDictionary : IDictionary {
        #region ReadOnlyDictionary members

        private readonly IDictionary _originalDictionary;

        private ReadOnlyDictionary(IDictionary original) {
            _originalDictionary = original;
        }

        /// <summary>
        /// Return a read only wrapper to an existing dictionary.
        /// Any change to the underlying dictionary will be 
        /// propagated to the read-only wrapper.
        public static ReadOnlyDictionary ReadOnly(IDictionary dictionary) {
            return new ReadOnlyDictionary(dictionary);
        }

        private void ReportNotSupported() {
            throw new NotSupportedException("Collection is read-only.");
        }

        #endregion

        #region IDictionary Members

        virtual public bool IsReadOnly {
            get { return true; }
        }

        virtual public IDictionaryEnumerator GetEnumerator() {
            return _originalDictionary.GetEnumerator();
        }

        public object this[object key] {
            get { return _originalDictionary[key]; }
            set { throw new NotSupportedException("Collection is read-only."); }
        }

        virtual public void Remove(object key) {
            ReportNotSupported();
        }

        virtual public bool Contains(object key) {
            return _originalDictionary.Contains(key);
        }

        virtual public void Clear() {
            ReportNotSupported();
        }

        virtual public ICollection Values {
            get {
                // no need to wrap with a read-only thing,
                // as ICollection is always read-only
                return _originalDictionary.Values;
            }
        }

        virtual public void Add(object key, object value) {
            ReportNotSupported();
        }

        virtual public ICollection Keys {
            get {
                // no need to wrap with a read-only thing,
                // as ICollection is always read-only
                return _originalDictionary.Keys;
            }
        }

        virtual public bool IsFixedSize {
            get { return _originalDictionary.IsFixedSize; }
        }

        virtual public bool IsSynchronized {
            get { return _originalDictionary.IsSynchronized; }
        }

        virtual public int Count {
            get { return _originalDictionary.Count; }
        }

        virtual public void CopyTo(Array array, int index) {
            _originalDictionary.CopyTo(array, index);
        }

        virtual public object SyncRoot {
            get { return _originalDictionary.SyncRoot; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _originalDictionary.GetEnumerator();
        }

        #endregion
    }
}
