using System;
using System.Collections.Generic;
using System.util;
using iTextSharp.text.factories;

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
    /// An Annotation is a little note that can be added to a page
    /// on a document.
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Anchor"/>
    public class Annotation : IElement {

        // membervariables

        /// <summary>This is a possible annotation type.</summary>
        public const int TEXT = 0;
        /// <summary>This is a possible annotation type.</summary>
        public const int URL_NET = 1;
        /// <summary>This is a possible annotation type.</summary>
        public const int URL_AS_STRING = 2;
        /// <summary>This is a possible annotation type.</summary>
        public const int FILE_DEST = 3;
        /// <summary>This is a possible annotation type.</summary>
        public const int FILE_PAGE = 4;
        /// <summary>This is a possible annotation type.</summary>
        public const int NAMED_DEST = 5;
        /// <summary>This is a possible annotation type.</summary>
        public const int LAUNCH = 6;
        /// <summary>This is a possible annotation type.</summary>
        public const int SCREEN = 7;

        /// <summary>This is a possible attribute.</summary>
        public const string TITLE = "title";
        /// <summary>This is a possible attribute.</summary>
        public const string CONTENT = "content";
        /// <summary>This is a possible attribute.</summary>
        public const string URL = "url";
        /// <summary>This is a possible attribute.</summary>
        public const string FILE = "file";
        /// <summary>This is a possible attribute.</summary>
        public const string DESTINATION = "destination";
        /// <summary>This is a possible attribute.</summary>
        public const string PAGE = "page";
        /// <summary>This is a possible attribute.</summary>
        public const string NAMED = "named";
        /// <summary>This is a possible attribute.</summary>
        public const string APPLICATION = "application";
        /// <summary>This is a possible attribute.</summary>
        public const string PARAMETERS = "parameters";
        /// <summary>This is a possible attribute.</summary>
        public const string OPERATION = "operation";
        /// <summary>This is a possible attribute.</summary>
        public const string DEFAULTDIR = "defaultdir";
        /// <summary>This is a possible attribute.</summary>
        public const string LLX = "llx";
        /// <summary>This is a possible attribute.</summary>
        public const string LLY = "lly";
        /// <summary>This is a possible attribute.</summary>
        public const string URX = "urx";
        /// <summary>This is a possible attribute.</summary>
        public const string URY = "ury";
        /// <summary>This is a possible attribute.</summary>
        public const string MIMETYPE = "mime";

        /// <summary>This is the type of annotation.</summary>
        protected int annotationtype;

        /// <summary>This is the title of the Annotation.</summary>
        protected Dictionary<string, object> annotationAttributes = new Dictionary<string, object>();

        /// <summary>This is the lower left x-value</summary>
        private float llx = float.NaN;
        /// <summary>This is the lower left y-value</summary>
        private float lly = float.NaN;
        /// <summary>This is the upper right x-value</summary>
        private float urx = float.NaN;
        /// <summary>This is the upper right y-value</summary>
        private float ury = float.NaN;

        // constructors

        /// <summary>
        /// Constructs an Annotation with a certain title and some text.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        private Annotation(float llx, float lly, float urx, float ury) {
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        public Annotation(Annotation an) {
            annotationtype = an.annotationtype;
            annotationAttributes = an.annotationAttributes;
            llx = an.llx;
            lly = an.lly;
            urx = an.urx;
            ury = an.ury;
        }

        /// <summary>
        /// Constructs an Annotation with a certain title and some text.
        /// </summary>
        /// <param name="title">the title of the annotation</param>
        /// <param name="text">the content of the annotation</param>
        public Annotation(string title, string text) {
            annotationtype = TEXT;
            annotationAttributes[TITLE] = title;
            annotationAttributes[CONTENT] = text;
        }

        /// <summary>
        /// Constructs an Annotation with a certain title and some text.
        /// </summary>
        /// <param name="title">the title of the annotation</param>
        /// <param name="text">the content of the annotation</param>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        public Annotation(string title, string text, float llx, float lly, float urx, float ury)
            : this(llx, lly, urx, ury) {
            annotationtype = TEXT;
            annotationAttributes[TITLE] = title;
            annotationAttributes[CONTENT] = text;
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="url">the external reference</param>
        public Annotation(float llx, float lly, float urx, float ury, Uri url)
            : this(llx, lly, urx, ury) {
            annotationtype = URL_NET;
            annotationAttributes[URL] = url;
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="url">the external reference</param>
        public Annotation(float llx, float lly, float urx, float ury, string url)
            : this(llx, lly, urx, ury) {
            annotationtype = URL_AS_STRING;
            annotationAttributes[FILE] = url;
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="file">an external PDF file</param>
        /// <param name="dest">the destination in this file</param>
        public Annotation(float llx, float lly, float urx, float ury, string file, string dest)
            : this(llx, lly, urx, ury) {
            annotationtype = FILE_DEST;
            annotationAttributes[FILE] = file;
            annotationAttributes[DESTINATION] = dest;
        }

        /// <summary>
        /// Creates a Screen anotation to embed media clips
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="moviePath">path to the media clip file</param>
        /// <param name="mimeType">mime type of the media</param>
        /// <param name="showOnDisplay">if true play on display of the page</param>
        public Annotation(float llx, float lly, float urx, float ury,
            string moviePath, string mimeType, bool showOnDisplay)
            : this(llx, lly, urx, ury) {
            annotationtype = SCREEN;
            annotationAttributes[FILE] = moviePath;
            annotationAttributes[MIMETYPE] = mimeType;
            annotationAttributes[PARAMETERS] = new bool[] { false /* embedded */, showOnDisplay };
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="file">an external PDF file</param>
        /// <param name="page">a page number in this file</param>
        public Annotation(float llx, float lly, float urx, float ury, string file, int page)
            : this(llx, lly, urx, ury) {
            annotationtype = FILE_PAGE;
            annotationAttributes[FILE] = file;
            annotationAttributes[PAGE] = page;
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="named">a named destination in this file</param>
        /// <overloads>
        /// Has nine overloads.
        /// </overloads>
        public Annotation(float llx, float lly, float urx, float ury, int named)
            : this(llx, lly, urx, ury) {
            annotationtype = NAMED_DEST;
            annotationAttributes[NAMED] = named;
        }

        /// <summary>
        /// Constructs an Annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        /// <param name="application">an external application</param>
        /// <param name="parameters">parameters to pass to this application</param>
        /// <param name="operation">the operation to pass to this application</param>
        /// <param name="defaultdir">the default directory to run this application in</param>
        public Annotation(float llx, float lly, float urx, float ury, string application, string parameters, string operation, string defaultdir)
            : this(llx, lly, urx, ury) {
            annotationtype = LAUNCH;
            annotationAttributes[APPLICATION] = application;
            annotationAttributes[PARAMETERS] = parameters;
            annotationAttributes[OPERATION] = operation;
            annotationAttributes[DEFAULTDIR] = defaultdir;
        }

        // implementation of the Element-methods

        /// <summary>
        /// Gets the type of the text element
        /// </summary>
        virtual public int Type {
            get {
                return Element.ANNOTATION;
            }
        }

        // methods

        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was process successfully</returns>
        virtual public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException) {
                return false;
            }
        }

        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        virtual public IList<Chunk> Chunks {
            get {
                return new List<Chunk>();
            }
        }

        // methods

        /// <summary>
        /// Sets the dimensions of this annotation.
        /// </summary>
        /// <param name="llx">the lower left x-value</param>
        /// <param name="lly">the lower left y-value</param>
        /// <param name="urx">the upper right x-value</param>
        /// <param name="ury">the upper right y-value</param>
        virtual public void SetDimensions(float llx, float lly, float urx, float ury) {
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        // methods to retrieve information

        /// <summary>
        /// Returns the lower left x-value.
        /// </summary>
        /// <returns>a value</returns>
        virtual public float GetLlx() {
            return llx;
        }

        /// <summary>
        /// Returns the lower left y-value.
        /// </summary>
        /// <returns>a value</returns>
        virtual public float GetLly() {
            return lly;
        }

        /// <summary>
        /// Returns the uppper right x-value.
        /// </summary>
        /// <returns>a value</returns>
        virtual public float GetUrx() {
            return urx;
        }

        /// <summary>
        /// Returns the uppper right y-value.
        /// </summary>
        /// <returns>a value</returns>
        virtual public float GetUry() {
            return ury;
        }

        /// <summary>
        /// Returns the lower left x-value.
        /// </summary>
        /// <param name="def">the default value</param>
        /// <returns>a value</returns>
        virtual public float GetLlx(float def) {
            if (float.IsNaN(llx))
                return def;
            return llx;
        }

        /// <summary>
        /// Returns the lower left y-value.
        /// </summary>
        /// <param name="def">the default value</param>
        /// <returns>a value</returns>
        virtual public float GetLly(float def) {
            if (float.IsNaN(lly))
                return def;
            return lly;
        }

        /// <summary>
        /// Returns the upper right x-value.
        /// </summary>
        /// <param name="def">the default value</param>
        /// <returns>a value</returns>
        virtual public float GetUrx(float def) {
            if (float.IsNaN(urx))
                return def;
            return urx;
        }

        /// <summary>
        /// Returns the upper right y-value.
        /// </summary>
        /// <param name="def">the default value</param>
        /// <returns>a value</returns>
        virtual public float GetUry(float def) {
            if (float.IsNaN(ury))
                return def;
            return ury;
        }

        /// <summary>
        /// Returns the type of this Annotation.
        /// </summary>
        /// <value>a type</value>
        virtual public int AnnotationType {
            get {
                return annotationtype;
            }
        }

        /// <summary>
        /// Returns the title of this Annotation.
        /// </summary>
        /// <value>a name</value>
        virtual public string Title {
            get {
                if (annotationAttributes.ContainsKey(TITLE))
                    return (string)annotationAttributes[TITLE];
                else
                    return "";
            }
        }

        /// <summary>
        /// Gets the content of this Annotation.
        /// </summary>
        /// <value>a reference</value>
        virtual public string Content {
            get {
                if (annotationAttributes.ContainsKey(CONTENT))
                    return (string)annotationAttributes[CONTENT];
                else
                    return "";
            }
        }

        /// <summary>
        /// Gets the content of this Annotation.
        /// </summary>
        /// <value>a reference</value>
        virtual public Dictionary<string, object> Attributes {
            get {
                return annotationAttributes;
            }
        }

        /**
        * @see com.lowagie.text.Element#isContent()
        * @since   iText 2.0.8
        */
        virtual public bool IsContent() {
            return true;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        virtual public bool IsNestable() {
            return true;
        }

        public override string ToString() {
            return base.ToString();
        }
    }
}
