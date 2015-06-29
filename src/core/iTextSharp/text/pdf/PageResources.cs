using System.Collections.Generic;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf {
    public class PageResources {
        
        protected PdfDictionary fontDictionary = new PdfDictionary();
        protected PdfDictionary xObjectDictionary = new PdfDictionary();
        protected PdfDictionary colorDictionary = new PdfDictionary();
        protected PdfDictionary patternDictionary = new PdfDictionary();
        protected PdfDictionary shadingDictionary = new PdfDictionary();
        protected PdfDictionary extGStateDictionary = new PdfDictionary();
        protected PdfDictionary propertyDictionary = new PdfDictionary();
        protected Dictionary<PdfName,object> forbiddenNames;
        protected PdfDictionary originalResources;
        protected int[] namePtr = {0};
        protected Dictionary<PdfName,PdfName> usedNames;

        internal PageResources() {
        }
        
        internal void SetOriginalResources(PdfDictionary resources, int[] newNamePtr) {
            if (newNamePtr != null)
                namePtr = newNamePtr;
            forbiddenNames = new Dictionary<PdfName,object>();
            usedNames = new Dictionary<PdfName,PdfName>();
            if (resources == null)
                return;
            originalResources = new PdfDictionary();
            originalResources.Merge(resources);
            foreach (PdfName key in resources.Keys) {
                PdfObject sub = PdfReader.GetPdfObject(resources.Get(key));
                if (sub != null && sub.IsDictionary()) {
                    PdfDictionary dic = (PdfDictionary)sub;
                    foreach (PdfName name in dic.Keys) {
                        forbiddenNames[name] = null;
                    }
                    PdfDictionary dic2 = new PdfDictionary();
                    dic2.Merge(dic);
                    originalResources.Put(key, dic2);
                }
            }
        }
        
        internal PdfName TranslateName(PdfName name) {
            PdfName translated = name;
            if (forbiddenNames != null) {
                usedNames.TryGetValue(name, out translated);
                if (translated == null) {
                    while (true) {
                        translated = new PdfName("Xi" + (namePtr[0]++));
                        if (!forbiddenNames.ContainsKey(translated))
                            break;
                    }
                    usedNames[name] = translated;
                }
            }
            return translated;
        }
        
        internal PdfName AddFont(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            fontDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddXObject(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            xObjectDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddColor(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            colorDictionary.Put(name, reference);
            return name;
        }

        internal void AddDefaultColor(PdfName name, PdfObject obj) {
            if (obj == null || obj.IsNull())
                colorDictionary.Remove(name);
            else
                colorDictionary.Put(name, obj);
        }

        internal void AddDefaultColor(PdfDictionary dic) {
            colorDictionary.Merge(dic);
        }

        internal void AddDefaultColorDiff(PdfDictionary dic) {
            colorDictionary.MergeDifferent(dic);
        }

        internal PdfName AddShading(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            shadingDictionary.Put(name, reference);
            return name;
        }
        
        internal PdfName AddPattern(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            patternDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddExtGState(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            extGStateDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddProperty(PdfName name, PdfIndirectReference reference) {
            name = TranslateName(name);
            propertyDictionary.Put(name, reference);
            return name;
        }

        internal PdfDictionary Resources {
            get {
                PdfResources resources = new PdfResources();
                if (originalResources != null)
                    resources.Merge(originalResources);
                resources.Add(PdfName.FONT, fontDictionary);
                resources.Add(PdfName.XOBJECT, xObjectDictionary);
                resources.Add(PdfName.COLORSPACE, colorDictionary);
                resources.Add(PdfName.PATTERN, patternDictionary);
                resources.Add(PdfName.SHADING, shadingDictionary);
                resources.Add(PdfName.EXTGSTATE, extGStateDictionary);
                resources.Add(PdfName.PROPERTIES, propertyDictionary);
                return resources;
            }
        }
    
        internal bool HasResources() {
            return (fontDictionary.Size > 0
                || xObjectDictionary.Size > 0
                || colorDictionary.Size > 0
                || patternDictionary.Size > 0
                || shadingDictionary.Size > 0
                || extGStateDictionary.Size > 0
                || propertyDictionary.Size > 0);
        }
    }
}
