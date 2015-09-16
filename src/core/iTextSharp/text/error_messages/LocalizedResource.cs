using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.util;

/*
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
    public class LocalizedResource {
        private static readonly char[] splt = new char[]{'_'};
        private Properties msgs = new Properties();

        public LocalizedResource(string resourceRoot, CultureInfo culture, Assembly assembly) {
            Stream istr = null;
            string name = culture.Name.Replace('-', '_');
            if (name != "") {
                try {
                    istr = assembly.GetManifestResourceStream(resourceRoot + "_" + name + ".properties");
                }
                catch {}
                if (istr == null) {
                    string[] nameSplit = name.Split(splt, 2);
                    if (nameSplit.Length == 2) {
                        try {
                            istr = assembly.GetManifestResourceStream(resourceRoot + "_" + nameSplit[0] + ".properties");
                        }
                        catch {}
                    }
                }
            }
            if (istr == null) {
                try {
                    istr = assembly.GetManifestResourceStream(resourceRoot + ".properties");
                }
                catch {}
            }
            if (istr != null) {
                msgs.Load(istr);
            }
            List<string> keys = new List<string>(msgs.Keys);
            foreach (string key in keys) {
                string v = msgs[key];
                StringBuilder sb = new StringBuilder();
                int n = 0;
                int state = 0;
                foreach (char c in v) {
                    switch (state) {
                        case 0:
                            if (c == '%') {
                                state = 1;
                                break;
                            }
                            else if (c == '{' || c == '}') {
                                sb.Append(c);
                            }
                            sb.Append(c);
                            break;
                        case 1:
                            if (c == '%') {
                                sb.Append(c);
                                state = 0;
                            }
                            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                                state = 0;
                            sb.Append('{').Append(n++).Append('}');
                            break;
                    }
                }
                msgs[key] = sb.ToString();
            }
        }

        virtual public string GetMessage(string key) {
            string v = msgs[key];
            if (v == null)
                return key;
            else
                return v;
        }
    }
}
