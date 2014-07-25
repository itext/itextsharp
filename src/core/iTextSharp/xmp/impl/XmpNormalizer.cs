using System;
using System.Collections;
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
    using XmpConst = XmpConst;
    using XMPDateTime = IXmpDateTime;
    using XMPError = XmpError;
    using XmpException = XmpException;
    using XMPMeta = IXmpMeta;
    using XMPMetaFactory = XmpMetaFactory;
    using XMPUtils = XmpUtils;
    using XMPAliasInfo = IXmpAliasInfo;

    /// <summary>
    /// @since   Aug 18, 2006
    /// </summary>
    public class XmpNormalizer {
        /// <summary>
        /// caches the correct dc-property array forms </summary>
        private static IDictionary _dcArrayForms;

        /// <summary>
        /// init char tables </summary>
        static XmpNormalizer() {
            InitDcArrays();
        }


        /// <summary>
        /// Hidden constructor
        /// </summary>
        private XmpNormalizer() {
            // EMPTY
        }


        /// <summary>
        /// Normalizes a raw parsed XMPMeta-Object </summary>
        /// <param name="xmp"> the raw metadata object </param>
        /// <param name="options"> the parsing options </param>
        /// <returns> Returns the normalized metadata object </returns>
        /// <exception cref="XmpException"> Collects all severe processing errors.  </exception>
        internal static XMPMeta Process(XmpMetaImpl xmp, ParseOptions options) {
            XmpNode tree = xmp.Root;

            TouchUpDataModel(xmp);
            MoveExplicitAliases(tree, options);

            TweakOldXmp(tree);

            DeleteEmptySchemas(tree);

            return xmp;
        }


        /// <summary>
        /// Tweak old XMP: Move an instance ID from rdf:about to the
        /// <em>xmpMM:InstanceID</em> property. An old instance ID usually looks
        /// like &quot;uuid:bac965c4-9d87-11d9-9a30-000d936b79c4&quot;, plus InDesign
        /// 3.0 wrote them like &quot;bac965c4-9d87-11d9-9a30-000d936b79c4&quot;. If
        /// the name looks like a UUID simply move it to <em>xmpMM:InstanceID</em>,
        /// don't worry about any existing <em>xmpMM:InstanceID</em>. Both will
        /// only be present when a newer file with the <em>xmpMM:InstanceID</em>
        /// property is updated by an old app that uses <em>rdf:about</em>.
        /// </summary>
        /// <param name="tree"> the root of the metadata tree </param>
        /// <exception cref="XmpException"> Thrown if tweaking fails.  </exception>
        private static void TweakOldXmp(XmpNode tree) {
            if (tree.Name != null && tree.Name.Length >= Utils.UUID_LENGTH) {
                string nameStr = tree.Name.ToLower();
                if (nameStr.StartsWith("uuid:")) {
                    nameStr = nameStr.Substring(5);
                }

                if (Utils.CheckUuidFormat(nameStr)) {
                    // move UUID to xmpMM:InstanceID and remove it from the root node
                    XmpPath path = XmpPathParser.ExpandXPath(XmpConst.NS_XMP_MM, "InstanceID");
                    XmpNode idNode = XmpNodeUtils.FindNode(tree, path, true, null);
                    if (idNode != null) {
                        idNode.Options = null; // Clobber any existing xmpMM:InstanceID.
                        idNode.Value = "uuid:" + nameStr;
                        idNode.RemoveChildren();
                        idNode.RemoveQualifiers();
                        tree.Name = null;
                    }
                    else {
                        throw new XmpException("Failure creating xmpMM:InstanceID", XmpError.INTERNALFAILURE);
                    }
                }
            }
        }


        /// <summary>
        /// Visit all schemas to do general fixes and handle special cases.
        /// </summary>
        /// <param name="xmp"> the metadata object implementation </param>
        /// <exception cref="XmpException"> Thrown if the normalisation fails. </exception>
        private static void TouchUpDataModel(XmpMetaImpl xmp) {
            // make sure the DC schema is existing, because it might be needed within the normalization
            // if not touched it will be removed by removeEmptySchemas
            XmpNodeUtils.FindSchemaNode(xmp.Root, XmpConst.NS_DC, true);

            // Do the special case fixes within each schema.
            IEnumerator it = xmp.Root.IterateChildren();
            while (it.MoveNext()) {
                XmpNode currSchema = (XmpNode) it.Current;
                if (currSchema != null && XmpConst.NS_DC.Equals(currSchema.Name)) {
                    NormalizeDcArrays(currSchema);
                }
                else if (currSchema != null && XmpConst.NS_EXIF.Equals(currSchema.Name)) {
                    // Do a special case fix for exif:GPSTimeStamp.
                    FixGpsTimeStamp(currSchema);
                    XmpNode arrayNode = XmpNodeUtils.FindChildNode(currSchema, "exif:UserComment", false);
                    if (arrayNode != null) {
                        RepairAltText(arrayNode);
                    }
                }
                else if (currSchema != null && XmpConst.NS_DM.Equals(currSchema.Name)) {
                    // Do a special case migration of xmpDM:copyright to
                    // dc:rights['x-default'].
                    XmpNode dmCopyright = XmpNodeUtils.FindChildNode(currSchema, "xmpDM:copyright", false);
                    if (dmCopyright != null) {
                        MigrateAudioCopyright(xmp, dmCopyright);
                    }
                }
                else if (currSchema != null && XmpConst.NS_XMP_RIGHTS.Equals(currSchema.Name)) {
                    XmpNode arrayNode = XmpNodeUtils.FindChildNode(currSchema, "xmpRights:UsageTerms", false);
                    if (arrayNode != null) {
                        RepairAltText(arrayNode);
                    }
                }
            }
        }


        /// <summary>
        /// Undo the denormalization performed by the XMP used in Acrobat 5.<br> 
        /// If a Dublin Core array had only one item, it was serialized as a simple
        /// property. <br>
        /// The <code>xml:lang</code> attribute was dropped from an
        /// <code>alt-text</code> item if the language was <code>x-default</code>.
        /// </summary>
        /// <param name="dcSchema"> the DC schema node </param>
        /// <exception cref="XmpException"> Thrown if normalization fails </exception>
        private static void NormalizeDcArrays(XmpNode dcSchema) {
            for (int i = 1; i <= dcSchema.ChildrenLength; i++) {
                XmpNode currProp = dcSchema.GetChild(i);

                PropertyOptions arrayForm = (PropertyOptions) _dcArrayForms[currProp.Name];
                if (arrayForm == null) {
                    continue;
                }
                if (currProp.Options.Simple) {
                    // create a new array and add the current property as child, 
                    // if it was formerly simple 
                    XmpNode newArray = new XmpNode(currProp.Name, arrayForm);
                    currProp.Name = XmpConst.ARRAY_ITEM_NAME;
                    newArray.AddChild(currProp);
                    dcSchema.ReplaceChild(i, newArray);

                    // fix language alternatives
                    if (arrayForm.ArrayAltText && !currProp.Options.HasLanguage) {
                        XmpNode newLang = new XmpNode(XmpConst.XML_LANG, XmpConst.X_DEFAULT, null);
                        currProp.AddQualifier(newLang);
                    }
                }
                else {
                    // clear array options and add corrected array form if it has been an array before
                    currProp.Options.SetOption(
                        PropertyOptions.ARRAY | PropertyOptions.ARRAY_ORDERED | PropertyOptions.ARRAY_ALTERNATE |
                        PropertyOptions.ARRAY_ALT_TEXT, false);
                    currProp.Options.MergeWith(arrayForm);

                    if (arrayForm.ArrayAltText) {
                        // applying for "dc:description", "dc:rights", "dc:title"
                        RepairAltText(currProp);
                    }
                }
            }
        }


        /// <summary>
        /// Make sure that the array is well-formed AltText. Each item must be simple
        /// and have an "xml:lang" qualifier. If repairs are needed, keep simple
        /// non-empty items by adding the "xml:lang" with value "x-repair". </summary>
        /// <param name="arrayNode"> the property node of the array to repair. </param>
        /// <exception cref="XmpException"> Forwards unexpected exceptions. </exception>
        private static void RepairAltText(XmpNode arrayNode) {
            if (arrayNode == null || !arrayNode.Options.Array) {
                // Already OK or not even an array.
                return;
            }

            // fix options
            arrayNode.Options.ArrayOrdered = true;
            arrayNode.Options.ArrayAlternate = true;
            arrayNode.Options.ArrayAltText = true;
            ArrayList currChildsToRemove = new ArrayList();
            IEnumerator it = arrayNode.IterateChildren();
            while (it.MoveNext()) {
                XmpNode currChild = (XmpNode) it.Current;
                if (currChild == null)
                    continue;
                if (currChild.Options.CompositeProperty) {
                    // Delete non-simple children.
                    currChildsToRemove.Add(currChild);
                }
                else if (!currChild.Options.HasLanguage) {
                    string childValue = currChild.Value;
                    if (String.IsNullOrEmpty(childValue)) {
                        // Delete empty valued children that have no xml:lang.
                        currChildsToRemove.Add(currChild);
                    }
                    else {
                        // Add an xml:lang qualifier with the value "x-repair".
                        XmpNode repairLang = new XmpNode(XmpConst.XML_LANG, "x-repair", null);
                        currChild.AddQualifier(repairLang);
                    }
                }
            }
            foreach (object o in currChildsToRemove) {
                arrayNode.Children.Remove(o);
            }
        }


        /// <summary>
        /// Visit all of the top level nodes looking for aliases. If there is
        /// no base, transplant the alias subtree. If there is a base and strict
        /// aliasing is on, make sure the alias and base subtrees match.
        /// </summary>
        /// <param name="tree"> the root of the metadata tree </param>
        /// <param name="options"> th parsing options </param>
        /// <exception cref="XmpException"> Forwards XMP errors </exception>
        private static void MoveExplicitAliases(XmpNode tree, ParseOptions options) {
            if (!tree.HasAliases) {
                return;
            }
            tree.HasAliases = false;

            bool strictAliasing = options.StrictAliasing;
            IEnumerator schemaIt = tree.UnmodifiableChildren.GetEnumerator();
            while (schemaIt.MoveNext()) {
                XmpNode currSchema = (XmpNode) schemaIt.Current;
                if (currSchema == null)
                    continue;
                if (!currSchema.HasAliases) {
                    continue;
                }

                ArrayList currPropsToRemove = new ArrayList();
                IEnumerator propertyIt = currSchema.IterateChildren();
                while (propertyIt.MoveNext()) {
                    XmpNode currProp = (XmpNode) propertyIt.Current;
                    if (currProp == null)
                        continue;

                    if (!currProp.Alias) {
                        continue;
                    }

                    currProp.Alias = false;

                    // Find the base path, look for the base schema and root node.
                    XMPAliasInfo info = XMPMetaFactory.SchemaRegistry.FindAlias(currProp.Name);
                    if (info != null) {
                        // find or create schema
                        XmpNode baseSchema = XmpNodeUtils.FindSchemaNode(tree, info.Namespace, null, true);
                        baseSchema.Implicit = false;

                        XmpNode baseNode = XmpNodeUtils.FindChildNode(baseSchema, info.Prefix + info.PropName, false);
                        if (baseNode == null) {
                            if (info.AliasForm.Simple) {
                                // A top-to-top alias, transplant the property.
                                // change the alias property name to the base name
                                string qname = info.Prefix + info.PropName;
                                currProp.Name = qname;
                                baseSchema.AddChild(currProp);
                            }
                            else {
                                // An alias to an array item, 
                                // create the array and transplant the property.
                                baseNode = new XmpNode(info.Prefix + info.PropName, info.AliasForm.ToPropertyOptions());
                                baseSchema.AddChild(baseNode);
                                TransplantArrayItemAlias(currProp, baseNode);
                            }
                            currPropsToRemove.Add(currProp);
                        }
                        else if (info.AliasForm.Simple) {
                            // The base node does exist and this is a top-to-top alias.
                            // Check for conflicts if strict aliasing is on. 
                            // Remove and delete the alias subtree.
                            if (strictAliasing) {
                                CompareAliasedSubtrees(currProp, baseNode, true);
                            }
                            currPropsToRemove.Add(currProp);
                        }
                        else {
                            // This is an alias to an array item and the array exists.
                            // Look for the aliased item.
                            // Then transplant or check & delete as appropriate.

                            XmpNode itemNode = null;
                            if (info.AliasForm.ArrayAltText) {
                                int xdIndex = XmpNodeUtils.LookupLanguageItem(baseNode, XmpConst.X_DEFAULT);
                                if (xdIndex != -1) {
                                    itemNode = baseNode.GetChild(xdIndex);
                                }
                            }
                            else if (baseNode.HasChildren()) {
                                itemNode = baseNode.GetChild(1);
                            }

                            if (itemNode == null) {
                                TransplantArrayItemAlias(currProp, baseNode);
                            }
                            else {
                                if (strictAliasing) {
                                    CompareAliasedSubtrees(currProp, itemNode, true);
                                }
                            }
                            currPropsToRemove.Add(currProp);
                        }
                    }
                }
                foreach (object o in currPropsToRemove)
                    currSchema.Children.Remove(o);
                currPropsToRemove.Clear();
                currSchema.HasAliases = false;
            }
        }


        /// <summary>
        /// Moves an alias node of array form to another schema into an array </summary>
        /// <param name="childNode"> the node to be moved </param>
        /// <param name="baseArray"> the base array for the array item </param>
        /// <exception cref="XmpException"> Forwards XMP errors </exception>
        private static void TransplantArrayItemAlias(XmpNode childNode, XmpNode baseArray) {
            if (baseArray.Options.ArrayAltText) {
                if (childNode.Options.HasLanguage) {
                    throw new XmpException("Alias to x-default already has a language qualifier",
                                           XmpError.BADXMP);
                }

                XmpNode langQual = new XmpNode(XmpConst.XML_LANG, XmpConst.X_DEFAULT, null);
                childNode.AddQualifier(langQual);
            }

            childNode.Name = XmpConst.ARRAY_ITEM_NAME;
            baseArray.AddChild(childNode);
        }


        /// <summary>
        /// Fixes the GPS Timestamp in EXIF. </summary>
        /// <param name="exifSchema"> the EXIF schema node </param>
        /// <exception cref="XmpException"> Thrown if the date conversion fails. </exception>
        private static void FixGpsTimeStamp(XmpNode exifSchema) {
            // Note: if dates are not found the convert-methods throws an exceptions,
            // 		 and this methods returns.
            XmpNode gpsDateTime = XmpNodeUtils.FindChildNode(exifSchema, "exif:GPSTimeStamp", false);
            if (gpsDateTime == null) {
                return;
            }

            try {
                XMPDateTime binGpsStamp = XMPUtils.ConvertToDate(gpsDateTime.Value);
                if (binGpsStamp.Year != 0 || binGpsStamp.Month != 0 || binGpsStamp.Day != 0) {
                    return;
                }

                XmpNode otherDate = XmpNodeUtils.FindChildNode(exifSchema, "exif:DateTimeOriginal", false);
                otherDate = otherDate ?? XmpNodeUtils.FindChildNode(exifSchema, "exif:DateTimeDigitized", false);

                XMPDateTime binOtherDate = XMPUtils.ConvertToDate(otherDate.Value);
                XmpCalendar cal = binGpsStamp.Calendar;
                DateTime dt = new DateTime(binOtherDate.Year, binOtherDate.Month, binOtherDate.Day, cal.DateTime.Hour,
                                           cal.DateTime.Minute, cal.DateTime.Second, cal.DateTime.Millisecond);
                cal.DateTime = dt;
                binGpsStamp = new XmpDateTimeImpl(cal);
                gpsDateTime.Value = XMPUtils.ConvertFromDate(binGpsStamp);
            }
            catch (XmpException) {
            }
        }


        /// <summary>
        /// Remove all empty schemas from the metadata tree that were generated during the rdf parsing. </summary>
        /// <param name="tree"> the root of the metadata tree </param>
        private static void DeleteEmptySchemas(XmpNode tree) {
            // Delete empty schema nodes. Do this last, other cleanup can make empty
            // schema.
            ArrayList schemasToRemove = new ArrayList();
            foreach (XmpNode schema in tree.Children) {
                if (!schema.HasChildren()) {
                    schemasToRemove.Add(schema);
                }
            }
            foreach (XmpNode xmpNode in schemasToRemove) {
                tree.Children.Remove(xmpNode);
            }
        }


        /// <summary>
        /// The outermost call is special. The names almost certainly differ. The
        /// qualifiers (and hence options) will differ for an alias to the x-default
        /// item of a langAlt array.
        /// </summary>
        /// <param name="aliasNode"> the alias node </param>
        /// <param name="baseNode"> the base node of the alias </param>
        /// <param name="outerCall"> marks the outer call of the recursion </param>
        /// <exception cref="XmpException"> Forwards XMP errors  </exception>
        private static void CompareAliasedSubtrees(XmpNode aliasNode, XmpNode baseNode, bool outerCall) {
            if (!aliasNode.Value.Equals(baseNode.Value) || aliasNode.ChildrenLength != baseNode.ChildrenLength) {
                throw new XmpException("Mismatch between alias and base nodes", XmpError.BADXMP);
            }

            if (!outerCall &&
                (!aliasNode.Name.Equals(baseNode.Name) || !aliasNode.Options.Equals(baseNode.Options) ||
                 aliasNode.QualifierLength != baseNode.QualifierLength)) {
                throw new XmpException("Mismatch between alias and base nodes", XmpError.BADXMP);
            }

            for (IEnumerator an = aliasNode.IterateChildren(), bn = baseNode.IterateChildren();
                 an.MoveNext() && bn.MoveNext();) {
                XmpNode aliasChild = (XmpNode) an.Current;
                XmpNode baseChild = (XmpNode) bn.Current;
                CompareAliasedSubtrees(aliasChild, baseChild, false);
            }


            for (IEnumerator an = aliasNode.IterateQualifier(), bn = baseNode.IterateQualifier();
                 an.MoveNext() && bn.MoveNext();) {
                XmpNode aliasQual = (XmpNode) an.Current;
                XmpNode baseQual = (XmpNode) bn.Current;
                CompareAliasedSubtrees(aliasQual, baseQual, false);
            }
        }


        /// <summary>
        /// The initial support for WAV files mapped a legacy ID3 audio copyright
        /// into a new xmpDM:copyright property. This is special case code to migrate
        /// that into dc:rights['x-default']. The rules:
        /// 
        /// <pre>
        /// 1. If there is no dc:rights array, or an empty array -
        ///    Create one with dc:rights['x-default'] set from double linefeed and xmpDM:copyright.
        /// 
        /// 2. If there is a dc:rights array but it has no x-default item -
        ///    Create an x-default item as a copy of the first item then apply rule #3.
        /// 
        /// 3. If there is a dc:rights array with an x-default item, 
        ///    Look for a double linefeed in the value.
        ///     A. If no double linefeed, compare the x-default value to the xmpDM:copyright value.
        ///         A1. If they match then leave the x-default value alone.
        ///         A2. Otherwise, append a double linefeed and 
        ///             the xmpDM:copyright value to the x-default value.
        ///     B. If there is a double linefeed, compare the trailing text to the xmpDM:copyright value.
        ///         B1. If they match then leave the x-default value alone.
        ///         B2. Otherwise, replace the trailing x-default text with the xmpDM:copyright value.
        /// 
        /// 4. In all cases, delete the xmpDM:copyright property.
        /// </pre>
        /// </summary>
        /// <param name="xmp"> the metadata object </param>
        /// <param name="dmCopyright"> the "dm:copyright"-property </param>
        private static void MigrateAudioCopyright(XMPMeta xmp, XmpNode dmCopyright) {
            try {
                XmpNode dcSchema = XmpNodeUtils.FindSchemaNode(((XmpMetaImpl) xmp).Root, XmpConst.NS_DC, true);

                string dmValue = dmCopyright.Value;
                const string doubleLf = "\n\n";

                XmpNode dcRightsArray = XmpNodeUtils.FindChildNode(dcSchema, "dc:rights", false);

                if (dcRightsArray == null || !dcRightsArray.HasChildren()) {
                    // 1. No dc:rights array, create from double linefeed and xmpDM:copyright.
                    dmValue = doubleLf + dmValue;
                    xmp.SetLocalizedText(XmpConst.NS_DC, "rights", "", XmpConst.X_DEFAULT, dmValue, null);
                }
                else {
                    int xdIndex = XmpNodeUtils.LookupLanguageItem(dcRightsArray, XmpConst.X_DEFAULT);

                    if (xdIndex < 0) {
                        // 2. No x-default item, create from the first item.
                        string firstValue = dcRightsArray.GetChild(1).Value;
                        xmp.SetLocalizedText(XmpConst.NS_DC, "rights", "", XmpConst.X_DEFAULT, firstValue, null);
                        xdIndex = XmpNodeUtils.LookupLanguageItem(dcRightsArray, XmpConst.X_DEFAULT);
                    }

                    // 3. Look for a double linefeed in the x-default value.
                    XmpNode defaultNode = dcRightsArray.GetChild(xdIndex);
                    string defaultValue = defaultNode.Value;
                    int lfPos = defaultValue.IndexOf(doubleLf);

                    if (lfPos < 0) {
                        // 3A. No double LF, compare whole values.
                        if (!dmValue.Equals(defaultValue)) {
                            // 3A2. Append the xmpDM:copyright to the x-default
                            // item.
                            defaultNode.Value = defaultValue + doubleLf + dmValue;
                        }
                    }
                    else {
                        // 3B. Has double LF, compare the tail.
                        if (!defaultValue.Substring(lfPos + 2).Equals(dmValue)) {
                            // 3B2. Replace the x-default tail.
                            defaultNode.Value = defaultValue.Substring(0, lfPos + 2) + dmValue;
                        }
                    }
                }

                // 4. Get rid of the xmpDM:copyright.
                dmCopyright.Parent.RemoveChild(dmCopyright);
            }
            catch (XmpException) {
                // Don't let failures (like a bad dc:rights form) stop other
                // cleanup.
            }
        }


        /// <summary>
        /// Initializes the map that contains the known arrays, that are fixed by 
        /// <seealso cref="NormalizeDcArrays"/>. 
        /// </summary>
        private static void InitDcArrays() {
            _dcArrayForms = new Hashtable();

            // Properties supposed to be a "Bag".
            PropertyOptions bagForm = new PropertyOptions();
            bagForm.Array = true;
            _dcArrayForms["dc:contributor"] = bagForm;
            _dcArrayForms["dc:language"] = bagForm;
            _dcArrayForms["dc:publisher"] = bagForm;
            _dcArrayForms["dc:relation"] = bagForm;
            _dcArrayForms["dc:subject"] = bagForm;
            _dcArrayForms["dc:type"] = bagForm;

            // Properties supposed to be a "Seq".
            PropertyOptions seqForm = new PropertyOptions();
            seqForm.Array = true;
            seqForm.ArrayOrdered = true;
            _dcArrayForms["dc:creator"] = seqForm;
            _dcArrayForms["dc:date"] = seqForm;

            // Properties supposed to be an "Alt" in alternative-text form.
            PropertyOptions altTextForm = new PropertyOptions();
            altTextForm.Array = true;
            altTextForm.ArrayOrdered = true;
            altTextForm.ArrayAlternate = true;
            altTextForm.ArrayAltText = true;
            _dcArrayForms["dc:description"] = altTextForm;
            _dcArrayForms["dc:rights"] = altTextForm;
            _dcArrayForms["dc:title"] = altTextForm;
        }
    }
}
