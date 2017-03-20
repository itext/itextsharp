/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.tool.xml;
/**
 *
 */
namespace iTextSharp.tool.xml.css {

    /**
     * @author Emiel Ackermann
     *
     */
    public class FontSizeTranslator {

        public const float DEFAULT_FONT_SIZE = 12f;


        /**
         *
         */
        private static CssUtils utils = CssUtils.GetInstance();
        private static FontSizeTranslator myself;
        private static object syncroot = new object();

        /**
         * @return Singleton instance of FontSizeTranslater.
         */
        public static FontSizeTranslator GetInstance() {
                if (myself != null)
                    return myself;
                lock (syncroot) {
                    if (null == myself) {
                        myself = new FontSizeTranslator();
                    }
                    return myself;
                }
        }

        /**
         * Returns the css value of the style <b>font-size</b> in a pt-value. Possible font-size values:
         * <ul>
         *  <li>a constant in px, in, cm, mm, pc, em or ex,</li>
         *  <li>xx-small,</li>
         *  <li>x-small,</li>
         *  <li>small,</li>
         *  <li>medium,</li>
         *  <li>large,</li>
         *  <li>x-large,</li>
         *  <li>xx-large,</li>
         *  <li>smaller (than tag's parent size),</li>
         *  <li>larger (than tag's parent size),</li>
         *  <li>a percentage (e.g font-size:250%) of tag's parent size,</li>
         * </ul>
         * @param tag to get the font size of.
         * @return float font size of the content of the tag in pt.
         */
        virtual public float TranslateFontSize(Tag tag) {
            float size = Font.UNDEFINED;
            if (tag.CSS.ContainsKey(CSS.Property.FONT_SIZE)) {
                String value = tag.CSS[CSS.Property.FONT_SIZE];
                 if (Util.EqualsIgnoreCase(value, CSS.Value.XX_SMALL)){
                     size = 6.75f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.X_SMALL)){
                     size = 7.5f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.SMALL)){
                     size = 9.75f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.MEDIUM)){
                     size = 12f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.LARGE)){
                     size = 13.5f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.X_LARGE)){
                     size = 18f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.XX_LARGE)){
                     size = 24f;
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.SMALLER)){
                     if (tag.Parent != null) {
                         float parentSize = GetFontSize(tag.Parent); // if the font-size of the parent can be set in some memory the translation part is not needed anymore.
                         if (parentSize == Font.UNDEFINED) {
                             size = 9.75f;
                         } else if (parentSize <= 6.75f) {
                             size = parentSize - 1;
                         } else if (parentSize == 7.5f) {
                             size = 6.75f;
                         } else if (parentSize == 9.75f) {
                             size = 7.5f;
                         } else if (parentSize == 12f) {
                             size = 9.75f;
                         } else if (parentSize == 13.5f) {
                             size = 12f;
                         } else if (parentSize == 18f) {
                             size = 13.5f;
                         } else if (parentSize == 24f) {
                             size = 18f;
                         } else if (parentSize < 24f) {
                             size = parentSize*0.85f;
                         } else if (parentSize >= 24) {
                             size = parentSize*2/3;
                         }
                     } else {
                         size = 9.75f;
                     }
                 } else if (Util.EqualsIgnoreCase(value, CSS.Value.LARGER)){
                     if (tag.Parent != null) {
                         float parentSize = GetFontSize(tag.Parent); // if the font-size of the parent can be set in some memory the translation part is not needed anymore.
                         if (parentSize == Font.UNDEFINED) {
                             size = 13.5f;
                         } else if (parentSize == 6.75f) {
                             size = 7.5f;
                         } else if (parentSize == 7.5f){
                             size = 9.75f;
                         } else if (parentSize == 9.75f){
                             size = 12f;
                         } else if (parentSize == 12f){
                             size = 13.5f;
                         } else if (parentSize == 13.5f){
                             size = 18f;
                         } else if (parentSize == 18f){
                             size = 24f;
                         } else {
                             size = parentSize * 1.5f;
                         }
                     } else {
                         size = 13.5f;
                     }
                 } else if (utils.IsMetricValue(value)||utils.IsNumericValue(value)){
                     size = utils.ParsePxInCmMmPcToPt(value);
                 } else if (utils.IsRelativeValue(value)) {
                    float baseValue = Font.UNDEFINED;
                    if (tag.Parent != null) {
                        baseValue = GetFontSize(tag.Parent);
                    }
                    if (baseValue == Font.UNDEFINED) {
                        baseValue = 12;
                    }
                    size = utils.ParseRelativeValue(value, baseValue);
                 }
            }
            return size;
        }

        /**
         * Retrieves the pt font size from {@link Tag#getCSS()} with {@link CSS.Property#FONT_SIZE} or returns default 12pt
         * @param tag the tag to get the font-size from.
         * @return the font size
         */
        virtual public float GetFontSize(Tag tag) {
            String str;
            tag.CSS.TryGetValue(CSS.Property.FONT_SIZE, out str);
            if (null != str) {
                return float.Parse(str.Replace("pt", ""), CultureInfo.InvariantCulture);
            }
            return Font.UNDEFINED;
        }
    }
}
