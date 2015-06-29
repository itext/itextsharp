using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.error_messages;

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

namespace iTextSharp.text.pdf
{
    /// <summary>
    /// Summary description for ICC_Profile.
    /// </summary>
    public class ICC_Profile
    {
        protected byte[] data;
        protected int numComponents;
        private static Dictionary<string,int> cstags = new Dictionary<string,int>();
        
        protected ICC_Profile() {
        }

        public static ICC_Profile GetInstance(byte[] data, int numComponents) {
            if (data.Length < 128 || data[36] != 0x61 || data[37] != 0x63 
                || data[38] != 0x73 || data[39] != 0x70)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
            ICC_Profile icc = new ICC_Profile();
            icc.data = data;

            if (!cstags.TryGetValue(Encoding.ASCII.GetString(data, 16, 4), out icc.numComponents)) {
                icc.numComponents = 0;
            }
            // invalid ICC
            if (icc.numComponents != numComponents) {
                throw new ArgumentException("ICC profile contains " + icc.numComponents + " component(s), the image data contains " + numComponents + " component(s)");
            }
            return icc;
        }

        public static ICC_Profile GetInstance(byte[] data) {
            int numComponents;
            if (!cstags.TryGetValue(Encoding.ASCII.GetString(data, 16, 4), out numComponents)) {
                numComponents = 0;
            }
            return GetInstance(data, numComponents);
        }
        
        public static ICC_Profile GetInstance(Stream file) {
            byte[] head = new byte[128];
            int remain = head.Length;
            int ptr = 0;
            while (remain > 0) {
                int n = file.Read(head, ptr, remain);
                if (n <= 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
                remain -= n;
                ptr += n;
            }
            if (head[36] != 0x61 || head[37] != 0x63 
                || head[38] != 0x73 || head[39] != 0x70)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
            remain = ((head[0] & 0xff) << 24) | ((head[1] & 0xff) << 16)
                      | ((head[2] & 0xff) <<  8) | (head[3] & 0xff);
            byte[] icc = new byte[remain];
            System.Array.Copy(head, 0, icc, 0, head.Length);
            remain -= head.Length;
            ptr = head.Length;
            while (remain > 0) {
                int n = file.Read(icc, ptr, remain);
                if (n <= 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
                remain -= n;
                ptr += n;
            }
            return GetInstance(icc);
        }

        public static ICC_Profile GetInstance(String fname) {
            FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = GetInstance(fs);
            fs.Close();
            return icc;
        }

        virtual public byte[] Data {
            get {
                return data;
            }
        }
        
        virtual public int NumComponents {
            get {
                return numComponents;
            }
        }

        static ICC_Profile() {
            cstags["XYZ "] = 3;
            cstags["Lab "] = 3;
            cstags["Luv "] = 3;
            cstags["YCbr"] = 3;
            cstags["Yxy "] = 3;
            cstags["RGB "] = 3;
            cstags["GRAY"] = 1;
            cstags["HSV "] = 3;
            cstags["HLS "] = 3;
            cstags["CMYK"] = 4;
            cstags["CMY "] = 3;
            cstags["2CLR"] = 2;
            cstags["3CLR"] = 3;
            cstags["4CLR"] = 4;
            cstags["5CLR"] = 5;
            cstags["6CLR"] = 6;
            cstags["7CLR"] = 7;
            cstags["8CLR"] = 8;
            cstags["9CLR"] = 9;
            cstags["ACLR"] = 10;
            cstags["BCLR"] = 11;
            cstags["CCLR"] = 12;
            cstags["DCLR"] = 13;
            cstags["ECLR"] = 14;
            cstags["FCLR"] = 15;
        }
    }
}
