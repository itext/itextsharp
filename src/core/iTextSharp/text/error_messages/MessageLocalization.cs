using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iTextSharp.text.io;

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

namespace iTextSharp.text.error_messages {

    /**
    * Localizes error messages. The messages are located in the package
    * com.lowagie.text.error_messages in the form language_country.lng.
    * The internal file encoding is UTF-8 without any escape chars, it's not a
    * normal property file. See en.lng for more information on the internal format.
    * @author Paulo Soares (psoares@glintt.com)
    */
    public class MessageLocalization {
        private static Dictionary<string,string> defaultLanguage = new Dictionary<string,string>();
        private static Dictionary<string,string> currentLanguage;
        private const String BASE_PATH = "iTextSharp.text.error_messages.";

        private MessageLocalization() {
        }

        static MessageLocalization() {
            try {
                defaultLanguage = GetLanguageMessages("en", null);
            } catch {
                // do nothing
            }
            if (defaultLanguage == null)
                defaultLanguage = new Dictionary<string,string>();
        }

        /**
        * Get a message without parameters.
        * @param key the key to the message
        * @return the message
        */
        public static String GetMessage(String key) {
            return GetMessage(key, true);
        }


        public static String GetMessage(String key, bool useDefaultLanguageIfMessageNotFound) {
            Dictionary<string,string> cl = currentLanguage;
            String val;
            if (cl != null) {
                cl.TryGetValue(key, out val);
                if (val != null)
                    return val;
            }

            if (useDefaultLanguageIfMessageNotFound) {
                cl = defaultLanguage;
                cl.TryGetValue(key, out val);
                if (val != null)
                    return val;
            }

            return "No message found for " + key;
        }

        /**
        * Get a message with parameters. The parameters will replace the strings
        * "{1}", "{2}", ..., "{n}" found in the message.
        * @param key the key to the message
        * @param p the variable parameter
        * @return the message
        */
        public static String GetComposedMessage(String key, params object[] p) {
            String msg = GetMessage(key);
            for (int k = 0; k < p.Length; ++k) {
                msg = msg.Replace("{"+(k+1)+"}", p[k].ToString());
            }
            return msg;
        }

        /**
        * Sets the language to be used globally for the error messages. The language
        * is a two letter lowercase country designation like "en" or "pt". The country
        * is an optional two letter uppercase code like "US" or "PT".
        * @param language the language
        * @param country the country
        * @return true if the language was found, false otherwise
        * @throws IOException on error
        */
        public static bool SetLanguage(String language, String country) {
            Dictionary<string,string> lang = GetLanguageMessages(language, country);
            if (lang == null)
                return false;
            currentLanguage = lang;
            return true;
        }

        /**
        * Sets the error messages directly from a Reader.
        * @param r the Reader
        * @throws IOException on error
        */
        public static void SetMessages(TextReader r) {
            currentLanguage = ReadLanguageStream(r);
        }

        private static Dictionary<string,string> GetLanguageMessages(String language, String country) {
            if (language == null)
                throw new ArgumentException("The language cannot be null.");
            Stream isp = null;
            try {
                String file;
                if (country != null)
                    file = language + "_" + country + ".lng";
                else
                    file = language + ".lng";
                isp = StreamUtil.GetResourceStream(BASE_PATH + file);
                if (isp != null)
                    return ReadLanguageStream(isp);
                if (country == null)
                    return null;
                file = language + ".lng";
                isp = StreamUtil.GetResourceStream(BASE_PATH + file);
                if (isp != null)
                    return ReadLanguageStream(isp);
                else
                    return null;
            }
            finally {
                try {
                    isp.Close();
                } catch {
                }
                // do nothing
            }
        }

        private static Dictionary<string,string> ReadLanguageStream(Stream isp) {
            return ReadLanguageStream(new StreamReader(isp, Encoding.UTF8));
        }

        private static Dictionary<string,string> ReadLanguageStream(TextReader br) {
            Dictionary<string,string> lang = new Dictionary<string,string>();
            String line;
            while ((line = br.ReadLine()) != null) {
                int idxeq = line.IndexOf('=');
                if (idxeq < 0)
                    continue;
                String key = line.Substring(0, idxeq).Trim();
                if (key.StartsWith("#"))
                    continue;
                lang[key] = line.Substring(idxeq + 1);
            }
            return lang;
        }
    }
}
