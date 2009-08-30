using System;
using System.Collections;
using System.Text;
using System.IO;

using iTextSharp.text.pdf;

/*
 * $Id: $
 *
 * Copyright 2009 by Paulo Soares.
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
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
        private static Hashtable defaultLanguage = new Hashtable();
        private static Hashtable currentLanguage;
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
                defaultLanguage = new Hashtable();
        }

        /**
        * Get a message without parameters.
        * @param key the key to the message
        * @return the message
        */
        public static String GetMessage(String key) {
            Hashtable cl = currentLanguage;
            String val;
            if (cl != null) {
                val = (String)cl[key];
                if (val != null)
                    return val;
            }
            cl = defaultLanguage;
            val = (String)cl[key];
            if (val != null)
                return val;
            return "No message found for " + key;
        }

        /**
        * Get a message without parameters.
        * @param key the key to the message
        * @return the message
        */
        public static String GetComposedMessage(String key) {
            return GetComposedMessage(key, null, null);
        }

        /**
        * Get a message with one parameter. The parameter will replace the string
        * "{1}" found in the message.
        * @param key the key to the message
        * @param p1 the parameter
        * @return the message
        */
        public static String GetComposedMessage(String key, Object p1) {
            return GetComposedMessage(key, p1, null);
        }

        /**
        * Get a message with one parameter. The parameter will replace the string
        * "{1}", "{2}" found in the message.
        * @param key the key to the message
        * @param p1 the parameter
        * @param p2 the parameter
        * @return the message
        */
        public static String GetComposedMessage(String key, Object p1, Object p2) {
            return GetComposedMessage(key, p1, p2, null, null);
        }

        /**
        * Get a message with one parameter. The parameter will replace the string
        * "{1}", "{2}", "{3}" found in the message.
        * @param key the key to the message
        * @param p1 the parameter
        * @param p2 the parameter
        * @param p3 the parameter
        * @return the message
        */
        public static String GetComposedMessage(String key, Object p1, Object p2, Object p3) {
            return GetComposedMessage(key, p1, p2, p3, null);
        }

        /**
        * Get a message with two parameters. The parameters will replace the strings
        * "{1}", "{2}", "{3}", "{4}" found in the message.
        * @param key the key to the message
        * @param p1 the parameter
        * @param p2 the parameter
        * @param p3 the parameter
        * @param p4 the parameter
        * @return the message
        */
        public static String GetComposedMessage(String key, Object p1, Object p2, Object p3, Object p4) {
            String msg = GetMessage(key);
            if (p1 != null) {
                msg = msg.Replace("{1}", p1.ToString());
            }
            if (p2 != null) {
                msg = msg.Replace("{2}", p2.ToString());
            }
            if (p3 != null) {
                msg = msg.Replace("{3}", p3.ToString());
            }
            if (p4 != null) {
                msg = msg.Replace("{4}", p4.ToString());
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
            Hashtable lang = GetLanguageMessages(language, country);
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

        private static Hashtable GetLanguageMessages(String language, String country) {
            if (language == null)
                throw new ArgumentException("The language cannot be null.");
            Stream isp = null;
            try {
                String file;
                if (country != null)
                    file = language + "_" + country + ".lng";
                else
                    file = language + ".lng";
                isp = BaseFont.GetResourceStream(BASE_PATH + file);
                if (isp != null)
                    return ReadLanguageStream(isp);
                if (country == null)
                    return null;
                file = language + ".lng";
                isp = BaseFont.GetResourceStream(BASE_PATH + file);
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

        private static Hashtable ReadLanguageStream(Stream isp) {
            return ReadLanguageStream(new StreamReader(isp, Encoding.UTF8));
        }

        private static Hashtable ReadLanguageStream(TextReader br) {
            Hashtable lang = new Hashtable();
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