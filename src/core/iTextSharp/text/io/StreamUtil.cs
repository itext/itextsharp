using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/*
 * $Id$
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * BVBA Authors: Kevin Day, Bruno Lowagie, et al.
 *
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Affero General License version 3 as published by the
 * Free Software Foundation with the addition of the following permission added
 * to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK
 * IN WHICH THE COPYRIGHT IS OWNED BY ITEXT GROUP, ITEXT GROUP DISCLAIMS THE WARRANTY OF NON
 * INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Affero General License for more
 * details. You should have received a copy of the GNU Affero General License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General License.
 *
 * In accordance with Section 7(b) of the GNU Affero General License, a covered
 * work must retain the producer line in every PDF that is created or
 * manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing a
 * commercial license. Buying such a license is mandatory as soon as you develop
 * commercial activities involving the iText software without disclosing the
 * source code of your own applications. These activities include: offering paid
 * services to customers as an ASP, serving PDFs on the fly in a web
 * application, shipping iText with a closed source product.
 *
 * For more information, please contact iText Software Corp. at this address:
 * sales@itextpdf.com
 */
namespace iTextSharp.text.io {

    /**
     * Utility class with commonly used stream operations
     * @since 5.3.5
     *
     */
    public static class StreamUtil {

        /**
         * Reads the full content of a stream and returns them in a byte array
         * @param is the stream to read
         * @return a byte array containing all of the bytes from the stream
         * @throws IOException if there is a problem reading from the input stream
         */
        public static byte[] InputStreamToArray(Stream inp) {
            byte[] b = new byte[8192];
            MemoryStream outp = new MemoryStream();
            while (true) {
                int read = inp.Read(b, 0, b.Length);
                if (read < 1)
                    break;
                outp.Write(b, 0, read);
            }
            outp.Close();
            return outp.ToArray();
        }
        
        public static void CopyBytes(IRandomAccessSource source, long start, long length, Stream outs) {
            if (length <= 0)
                return;
            long idx = start;
            byte[] buf = new byte[8192];
            while (length > 0) {
                long n = source.Get(idx, buf,0, (int)Math.Min((long)buf.Length, length));
                if (n <= 0)
                    throw new EndOfStreamException();
                outs.Write(buf, 0, (int)n);
                idx += n;
                length -= n;
            }
        }

        internal static List<object> resourceSearch = new List<object>();

        public static void AddToResourceSearch(object obj)
        {
            lock(resourceSearch)
            {
                if(obj is Assembly)
                {
                    resourceSearch.Add(obj);
                }
                else if(obj is string)
                {
                    string f = (string)obj;
                    if(Directory.Exists(f) || File.Exists(f))
                        resourceSearch.Add(obj);
                }
            }
        }

        /** Gets the font resources.
         * @param key the name of the resource
         * @return the <CODE>Stream</CODE> to get the resource or
         * <CODE>null</CODE> if not found
         */
        public static Stream GetResourceStream(string key)
        {
            Stream istr = null;
            // Try to use resource loader to load the properties file.
            try
            {
                Assembly assm = Assembly.GetExecutingAssembly();
                istr = assm.GetManifestResourceStream(key);
            }
            catch
            {
            }
            if(istr != null)
                return istr;
            int count;
            lock(resourceSearch) {
                count = resourceSearch.Count;
            }
            for(int k = 0; k < count; ++k) {
                object obj;
                lock(resourceSearch) {
                    obj = resourceSearch[k];
                }
                try {
                    if(obj is Assembly) {
                        istr = ((Assembly)obj).GetManifestResourceStream(key);
                        if(istr != null)
                            return istr;
                    } else if(obj is string) {
                        string dir = (string)obj;
                        try {
                            istr = Assembly.LoadFrom(dir).GetManifestResourceStream(key);
                        }
                        catch { }
                        if(istr != null)
                            return istr;
                        string modkey = key.Replace('.', '/');
                        string fullPath = Path.Combine(dir, modkey);
                        if(File.Exists(fullPath)) {
                            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        int idx = modkey.LastIndexOf('/');
                        if(idx >= 0) {
                            modkey = modkey.Substring(0, idx) + "." + modkey.Substring(idx + 1);
                            fullPath = Path.Combine(dir, modkey);
                            if(File.Exists(fullPath))
                                return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                    }
                } catch { }
            }

            return istr;
        }
    }
}
