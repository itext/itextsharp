using System;
using System.Collections;
using System.Diagnostics;
using iTextSharp.xmp.impl.xpath;
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
    using XmpConst = XmpConst;
    using XMPDateTime = IXmpDateTime;
    using XMPDateTimeFactory = XmpDateTimeFactory;
    using XMPError = XmpError;
    using XmpException = XmpException;
    using XMPMetaFactory = XmpMetaFactory;
    using XMPUtils = XmpUtils;


    /// <summary>
    /// Utilities for <code>XMPNode</code>.
    /// 
    /// @since   Aug 28, 2006
    /// </summary>
    public class XmpNodeUtils : XmpConst {
        internal const int CLT_NO_VALUES = 0;
        internal const int CLT_SPECIFIC_MATCH = 1;
        internal const int CLT_SINGLE_GENERIC = 2;
        internal const int CLT_MULTIPLE_GENERIC = 3;
        internal const int CLT_XDEFAULT = 4;
        internal const int CLT_FIRST_ITEM = 5;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private XmpNodeUtils() {
            // EMPTY
        }


        /// <summary>
        /// Find or create a schema node if <code>createNodes</code> is false and
        /// </summary>
        /// <param name="tree"> the root of the xmp tree. </param>
        /// <param name="namespaceUri"> a namespace </param>
        /// <param name="createNodes"> a flag indicating if the node shall be created if not found.
        /// 		  <em>Note:</em> The namespace must be registered prior to this call.
        /// </param>
        /// <returns> Returns the schema node if found, <code>null</code> otherwise.
        /// 		   Note: If <code>createNodes</code> is <code>true</code>, it is <b>always</b>
        /// 		   returned a valid node. </returns>
        /// <exception cref="XmpException"> An exception is only thrown if an error occurred, not if a
        ///         		node was not found. </exception>
        internal static XmpNode FindSchemaNode(XmpNode tree, string namespaceUri, bool createNodes) {
            return FindSchemaNode(tree, namespaceUri, null, createNodes);
        }


        /// <summary>
        /// Find or create a schema node if <code>createNodes</code> is true.
        /// </summary>
        /// <param name="tree"> the root of the xmp tree. </param>
        /// <param name="namespaceUri"> a namespace </param>
        /// <param name="suggestedPrefix"> If a prefix is suggested, the namespace is allowed to be registered. </param>
        /// <param name="createNodes"> a flag indicating if the node shall be created if not found.
        /// 		  <em>Note:</em> The namespace must be registered prior to this call.
        /// </param>
        /// <returns> Returns the schema node if found, <code>null</code> otherwise.
        /// 		   Note: If <code>createNodes</code> is <code>true</code>, it is <b>always</b>
        /// 		   returned a valid node. </returns>
        /// <exception cref="XmpException"> An exception is only thrown if an error occurred, not if a
        ///         		node was not found. </exception>
        internal static XmpNode FindSchemaNode(XmpNode tree, string namespaceUri, string suggestedPrefix,
                                               bool createNodes) {
            Debug.Assert(tree.Parent == null); // make sure that its the root
            XmpNode schemaNode = tree.FindChildByName(namespaceUri);

            if (schemaNode == null && createNodes) {
                PropertyOptions propertyOptions = new PropertyOptions();
                propertyOptions.SchemaNode = true;
                schemaNode = new XmpNode(namespaceUri, propertyOptions);
                schemaNode.Implicit = true;

                // only previously registered schema namespaces are allowed in the XMP tree.
                string prefix = XMPMetaFactory.SchemaRegistry.GetNamespacePrefix(namespaceUri);
                if (prefix == null) {
                    if (!String.IsNullOrEmpty(suggestedPrefix)) {
                        prefix = XMPMetaFactory.SchemaRegistry.RegisterNamespace(namespaceUri, suggestedPrefix);
                    }
                    else {
                        throw new XmpException("Unregistered schema namespace URI", XmpError.BADSCHEMA);
                    }
                }

                schemaNode.Value = prefix;

                tree.AddChild(schemaNode);
            }

            return schemaNode;
        }


        /// <summary>
        /// Find or create a child node under a given parent node. If the parent node is no 
        /// Returns the found or created child node.
        /// </summary>
        /// <param name="parent">
        ///            the parent node </param>
        /// <param name="childName">
        ///            the node name to find </param>
        /// <param name="createNodes">
        ///            flag, if new nodes shall be created. </param>
        /// <returns> Returns the found or created node or <code>null</code>. </returns>
        /// <exception cref="XmpException"> Thrown if  </exception>
        internal static XmpNode FindChildNode(XmpNode parent, string childName, bool createNodes) {
            if (!parent.Options.SchemaNode && !parent.Options.Struct) {
                if (!parent.Implicit) {
                    throw new XmpException("Named children only allowed for schemas and structs",
                                           XmpError.BADXPATH);
                }
                if (parent.Options.Array) {
                    throw new XmpException("Named children not allowed for arrays", XmpError.BADXPATH);
                }
                if (createNodes) {
                    parent.Options.Struct = true;
                }
            }

            XmpNode childNode = parent.FindChildByName(childName);

            if (childNode == null && createNodes) {
                PropertyOptions options = new PropertyOptions();
                childNode = new XmpNode(childName, options);
                childNode.Implicit = true;
                parent.AddChild(childNode);
            }

            Debug.Assert(childNode != null || !createNodes);

            return childNode;
        }


        /// <summary>
        /// Follow an expanded path expression to find or create a node.
        /// </summary>
        /// <param name="xmpTree"> the node to begin the search. </param>
        /// <param name="xpath"> the complete xpath </param>
        /// <param name="createNodes"> flag if nodes shall be created 
        /// 			(when called by <code>setProperty()</code>) </param>
        /// <param name="leafOptions"> the options for the created leaf nodes (only when
        ///			<code>createNodes == true</code>). </param>
        /// <returns> Returns the node if found or created or <code>null</code>. </returns>
        /// <exception cref="XmpException"> An exception is only thrown if an error occurred, 
        /// 			not if a node was not found. </exception>
        internal static XmpNode FindNode(XmpNode xmpTree, XmpPath xpath, bool createNodes, PropertyOptions leafOptions) {
            // check if xpath is set.
            if (xpath == null || xpath.Size() == 0) {
                throw new XmpException("Empty XmpPath", XmpError.BADXPATH);
            }

            // Root of implicitly created subtree to possible delete it later. 
            // Valid only if leaf is new.
            XmpNode rootImplicitNode = null;

            // resolve schema step
            XmpNode currNode = FindSchemaNode(xmpTree, xpath.GetSegment((int) XmpPath.STEP_SCHEMA).Name, createNodes);
            if (currNode == null) {
                return null;
            }
            if (currNode.Implicit) {
                currNode.Implicit = false; // Clear the implicit node bit.
                rootImplicitNode = currNode; // Save the top most implicit node.
            }


            // Now follow the remaining steps of the original XmpPath.
            try {
                for (int i = 1; i < xpath.Size(); i++) {
                    currNode = FollowXPathStep(currNode, xpath.GetSegment(i), createNodes);
                    if (currNode == null) {
                        if (createNodes) {
                            // delete implicitly created nodes
                            DeleteNode(rootImplicitNode);
                        }
                        return null;
                    }
                    if (currNode.Implicit) {
                        // clear the implicit node flag
                        currNode.Implicit = false;

                        // if node is an ALIAS (can be only in root step, auto-create array 
                        // when the path has been resolved from a not simple alias type
                        if (i == 1 && xpath.GetSegment(i).Alias && xpath.GetSegment(i).AliasForm != 0) {
                            currNode.Options.SetOption(xpath.GetSegment(i).AliasForm, true);
                        }
                            // "CheckImplicitStruct" in C++
                        else if (i < xpath.Size() - 1 && xpath.GetSegment(i).Kind == XmpPath.STRUCT_FIELD_STEP &&
                                 !currNode.Options.CompositeProperty) {
                            currNode.Options.Struct = true;
                        }

                        if (rootImplicitNode == null) {
                            rootImplicitNode = currNode; // Save the top most implicit node.
                        }
                    }
                }
            }
            catch (XmpException e) {
                // if new notes have been created prior to the error, delete them
                if (rootImplicitNode != null) {
                    DeleteNode(rootImplicitNode);
                }
                throw e;
            }


            if (rootImplicitNode != null) {
                // set options only if a node has been successful created
                currNode.Options.MergeWith(leafOptions);
                currNode.Options = currNode.Options;
            }

            return currNode;
        }


        /// <summary>
        /// Deletes the the given node and its children from its parent.
        /// Takes care about adjusting the flags. </summary>
        /// <param name="node"> the top-most node to delete. </param>
        internal static void DeleteNode(XmpNode node) {
            XmpNode parent = node.Parent;

            if (node.Options.Qualifier) {
                // root is qualifier
                parent.RemoveQualifier(node);
            }
            else {
                // root is NO qualifier
                parent.RemoveChild(node);
            }

            // delete empty Schema nodes
            if (!parent.HasChildren() && parent.Options.SchemaNode) {
                parent.Parent.RemoveChild(parent);
            }
        }


        /// <summary>
        /// This is setting the value of a leaf node.
        /// </summary>
        /// <param name="node"> an XMPNode </param>
        /// <param name="value"> a value </param>
        internal static void SetNodeValue(XmpNode node, object value) {
            string strValue = SerializeNodeValue(value);
            if (!(node.Options.Qualifier && XML_LANG.Equals(node.Name))) {
                node.Value = strValue;
            }
            else {
                node.Value = Utils.NormalizeLangValue(strValue);
            }
        }


        /// <summary>
        /// Verifies the PropertyOptions for consistancy and updates them as needed. 
        /// If options are <code>null</code> they are created with default values.
        /// </summary>
        /// <param name="options"> the <code>PropertyOptions</code> </param>
        /// <param name="itemValue"> the node value to set </param>
        /// <returns> Returns the updated options. </returns>
        /// <exception cref="XmpException"> If the options are not consistant.  </exception>
        internal static PropertyOptions VerifySetOptions(PropertyOptions options, object itemValue) {
            // create empty and fix existing options
            if (options == null) {
                // set default options
                options = new PropertyOptions();
            }

            if (options.ArrayAltText) {
                options.ArrayAlternate = true;
            }

            if (options.ArrayAlternate) {
                options.ArrayOrdered = true;
            }

            if (options.ArrayOrdered) {
                options.Array = true;
            }

            if (options.CompositeProperty && itemValue != null && itemValue.ToString().Length > 0) {
                throw new XmpException("Structs and arrays can't have values", XmpError.BADOPTIONS);
            }

            options.AssertConsistency(options.Options);

            return options;
        }


        /// <summary>
        /// Converts the node value to String, apply special conversions for defined
        /// types in XMP.
        /// </summary>
        /// <param name="value">
        ///            the node value to set </param>
        /// <returns> Returns the String representation of the node value. </returns>
        internal static string SerializeNodeValue(object value) {
            string strValue;
            if (value == null) {
                strValue = null;
            }
            else if (value is bool?) {
                strValue = XMPUtils.ConvertFromBoolean((bool) ((bool?) value));
            }
            else if (value is int?) {
                strValue = XMPUtils.ConvertFromInteger((int) ((int?) value));
            }
            else if (value is long?) {
                strValue = XMPUtils.ConvertFromLong((long) ((long?) value));
            }
            else if (value is double?) {
                strValue = XMPUtils.ConvertFromDouble((double) ((double?) value));
            }
            else if (value is XMPDateTime) {
                strValue = XMPUtils.ConvertFromDate((XMPDateTime) value);
            }
            else if (value is XmpCalendar) {
                XMPDateTime dt = XMPDateTimeFactory.CreateFromCalendar((XmpCalendar) value);
                strValue = XMPUtils.ConvertFromDate(dt);
            }
            else if (value is byte[]) {
                strValue = XMPUtils.EncodeBase64((byte[]) value);
            }
            else {
                strValue = value.ToString();
            }

            return strValue != null ? Utils.RemoveControlChars(strValue) : null;
        }


        /// <summary>
        /// After processing by ExpandXPath, a step can be of these forms:
        /// <ul>
        /// 	<li>qualName - A top level property or struct field.
        /// <li>[index] - An element of an array.
        /// <li>[last()] - The last element of an array.
        /// <li>[qualName="value"] - An element in an array of structs, chosen by a field value.
        /// <li>[?qualName="value"] - An element in an array, chosen by a qualifier value.
        /// <li>?qualName - A general qualifier.
        /// </ul>
        /// Find the appropriate child node, resolving aliases, and optionally creating nodes.
        /// </summary>
        /// <param name="parentNode"> the node to start to start from </param>
        /// <param name="nextStep"> the xpath segment </param>
        /// <param name="createNodes"> </param>
        /// <returns> returns the found or created XmpPath node </returns>
        /// <exception cref="XmpException">  </exception>
        private static XmpNode FollowXPathStep(XmpNode parentNode, XmpPathSegment nextStep, bool createNodes) {
            XmpNode nextNode = null;
            uint stepKind = nextStep.Kind;

            if (stepKind == XmpPath.STRUCT_FIELD_STEP) {
                nextNode = FindChildNode(parentNode, nextStep.Name, createNodes);
            }
            else if (stepKind == XmpPath.QUALIFIER_STEP) {
                nextNode = FindQualifierNode(parentNode, nextStep.Name.Substring(1), createNodes);
            }
            else {
                // This is an array indexing step. First get the index, then get the node.
                int index;

                if (!parentNode.Options.Array) {
                    throw new XmpException("Indexing applied to non-array", XmpError.BADXPATH);
                }

                if (stepKind == XmpPath.ARRAY_INDEX_STEP) {
                    index = FindIndexedItem(parentNode, nextStep.Name, createNodes);
                }
                else if (stepKind == XmpPath.ARRAY_LAST_STEP) {
                    index = parentNode.ChildrenLength;
                }
                else if (stepKind == XmpPath.FIELD_SELECTOR_STEP) {
                    string[] result = Utils.SplitNameAndValue(nextStep.Name);
                    string fieldName = result[0];
                    string fieldValue = result[1];
                    index = LookupFieldSelector(parentNode, fieldName, fieldValue);
                }
                else if (stepKind == XmpPath.QUAL_SELECTOR_STEP) {
                    string[] result = Utils.SplitNameAndValue(nextStep.Name);
                    string qualName = result[0];
                    string qualValue = result[1];
                    index = LookupQualSelector(parentNode, qualName, qualValue, nextStep.AliasForm);
                }
                else {
                    throw new XmpException("Unknown array indexing step in FollowXPathStep",
                                           XmpError.INTERNALFAILURE);
                }

                if (1 <= index && index <= parentNode.ChildrenLength) {
                    nextNode = parentNode.GetChild(index);
                }
            }

            return nextNode;
        }


        /// <summary>
        /// Find or create a qualifier node under a given parent node. Returns a pointer to the 
        /// qualifier node, and optionally an iterator for the node's position in 
        /// the parent's vector of qualifiers. The iterator is unchanged if no qualifier node (null) 
        /// is returned.
        /// <em>Note:</em> On entry, the qualName parameter must not have the leading '?' from the 
        /// XmpPath step.
        /// </summary>
        /// <param name="parent"> the parent XMPNode </param>
        /// <param name="qualName"> the qualifier name </param>
        /// <param name="createNodes"> flag if nodes shall be created </param>
        /// <returns> Returns the qualifier node if found or created, <code>null</code> otherwise. </returns>
        /// <exception cref="XmpException">  </exception>
        private static XmpNode FindQualifierNode(XmpNode parent, string qualName, bool createNodes) {
            Debug.Assert(!qualName.StartsWith("?"));

            XmpNode qualNode = parent.FindQualifierByName(qualName);

            if (qualNode == null && createNodes) {
                qualNode = new XmpNode(qualName, null);
                qualNode.Implicit = true;

                parent.AddQualifier(qualNode);
            }

            return qualNode;
        }


        /// <param name="arrayNode"> an array node </param>
        /// <param name="segment"> the segment containing the array index </param>
        /// <param name="createNodes"> flag if new nodes are allowed to be created. </param>
        /// <returns> Returns the index or index = -1 if not found </returns>
        /// <exception cref="XmpException"> Throws Exceptions </exception>
        private static int FindIndexedItem(XmpNode arrayNode, string segment, bool createNodes) {
            int index;

            try {
                segment = segment.Substring(1, segment.Length - 1 - 1);
                index = Convert.ToInt32(segment);
                if (index < 1) {
                    throw new XmpException("Array index must be larger than zero", XmpError.BADXPATH);
                }
            }
            catch (FormatException) {
                throw new XmpException("Array index not digits.", XmpError.BADXPATH);
            }

            if (createNodes && index == arrayNode.ChildrenLength + 1) {
                // Append a new last + 1 node.
                XmpNode newItem = new XmpNode(ARRAY_ITEM_NAME, null);
                newItem.Implicit = true;
                arrayNode.AddChild(newItem);
            }

            return index;
        }


        /// <summary>
        /// Searches for a field selector in a node:
        /// [fieldName="value] - an element in an array of structs, chosen by a field value.
        /// No implicit nodes are created by field selectors. 
        /// </summary>
        /// <param name="arrayNode"> </param>
        /// <param name="fieldName"> </param>
        /// <param name="fieldValue"> </param>
        /// <returns> Returns the index of the field if found, otherwise -1. </returns>
        /// <exception cref="XmpException">  </exception>
        private static int LookupFieldSelector(XmpNode arrayNode, string fieldName, string fieldValue) {
            int result = -1;

            for (int index = 1; index <= arrayNode.ChildrenLength && result < 0; index++) {
                XmpNode currItem = arrayNode.GetChild(index);

                if (!currItem.Options.Struct) {
                    throw new XmpException("Field selector must be used on array of struct", XmpError.BADXPATH);
                }

                for (int f = 1; f <= currItem.ChildrenLength; f++) {
                    XmpNode currField = currItem.GetChild(f);
                    if (!fieldName.Equals(currField.Name)) {
                        continue;
                    }
                    if (fieldValue.Equals(currField.Value)) {
                        result = index;
                        break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Searches for a qualifier selector in a node:
        /// [?qualName="value"] - an element in an array, chosen by a qualifier value.
        /// No implicit nodes are created for qualifier selectors, 
        /// except for an alias to an x-default item.
        /// </summary>
        /// <param name="arrayNode"> an array node </param>
        /// <param name="qualName"> the qualifier name </param>
        /// <param name="qualValue"> the qualifier value </param>
        /// <param name="aliasForm"> in case the qual selector results from an alias,
        /// 		  an x-default node is created if there has not been one. </param>
        /// <returns> Returns the index of th </returns>
        /// <exception cref="XmpException">  </exception>
        private static int LookupQualSelector(XmpNode arrayNode, string qualName, string qualValue, uint aliasForm) {
            if (XML_LANG.Equals(qualName)) {
                qualValue = Utils.NormalizeLangValue(qualValue);
                int index = LookupLanguageItem(arrayNode, qualValue);
                if (index < 0 && (aliasForm & AliasOptions.PROP_ARRAY_ALT_TEXT) > 0) {
                    XmpNode langNode = new XmpNode(ARRAY_ITEM_NAME, null);
                    XmpNode xdefault = new XmpNode(XML_LANG, X_DEFAULT, null);
                    langNode.AddQualifier(xdefault);
                    arrayNode.AddChild(1, langNode);
                    return 1;
                }
                return index;
            }
            for (int index = 1; index < arrayNode.ChildrenLength; index++) {
                XmpNode currItem = arrayNode.GetChild(index);

                for (IEnumerator it = currItem.IterateQualifier(); it.MoveNext();) {
                    XmpNode qualifier = (XmpNode) it.Current;
                    if (qualifier != null && qualName.Equals(qualifier.Name) && qualValue.Equals(qualifier.Value)) {
                        return index;
                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// Make sure the x-default item is first. Touch up &quot;single value&quot;
        /// arrays that have a default plus one real language. This case should have
        /// the same value for both items. Older Adobe apps were hardwired to only
        /// use the &quot;x-default&quot; item, so we copy that value to the other
        /// item.
        /// </summary>
        /// <param name="arrayNode">
        ///            an alt text array node </param>
        internal static void NormalizeLangArray(XmpNode arrayNode) {
            if (!arrayNode.Options.ArrayAltText) {
                return;
            }

            // check if node with x-default qual is first place
            for (int i = 2; i <= arrayNode.ChildrenLength; i++) {
                XmpNode child = arrayNode.GetChild(i);
                if (child.HasQualifier() && X_DEFAULT.Equals(child.GetQualifier(1).Value)) {
                    // move node to first place
                    try {
                        arrayNode.RemoveChild(i);
                        arrayNode.AddChild(1, child);
                    }
                    catch (XmpException) {
                        // cannot occur, because same child is removed before
                        Debug.Assert(false);
                    }

                    if (i == 2) {
                        arrayNode.GetChild(2).Value = child.Value;
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// See if an array is an alt-text array. If so, make sure the x-default item
        /// is first.
        /// </summary>
        /// <param name="arrayNode">
        ///            the array node to check if its an alt-text array </param>
        internal static void DetectAltText(XmpNode arrayNode) {
            if (arrayNode.Options.ArrayAlternate && arrayNode.HasChildren()) {
                bool isAltText = false;
                for (IEnumerator it = arrayNode.IterateChildren(); it.MoveNext();) {
                    XmpNode child = (XmpNode) it.Current;
                    if (child != null && child.Options != null && child.Options.HasLanguage) {
                        isAltText = true;
                        break;
                    }
                }

                if (isAltText) {
                    arrayNode.Options.ArrayAltText = true;
                    NormalizeLangArray(arrayNode);
                }
            }
        }


        /// <summary>
        /// Appends a language item to an alt text array.
        /// </summary>
        /// <param name="arrayNode"> the language array </param>
        /// <param name="itemLang"> the language of the item </param>
        /// <param name="itemValue"> the content of the item </param>
        /// <exception cref="XmpException"> Thrown if a duplicate property is added </exception>
        internal static void AppendLangItem(XmpNode arrayNode, string itemLang, string itemValue) {
            XmpNode newItem = new XmpNode(ARRAY_ITEM_NAME, itemValue, null);
            XmpNode langQual = new XmpNode(XML_LANG, itemLang, null);
            newItem.AddQualifier(langQual);

            if (!X_DEFAULT.Equals(langQual.Value)) {
                arrayNode.AddChild(newItem);
            }
            else {
                arrayNode.AddChild(1, newItem);
            }
        }


        /// <summary>
        /// <ol>
        /// <li>Look for an exact match with the specific language.
        /// <li>If a generic language is given, look for partial matches.
        /// <li>Look for an "x-default"-item.
        /// <li>Choose the first item.
        /// </ol>
        /// </summary>
        /// <param name="arrayNode">
        ///            the alt text array node </param>
        /// <param name="genericLang">
        ///            the generic language </param>
        /// <param name="specificLang">
        ///            the specific language </param>
        /// <returns> Returns the kind of match as an Integer and the found node in an
        ///         array.
        /// </returns>
        /// <exception cref="XmpException"> </exception>
        internal static object[] ChooseLocalizedText(XmpNode arrayNode, string genericLang, string specificLang) {
            // See if the array has the right form. Allow empty alt arrays,
            // that is what parsing returns.
            if (!arrayNode.Options.ArrayAltText) {
                throw new XmpException("Localized text array is not alt-text", XmpError.BADXPATH);
            }
            if (!arrayNode.HasChildren()) {
                return new object[] {CLT_NO_VALUES, null};
            }

            int foundGenericMatches = 0;
            XmpNode resultNode = null;
            XmpNode xDefault = null;

            // Look for the first partial match with the generic language.
            for (IEnumerator it = arrayNode.IterateChildren(); it.MoveNext();) {
                XmpNode currItem = (XmpNode) it.Current;

                // perform some checks on the current item
                if (currItem == null || currItem.Options == null || currItem.Options.CompositeProperty) {
                    throw new XmpException("Alt-text array item is not simple", XmpError.BADXPATH);
                }
                if (!currItem.HasQualifier() || !XML_LANG.Equals(currItem.GetQualifier(1).Name)) {
                    throw new XmpException("Alt-text array item has no language qualifier", XmpError.BADXPATH);
                }

                string currLang = currItem.GetQualifier(1).Value;

                // Look for an exact match with the specific language.
                if (specificLang.Equals(currLang)) {
                    return new object[] {CLT_SPECIFIC_MATCH, currItem};
                }
                if (genericLang != null && currLang.StartsWith(genericLang)) {
                    if (resultNode == null) {
                        resultNode = currItem;
                    }
                    // ! Don't return/break, need to look for other matches.
                    foundGenericMatches++;
                }
                else if (X_DEFAULT.Equals(currLang)) {
                    xDefault = currItem;
                }
            }

            // evaluate loop
            if (foundGenericMatches == 1) {
                return new object[] {CLT_SINGLE_GENERIC, resultNode};
            }
            if (foundGenericMatches > 1) {
                return new object[] {CLT_MULTIPLE_GENERIC, resultNode};
            }
            if (xDefault != null) {
                return new object[] {CLT_XDEFAULT, xDefault};
            }
            {
                // Everything failed, choose the first item.
                return new object[] {CLT_FIRST_ITEM, arrayNode.GetChild(1)};
            }
        }


        /// <summary>
        /// Looks for the appropriate language item in a text alternative array.item
        /// </summary>
        /// <param name="arrayNode">
        ///            an array node </param>
        /// <param name="language">
        ///            the requested language </param>
        /// <returns> Returns the index if the language has been found, -1 otherwise. </returns>
        /// <exception cref="XmpException"> </exception>
        internal static int LookupLanguageItem(XmpNode arrayNode, string language) {
            if (!arrayNode.Options.Array) {
                throw new XmpException("Language item must be used on array", XmpError.BADXPATH);
            }

            for (int index = 1; index <= arrayNode.ChildrenLength; index++) {
                XmpNode child = arrayNode.GetChild(index);
                if (!child.HasQualifier() || !XML_LANG.Equals(child.GetQualifier(1).Name)) {
                    continue;
                }
                if (language.Equals(child.GetQualifier(1).Value)) {
                    return index;
                }
            }

            return -1;
        }
    }
}
