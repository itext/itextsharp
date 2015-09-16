using System;
using System.Text;
using System.Xml;
using iTextSharp.text.pdf.security;
/*
 * $Id: XfaXpathConstructor.java 5830 2013-05-31 09:29:15Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Pavel Alay, Bruno Lowagie, et al.
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
    /**
     * Constructor for xpath expression for signing XfaForm
     */
    public class XfaXpathConstructor : IXpathConstructor
    {
        /**
         * Possible xdp packages to sign
         */
        public enum XdpPackage {
            Config,
            ConnectionSet,
            Datasets,
            LocaleSet,
            Pdf,
            SourceSet,
            Stylesheet,
            Template,
            Xdc,
            Xfdf,
            Xmpmeta
        }

        private const String CONFIG = "config";
        private const String CONNECTIONSET = "connectionSet";
        private const String DATASETS = "datasets";
        private const String LOCALESET = "localeSet";
        private const String PDF = "pdf";
        private const String SOURCESET = "sourceSet";
        private const String STYLESHEET = "stylesheet";
        private const String TEMPLATE = "template";
        private const String XDC = "xdc";
        private const String XFDF = "xfdf";
        private const String XMPMETA = "xmpmeta";

        /**
         * Empty constructor, no transform.
         */
        public XfaXpathConstructor() {
            this.xpathExpression = "";
        }

        /**
         * Construct for Xpath expression. Depends from selected xdp package.
         * @param xdpPackage
         */
        public XfaXpathConstructor(XdpPackage xdpPackage) {
            String strPackage;
            switch (xdpPackage) {
                case XdpPackage.Config:
                    strPackage = CONFIG;
                    break;
                case XdpPackage.ConnectionSet:
                    strPackage = CONNECTIONSET;
                    break;
                case XdpPackage.Datasets:
                    strPackage = DATASETS;
                    break;
                case XdpPackage.LocaleSet:
                    strPackage = LOCALESET;
                    break;
                case XdpPackage.Pdf:
                    strPackage = PDF;
                    break;
                case XdpPackage.SourceSet:
                    strPackage = SOURCESET;
                    break;
                case XdpPackage.Stylesheet:
                    strPackage = STYLESHEET;
                    break;
                case XdpPackage.Template:
                    strPackage = TEMPLATE;
                    break;
                case XdpPackage.Xdc:
                    strPackage = XDC;
                    break;
                case XdpPackage.Xfdf:
                    strPackage = XFDF;
                    break;
                case XdpPackage.Xmpmeta:
                    strPackage = XMPMETA;
                    break;
                default:
                    xpathExpression = "";
                    return;
            }

            StringBuilder builder = new StringBuilder("/xdp:xdp/*[local-name()='");
            builder.Append(strPackage).Append("']");
            xpathExpression = builder.ToString();
            namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("xdp", "http://ns.adobe.com/xdp/");
        }

        private String xpathExpression;
        private XmlNamespaceManager namespaceManager;

        /**
         * Get XPath expression
         */
        virtual public String GetXpathExpression() {
            return xpathExpression;
        }

        virtual public XmlNamespaceManager GetNamespaceManager() {
            return namespaceManager;
        }
    }
}
