using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Permissions;
using iTextSharp.text.pdf;
using iTextSharp.text.log;
using iTextSharp.text;

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
    /// to this class first and then create fonts in your code using one of the getFont method
    /// without having to enter a path as parameter.
    /// </summary>
    /// 
   
    public class FontFactoryImp : IFontProvider
    {

        protected static string SystemPath = null;

        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(FontFactoryImp));
        
        /// <summary> This is a map of postscriptfontnames of True Type fonts and the path of their ttf- or ttc-file. </summary>
        private Dictionary<string,string> trueTypeFonts = new Dictionary<string,string>();

        private static String[] TTFamilyOrder = {
            "3", "1", "1033",
            "3", "0", "1033",
            "1", "0", "0",
            "0", "3", "0"
        };
        
        /// <summary> This is a map of fontfamilies. </summary>
        private Dictionary<string,List<string>> fontFamilies = new Dictionary<string,List<string>>();
    
        /// <summary> This is the default encoding to use. </summary>
        private string defaultEncoding = BaseFont.WINANSI;
    
        /// <summary> This is the default value of the <VAR>embedded</VAR> variable. </summary>
        private bool defaultEmbedding = BaseFont.NOT_EMBEDDED;
    
        /// <summary> Creates new FontFactory </summary>
        public FontFactoryImp() {
            trueTypeFonts.Add(FontFactory.COURIER.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER);
            trueTypeFonts.Add(FontFactory.COURIER_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_BOLD);
            trueTypeFonts.Add(FontFactory.COURIER_OBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_OBLIQUE);
            trueTypeFonts.Add(FontFactory.COURIER_BOLDOBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_BOLDOBLIQUE);
            trueTypeFonts.Add(FontFactory.HELVETICA.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA);
            trueTypeFonts.Add(FontFactory.HELVETICA_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_BOLD);
            trueTypeFonts.Add(FontFactory.HELVETICA_OBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_OBLIQUE);
            trueTypeFonts.Add(FontFactory.HELVETICA_BOLDOBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_BOLDOBLIQUE);
            trueTypeFonts.Add(FontFactory.SYMBOL.ToLower(CultureInfo.InvariantCulture), FontFactory.SYMBOL);
            trueTypeFonts.Add(FontFactory.TIMES_ROMAN.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_ROMAN);
            trueTypeFonts.Add(FontFactory.TIMES_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_BOLD);
            trueTypeFonts.Add(FontFactory.TIMES_ITALIC.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_ITALIC);
            trueTypeFonts.Add(FontFactory.TIMES_BOLDITALIC.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_BOLDITALIC);
            trueTypeFonts.Add(FontFactory.ZAPFDINGBATS.ToLower(CultureInfo.InvariantCulture), FontFactory.ZAPFDINGBATS);

            List<string> tmp;
            tmp = new List<string>();
            tmp.Add(FontFactory.COURIER);
            tmp.Add(FontFactory.COURIER_BOLD);
            tmp.Add(FontFactory.COURIER_OBLIQUE);
            tmp.Add(FontFactory.COURIER_BOLDOBLIQUE);
            fontFamilies[FontFactory.COURIER.ToLower(CultureInfo.InvariantCulture)] = tmp;
            tmp = new List<string>();
            tmp.Add(FontFactory.HELVETICA);
            tmp.Add(FontFactory.HELVETICA_BOLD);
            tmp.Add(FontFactory.HELVETICA_OBLIQUE);
            tmp.Add(FontFactory.HELVETICA_BOLDOBLIQUE);
            fontFamilies[FontFactory.HELVETICA.ToLower(CultureInfo.InvariantCulture)] = tmp;
            tmp = new List<string>();
            tmp.Add(FontFactory.SYMBOL);
            fontFamilies[FontFactory.SYMBOL.ToLower(CultureInfo.InvariantCulture)] = tmp;
            tmp = new List<string>();
            tmp.Add(FontFactory.TIMES_ROMAN);
            tmp.Add(FontFactory.TIMES_BOLD);
            tmp.Add(FontFactory.TIMES_ITALIC);
            tmp.Add(FontFactory.TIMES_BOLDITALIC);
            fontFamilies[FontFactory.TIMES.ToLower(CultureInfo.InvariantCulture)] = tmp;
            fontFamilies[FontFactory.TIMES_ROMAN.ToLower(CultureInfo.InvariantCulture)] = tmp;
            tmp = new List<string>();
            tmp.Add(FontFactory.ZAPFDINGBATS);
            fontFamilies[FontFactory.ZAPFDINGBATS.ToLower(CultureInfo.InvariantCulture)] = tmp;
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
        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color) {
            return GetFont(fontname, encoding, embedded, size, style, color, true);
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
        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color, bool cached) {
            if (fontname == null) return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            string lowercasefontname = fontname.ToLower(CultureInfo.InvariantCulture);
            List<string> tmp;
            fontFamilies.TryGetValue(lowercasefontname, out tmp);
            if(tmp != null) {
                lock (tmp) {
                    // some bugs were fixed here by Daniel Marczisovszky
                    int s = style == Font.UNDEFINED ? Font.NORMAL : style;
                    int fs = Font.NORMAL;
                    bool found = false;
                    foreach (string f in tmp) {
                        string lcf = f.ToLower(CultureInfo.InvariantCulture);
                        fs = Font.NORMAL;
                        if (lcf.IndexOf("bold") != -1) fs |= Font.BOLD;
                        if (lcf.IndexOf("italic") != -1 || lcf.IndexOf("oblique") != -1) fs |= Font.ITALIC;
                        if ((s & Font.BOLDITALIC) == fs) {
                            fontname = f;
                            found = true;
                            break;
                        }
                    }
                    if(style != Font.UNDEFINED && found) {
                        style &= ~fs;
                    }
                }
            }
            BaseFont basefont = null;
            try {
                basefont = GetBaseFont(fontname, encoding, embedded, cached);
                if (basefont == null) {
                    // the font is not registered as truetype font
                    return new Font(Font.FontFamily.UNDEFINED, size, style, color);
                }
            } catch (DocumentException de) {
                // this shouldn't happen
                throw de;
            } catch (System.IO.IOException) {
                // the font is registered as a true type font, but the path was wrong
                return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            } catch {
                // null was entered as fontname and/or encoding
                return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            }
            return new Font(basefont, size, style, color);
        }

        protected virtual BaseFont GetBaseFont(String fontname, String encoding, bool embedded, bool cached) {
            BaseFont basefont = null;
            try {
                // the font is a type 1 font or CJK font
                basefont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null, true);
            } catch (DocumentException de) {
            }
            if (basefont == null) {
                // the font is a true type font or an unknown font
                trueTypeFonts.TryGetValue(fontname.ToLowerInvariant(), out fontname);
                // the font is not registered as truetype font
                if (fontname != null)
                    basefont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null);
            }

            return basefont;
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
        virtual public Font GetFont(string fontname, string encoding, bool embedded, float size, int style) {
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
        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size) {
            return GetFont(fontname, encoding, embedded, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="embedded">true if the font is to be embedded in the PDF</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, string encoding, bool embedded) {
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
        public virtual Font GetFont(string fontname, string encoding, float size, int style, BaseColor color) {
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
        public virtual Font GetFont(string fontname, string encoding, float size, int style) {
            return GetFont(fontname, encoding, defaultEmbedding, size, style, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <param name="size">the size of this font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, string encoding, float size) {
            return GetFont(fontname, encoding, defaultEmbedding, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="encoding">the encoding of the font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, string encoding) {
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
        public virtual Font GetFont(string fontname, float size, int style, BaseColor color) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, color);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="color">the BaseColor of this font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, float size, BaseColor color) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, color);
        }
        
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, float size, int style) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <param name="size">the size of this font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname, float size) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, null);
        }
    
        /// <summary>
        /// Constructs a Font-object.
        /// </summary>
        /// <param name="fontname">the name of the font</param>
        /// <returns>a Font object</returns>
        public virtual Font GetFont(string fontname) {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, Font.UNDEFINED, Font.UNDEFINED, null);
        }

        /**
        * Register a font by giving explicitly the font family and name.
        * @param familyName the font family
        * @param fullName the font name
        * @param path the font path
        */
        virtual public void RegisterFamily(String familyName, String fullName, String path) {
            if (path != null)
                trueTypeFonts[fullName] = path;
            List<string> tmp;
            lock (fontFamilies) {
                fontFamilies.TryGetValue(familyName, out tmp);
                if (tmp == null) {
                    tmp = new List<string>();
                    tmp.Add(fullName);
                    fontFamilies[familyName] = tmp;
                }
            }
            lock (tmp) {
                if (!tmp.Contains(fullName)) {
                    int fullNameLength = fullName.Length;
                    bool inserted = false;
                    for (int j = 0; j < tmp.Count; ++j) {
                        if (tmp[j].Length >= fullNameLength) {
                            tmp.Insert(j, fullName);
                            inserted = true;
                            break;
                        }
                    }
                    if (!inserted) {
                        tmp.Add(fullName);
                        String newFullName = fullName.ToLower();
                        if (newFullName.EndsWith("regular")) {
                            //remove "regular" at the end of the font name
                            newFullName = newFullName.Substring(0, newFullName.Length - 7).Trim();
                            //insert this font name at the first position for higher priority
                            tmp.Insert(0, fullName.Substring(0, newFullName.Length));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register a ttf- or a ttc-file.
        /// </summary>
        /// <param name="path">the path to a ttf- or ttc-file</param>
        public virtual void Register(string path) {
            Register(path, null);
        }
    
        /// <summary>
        /// Register a ttf- or a ttc-file and use an alias for the font contained in the ttf-file.
        /// </summary>
        /// <param name="path">the path to a ttf- or ttc-file</param>
        /// <param name="alias">the alias you want to use for the font</param>
        public virtual void Register(string path, string alias) {
            try {
                if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || path.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") > 0) {
                    Object[] allNames = BaseFont.GetAllFontNames(path, BaseFont.WINANSI, null);
                    trueTypeFonts[((string)allNames[0]).ToLower(CultureInfo.InvariantCulture)] = path;
                    if (alias != null) {
                        string lcAlias = alias.ToLower(CultureInfo.InvariantCulture);
                        trueTypeFonts[lcAlias] = path;
                        if (lcAlias.EndsWith("regular")) {
                            //do this job to give higher priority to regular fonts in comparison with light, narrow, etc
                            SaveCopyOfRegularFont(lcAlias, path);
                        }
                    }
                    // register all the font names with all the locales
                    string[][] names = (string[][])allNames[2]; //full name
                    for (int i = 0; i < names.Length; i++) {
                        string lcName = names[i][3].ToLower(CultureInfo.InvariantCulture);
                        trueTypeFonts[lcName] = path;
                        if (lcName.EndsWith("regular")) {
                            //do this job to give higher priority to regular fonts in comparison with light, narrow, etc
                            SaveCopyOfRegularFont(lcName, path);
                        };
                    }
                    string fullName = null;
                    string familyName = null;
                    names = (string[][])allNames[1]; //family name
                    for (int k = 0; k < TTFamilyOrder.Length; k += 3) {
                        foreach (string[] name in names) {
                            if (TTFamilyOrder[k].Equals(name[0]) && TTFamilyOrder[k + 1].Equals(name[1]) && TTFamilyOrder[k + 2].Equals(name[2])) {
                                familyName = name[3].ToLower(CultureInfo.InvariantCulture);
                                k = TTFamilyOrder.Length;
                                break;
                            }
                        }
                    }
                    if (familyName != null) {
                        String lastName = "";
                        names = (string[][])allNames[2]; //full name
                        foreach (string[] name in names) {
                            for (int k = 0; k < TTFamilyOrder.Length; k += 3) {
                                if (TTFamilyOrder[k].Equals(name[0]) && TTFamilyOrder[k + 1].Equals(name[1]) && TTFamilyOrder[k + 2].Equals(name[2])) {
                                    fullName = name[3];
                                    if (fullName.Equals(lastName))
                                        continue;
                                    lastName = fullName;
                                    RegisterFamily(familyName, fullName, null);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttc")) {
                    if (alias != null)
                        LOGGER.Error("You can't define an alias for a true type collection.");
                    string[] names = BaseFont.EnumerateTTCNames(path);
                    for (int i = 0; i < names.Length; i++) {
                        Register(path + "," + i);
                    }
                }
                else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".afm") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".pfm")) {
                    BaseFont bf = BaseFont.CreateFont(path, BaseFont.CP1252, false);
                    String fullName = (bf.FullFontName[0][3]).ToLower(CultureInfo.InvariantCulture);
                    String familyName = (bf.FamilyFontName[0][3]).ToLower(CultureInfo.InvariantCulture);
                    String psName = bf.PostscriptFontName.ToLower(CultureInfo.InvariantCulture);
                    RegisterFamily(familyName, fullName, null);
                    trueTypeFonts[psName] = path;
                    trueTypeFonts[fullName] = path;
                }
                if (LOGGER.IsLogging(Level.TRACE)) {
                    LOGGER.Trace(String.Format("Registered {0}", path));
                }
            }
            catch (DocumentException de) {
                // this shouldn't happen
                throw de;
            }
            catch (System.IO.IOException ioe) {
                throw ioe;
            }
        }

        // remove regular and correct last symbol
        // do this job to give higher priority to regular fonts in comparison with light, narrow, etc
        // Don't use this method for not regular fonts!
        protected bool SaveCopyOfRegularFont(string regularFontName, string path) {
            //remove "regular" at the end of the font name
            String alias = regularFontName.Substring(0, regularFontName.Length - 7).Trim();
            if (!trueTypeFonts.ContainsKey(alias)) {
                trueTypeFonts[alias] = path;
                return true;
            }
            return false;
        }
    
        /** Register all the fonts in a directory.
        * @param dir the directory
        * @return the number of fonts registered
        */    
        public virtual int RegisterDirectory(String dir) {
            return RegisterDirectory(dir, false);
        }

        /**
        * Register all the fonts in a directory and possibly its subdirectories.
        * @param dir the directory
        * @param scanSubdirectories recursively scan subdirectories if <code>true</true>
        * @return the number of fonts registered
        * @since 2.1.2
        */
        virtual public int RegisterDirectory(String dir, bool scanSubdirectories) {
            if (LOGGER.IsLogging(Level.DEBUG)) {
                LOGGER.Debug(String.Format("Registering directory {0}, looking for fonts", dir));
            }
            int count = 0;
            try {
                if (!Directory.Exists(dir))
                    return 0;
                string[] files = Directory.GetFiles(dir);
                if (files == null)
                    return 0;
                for (int k = 0; k < files.Length; ++k) {
                    try {
                        if (Directory.Exists(files[k])) {
                            if (scanSubdirectories) {
                                count += RegisterDirectory(Path.GetFullPath(files[k]), true);
                            }
                        } else {
                            String name = Path.GetFullPath(files[k]);
                            String suffix = name.Length < 4 ? null : name.Substring(name.Length - 4).ToLower(CultureInfo.InvariantCulture);
                            if (".afm".Equals(suffix) || ".pfm".Equals(suffix)) {
                                /* Only register Type 1 fonts with matching .pfb files */
                                string pfb = name.Substring(0, name.Length - 4) + ".pfb";
                                if (File.Exists(pfb)) {
                                    Register(name, null);
                                    ++count;
                                }
                            } else if (".ttf".Equals(suffix) || ".otf".Equals(suffix) || ".ttc".Equals(suffix)) {
                                Register(name, null);
                                ++count;
                            }
                        }
                    }
                    catch  {
                        //empty on purpose
                    }
                }
            }
            catch  {
                //empty on purpose
            }
            return count;
        }

        /** Register fonts in windows
        * @return the number of fonts registered
        */    
        public virtual int RegisterDirectories() {
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // Use paths specific to Linux and MacOSX
                int count = 0;

                // Linux
                count += RegisterDirectory("/usr/share/X11/fonts", true);
                count += RegisterDirectory("/usr/X/lib/X11/fonts", true);
                count += RegisterDirectory("/usr/openwin/lib/X11/fonts", true);
                count += RegisterDirectory("/usr/share/fonts", true);
                count += RegisterDirectory("/usr/X11R6/lib/X11/fonts", true);

                // MacOSX
                count += RegisterDirectory("/Library/Fonts");
                count += RegisterDirectory("/System/Library/Fonts");
                return count;
            }

            try {
                
                if (SystemPath == null) {
                    SystemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                }
                string dir = Path.Combine(SystemPath,"Fonts");

                return RegisterDirectory(dir);
            }
            catch (Exception xc) {}
            return 0;
            
        }

        /// <summary>
        /// Gets a set of registered fontnames.
        /// </summary>
        /// <value>a set of registered fontnames</value>
        public virtual ICollection<string> RegisteredFonts {
            get {
                return trueTypeFonts.Keys;
            }
        }
    
        /// <summary>
        /// Gets a set of registered font families.
        /// </summary>
        /// <value>a set of registered font families</value>
        public virtual ICollection<string> RegisteredFamilies {
            get {
                return fontFamilies.Keys;
            }
        }
    
        /// <summary>
        /// Checks if a certain font is registered.
        /// </summary>
        /// <param name="fontname">the name of the font that has to be checked</param>
        /// <returns>true if the font is found</returns>
        public virtual bool IsRegistered(string fontname) {
            return trueTypeFonts.ContainsKey(fontname.ToLower(CultureInfo.InvariantCulture));
        }

        public virtual string DefaultEncoding {
            get {
                return defaultEncoding;
            }
            set {
                defaultEncoding = value;
            }
        }

        public virtual bool DefaultEmbedding {
            get {
                return defaultEmbedding;
            }
            set {
                defaultEmbedding = value;
            }
        }
    }
}
