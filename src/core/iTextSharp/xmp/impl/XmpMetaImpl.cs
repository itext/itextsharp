using System;
using System.Diagnostics;
using iTextSharp.xmp.impl.xpath;
using iTextSharp.xmp.options;
using iTextSharp.xmp.properties;

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
    /// Implementation for <seealso cref="IXmpMeta"/>.
    /// 
    /// @since 17.02.2006
    /// </summary>
    public class XmpMetaImpl : XmpConst, IXmpMeta {
        /// <summary>
        /// Property values are Strings by default </summary>
        private const int VALUE_STRING = 0;

        private const int VALUE_BOOLEAN = 1;
        private const int VALUE_INTEGER = 2;
        private const int VALUE_LONG = 3;
        private const int VALUE_DOUBLE = 4;
        private const int VALUE_DATE = 5;
        private const int VALUE_CALENDAR = 6;
        private const int VALUE_BASE64 = 7;

        /// <summary>
        /// root of the metadata tree </summary>
        private readonly XmpNode _tree;

        /// <summary>
        /// the xpacket processing instructions content </summary>
        private string _packetHeader;


        /// <summary>
        /// Constructor for an empty metadata object.
        /// </summary>
        public XmpMetaImpl() {
            // create root node
            _tree = new XmpNode(null, null, null);
        }


        /// <summary>
        /// Constructor for a cloned metadata tree.
        /// </summary>
        /// <param name="tree">
        ///            an prefilled metadata tree which fulfills all
        ///            <code>XMPNode</code> contracts. </param>
        public XmpMetaImpl(XmpNode tree) {
            _tree = tree;
        }

        /// <returns> Returns the root node of the XMP tree. </returns>
        public virtual XmpNode Root {
            get { return _tree; }
        }

        #region XmpMeta Members

        /// <seealso cref= XMPMeta#appendArrayItem(String, String, PropertyOptions, String,
        ///      PropertyOptions) </seealso>
        public virtual void AppendArrayItem(string schemaNs, string arrayName, PropertyOptions arrayOptions,
                                            string itemValue, PropertyOptions itemOptions) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);

            if (arrayOptions == null) {
                arrayOptions = new PropertyOptions();
            }
            if (!arrayOptions.OnlyArrayOptions) {
                throw new XmpException("Only array form flags allowed for arrayOptions", XmpError.BADOPTIONS);
            }

            // Check if array options are set correctly.
            arrayOptions = XmpNodeUtils.VerifySetOptions(arrayOptions, null);


            // Locate or create the array. If it already exists, make sure the array
            // form from the options
            // parameter is compatible with the current state.
            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);


            // Just lookup, don't try to create.
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, false, null);

            if (arrayNode != null) {
                // The array exists, make sure the form is compatible. Zero
                // arrayForm means take what exists.
                if (!arrayNode.Options.Array) {
                    throw new XmpException("The named property is not an array", XmpError.BADXPATH);
                }
                // if (arrayOptions != null && !arrayOptions.equalArrayTypes(arrayNode.getOptions()))
                // {
                // throw new XmpException("Mismatch of existing and specified array form", BADOPTIONS);
                // }
            }
            else {
                // The array does not exist, try to create it.
                if (arrayOptions.Array) {
                    arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, true, arrayOptions);
                    if (arrayNode == null) {
                        throw new XmpException("Failure creating array node", XmpError.BADXPATH);
                    }
                }
                else {
                    // array options missing
                    throw new XmpException("Explicit arrayOptions required to create new array",
                                           XmpError.BADOPTIONS);
                }
            }

            DoSetArrayItem(arrayNode, ARRAY_LAST_ITEM, itemValue, itemOptions, true);
        }


        /// <seealso cref= XMPMeta#appendArrayItem(String, String, String) </seealso>
        public virtual void AppendArrayItem(string schemaNs, string arrayName, string itemValue) {
            AppendArrayItem(schemaNs, arrayName, null, itemValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#countArrayItems(String, String) </seealso>
        public virtual int CountArrayItems(string schemaNs, string arrayName) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);

            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, false, null);

            if (arrayNode == null) {
                return 0;
            }

            if (arrayNode.Options.Array) {
                return arrayNode.ChildrenLength;
            }
            throw new XmpException("The named property is not an array", XmpError.BADXPATH);
        }


        /// <seealso cref= XMPMeta#deleteArrayItem(String, String, int) </seealso>
        public virtual void DeleteArrayItem(string schemaNs, string arrayName, int itemIndex) {
            try {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertArrayName(arrayName);

                string itemPath = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
                DeleteProperty(schemaNs, itemPath);
            }
            catch (XmpException) {
                // EMPTY, exceptions are ignored within delete
            }
        }


        /// <seealso cref= XMPMeta#deleteProperty(String, String) </seealso>
        public virtual void DeleteProperty(string schemaNs, string propName) {
            try {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);

                XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);

                XmpNode propNode = XmpNodeUtils.FindNode(_tree, expPath, false, null);
                if (propNode != null) {
                    XmpNodeUtils.DeleteNode(propNode);
                }
            }
            catch (XmpException) {
                // EMPTY, exceptions are ignored within delete
            }
        }


        /// <seealso cref= XMPMeta#deleteQualifier(String, String, String, String) </seealso>
        public virtual void DeleteQualifier(string schemaNs, string propName, string qualNs, string qualName) {
            try {
                // Note: qualNs and qualName are checked inside composeQualfierPath
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);

                string qualPath = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
                DeleteProperty(schemaNs, qualPath);
            }
            catch (XmpException) {
                // EMPTY, exceptions within delete are ignored
            }
        }


        /// <seealso cref= XMPMeta#deleteStructField(String, String, String, String) </seealso>
        public virtual void DeleteStructField(string schemaNs, string structName, string fieldNs, string fieldName) {
            try {
                // fieldNs and fieldName are checked inside composeStructFieldPath
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertStructName(structName);

                string fieldPath = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
                DeleteProperty(schemaNs, fieldPath);
            }
            catch (XmpException) {
                // EMPTY, exceptions within delete are ignored
            }
        }


        /// <seealso cref= XMPMeta#doesPropertyExist(String, String) </seealso>
        public virtual bool DoesPropertyExist(string schemaNs, string propName) {
            try {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);

                XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);
                XmpNode propNode = XmpNodeUtils.FindNode(_tree, expPath, false, null);
                return propNode != null;
            }
            catch (XmpException) {
                return false;
            }
        }


        /// <seealso cref= XMPMeta#doesArrayItemExist(String, String, int) </seealso>
        public virtual bool DoesArrayItemExist(string schemaNs, string arrayName, int itemIndex) {
            try {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertArrayName(arrayName);

                string path = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
                return DoesPropertyExist(schemaNs, path);
            }
            catch (XmpException) {
                return false;
            }
        }


        /// <seealso cref= XMPMeta#doesStructFieldExist(String, String, String, String) </seealso>
        public virtual bool DoesStructFieldExist(string schemaNs, string structName, string fieldNs, string fieldName) {
            try {
                // fieldNs and fieldName are checked inside composeStructFieldPath()
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertStructName(structName);

                string path = XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
                return DoesPropertyExist(schemaNs, structName + path);
            }
            catch (XmpException) {
                return false;
            }
        }


        /// <seealso cref= XMPMeta#doesQualifierExist(String, String, String, String) </seealso>
        public virtual bool DoesQualifierExist(string schemaNs, string propName, string qualNs, string qualName) {
            try {
                // qualNs and qualName are checked inside composeQualifierPath()
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);

                string path = XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
                return DoesPropertyExist(schemaNs, propName + path);
            }
            catch (XmpException) {
                return false;
            }
        }


        /// <seealso cref= XMPMeta#getArrayItem(String, String, int) </seealso>
        public virtual IXmpProperty GetArrayItem(string schemaNs, string arrayName, int itemIndex) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);

            string itemPath = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
            return GetProperty(schemaNs, itemPath);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#getLocalizedText(String, String, String, String) </seealso>
        public virtual IXmpProperty GetLocalizedText(string schemaNs, string altTextName, string genericLang,
                                                     string specificLang) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(altTextName);
            ParameterAsserts.AssertSpecificLang(specificLang);

            genericLang = genericLang != null ? Utils.NormalizeLangValue(genericLang) : null;
            specificLang = Utils.NormalizeLangValue(specificLang);

            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, altTextName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, false, null);
            if (arrayNode == null) {
                return null;
            }

            object[] result = XmpNodeUtils.ChooseLocalizedText(arrayNode, genericLang, specificLang);
            int match = (int) ((int?) result[0]);
            XmpNode itemNode = (XmpNode) result[1];

            if (match != XmpNodeUtils.CLT_NO_VALUES) {
                return new XmpPropertyImpl1(itemNode);
            }
            return null;
        }


        /// <seealso cref= XMPMeta#setLocalizedText(String, String, String, String, String,
        ///      PropertyOptions) </seealso>
        public virtual void SetLocalizedText(string schemaNs, string altTextName, string genericLang,
                                             string specificLang, string itemValue, PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(altTextName);
            ParameterAsserts.AssertSpecificLang(specificLang);

            genericLang = genericLang != null ? Utils.NormalizeLangValue(genericLang) : null;
            specificLang = Utils.NormalizeLangValue(specificLang);

            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, altTextName);

            // Find the array node and set the options if it was just created.
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, true,
                                                      new PropertyOptions(PropertyOptions.ARRAY |
                                                                          PropertyOptions.ARRAY_ORDERED |
                                                                          PropertyOptions.ARRAY_ALTERNATE |
                                                                          PropertyOptions.ARRAY_ALT_TEXT));

            if (arrayNode == null) {
                throw new XmpException("Failed to find or create array node", XmpError.BADXPATH);
            }
            if (!arrayNode.Options.ArrayAltText) {
                if (!arrayNode.HasChildren() && arrayNode.Options.ArrayAlternate) {
                    arrayNode.Options.ArrayAltText = true;
                }
                else {
                    throw new XmpException("Specified property is no alt-text array", XmpError.BADXPATH);
                }
            }

            // Make sure the x-default item, if any, is first.
            bool haveXDefault = false;
            XmpNode xdItem = null;

            foreach (XmpNode currItem in arrayNode.Children) {
                if (!currItem.HasQualifier() || !XML_LANG.Equals(currItem.GetQualifier(1).Name)) {
                    throw new XmpException("Language qualifier must be first", XmpError.BADXPATH);
                }
                if (X_DEFAULT.Equals(currItem.GetQualifier(1).Value)) {
                    xdItem = currItem;
                    haveXDefault = true;
                    break;
                }
            }

            // Moves x-default to the beginning of the array
            if (xdItem != null && arrayNode.ChildrenLength > 1) {
                arrayNode.RemoveChild(xdItem);
                arrayNode.AddChild(1, xdItem);
            }

            // Find the appropriate item.
            // chooseLocalizedText will make sure the array is a language
            // alternative.
            object[] result = XmpNodeUtils.ChooseLocalizedText(arrayNode, genericLang, specificLang);
            int match = (int) ((int?) result[0]);
            XmpNode itemNode = (XmpNode) result[1];

            bool specificXDefault = X_DEFAULT.Equals(specificLang);

            switch (match) {
                case XmpNodeUtils.CLT_NO_VALUES:

                    // Create the array items for the specificLang and x-default, with
                    // x-default first.
                    XmpNodeUtils.AppendLangItem(arrayNode, X_DEFAULT, itemValue);
                    haveXDefault = true;
                    if (!specificXDefault) {
                        XmpNodeUtils.AppendLangItem(arrayNode, specificLang, itemValue);
                    }
                    break;

                case XmpNodeUtils.CLT_SPECIFIC_MATCH:

                    if (!specificXDefault) {
                        // Update the specific item, update x-default if it matches the
                        // old value.
                        if (haveXDefault && xdItem != itemNode && xdItem != null && xdItem.Value.Equals(itemNode.Value)) {
                            xdItem.Value = itemValue;
                        }
                        // ! Do this after the x-default check!
                        itemNode.Value = itemValue;
                    }
                    else {
                        // Update all items whose values match the old x-default value.
                        Debug.Assert(haveXDefault && xdItem == itemNode);
                        foreach (XmpNode currItem in arrayNode.Children) {
                            if (currItem == xdItem || !currItem.Value.Equals(xdItem != null ? xdItem.Value : null)) {
                                continue;
                            }
                            currItem.Value = itemValue;
                        }
                        // And finally do the x-default item.
                        if (xdItem != null) {
                            xdItem.Value = itemValue;
                        }
                    }
                    break;

                case XmpNodeUtils.CLT_SINGLE_GENERIC:

                    // Update the generic item, update x-default if it matches the old
                    // value.
                    if (haveXDefault && xdItem != itemNode && xdItem != null && xdItem.Value.Equals(itemNode.Value)) {
                        xdItem.Value = itemValue;
                    }
                    itemNode.Value = itemValue; // ! Do this after
                    // the x-default
                    // check!
                    break;

                case XmpNodeUtils.CLT_MULTIPLE_GENERIC:

                    // Create the specific language, ignore x-default.
                    XmpNodeUtils.AppendLangItem(arrayNode, specificLang, itemValue);
                    if (specificXDefault) {
                        haveXDefault = true;
                    }
                    break;

                case XmpNodeUtils.CLT_XDEFAULT:

                    // Create the specific language, update x-default if it was the only
                    // item.
                    if (xdItem != null && arrayNode.ChildrenLength == 1) {
                        xdItem.Value = itemValue;
                    }
                    XmpNodeUtils.AppendLangItem(arrayNode, specificLang, itemValue);
                    break;

                case XmpNodeUtils.CLT_FIRST_ITEM:

                    // Create the specific language, don't add an x-default item.
                    XmpNodeUtils.AppendLangItem(arrayNode, specificLang, itemValue);
                    if (specificXDefault) {
                        haveXDefault = true;
                    }
                    break;

                default:
                    // does not happen under normal circumstances
                    throw new XmpException("Unexpected result from ChooseLocalizedText",
                                           XmpError.INTERNALFAILURE);
            }

            // Add an x-default at the front if needed.
            if (!haveXDefault && arrayNode.ChildrenLength == 1) {
                XmpNodeUtils.AppendLangItem(arrayNode, X_DEFAULT, itemValue);
            }
        }


        /// <seealso cref= XMPMeta#setLocalizedText(String, String, String, String, String) </seealso>
        public virtual void SetLocalizedText(string schemaNs, string altTextName, string genericLang,
                                             string specificLang, string itemValue) {
            SetLocalizedText(schemaNs, altTextName, genericLang, specificLang, itemValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#GetProperty(String, String) </seealso>
        public virtual IXmpProperty GetProperty(string schemaNs, string propName) {
            return GetProperty(schemaNs, propName, VALUE_STRING);
        }


        /// <seealso cref= XMPMeta#GetPropertyBoolean(String, String) </seealso>
        public virtual bool? GetPropertyBoolean(string schemaNs, string propName) {
            return (bool?) GetPropertyObject(schemaNs, propName, VALUE_BOOLEAN);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#SetPropertyBoolean(String, String, boolean, PropertyOptions) </seealso>
        public virtual void SetPropertyBoolean(string schemaNs, string propName, bool propValue, PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue ? TRUESTR : FALSESTR, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyBoolean(String, String, boolean) </seealso>
        public virtual void SetPropertyBoolean(string schemaNs, string propName, bool propValue) {
            SetProperty(schemaNs, propName, propValue ? TRUESTR : FALSESTR, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyInteger(String, String) </seealso>
        public virtual int? GetPropertyInteger(string schemaNs, string propName) {
            return (int?) GetPropertyObject(schemaNs, propName, VALUE_INTEGER);
        }


        /// <seealso cref= XMPMeta#SetPropertyInteger(String, String, int, PropertyOptions) </seealso>
        public virtual void SetPropertyInteger(string schemaNs, string propName, int propValue, PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyInteger(String, String, int) </seealso>
        public virtual void SetPropertyInteger(string schemaNs, string propName, int propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyLong(String, String) </seealso>
        public virtual long? GetPropertyLong(string schemaNs, string propName) {
            return (long?) GetPropertyObject(schemaNs, propName, VALUE_LONG);
        }


        /// <seealso cref= XMPMeta#SetPropertyLong(String, String, long, PropertyOptions) </seealso>
        public virtual void SetPropertyLong(string schemaNs, string propName, long propValue, PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyLong(String, String, long) </seealso>
        public virtual void SetPropertyLong(string schemaNs, string propName, long propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyDouble(String, String) </seealso>
        public virtual double? GetPropertyDouble(string schemaNs, string propName) {
            return (double?) GetPropertyObject(schemaNs, propName, VALUE_DOUBLE);
        }


        /// <seealso cref= XMPMeta#SetPropertyDouble(String, String, double, PropertyOptions) </seealso>
        public virtual void SetPropertyDouble(string schemaNs, string propName, double propValue,
                                              PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyDouble(String, String, double) </seealso>
        public virtual void SetPropertyDouble(string schemaNs, string propName, double propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyDate(String, String) </seealso>
        public virtual IXmpDateTime GetPropertyDate(string schemaNs, string propName) {
            return (IXmpDateTime) GetPropertyObject(schemaNs, propName, VALUE_DATE);
        }


        /// <seealso cref= XMPMeta#SetPropertyDate(String, String, XMPDateTime,
        ///      PropertyOptions) </seealso>
        public virtual void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue,
                                            PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyDate(String, String, XMPDateTime) </seealso>
        public virtual void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyCalendar(String, String) </seealso>
        public virtual DateTime GetPropertyCalendar(string schemaNs, string propName) {
            return (DateTime) GetPropertyObject(schemaNs, propName, VALUE_CALENDAR);
        }


        /// <seealso cref= XMPMeta#SetPropertyCalendar(String, String, Calendar,
        ///      PropertyOptions) </seealso>
        public virtual void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue,
                                                PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyCalendar(String, String, Calendar) </seealso>
        public virtual void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <seealso cref= XMPMeta#GetPropertyBase64(String, String) </seealso>
        public virtual sbyte[] GetPropertyBase64(string schemaNs, string propName) {
            return (sbyte[]) GetPropertyObject(schemaNs, propName, VALUE_BASE64);
        }


        /// <seealso cref= XMPMeta#GetPropertyString(String, String) </seealso>
        public virtual string GetPropertyString(string schemaNs, string propName) {
            return (string) GetPropertyObject(schemaNs, propName, VALUE_STRING);
        }


        /// <seealso cref= XMPMeta#SetPropertyBase64(String, String, byte[], PropertyOptions) </seealso>
        public virtual void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue,
                                              PropertyOptions options) {
            SetProperty(schemaNs, propName, propValue, options);
        }


        /// <seealso cref= XMPMeta#SetPropertyBase64(String, String, byte[]) </seealso>
        public virtual void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#getQualifier(String, String, String, String) </seealso>
        public virtual IXmpProperty GetQualifier(string schemaNs, string propName, string qualNs, string qualName) {
            // qualNs and qualName are checked inside composeQualfierPath
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);

            string qualPath = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
            return GetProperty(schemaNs, qualPath);
        }


        /// <seealso cref= XMPMeta#getStructField(String, String, String, String) </seealso>
        public virtual IXmpProperty GetStructField(string schemaNs, string structName, string fieldNs, string fieldName) {
            // fieldNs and fieldName are checked inside composeStructFieldPath
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertStructName(structName);

            string fieldPath = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
            return GetProperty(schemaNs, fieldPath);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#iterator() </seealso>
        public virtual IXmpIterator Iterator() {
            return Iterator(null, null, null);
        }


        /// <seealso cref= XMPMeta#iterator(IteratorOptions) </seealso>
        public virtual IXmpIterator Iterator(IteratorOptions options) {
            return Iterator(null, null, options);
        }


        /// <seealso cref= XMPMeta#iterator(String, String, IteratorOptions) </seealso>
        public virtual IXmpIterator Iterator(string schemaNs, string propName, IteratorOptions options) {
            return new XmpIteratorImpl(this, schemaNs, propName, options);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#setArrayItem(String, String, int, String, PropertyOptions) </seealso>
        public virtual void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue,
                                         PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);

            // Just lookup, don't try to create.
            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, false, null);

            if (arrayNode != null) {
                DoSetArrayItem(arrayNode, itemIndex, itemValue, options, false);
            }
            else {
                throw new XmpException("Specified array does not exist", XmpError.BADXPATH);
            }
        }


        /// <seealso cref= XMPMeta#setArrayItem(String, String, int, String) </seealso>
        public virtual void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue) {
            SetArrayItem(schemaNs, arrayName, itemIndex, itemValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#insertArrayItem(String, String, int, String,
        ///      PropertyOptions) </seealso>
        public virtual void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue,
                                            PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);

            // Just lookup, don't try to create.
            XmpPath arrayPath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode arrayNode = XmpNodeUtils.FindNode(_tree, arrayPath, false, null);

            if (arrayNode != null) {
                DoSetArrayItem(arrayNode, itemIndex, itemValue, options, true);
            }
            else {
                throw new XmpException("Specified array does not exist", XmpError.BADXPATH);
            }
        }


        /// <seealso cref= XMPMeta#insertArrayItem(String, String, int, String) </seealso>
        public virtual void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue) {
            InsertArrayItem(schemaNs, arrayName, itemIndex, itemValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#SetProperty(String, String, Object, PropertyOptions) </seealso>
        public virtual void SetProperty(string schemaNs, string propName, object propValue, PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);

            options = XmpNodeUtils.VerifySetOptions(options, propValue);

            XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);

            XmpNode propNode = XmpNodeUtils.FindNode(_tree, expPath, true, options);
            if (propNode != null) {
                SetNode(propNode, propValue, options, false);
            }
            else {
                throw new XmpException("Specified property does not exist", XmpError.BADXPATH);
            }
        }


        /// <seealso cref= XMPMeta#SetProperty(String, String, Object) </seealso>
        public virtual void SetProperty(string schemaNs, string propName, object propValue) {
            SetProperty(schemaNs, propName, propValue, null);
        }


        /// <exception cref="XmpException"> </exception>
        /// <seealso cref= XMPMeta#setQualifier(String, String, String, String, String,
        ///      PropertyOptions) </seealso>
        public virtual void SetQualifier(string schemaNs, string propName, string qualNs, string qualName,
                                         string qualValue, PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);

            if (!DoesPropertyExist(schemaNs, propName)) {
                throw new XmpException("Specified property does not exist!", XmpError.BADXPATH);
            }

            string qualPath = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
            SetProperty(schemaNs, qualPath, qualValue, options);
        }


        /// <seealso cref= XMPMeta#setQualifier(String, String, String, String, String) </seealso>
        public virtual void SetQualifier(string schemaNs, string propName, string qualNs, string qualName,
                                         string qualValue) {
            SetQualifier(schemaNs, propName, qualNs, qualName, qualValue, null);
        }


        /// <seealso cref= XMPMeta#setStructField(String, String, String, String, String,
        ///      PropertyOptions) </seealso>
        public virtual void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName,
                                           string fieldValue, PropertyOptions options) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertStructName(structName);

            string fieldPath = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
            SetProperty(schemaNs, fieldPath, fieldValue, options);
        }


        /// <seealso cref= XMPMeta#setStructField(String, String, String, String, String) </seealso>
        public virtual void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName,
                                           string fieldValue) {
            SetStructField(schemaNs, structName, fieldNs, fieldName, fieldValue, null);
        }


        /// <seealso cref= XMPMeta#getObjectName() </seealso>
        public virtual string ObjectName {
            get { return _tree.Name ?? ""; }
            set { _tree.Name = value; }
        }


        /// <seealso cref= XMPMeta#getPacketHeader() </seealso>
        public virtual string PacketHeader {
            get { return _packetHeader; }
            set { _packetHeader = value; }
        }


        /// <summary>
        /// Performs a deep clone of the XMPMeta-object
        /// </summary>
        /// <seealso cref= java.lang.Object#clone() </seealso>
        virtual public object Clone() {
            XmpNode clonedTree = (XmpNode) _tree.Clone();
            return new XmpMetaImpl(clonedTree);
        }


        /// <seealso cref= XMPMeta#dumpObject() </seealso>
        public virtual string DumpObject() {
            // renders tree recursively
            return Root.DumpNode(true);
        }


        /// <seealso cref= XMPMeta#sort() </seealso>
        public virtual void Sort() {
            _tree.Sort();
        }


        /// <seealso cref= XMPMeta#normalize(ParseOptions) </seealso>
        public virtual void Normalize(ParseOptions options) {
            if (options == null) {
                options = new ParseOptions();
            }
            XmpNormalizer.Process(this, options);
        }

        #endregion

        /// <summary>
        /// Returns a property, but the result value can be requested. It can be one
        /// of <seealso cref="XMPMetaImpl#VALUE_STRING"/>, <seealso cref="XMPMetaImpl#VALUE_BOOLEAN"/>,
        /// <seealso cref="XMPMetaImpl#VALUE_INTEGER"/>, <seealso cref="XMPMetaImpl#VALUE_LONG"/>,
        /// <seealso cref="XMPMetaImpl#VALUE_DOUBLE"/>, <seealso cref="XMPMetaImpl#VALUE_DATE"/>,
        /// <seealso cref="XMPMetaImpl#VALUE_CALENDAR"/>, <seealso cref="XMPMetaImpl#VALUE_BASE64"/>.
        /// </summary>
        /// <seealso cref= XMPMeta#GetProperty(String, String) </seealso>
        /// <param name="schemaNs">
        ///            a schema namespace </param>
        /// <param name="propName">
        ///            a property name or path </param>
        /// <param name="valueType">
        ///            the type of the value, see VALUE_... </param>
        /// <returns> Returns an <code>XMPProperty</code> </returns>
        /// <exception cref="XmpException">
        ///             Collects any exception that occurs. </exception>
        protected internal virtual IXmpProperty GetProperty(string schemaNs, string propName, int valueType) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);

            XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);
            XmpNode propNode = XmpNodeUtils.FindNode(_tree, expPath, false, null);

            if (propNode != null) {
                if (valueType != VALUE_STRING && propNode.Options.CompositeProperty) {
                    throw new XmpException("Property must be simple when a value type is requested",
                                           XmpError.BADXPATH);
                }

                object value = evaluateNodeValue(valueType, propNode);

                return new XmpPropertyImpl2(propNode, value);
            }
            return null;
        }

        /// <summary>
        /// Returns a property, but the result value can be requested.
        /// </summary>
        /// <seealso cref= XMPMeta#GetProperty(String, String) </seealso>
        /// <param name="schemaNs">
        ///            a schema namespace </param>
        /// <param name="propName">
        ///            a property name or path </param>
        /// <param name="valueType">
        ///            the type of the value, see VALUE_... </param>
        /// <returns> Returns the node value as an object according to the
        ///         <code>valueType</code>. </returns>
        /// <exception cref="XmpException">
        ///             Collects any exception that occurs. </exception>
        protected internal virtual object GetPropertyObject(string schemaNs, string propName, int valueType) {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);

            XmpPath expPath = XmpPathParser.ExpandXPath(schemaNs, propName);
            XmpNode propNode = XmpNodeUtils.FindNode(_tree, expPath, false, null);

            if (propNode != null) {
                if (valueType != VALUE_STRING && propNode.Options.CompositeProperty) {
                    throw new XmpException("Property must be simple when a value type is requested",
                                           XmpError.BADXPATH);
                }

                return evaluateNodeValue(valueType, propNode);
            }
            return null;
        }


        // -------------------------------------------------------------------------------------
        // private


        /// <summary>
        /// Locate or create the item node and set the value. Note the index
        /// parameter is one-based! The index can be in the range [1..size + 1] or
        /// "last()", normalize it and check the insert flags. The order of the
        /// normalization checks is important. If the array is empty we end up with
        /// an index and location to set item size + 1.
        /// </summary>
        /// <param name="arrayNode"> an array node </param>
        /// <param name="itemIndex"> the index where to insert the item </param>
        /// <param name="itemValue"> the item value </param>
        /// <param name="itemOptions"> the options for the new item </param>
        /// <param name="insert"> insert oder overwrite at index position? </param>
        /// <exception cref="XmpException"> </exception>
        private void DoSetArrayItem(XmpNode arrayNode, int itemIndex, string itemValue, PropertyOptions itemOptions,
                                    bool insert) {
            XmpNode itemNode = new XmpNode(ARRAY_ITEM_NAME, null);
            itemOptions = XmpNodeUtils.VerifySetOptions(itemOptions, itemValue);

            // in insert mode the index after the last is allowed,
            // even ARRAY_LAST_ITEM points to the index *after* the last.
            int maxIndex = insert ? arrayNode.ChildrenLength + 1 : arrayNode.ChildrenLength;
            if (itemIndex == ARRAY_LAST_ITEM) {
                itemIndex = maxIndex;
            }

            if (1 <= itemIndex && itemIndex <= maxIndex) {
                if (!insert) {
                    arrayNode.RemoveChild(itemIndex);
                }
                arrayNode.AddChild(itemIndex, itemNode);
                SetNode(itemNode, itemValue, itemOptions, false);
            }
            else {
                throw new XmpException("Array index out of bounds", XmpError.BADINDEX);
            }
        }


        /// <summary>
        /// The internals for SetProperty() and related calls, used after the node is
        /// found or created.
        /// </summary>
        /// <param name="node">
        ///            the newly created node </param>
        /// <param name="value">
        ///            the node value, can be <code>null</code> </param>
        /// <param name="newOptions">
        ///            options for the new node, must not be <code>null</code>. </param>
        /// <param name="deleteExisting"> flag if the existing value is to be overwritten </param>
        /// <exception cref="XmpException"> thrown if options and value do not correspond </exception>
        internal virtual void SetNode(XmpNode node, object value, PropertyOptions newOptions, bool deleteExisting) {
            if (deleteExisting) {
                node.Clear();
            }

            // its checked by setOptions(), if the merged result is a valid options set
            node.Options.MergeWith(newOptions);

            if (!node.Options.CompositeProperty) {
                // This is setting the value of a leaf node.
                XmpNodeUtils.SetNodeValue(node, value);
            }
            else {
                if (value != null && value.ToString().Length > 0) {
                    throw new XmpException("Composite nodes can't have values", XmpError.BADXPATH);
                }

                node.RemoveChildren();
            }
        }


        /// <summary>
        /// Evaluates a raw node value to the given value type, apply special
        /// conversions for defined types in XMP.
        /// </summary>
        /// <param name="valueType">
        ///            an int indicating the value type </param>
        /// <param name="propNode">
        ///            the node containing the value </param>
        /// <returns> Returns a literal value for the node. </returns>
        /// <exception cref="XmpException"> </exception>
        private object evaluateNodeValue(int valueType, XmpNode propNode) {
            object value;
            string rawValue = propNode.Value;
            switch (valueType) {
                case VALUE_BOOLEAN:
                    value = XmpUtils.ConvertToBoolean(rawValue);
                    break;
                case VALUE_INTEGER:
                    value = XmpUtils.ConvertToInteger(rawValue);
                    break;
                case VALUE_LONG:
                    value = XmpUtils.ConvertToLong(rawValue);
                    break;
                case VALUE_DOUBLE:
                    value = XmpUtils.ConvertToDouble(rawValue);
                    break;
                case VALUE_DATE:
                    value = XmpUtils.ConvertToDate(rawValue);
                    break;
                case VALUE_CALENDAR:
                    IXmpDateTime dt = XmpUtils.ConvertToDate(rawValue);
                    value = dt.Calendar;
                    break;
                case VALUE_BASE64:
                    value = XmpUtils.DecodeBase64(rawValue);
                    break;
                default:
                    // leaf values return empty string instead of null
                    // for the other cases the converter methods provides a "null"
                    // value.
                    // a default value can only occur if this method is made public.
                    value = rawValue != null || propNode.Options.CompositeProperty ? rawValue : "";
                    break;
            }
            return value;
        }

        #region Nested type: XmpPropertyImpl1

        private class XmpPropertyImpl1 : IXmpProperty {
            private readonly XmpNode _itemNode;

            public XmpPropertyImpl1(XmpNode itemNode) {
                _itemNode = itemNode;
            }

            #region IXmpProperty Members

            virtual public string Value {
                get { return _itemNode.Value; }
            }


            virtual public PropertyOptions Options {
                get { return _itemNode.Options; }
            }


            virtual public string Language {
                get { return _itemNode.GetQualifier(1).Value; }
            }

            #endregion

            public override string ToString() {
                return _itemNode.Value;
            }
        }

        #endregion

        #region Nested type: XmpPropertyImpl2

        private class XmpPropertyImpl2 : IXmpProperty {
            private readonly XmpNode _propNode;
            private readonly object _value;

            public XmpPropertyImpl2(XmpNode propNode, object value) {
                _value = value;
                _propNode = propNode;
            }

            #region IXmpProperty Members

            virtual public string Value {
                get { return _value != null ? _value.ToString() : null; }
            }


            virtual public PropertyOptions Options {
                get { return _propNode.Options; }
            }


            virtual public string Language {
                get { return null; }
            }

            #endregion

            public override string ToString() {
                return _value.ToString();
            }
        }

        #endregion
    }
}
