using System;
using System.Collections.Generic;
using System.util;

using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;

/*
 * $Id$
 * 
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
namespace iTextSharp.text {
    /// <summary>
    /// If you are using True Type fonts, you can declare the paths of the different ttf- and ttc-files
    /// to this static class first and then create fonts in your code using one of the static getFont-method
    /// without having to enter a path as parameter.
    /// </summary>
    public sealed class FontFactory {
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string COURIER = BaseFont.COURIER;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string COURIER_BOLD = BaseFont.COURIER_BOLD;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string COURIER_OBLIQUE = BaseFont.COURIER_OBLIQUE;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string COURIER_BOLDOBLIQUE = BaseFont.COURIER_BOLDOBLIQUE;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string HELVETICA = BaseFont.HELVETICA;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string HELVETICA_BOLD = BaseFont.HELVETICA_BOLD;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string HELVETICA_OBLIQUE = BaseFont.HELVETICA_OBLIQUE;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string HELVETICA_BOLDOBLIQUE = BaseFont.HELVETICA_BOLDOBLIQUE;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string SYMBOL = BaseFont.SYMBOL;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string TIMES = "Times";
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string TIMES_ROMAN = BaseFont.TIMES_ROMAN;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string TIMES_BOLD = BaseFont.TIMES_BOLD;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string TIMES_ITALIC = BaseFont.TIMES_ITALIC;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string TIMES_BOLDITALIC = BaseFont.TIMES_BOLDITALIC;
    
        /// <summary> This is a possible value of a base 14 type 1 font </summary>
        public const string ZAPFDINGBATS = BaseFont.ZAPFDINGBATS;
        
        private static FontFactoryImp fontImp = new FontFactoryImp();

        /// <summary> This is the default encoding to use. </summary>
        private const string defaultEncoding = BaseFont.WINANSI;
    
        /// <summary> This is the default value of the <VAR>embedded</VAR> variable. </summary>
        private const bool defaultEmbedding = BaseFont.NOT_EMBEDDED;
    
        /// <summary> Creates new FontFactory </summary>
        private FontFactory() {
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color) {
            return fontImp.GetFont(fontname, encoding, embedded, size, style, color);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <param name="cached">true if the font comes from the cache or is added to the cache if new, false if the font is always created new</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color, bool cached) {
            return fontImp.GetFont(fontname, encoding, embedded, size, style, color, cached);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style) {
            return GetFont(fontname, encoding, embedded, size, style, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <param name="size">the size of this font</param>
        /// <returns></returns>
        public static Font GetFont(string fontname, string encoding, bool embedded, float size) {
            return GetFont(fontname, encoding, embedded, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, bool embedded) {
            return GetFont(fontname, encoding, embedded, Font.UNDEFINED, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, float size, int style, BaseColor color) {
            return GetFont(fontname, encoding, defaultEmbedding, size, style, color);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, float size, int style) {
            return GetFont(fontname, encoding, defaultEmbedding, size, style, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="size">the size of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding, float size) {
            return GetFont(fontname, encoding, defaultEmbedding, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, string encoding) {
            return GetFont(fontname, encoding, defaultEmbedding, Font.UNDEFINED, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, float size, int style, BaseColor color) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, color);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, float size, BaseColor color) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, color);
        }
        
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, float size, int style) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname, float size) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <returns>a Font object</returns>
        public static Font GetFont(string fontname) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, Font.UNDEFINED, Font.UNDEFINED, null);
        }

        /**
        * Register a font by giving explicitly the font family and name.
        * @param familyName the font family
        * @param fullName the font name
        * @param path the font path
        */
        public static void RegisterFamily(String familyName, String fullName, String path) {
            fontImp.RegisterFamily(familyName, fullName, path);
        }
        
        public static void Register(Properties attributes) {
            string path;
            string alias = null;

            path = attributes.Remove("path");
            alias = attributes.Remove("alias");

            fontImp.Register(path, alias);
        }
    
        /// <summary>
        /// Register a ttf- or a ttc-file.
        /// </summary>
        /// <param name="path">the path to a ttf- or ttc-file</param>
        public static void Register(string path) {
            Register(path, null);
        }
    
        /// <summary>
        /// Register a ttf- or a ttc-file and use an alias for the font contained in the ttf-file.
        /// </summary>
        /// <param name="path">the path to a ttf- or ttc-file</param>
        /// <param name="alias">the alias you want to use for the font</param>
        public static void Register(string path, string alias) {
            fontImp.Register(path, alias);
        }
    
        /** Register all the fonts in a directory.
        * @param dir the directory
        * @return the number of fonts registered
        */    
        public static int RegisterDirectory(String dir) {
            return fontImp.RegisterDirectory(dir);
        }

        /**
        * Register all the fonts in a directory and possibly its subdirectories.
        * @param dir the directory
        * @param scanSubdirectories recursively scan subdirectories if <code>true</true>
        * @return the number of fonts registered
        * @since 2.1.2
        */
        public static int RegisterDirectory(String dir, bool scanSubdirectories) {
            return fontImp.RegisterDirectory(dir, scanSubdirectories);
        }
            
        /** Register fonts in some probable directories. It usually works in Windows,
        * Linux and Solaris.
        * @return the number of fonts registered
        */    
        public static int RegisterDirectories() {
            return fontImp.RegisterDirectories();
        }

        /// <summary>
        /// Gets a set of registered fontnames.
        /// </summary>
        /// <value>a set of registered fontnames</value>
        public static ICollection<string> RegisteredFonts {
            get {
                return fontImp.RegisteredFonts;
            }
        }
    
        /// <summary>
        /// Gets a set of registered font families.
        /// </summary>
        /// <value>a set of registered font families</value>
        public static ICollection<string> RegisteredFamilies {
            get {
                return fontImp.RegisteredFamilies;
            }
        }
    
        /// <summary>
        /// Checks whether the given font is contained within the object
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <returns>true if font is contained within the object</returns>
        public static bool Contains(string fontname) {
            return fontImp.IsRegistered(fontname);
        }
    
        /// <summary>
        /// Checks if a certain font is registered.
        /// </summary>
        /// <param name="fontname">the name of the font that has to be checked</param>
        /// <returns>true if the font is found</returns>
        public static bool IsRegistered(string fontname) {
            return fontImp.IsRegistered(fontname);
        }

        public static string DefaultEncoding {
            get {
                return defaultEncoding;
            }
        }

        public static bool DefaultEmbedding {
            get {
                return defaultEmbedding;
            }
        }


        public static FontFactoryImp FontImp {
            get {
                return fontImp;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException(MessageLocalization.GetComposedMessage("fontfactoryimp.cannot.be.null"));
                fontImp = value;
            }
        }
    }
}
