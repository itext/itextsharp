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

using iTextSharp.xmp.properties;

namespace iTextSharp.xmp.impl.xpath {
    /**
     * Parser for XMP XPaths.
     *
     * @since   01.03.2006
     */

    public sealed class XmpPathParser {
        /**
	     * Private constructor
	     */

        internal XmpPathParser() {
            // empty
        }


        /**
	     * Split an XmpPath expression apart at the conceptual steps, adding the
	     * root namespace prefix to the first property component. The schema URI is
	     * put in the first (0th) slot in the expanded XmpPath. Check if the top
	     * level component is an alias, but don't resolve it.
	     * <p>
	     * In the most verbose case steps are separated by '/', and each step can be
	     * of these forms:
	     * <dl>
	     * <dt>prefix:name
	     * <dd> A top level property or struct field.
	     * <dt>[index]
	     * <dd> An element of an array.
	     * <dt>[last()]
	     * <dd> The last element of an array.
	     * <dt>[fieldName=&quot;value&quot;]
	     * <dd> An element in an array of structs, chosen by a field value.
	     * <dt>[@xml:lang=&quot;value&quot;]
	     * <dd> An element in an alt-text array, chosen by the xml:lang qualifier.
	     * <dt>[?qualName=&quot;value&quot;]
	     * <dd> An element in an array, chosen by a qualifier value.
	     * <dt>@xml:lang
	     * <dd> An xml:lang qualifier.
	     * <dt>?qualName
	     * <dd> A general qualifier.
	     * </dl>
	     * <p>
	     * The logic is complicated though by shorthand for arrays, the separating
	     * '/' and leading '*' are optional. These are all equivalent: array/*[2]
	     * array/[2] array*[2] array[2] All of these are broken into the 2 steps
	     * "array" and "[2]".
	     * <p>
	     * The value portion in the array selector forms is a string quoted by '''
	     * or '"'. The value may contain any character including a doubled quoting
	     * character. The value may be empty.
	     * <p>
	     * The syntax isn't checked, but an XML name begins with a letter or '_',
	     * and contains letters, digits, '.', '-', '_', and a bunch of special
	     * non-ASCII Unicode characters. An XML qualified name is a pair of names
	     * separated by a colon.
	     * @param schemaNs
	     *            schema namespace
	     * @param path
	     *            property name
	     * @return Returns the expandet XmpPath.
	     * @throws XmpException
	     *             Thrown if the format is not correct somehow.
	     * 
	     */

        public static XmpPath ExpandXPath(string schemaNs, string path) {
            if (schemaNs == null || path == null) {
                throw new XmpException("Parameter must not be null", XmpError.BADPARAM);
            }

            XmpPath expandedXPath = new XmpPath();
            PathPosition pos = new PathPosition();
            pos.Path = path;

            // Pull out the first component and do some special processing on it: add the schema
            // namespace prefix and and see if it is an alias. The start must be a "qualName".
            ParseRootNode(schemaNs, pos, expandedXPath);

            // Now continue to process the rest of the XmpPath string.
            while (pos.StepEnd < path.Length) {
                pos.StepBegin = pos.StepEnd;

                SkipPathDelimiter(path, pos);

                pos.StepEnd = pos.StepBegin;


                XmpPathSegment segment = path[pos.StepBegin] != '[' ? ParseStructSegment(pos) : ParseIndexSegment(pos);

                if (segment.Kind == XmpPath.STRUCT_FIELD_STEP) {
                    if (segment.Name[0] == '@') {
                        segment.Name = "?" + segment.Name.Substring(1);
                        if (!"?xml:lang".Equals(segment.Name)) {
                            throw new XmpException("Only xml:lang allowed with '@'", XmpError.BADXPATH);
                        }
                    }
                    if (segment.Name[0] == '?') {
                        pos.NameStart++;
                        segment.Kind = XmpPath.QUALIFIER_STEP;
                    }

                    VerifyQualName(pos.Path.Substring(pos.NameStart, pos.NameEnd - pos.NameStart));
                }
                else if (segment.Kind == XmpPath.FIELD_SELECTOR_STEP) {
                    if (segment.Name[1] == '@') {
                        segment.Name = "[?" + segment.Name.Substring(2);
                        if (!segment.Name.StartsWith("[?xml:lang=")) {
                            throw new XmpException("Only xml:lang allowed with '@'", XmpError.BADXPATH);
                        }
                    }

                    if (segment.Name[1] == '?') {
                        pos.NameStart++;
                        segment.Kind = XmpPath.QUAL_SELECTOR_STEP;
                        VerifyQualName(pos.Path.Substring(pos.NameStart, pos.NameEnd - pos.NameStart));
                    }
                }

                expandedXPath.Add(segment);
            }
            return expandedXPath;
        }


        /**
	     * @param path
	     * @param pos
	     * @throws XmpException
	     */

        internal static void SkipPathDelimiter(string path, PathPosition pos) {
            if (path[pos.StepBegin] == '/') {
                // skip slash

                pos.StepBegin++;

                // added for Java
                if (pos.StepBegin >= path.Length) {
                    throw new XmpException("Empty XmpPath segment", XmpError.BADXPATH);
                }
            }

            if (path[pos.StepBegin] == '*') {
                // skip asterisk

                pos.StepBegin++;
                if (pos.StepBegin >= path.Length || path[pos.StepBegin] != '[') {
                    throw new XmpException("Missing '[' after '*'", XmpError.BADXPATH);
                }
            }
        }


        /**
	     * Parses a struct segment
	     * @param pos the current position in the path
	     * @return Retusn the segment or an errror
	     * @throws XmpException If the sement is empty
	     */

        internal static XmpPathSegment ParseStructSegment(PathPosition pos) {
            pos.NameStart = pos.StepBegin;
            while (pos.StepEnd < pos.Path.Length && "/[*".IndexOf(pos.Path[pos.StepEnd]) < 0) {
                pos.StepEnd++;
            }
            pos.NameEnd = pos.StepEnd;

            if (pos.StepEnd == pos.StepBegin) {
                throw new XmpException("Empty XmpPath segment", XmpError.BADXPATH);
            }

            // ! Touch up later, also changing '@' to '?'.
            XmpPathSegment segment = new XmpPathSegment(pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin),
                                                        XmpPath.STRUCT_FIELD_STEP);
            return segment;
        }


        /**
	     * Parses an array index segment.
	     * 
	     * @param pos the xmp path 
	     * @return Returns the segment or an error
	     * @throws XmpException thrown on xmp path errors
	     * 
	     */

        internal static XmpPathSegment ParseIndexSegment(PathPosition pos) {
            XmpPathSegment segment;
            pos.StepEnd++; // Look at the character after the leading '['.

            if ('0' <= pos.Path[pos.StepEnd] && pos.Path[pos.StepEnd] <= '9') {
                // A numeric (decimal integer) array index.
                while (pos.StepEnd < pos.Path.Length && '0' <= pos.Path[pos.StepEnd] && pos.Path[pos.StepEnd] <= '9') {
                    pos.StepEnd++;
                }

                segment = new XmpPathSegment(null, XmpPath.ARRAY_INDEX_STEP);
            }
            else {
                // Could be "[last()]" or one of the selector forms. Find the ']' or '='.

                while (pos.StepEnd < pos.Path.Length && pos.Path[pos.StepEnd] != ']' && pos.Path[pos.StepEnd] != '=') {
                    pos.StepEnd++;
                }

                if (pos.StepEnd >= pos.Path.Length) {
                    throw new XmpException("Missing ']' or '=' for array index", XmpError.BADXPATH);
                }

                if (pos.Path[pos.StepEnd] == ']') {
                    if (!"[last()".Equals(pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin))) {
                        throw new XmpException("Invalid non-numeric array index", XmpError.BADXPATH);
                    }
                    segment = new XmpPathSegment(null, XmpPath.ARRAY_LAST_STEP);
                }
                else {
                    pos.NameStart = pos.StepBegin + 1;
                    pos.NameEnd = pos.StepEnd;
                    pos.StepEnd++; // Absorb the '=', remember the quote.
                    char quote = pos.Path[pos.StepEnd];
                    if (quote != '\'' && quote != '"') {
                        throw new XmpException("Invalid quote in array selector", XmpError.BADXPATH);
                    }

                    pos.StepEnd++; // Absorb the leading quote.
                    while (pos.StepEnd < pos.Path.Length) {
                        if (pos.Path[pos.StepEnd] == quote) {
                            // check for escaped quote
                            if (pos.StepEnd + 1 >= pos.Path.Length || pos.Path[pos.StepEnd + 1] != quote) {
                                break;
                            }
                            pos.StepEnd++;
                        }
                        pos.StepEnd++;
                    }

                    if (pos.StepEnd >= pos.Path.Length) {
                        throw new XmpException("No terminating quote for array selector", XmpError.BADXPATH);
                    }
                    pos.StepEnd++; // Absorb the trailing quote.

                    // ! Touch up later, also changing '@' to '?'.
                    segment = new XmpPathSegment(null, XmpPath.FIELD_SELECTOR_STEP);
                }
            }


            if (pos.StepEnd >= pos.Path.Length || pos.Path[pos.StepEnd] != ']') {
                throw new XmpException("Missing ']' for array index", XmpError.BADXPATH);
            }
            pos.StepEnd++;
            segment.Name = pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin);

            return segment;
        }


        /**
	     * Parses the root node of an XMP Path, checks if namespace and prefix fit together
	     * and resolve the property to the base property if it is an alias. 
	     * @param schemaNs the root namespace
	     * @param pos the parsing position helper
	     * @param expandedXPath  the path to contribute to
	     * @throws XmpException If the path is not valid.
	     */

        internal static void ParseRootNode(string schemaNs, PathPosition pos, XmpPath expandedXPath) {
            while (pos.StepEnd < pos.Path.Length && "/[*".IndexOf(pos.Path[pos.StepEnd]) < 0) {
                pos.StepEnd++;
            }

            if (pos.StepEnd == pos.StepBegin) {
                throw new XmpException("Empty initial XmpPath step", XmpError.BADXPATH);
            }

            string rootProp = VerifyXPathRoot(schemaNs, pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin));
            IXmpAliasInfo aliasInfo = XmpMetaFactory.SchemaRegistry.FindAlias(rootProp);
            if (aliasInfo == null) {
                // add schema xpath step
                expandedXPath.Add(new XmpPathSegment(schemaNs, XmpPath.SCHEMA_NODE));
                XmpPathSegment rootStep = new XmpPathSegment(rootProp, XmpPath.STRUCT_FIELD_STEP);
                expandedXPath.Add(rootStep);
            }
            else {
                // add schema xpath step and base step of alias
                expandedXPath.Add(new XmpPathSegment(aliasInfo.Namespace, XmpPath.SCHEMA_NODE));
                XmpPathSegment rootStep = new XmpPathSegment(VerifyXPathRoot(aliasInfo.Namespace, aliasInfo.PropName),
                                                             XmpPath.STRUCT_FIELD_STEP);
                rootStep.Alias = true;
                rootStep.AliasForm = aliasInfo.AliasForm.Options;
                expandedXPath.Add(rootStep);

                if (aliasInfo.AliasForm.ArrayAltText) {
                    XmpPathSegment qualSelectorStep = new XmpPathSegment("[?xml:lang='x-default']",
                                                                         XmpPath.QUAL_SELECTOR_STEP);
                    qualSelectorStep.Alias = true;
                    qualSelectorStep.AliasForm = aliasInfo.AliasForm.Options;
                    expandedXPath.Add(qualSelectorStep);
                }
                else if (aliasInfo.AliasForm.Array) {
                    XmpPathSegment indexStep = new XmpPathSegment("[1]", XmpPath.ARRAY_INDEX_STEP);
                    indexStep.Alias = true;
                    indexStep.AliasForm = aliasInfo.AliasForm.Options;
                    expandedXPath.Add(indexStep);
                }
            }
        }


        /**
	     * Verifies whether the qualifier name is not XML conformant or the
	     * namespace prefix has not been registered.
	     * 
	     * @param qualName
	     *            a qualifier name
	     * @throws XmpException
	     *             If the name is not conformant
	     */

        internal static void VerifyQualName(string qualName) {
            int colonPos = qualName.IndexOf(':');
            if (colonPos > 0) {
                string prefix = qualName.Substring(0, colonPos);
                if (Utils.IsXmlNameNs(prefix)) {
                    string regUri = XmpMetaFactory.SchemaRegistry.GetNamespaceUri(prefix);
                    if (regUri != null) {
                        return;
                    }

                    throw new XmpException("Unknown namespace prefix for qualified name", XmpError.BADXPATH);
                }
            }

            throw new XmpException("Ill-formed qualified name", XmpError.BADXPATH);
        }


        /**
	     * Verify if an XML name is conformant.
	     * 
	     * @param name
	     *            an XML name
	     * @throws XmpException
	     *             When the name is not XML conformant
	     */

        internal static void VerifySimpleXmlName(string name) {
            if (!Utils.IsXmlName(name)) {
                throw new XmpException("Bad XML name", XmpError.BADXPATH);
            }
        }


        /**
	     * Set up the first 2 components of the expanded XmpPath. Normalizes the various cases of using
	     * the full schema URI and/or a qualified root property name. Returns true for normal
	     * processing. If allowUnknownSchemaNS is true and the schema namespace is not registered, false
	     * is returned. If allowUnknownSchemaNS is false and the schema namespace is not registered, an
	     * exception is thrown
	     * <P>
	     * (Should someday check the full syntax:)
	     * 
	     * @param schemaNs schema namespace
	     * @param rootProp the root xpath segment
	     * @return Returns root QName.
	     * @throws XmpException Thrown if the format is not correct somehow.
	     */

        internal static string VerifyXPathRoot(string schemaNs, string rootProp) {
            // Do some basic checks on the URI and name. Try to lookup the URI. See if the name is
            // qualified.

            if (string.IsNullOrEmpty(schemaNs)) {
                throw new XmpException("Schema namespace URI is required", XmpError.BADSCHEMA);
            }

            if ((rootProp[0] == '?') || (rootProp[0] == '@')) {
                throw new XmpException("Top level name must not be a qualifier", XmpError.BADXPATH);
            }

            if (rootProp.IndexOf('/') >= 0 || rootProp.IndexOf('[') >= 0) {
                throw new XmpException("Top level name must be simple", XmpError.BADXPATH);
            }

            string prefix = XmpMetaFactory.SchemaRegistry.GetNamespacePrefix(schemaNs);
            if (prefix == null) {
                throw new XmpException("Unregistered schema namespace URI", XmpError.BADSCHEMA);
            }

            // Verify the various URI and prefix combinations. Initialize the
            // expanded XmpPath.
            int colonPos = rootProp.IndexOf(':');
            if (colonPos < 0) {
                // The propName is unqualified, use the schemaURI and associated
                // prefix.
                VerifySimpleXmlName(rootProp); // Verify the part before any colon
                return prefix + rootProp;
            }
            // The propName is qualified. Make sure the prefix is legit. Use the associated URI and
            // qualified name.

            // Verify the part before any colon
            VerifySimpleXmlName(rootProp.Substring(0, colonPos));
            VerifySimpleXmlName(rootProp.Substring(colonPos));

            prefix = rootProp.Substring(0, colonPos + 1);

            string regPrefix = XmpMetaFactory.SchemaRegistry.GetNamespacePrefix(schemaNs);
            if (regPrefix == null) {
                throw new XmpException("Unknown schema namespace prefix", XmpError.BADSCHEMA);
            }
            if (!prefix.Equals(regPrefix)) {
                throw new XmpException("Schema namespace URI and prefix mismatch", XmpError.BADSCHEMA);
            }

            return rootProp;
        }
    }


    /**
     * This objects contains all needed char positions to parse.
     */

    public class PathPosition {
        /** the complete path */
        /** the end of a segment name */
        internal int NameEnd;
        internal int NameStart;
        internal string Path;
        /** the begin of a step */
        internal int StepBegin;
        /** the end of a step */
        internal int StepEnd;
    }
}
