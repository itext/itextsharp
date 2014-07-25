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

using System.Collections;
using System.Diagnostics;
using System.Text;
using iTextSharp.xmp.impl.xpath;
using iTextSharp.xmp.options;
using iTextSharp.xmp.properties;

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// @since 11.08.2006
    /// </summary>
    public class XmpUtilsImpl : XmpConst {
        private const int UCK_NORMAL = 0;
        private const int UCK_SPACE = 1;
        private const int UCK_COMMA = 2;
        private const int UCK_SEMICOLON = 3;
        private const int UCK_QUOTE = 4;
        private const int UCK_CONTROL = 5;

        /// <summary>
        /// U+0022 ASCII space<br>
        /// U+3000, ideographic space<br>
        /// U+303F, ideographic half fill space<br>
        /// U+2000..U+200B, en quad through zero width space
        /// </summary>
        private const string SPACES = "\u0020\u3000\u303F";

        /// <summary>
        /// U+002C, ASCII comma<br>
        /// U+FF0C, full width comma<br>
        /// U+FF64, half width ideographic comma<br>
        /// U+FE50, small comma<br>
        /// U+FE51, small ideographic comma<br>
        /// U+3001, ideographic comma<br>
        /// U+060C, Arabic comma<br>
        /// U+055D, Armenian comma
        /// </summary>
        private const string COMMAS = "\u002C\uFF0C\uFF64\uFE50\uFE51\u3001\u060C\u055D";

        /// <summary>
        /// U+003B, ASCII semicolon<br>
        /// U+FF1B, full width semicolon<br>
        /// U+FE54, small semicolon<br>
        /// U+061B, Arabic semicolon<br>
        /// U+037E, Greek "semicolon" (really a question mark)
        /// </summary>
        private const string SEMICOLA = "\u003B\uFF1B\uFE54\u061B\u037E";

        /// <summary>
        /// U+0000..U+001F ASCII controls<br>
        /// U+2028, line separator.<br>
        /// U+2029, paragraph separator.
        /// </summary>
        private const string CONTROLS = "\u2028\u2029";

        /// <summary>
        /// U+0022 ASCII quote<br>
        /// The square brackets are not interpreted as quotes anymore (bug #2674672)
        /// (ASCII '[' (0x5B) and ']' (0x5D) are used as quotes in Chinese and
        /// Korean.)<br>
        /// U+00AB and U+00BB, guillemet quotes<br>
        /// U+3008..U+300F, various quotes.<br>
        /// U+301D..U+301F, double prime quotes.<br>
        /// U+2015, dash quote.<br>
        /// U+2018..U+201F, various quotes.<br>
        /// U+2039 and U+203A, guillemet quotes.
        /// </summary>
        private const string QUOTES = "\"\u00AB\u00BB\u301D\u301E\u301F\u2015\u2039\u203A";


        /// <summary>
        /// Private constructor, as
        /// </summary>
        private XmpUtilsImpl() {
            // EMPTY
        }


        /// <seealso cref= XMPUtils#catenateArrayItems(XMPMeta, String, String, String, String,
        ///      boolean)
        /// </seealso>
        /// <param name="xmp">
        ///            The XMP object containing the array to be catenated. </param>
        /// <param name="schemaNs">
        ///            The schema namespace URI for the array. Must not be null or
        ///            the empty string. </param>
        /// <param name="arrayName">
        ///            The name of the array. May be a general path expression, must
        ///            not be null or the empty string. Each item in the array must
        ///            be a simple string value. </param>
        /// <param name="separator">
        ///            The string to be used to separate the items in the catenated
        ///            string. Defaults to &quot;; &quot;, ASCII semicolon and space
        ///            (U+003B, U+0020). </param>
        /// <param name="quotes">
        ///            The characters to be used as quotes around array items that
        ///            contain a separator. Defaults to &apos;&quot;&apos; </param>
        /// <param name="allowCommas">
        ///            Option flag to control the catenation. </param>
        /// <returns> Returns the string containing the catenated array items. </returns>
        /// <exception cref="XmpException">
        ///             Forwards the Exceptions from the metadata processing </exception>
        public static string CatenateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string separator,
                                                string quotes, bool allowCommas) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            ParameterAsserts.AssertImplementation(xmp);
            if (string.IsNullOrEmpty(separator)) {
                separator = "; ";
            }
            if (string.IsNullOrEmpty(quotes)) {
                quotes = "\"";
            }

            XmpMetaImpl xmpImpl = (XmpMetaImpl) xmp;

            // Return an empty result if the array does not exist, 
            // hurl if it isn't the right form.
            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(xmpImpl.Root, arrayPath, false, null);
            if (arrayNode == null) {
                return "";
            }
            if (!arrayNode.Options.Array || arrayNode.Options.ArrayAlternate) {
                throw new XmpException("Named property must be non-alternate array", XmpError.BADPARAM);
            }

            // Make sure the separator is OK.
            CheckSeparator(separator);
            // Make sure the open and close quotes are a legitimate pair.
            char openQuote = quotes[0];
            char closeQuote = CheckQuotes(quotes, openQuote);

            // Build the result, quoting the array items, adding separators.
            // Hurl if any item isn't simple.

            StringBuilder catinatedString = new StringBuilder();

            for (IEnumerator it = arrayNode.IterateChildren(); it.MoveNext();) {
                XmpNode currItem = (XmpNode)it.Current;
                if (currItem == null)
                    continue;
                if (currItem.Options.CompositeProperty) {
                    throw new XmpException("Array items must be simple", XmpError.BADPARAM);
                }
                string str = ApplyQuotes(currItem.Value, openQuote, closeQuote, allowCommas);

                catinatedString.Append(str);
                if (it.MoveNext()) {
                    catinatedString.Append(separator);
                }
            }

            return catinatedString.ToString();
        }


        /// <summary>
        /// see {@link XMPUtils#separateArrayItems(XMPMeta, String, String, String, 
        /// PropertyOptions, boolean)}
        /// </summary>
        /// <param name="xmp">
        ///            The XMP object containing the array to be updated. </param>
        /// <param name="schemaNs">
        ///            The schema namespace URI for the array. Must not be null or
        ///            the empty string. </param>
        /// <param name="arrayName">
        ///            The name of the array. May be a general path expression, must
        ///            not be null or the empty string. Each item in the array must
        ///            be a simple string value. </param>
        /// <param name="catedStr">
        ///            The string to be separated into the array items. </param>
        /// <param name="arrayOptions">
        ///            Option flags to control the separation. </param>
        /// <param name="preserveCommas">
        ///            Flag if commas shall be preserved
        /// </param>
        /// <exception cref="XmpException">
        ///             Forwards the Exceptions from the metadata processing </exception>
        public static void SeparateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string catedStr,
                                              PropertyOptions arrayOptions, bool preserveCommas) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            if (catedStr == null) {
                throw new XmpException("Parameter must not be null", XmpError.BADPARAM);
            }
            ParameterAsserts.AssertImplementation(xmp);
            XmpMetaImpl xmpImpl = (XmpMetaImpl) xmp;

            // Keep a zero value, has special meaning below.
            XmpNode arrayNode = SeparateFindCreateArray(schemaNs, arrayName, arrayOptions, xmpImpl);

            // Extract the item values one at a time, until the whole input string is done.
            int charKind = UCK_NORMAL;
            char ch = (char) 0;

            int itemEnd = 0;
            int endPos = catedStr.Length;
            while (itemEnd < endPos) {
                
                string itemValue;
                int itemStart;
                // Skip any leading spaces and separation characters. Always skip commas here.
                // They can be kept when within a value, but not when alone between values.
                for (itemStart = itemEnd; itemStart < endPos; itemStart++) {
                    ch = catedStr[itemStart];
                    charKind = ClassifyCharacter(ch);
                    if (charKind == UCK_NORMAL || charKind == UCK_QUOTE) {
                        break;
                    }
                }
                if (itemStart >= endPos) {
                    break;
                }
                int nextKind;
                if (charKind != UCK_QUOTE) {
                    // This is not a quoted value. Scan for the end, create an array
                    // item from the substring.
                    for (itemEnd = itemStart; itemEnd < endPos; itemEnd++) {
                        ch = catedStr[itemEnd];
                        charKind = ClassifyCharacter(ch);

                        if (charKind == UCK_NORMAL || charKind == UCK_QUOTE || (charKind == UCK_COMMA && preserveCommas)) {
                            continue;
                        }
                        if (charKind != UCK_SPACE) {
                            break;
                        }
                        if ((itemEnd + 1) < endPos) {
                            ch = catedStr[itemEnd + 1];
                            nextKind = ClassifyCharacter(ch);
                            if (nextKind == UCK_NORMAL || nextKind == UCK_QUOTE ||
                                (nextKind == UCK_COMMA && preserveCommas)) {
                                continue;
                            }
                        }

                        // Anything left?
                        break; // Have multiple spaces, or a space followed by a
                        // separator.
                    }
                    itemValue = catedStr.Substring(itemStart, itemEnd - itemStart);
                }
                else {
                    // Accumulate quoted values into a local string, undoubling
                    // internal quotes that
                    // match the surrounding quotes. Do not undouble "unmatching"
                    // quotes.
                    
                    char openQuote = ch;
                    char closeQuote = GetClosingQuote(openQuote);

                    itemStart++; // Skip the opening quote;
                    itemValue = "";

                    for (itemEnd = itemStart; itemEnd < endPos; itemEnd++) {
                        ch = catedStr[itemEnd];
                        charKind = ClassifyCharacter(ch);

                        if (charKind != UCK_QUOTE || !IsSurroundingQuote(ch, openQuote, closeQuote)) {
                            // This is not a matching quote, just append it to the
                            // item value.
                            itemValue += ch;
                        }
                        else {
                            // This is a "matching" quote. Is it doubled, or the
                            // final closing quote?
                            // Tolerate various edge cases like undoubled opening
                            // (non-closing) quotes,
                            // or end of input.
                            char nextChar;
                            if ((itemEnd + 1) < endPos) {
                                nextChar = catedStr[itemEnd + 1];
                                nextKind = ClassifyCharacter(nextChar);
                            }
                            else {
                                nextKind = UCK_SEMICOLON;
                                nextChar = (char) 0x3B;
                            }

                            if (ch == nextChar) {
                                // This is doubled, copy it and skip the double.
                                itemValue += ch;
                                // Loop will add in charSize.
                                itemEnd++;
                            }
                            else if (!IsClosingingQuote(ch, openQuote, closeQuote)) {
                                // This is an undoubled, non-closing quote, copy it.
                                itemValue += ch;
                            }
                            else {
                                // This is an undoubled closing quote, skip it and
                                // exit the loop.
                                itemEnd++;
                                break;
                            }
                        }
                    }
                }

                // Add the separated item to the array. 
                // Keep a matching old value in case it had separators.
                int foundIndex = -1;
                for (int oldChild = 1; oldChild <= arrayNode.ChildrenLength; oldChild++) {
                    if (itemValue.Equals(arrayNode.GetChild(oldChild).Value)) {
                        foundIndex = oldChild;
                        break;
                    }
                }

                if (foundIndex < 0) {
                    XmpNode newItem = new XmpNode(ARRAY_ITEM_NAME, itemValue, null);
                    arrayNode.AddChild(newItem);
                }
            }
        }


        /// <summary>
        /// Utility to find or create the array used by <code>separateArrayItems()</code>. </summary>
        /// <param name="schemaNs"> a the namespace fo the array </param>
        /// <param name="arrayName"> the name of the array </param>
        /// <param name="arrayOptions"> the options for the array if newly created </param>
        /// <param name="xmp"> the xmp object </param>
        /// <returns> Returns the array node. </returns>
        /// <exception cref="XmpException"> Forwards exceptions </exception>
        private static XmpNode SeparateFindCreateArray(string schemaNs, string arrayName, PropertyOptions arrayOptions,
                                                       XmpMetaImpl xmp) {
            arrayOptions = XmpNodeUtils.VerifySetOptions(arrayOptions, null);
            if (!arrayOptions.OnlyArrayOptions) {
                throw new XmpException("Options can only provide array form", XmpError.BADOPTIONS);
            }

            // Find the array node, make sure it is OK. Move the current children
            // aside, to be readded later if kept.
            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(xmp.Root, arrayPath, false, null);
            if (arrayNode != null) {
                // The array exists, make sure the form is compatible. Zero
                // arrayForm means take what exists.
                PropertyOptions arrayForm = arrayNode.Options;
                if (!arrayForm.Array || arrayForm.ArrayAlternate) {
                    throw new XmpException("Named property must be non-alternate array", XmpError.BADXPATH);
                }
                if (arrayOptions.EqualArrayTypes(arrayForm)) {
                    throw new XmpException("Mismatch of specified and existing array form", XmpError.BADXPATH);
                    // *** Right error?
                }
            }
            else {
                // The array does not exist, try to create it.
                // don't modify the options handed into the method
                arrayOptions.Array = true;
                arrayNode = XmpNodeUtils.FindNode(xmp.Root, arrayPath, true, arrayOptions);
                if (arrayNode == null) {
                    throw new XmpException("Failed to create named array", XmpError.BADXPATH);
                }
            }
            return arrayNode;
        }


        /// <seealso cref= XMPUtils#removeProperties(XMPMeta, String, String, boolean, boolean)
        /// </seealso>
        /// <param name="xmp">
        ///            The XMP object containing the properties to be removed.
        /// </param>
        /// <param name="schemaNs">
        ///            Optional schema namespace URI for the properties to be
        ///            removed.
        /// </param>
        /// <param name="propName">
        ///            Optional path expression for the property to be removed.
        /// </param>
        /// <param name="doAllProperties">
        ///            Option flag to control the deletion: do internal properties in
        ///            addition to external properties. </param>
        /// <param name="includeAliases">
        ///            Option flag to control the deletion: Include aliases in the
        ///            "named schema" case above. </param>
        /// <exception cref="XmpException"> If metadata processing fails </exception>
        public static void RemoveProperties(IXmpMeta xmp, string schemaNs, string propName, bool doAllProperties,
                                            bool includeAliases) {
            ParameterAsserts.AssertImplementation(xmp);
            XmpMetaImpl xmpImpl = (XmpMetaImpl) xmp;

            if (!string.IsNullOrEmpty(propName)) {
                // Remove just the one indicated property. This might be an alias,
                // the named schema might not actually exist. So don't lookup the
                // schema node.

                if (string.IsNullOrEmpty(schemaNs)) {
                    throw new XmpException("Property name requires schema namespace", XmpError.BADPARAM);
                }

                XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);

                XmpNode propNode = XmpNodeUtils.FindNode(xmpImpl.Root, expPath, false, null);
                if (propNode != null) {
                    if (doAllProperties ||
                        !Utils.IsInternalProperty(expPath.GetSegment((int) XmpPath.STEP_SCHEMA).Name,
                                                  expPath.GetSegment((int) XmpPath.STEP_ROOT_PROP).Name)) {
                        XmpNode parent = propNode.Parent;
                        parent.RemoveChild(propNode);
                        if (parent.Options.SchemaNode && !parent.HasChildren()) {
                            // remove empty schema node
                            parent.Parent.RemoveChild(parent);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(schemaNs)) {
                // Remove all properties from the named schema. Optionally include
                // aliases, in which case
                // there might not be an actual schema node.

                // XMP_NodePtrPos schemaPos;
                XmpNode schemaNode = XmpNodeUtils.FindSchemaNode(xmpImpl.Root, schemaNs, false);
                if (schemaNode != null) {
                    if (RemoveSchemaChildren(schemaNode, doAllProperties)) {
                        xmpImpl.Root.RemoveChild(schemaNode);
                    }
                }

                if (includeAliases) {
                    // We're removing the aliases also. Look them up by their
                    // namespace prefix.
                    // But that takes more code and the extra speed isn't worth it.
                    // Lookup the XMP node
                    // from the alias, to make sure the actual exists.

                    IXmpAliasInfo[] aliases = XmpMetaFactory.SchemaRegistry.FindAliases(schemaNs);
                    for (int i = 0; i < aliases.Length; i++) {
                        IXmpAliasInfo info = aliases[i];
                        XmpPath path = XmpPathParser.ExpandXPath(info.Namespace, info.PropName);
                        XmpNode actualProp = XmpNodeUtils.FindNode(xmpImpl.Root, path, false, null);
                        if (actualProp != null) {
                            XmpNode parent = actualProp.Parent;
                            parent.RemoveChild(actualProp);
                        }
                    }
                }
            }
            else {
                // Remove all appropriate properties from all schema. In this case
                // we don't have to be
                // concerned with aliases, they are handled implicitly from the
                // actual properties.
                ArrayList schemasToRemove = new ArrayList();
                for (IEnumerator it = xmpImpl.Root.IterateChildren(); it.MoveNext();) {
                    XmpNode schema = (XmpNode) it.Current;
                    if (schema == null)
                        continue;
                    if (RemoveSchemaChildren(schema, doAllProperties)) {
                        schemasToRemove.Add(schema);
                    }
                }
                foreach (XmpNode xmpNode in schemasToRemove) {
                    xmpImpl.Root.Children.Remove(xmpNode);
                }
                schemasToRemove.Clear();
            }
        }


        /// <seealso cref= XMPUtils#appendProperties(XMPMeta, XMPMeta, boolean, boolean) </seealso>
        /// <param name="source"> The source XMP object. </param>
        /// <param name="destination"> The destination XMP object. </param>
        /// <param name="doAllProperties"> Do internal properties in addition to external properties. </param>
        /// <param name="replaceOldValues"> Replace the values of existing properties. </param>
        /// <param name="deleteEmptyValues"> Delete destination values if source property is empty. </param>
        /// <exception cref="XmpException"> Forwards the Exceptions from the metadata processing </exception>
        public static void AppendProperties(IXmpMeta source, IXmpMeta destination, bool doAllProperties,
                                            bool replaceOldValues, bool deleteEmptyValues) {
            ParameterAsserts.AssertImplementation(source);
            ParameterAsserts.AssertImplementation(destination);

            XmpMetaImpl src = (XmpMetaImpl) source;
            XmpMetaImpl dest = (XmpMetaImpl) destination;

            for (IEnumerator it = src.Root.IterateChildren(); it.MoveNext();) {
                XmpNode sourceSchema = (XmpNode) it.Current;
                if (sourceSchema == null)
                    continue;
                // Make sure we have a destination schema node
                XmpNode destSchema = XmpNodeUtils.FindSchemaNode(dest.Root, sourceSchema.Name, false);
                bool createdSchema = false;
                if (destSchema == null) {
                    PropertyOptions propertyOptions = new PropertyOptions();
                    propertyOptions.SchemaNode = true;
                    destSchema = new XmpNode(sourceSchema.Name, sourceSchema.Value,
                                             propertyOptions);
                    dest.Root.AddChild(destSchema);
                    createdSchema = true;
                }

                // Process the source schema's children.			
                for (IEnumerator ic = sourceSchema.IterateChildren(); ic.MoveNext();) {
                    XmpNode sourceProp = (XmpNode) ic.Current;
                    if (sourceProp == null)
                        continue;
                    if (doAllProperties || !Utils.IsInternalProperty(sourceSchema.Name, sourceProp.Name)) {
                        AppendSubtree(dest, sourceProp, destSchema, replaceOldValues, deleteEmptyValues);
                    }
                }

                if (!destSchema.HasChildren() && (createdSchema || deleteEmptyValues)) {
                    // Don't create an empty schema / remove empty schema.
                    dest.Root.RemoveChild(destSchema);
                }
            }
        }


        /// <summary>
        /// Remove all schema children according to the flag
        /// <code>doAllProperties</code>. Empty schemas are automatically remove
        /// by <code>XMPNode</code>
        /// </summary>
        /// <param name="schemaNode">
        ///            a schema node </param>
        /// <param name="doAllProperties">
        ///            flag if all properties or only externals shall be removed. </param>
        /// <returns> Returns true if the schema is empty after the operation. </returns>
        private static bool RemoveSchemaChildren(XmpNode schemaNode, bool doAllProperties) {
            ArrayList currPropsToRemove = new ArrayList();
            for (IEnumerator it = schemaNode.IterateChildren(); it.MoveNext();) {
                XmpNode currProp = (XmpNode) it.Current;
                if (currProp == null)
                    continue;
                if (doAllProperties || !Utils.IsInternalProperty(schemaNode.Name, currProp.Name)) {
                    currPropsToRemove.Add(currProp);
                }
            }
            foreach (XmpNode xmpNode in currPropsToRemove) {
                schemaNode.Children.Remove(xmpNode);
            }
            currPropsToRemove.Clear();
            return !schemaNode.HasChildren();
        }


        /// <seealso cref= XMPUtilsImpl#appendProperties(XMPMeta, XMPMeta, boolean, boolean, boolean) </seealso>
        /// <param name="destXmp"> The destination XMP object. </param>
        /// <param name="sourceNode"> the source node </param>
        /// <param name="destParent"> the parent of the destination node </param>
        /// <param name="replaceOldValues"> Replace the values of existing properties. </param>
        /// <param name="deleteEmptyValues"> flag if properties with empty values should be deleted 
        /// 		   in the destination object. </param>
        /// <exception cref="XmpException"> </exception>
        private static void AppendSubtree(XmpMetaImpl destXmp, XmpNode sourceNode, XmpNode destParent,
                                          bool replaceOldValues, bool deleteEmptyValues) {
            XmpNode destNode = XmpNodeUtils.FindChildNode(destParent, sourceNode.Name, false);

            bool valueIsEmpty = false;
            if (deleteEmptyValues) {
                valueIsEmpty = sourceNode.Options.Simple
                                   ? string.IsNullOrEmpty(sourceNode.Value)
                                   : !sourceNode.HasChildren();
            }

            if (deleteEmptyValues && valueIsEmpty) {
                if (destNode != null) {
                    destParent.RemoveChild(destNode);
                }
            }
            else if (destNode == null) {
                // The one easy case, the destination does not exist.
                destParent.AddChild((XmpNode) sourceNode.Clone());
            }
            else if (replaceOldValues) {
                // The destination exists and should be replaced.
                destXmp.SetNode(destNode, sourceNode.Value, sourceNode.Options, true);
                destParent.RemoveChild(destNode);
                destNode = (XmpNode) sourceNode.Clone();
                destParent.AddChild(destNode);
            }
            else {
                // The destination exists and is not totally replaced. Structs and
                // arrays are merged.

                PropertyOptions sourceForm = sourceNode.Options;
                PropertyOptions destForm = destNode.Options;
                if (sourceForm != destForm) {
                    return;
                }
                if (sourceForm.Struct) {
                    // To merge a struct process the fields recursively. E.g. add simple missing fields.
                    // The recursive call to AppendSubtree will handle deletion for fields with empty 
                    // values.
                    for (IEnumerator it = sourceNode.IterateChildren(); it.MoveNext();) {
                        XmpNode sourceField = (XmpNode) it.Current;
                        if (sourceField == null)
                            continue;
                        AppendSubtree(destXmp, sourceField, destNode, replaceOldValues, deleteEmptyValues);
                        if (deleteEmptyValues && !destNode.HasChildren()) {
                            destParent.RemoveChild(destNode);
                        }
                    }
                }
                else if (sourceForm.ArrayAltText) {
                    // Merge AltText arrays by the "xml:lang" qualifiers. Make sure x-default is first. 
                    // Make a special check for deletion of empty values. Meaningful in AltText arrays 
                    // because the "xml:lang" qualifier provides unambiguous source/dest correspondence.
                    for (IEnumerator it = sourceNode.IterateChildren(); it.MoveNext();) {
                        XmpNode sourceItem = (XmpNode) it.Current;
                        if (sourceItem == null)
                            continue;
                        if (!sourceItem.HasQualifier() || !XML_LANG.Equals(sourceItem.GetQualifier(1).Name)) {
                            continue;
                        }

                        int destIndex = XmpNodeUtils.LookupLanguageItem(destNode, sourceItem.GetQualifier(1).Value);
                        if (deleteEmptyValues && (string.IsNullOrEmpty(sourceItem.Value))) {
                            if (destIndex != -1) {
                                destNode.RemoveChild(destIndex);
                                if (!destNode.HasChildren()) {
                                    destParent.RemoveChild(destNode);
                                }
                            }
                        }
                        else if (destIndex == -1) {
                            // Not replacing, keep the existing item.						
                            if (!X_DEFAULT.Equals(sourceItem.GetQualifier(1).Value) || !destNode.HasChildren()) {
                                sourceItem.CloneSubtree(destNode);
                            }
                            else {
                                XmpNode destItem = new XmpNode(sourceItem.Name, sourceItem.Value, sourceItem.Options);
                                sourceItem.CloneSubtree(destItem);
                                destNode.AddChild(1, destItem);
                            }
                        }
                    }
                }
                else if (sourceForm.Array) {
                    // Merge other arrays by item values. Don't worry about order or duplicates. Source 
                    // items with empty values do not cause deletion, that conflicts horribly with 
                    // merging.

                    for (IEnumerator @is = sourceNode.IterateChildren(); @is.MoveNext();) {
                        XmpNode sourceItem = (XmpNode) @is.Current;
                        if (sourceItem == null)
                            continue;
                        bool match = false;
                        for (IEnumerator id = destNode.IterateChildren(); id.MoveNext();) {
                            XmpNode destItem = (XmpNode) id.Current;
                            if (destItem == null)
                                continue;
                            if (ItemValuesMatch(sourceItem, destItem)) {
                                match = true;
                            }
                        }
                        if (!match) {
                            destNode = (XmpNode) sourceItem.Clone();
                            destParent.AddChild(destNode);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Compares two nodes including its children and qualifier. </summary>
        /// <param name="leftNode"> an <code>XMPNode</code> </param>
        /// <param name="rightNode"> an <code>XMPNode</code> </param>
        /// <returns> Returns true if the nodes are equal, false otherwise. </returns>
        /// <exception cref="XmpException"> Forwards exceptions to the calling method. </exception>
        private static bool ItemValuesMatch(XmpNode leftNode, XmpNode rightNode) {
            PropertyOptions leftForm = leftNode.Options;
            PropertyOptions rightForm = rightNode.Options;

            if (leftForm.Equals(rightForm)) {
                return false;
            }

            if (leftForm.Options == 0) {
                // Simple nodes, check the values and xml:lang qualifiers.
                if (!leftNode.Value.Equals(rightNode.Value)) {
                    return false;
                }
                if (leftNode.Options.HasLanguage != rightNode.Options.HasLanguage) {
                    return false;
                }
                if (leftNode.Options.HasLanguage &&
                    !leftNode.GetQualifier(1).Value.Equals(rightNode.GetQualifier(1).Value)) {
                    return false;
                }
            }
            else if (leftForm.Struct) {
                // Struct nodes, see if all fields match, ignoring order.

                if (leftNode.ChildrenLength != rightNode.ChildrenLength) {
                    return false;
                }

                for (IEnumerator it = leftNode.IterateChildren(); it.MoveNext();) {
                    XmpNode leftField = (XmpNode) it.Current;
                    if (leftField == null)
                        continue;
                    XmpNode rightField = XmpNodeUtils.FindChildNode(rightNode, leftField.Name, false);
                    if (rightField == null || !ItemValuesMatch(leftField, rightField)) {
                        return false;
                    }
                }
            }
            else {
                // Array nodes, see if the "leftNode" values are present in the
                // "rightNode", ignoring order, duplicates,
                // and extra values in the rightNode-> The rightNode is the
                // destination for AppendProperties.

                Debug.Assert(leftForm.Array);

                for (IEnumerator il = leftNode.IterateChildren(); il.MoveNext();) {
                    XmpNode leftItem = (XmpNode) il.Current;
                    if (leftItem == null)
                        continue;

                    bool match = false;
                    for (IEnumerator ir = rightNode.IterateChildren(); ir.MoveNext();) {
                        XmpNode rightItem = (XmpNode) ir.Current;
                        if (rightItem == null)
                            continue;
                        if (ItemValuesMatch(leftItem, rightItem)) {
                            match = true;
                            break;
                        }
                    }
                    if (!match) {
                        return false;
                    }
                }
            }
            return true; // All of the checks passed.
        }


        /// <summary>
        /// Make sure the separator is OK. It must be one semicolon surrounded by
        /// zero or more spaces. Any of the recognized semicolons or spaces are
        /// allowed.
        /// </summary>
        /// <param name="separator"> </param>
        /// <exception cref="XmpException"> </exception>
        private static void CheckSeparator(string separator) {
            bool haveSemicolon = false;
            for (int i = 0; i < separator.Length; i++) {
                int charKind = ClassifyCharacter(separator[i]);
                if (charKind == UCK_SEMICOLON) {
                    if (haveSemicolon) {
                        throw new XmpException("Separator can have only one semicolon", XmpError.BADPARAM);
                    }
                    haveSemicolon = true;
                }
                else if (charKind != UCK_SPACE) {
                    throw new XmpException("Separator can have only spaces and one semicolon",
                                           XmpError.BADPARAM);
                }
            }
            if (!haveSemicolon) {
                throw new XmpException("Separator must have one semicolon", XmpError.BADPARAM);
            }
        }


        /// <summary>
        /// Make sure the open and close quotes are a legitimate pair and return the
        /// correct closing quote or an exception.
        /// </summary>
        /// <param name="quotes">
        ///            opened and closing quote in a string </param>
        /// <param name="openQuote">
        ///            the open quote </param>
        /// <returns> Returns a corresponding closing quote. </returns>
        /// <exception cref="XmpException"> </exception>
        private static char CheckQuotes(string quotes, char openQuote) {
            char closeQuote;

            int charKind = ClassifyCharacter(openQuote);
            if (charKind != UCK_QUOTE) {
                throw new XmpException("Invalid quoting character", XmpError.BADPARAM);
            }

            if (quotes.Length == 1) {
                closeQuote = openQuote;
            }
            else {
                closeQuote = quotes[1];
                charKind = ClassifyCharacter(closeQuote);
                if (charKind != UCK_QUOTE) {
                    throw new XmpException("Invalid quoting character", XmpError.BADPARAM);
                }
            }

            if (closeQuote != GetClosingQuote(openQuote)) {
                throw new XmpException("Mismatched quote pair", XmpError.BADPARAM);
            }
            return closeQuote;
        }


        /// <summary>
        /// Classifies the character into normal chars, spaces, semicola, quotes,
        /// control chars.
        /// </summary>
        /// <param name="ch">
        ///            a char </param>
        /// <returns> Return the character kind. </returns>
        private static int ClassifyCharacter(char ch) {
            if (SPACES.IndexOf(ch) >= 0 || (0x2000 <= ch && ch <= 0x200B)) {
                return UCK_SPACE;
            }
            if (COMMAS.IndexOf(ch) >= 0) {
                return UCK_COMMA;
            }
            if (SEMICOLA.IndexOf(ch) >= 0) {
                return UCK_SEMICOLON;
            }
            if (QUOTES.IndexOf(ch) >= 0 || (0x3008 <= ch && ch <= 0x300F) || (0x2018 <= ch && ch <= 0x201F)) {
                return UCK_QUOTE;
            }
            if (ch < 0x0020 || CONTROLS.IndexOf(ch) >= 0) {
                return UCK_CONTROL;
            }
            // Assume typical case.
            return UCK_NORMAL;
        }


        /// <param name="openQuote">
        ///            the open quote char </param>
        /// <returns> Returns the matching closing quote for an open quote. </returns>
        private static char GetClosingQuote(char openQuote) {
            switch ((int) openQuote) {
                case 0x0022:
                    return (char) 0x0022; // ! U+0022 is both opening and closing.
                    //		Not interpreted as brackets anymore
                    //		case 0x005B: 
                    //			return 0x005D;
                case 0x00AB:
                    return (char) 0x00BB; // ! U+00AB and U+00BB are reversible.
                case 0x00BB:
                    return (char) 0x00AB;
                case 0x2015:
                    return (char) 0x2015; // ! U+2015 is both opening and closing.
                case 0x2018:
                    return (char) 0x2019;
                case 0x201A:
                    return (char) 0x201B;
                case 0x201C:
                    return (char) 0x201D;
                case 0x201E:
                    return (char) 0x201F;
                case 0x2039:
                    return (char) 0x203A; // ! U+2039 and U+203A are reversible.
                case 0x203A:
                    return (char) 0x2039;
                case 0x3008:
                    return (char) 0x3009;
                case 0x300A:
                    return (char) 0x300B;
                case 0x300C:
                    return (char) 0x300D;
                case 0x300E:
                    return (char) 0x300F;
                case 0x301D:
                    return (char) 0x301F; // ! U+301E also closes U+301D.
                default:
                    return (char) 0;
            }
        }


        /// <summary>
        /// Add quotes to the item.
        /// </summary>
        /// <param name="item">
        ///            the array item </param>
        /// <param name="openQuote">
        ///            the open quote character </param>
        /// <param name="closeQuote">
        ///            the closing quote character </param>
        /// <param name="allowCommas">
        ///            flag if commas are allowed </param>
        /// <returns> Returns the value in quotes. </returns>
        private static string ApplyQuotes(string item, char openQuote, char closeQuote, bool allowCommas) {
            if (item == null) {
                item = "";
            }

            bool prevSpace = false;

            // See if there are any separators in the value. Stop at the first
            // occurrance. This is a bit
            // tricky in order to make typical typing work conveniently. The purpose
            // of applying quotes
            // is to preserve the values when splitting them back apart. That is
            // CatenateContainerItems
            // and SeparateContainerItems must round trip properly. For the most
            // part we only look for
            // separators here. Internal quotes, as in -- Irving "Bud" Jones --
            // won't cause problems in
            // the separation. An initial quote will though, it will make the value
            // look quoted.

            int i;
            for (i = 0; i < item.Length; i++) {
                char ch = item[i];
                int charKind = ClassifyCharacter(ch);
                if (i == 0 && charKind == UCK_QUOTE) {
                    break;
                }

                if (charKind == UCK_SPACE) {
                    // Multiple spaces are a separator.
                    if (prevSpace) {
                        break;
                    }
                    prevSpace = true;
                }
                else {
                    prevSpace = false;
                    if ((charKind == UCK_SEMICOLON || charKind == UCK_CONTROL) ||
                        (charKind == UCK_COMMA && !allowCommas)) {
                        break;
                    }
                }
            }


            if (i < item.Length) {
                // Create a quoted copy, doubling any internal quotes that match the
                // outer ones. Internal quotes did not stop the "needs quoting"
                // search, but they do need
                // doubling. So we have to rescan the front of the string for
                // quotes. Handle the special
                // case of U+301D being closed by either U+301E or U+301F.

                StringBuilder newItem = new StringBuilder(item.Length + 2);
                int splitPoint;
                for (splitPoint = 0; splitPoint <= i; splitPoint++) {
                    if (ClassifyCharacter(item[i]) == UCK_QUOTE) {
                        break;
                    }
                }

                // Copy the leading "normal" portion.
                newItem.Append(openQuote).Append(item.Substring(0, splitPoint));
                for (int charOffset = splitPoint; charOffset < item.Length; charOffset++) {
                    newItem.Append(item[charOffset]);
                    if (ClassifyCharacter(item[charOffset]) == UCK_QUOTE &&
                        IsSurroundingQuote(item[charOffset], openQuote, closeQuote)) {
                        newItem.Append(item[charOffset]);
                    }
                }

                newItem.Append(closeQuote);

                item = newItem.ToString();
            }

            return item;
        }


        /// <param name="ch"> a character </param>
        /// <param name="openQuote"> the opening quote char </param>
        /// <param name="closeQuote"> the closing quote char </param>
        /// <returns> Return it the character is a surrounding quote. </returns>
        private static bool IsSurroundingQuote(char ch, char openQuote, char closeQuote) {
            return ch == openQuote || IsClosingingQuote(ch, openQuote, closeQuote);
        }


        /// <param name="ch"> a character </param>
        /// <param name="openQuote"> the opening quote char </param>
        /// <param name="closeQuote"> the closing quote char </param>
        /// <returns> Returns true if the character is a closing quote. </returns>
        private static bool IsClosingingQuote(char ch, char openQuote, char closeQuote) {
            return ch == closeQuote || (openQuote == 0x301D && ch == 0x301E || ch == 0x301F);
        }
    }
}
