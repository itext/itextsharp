using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.net.exc;
/*
 * $Id: ImageRetrieve.java 122 2011-05-27 12:20:58Z redlab_b $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2014 iText Group NV
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
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
namespace iTextSharp.tool.xml.net {

    /**
     * @author redlab_b
     *
     */
    public class ImageRetrieve {
        private IImageProvider provider;
        /**
         * @param imageProvider the provider to use.
         *
         */
        public ImageRetrieve(IImageProvider imageProvider) {
            this.provider = imageProvider;
        }
        /**
         *
         */
        public ImageRetrieve() {
            this.provider = null;
        }
        /**
         * @param src an URI that can be used to retrieve an image
         * @return an iText Image object
         * @throws NoImageException if there is no image
         * @throws IOException if an IOException occurred
         */
        virtual public Image RetrieveImage(String src) {
            Image img = null;
            if (null != provider) {
                img = provider.Retrieve(src);
            }

            if (null == img) {
                String path = null;
                if (src.StartsWith("http")) {
                    // full url available
                    path = src;
                } else if (null != provider){
                    String root = this.provider.GetImageRootPath();
                    if (null != root) {
                        if (root.EndsWith("/") && src.StartsWith("/")) {
                            root = root.Substring(0, root.Length - 1);
                        }
                        path = root + src;
                    }
                } else {
                    path = src;
                }
                if (null != path) {
                    try {
                        if (path.StartsWith("http")) {
                            img = Image.GetInstance(path);
                        } else {
                            img = Image.GetInstance(Path.GetFullPath(path));
                        }
                        if (null != provider && null != img) {
                            provider.Store(src, img);
                        }
                    } catch (Exception e) {
                        throw new NoImageException(src, e);
                    }
                } else {
                    throw new NoImageException(src);
                }
            }
            return img;
        }
    }
}
