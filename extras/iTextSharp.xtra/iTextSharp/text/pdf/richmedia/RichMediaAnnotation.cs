using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.exceptions;
/*
 * $Id: RichMediaAnnotation.java 3927 2009-05-13 09:43:39Z blowagie $
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.pdf.richmedia {

    /**
     * Object that is able to create Rich Media Annotations as described
     * in the document "Acrobat Supplement to the ISO 32000", referenced
     * in the code as "ExtensionLevel 3". This annotation is described in
     * section 9.6 entitled "Rich Media" of this document.
     * Extension level 3 introduces rich media PDF constructs that support
     * playing a SWF file and provide enhanced rich media. With rich media
     * annotation, Flash applications, video, audio, and other multimedia
     * can be attached to a PDF with expanded functionality. It improves upon
     * the existing 3D annotation structure to support multiple multimedia
     * file assets, including Flash video and compatible variations on the
     * H.264 format. The new constructs allow a two-way scripting bridge between
     * Flash and a conforming application. There is support for generalized
     * linking of a Flash application state to a comment or view, which enables
     * video commenting. Finally, actions can be linked to video chapter points.
     * @since   5.0.0
     */
    public class RichMediaAnnotation {
        /** The PdfWriter to which the annotation will be added. */
        protected PdfWriter writer;
        /** The annotation object */
        protected PdfAnnotation annot;
        /** the rich media content (can be reused for different annotations) */
        protected PdfDictionary richMediaContent = null;
        /** a reference to the RichMediaContent that can be reused. */
        protected PdfIndirectReference richMediaContentReference = null;
        /** the rich media settings (specific for this annotation) */
        protected PdfDictionary richMediaSettings = new PdfDictionary(PdfName.RICHMEDIASETTINGS);
        /** a map with the assets (will be used to construct a name tree.) */
        protected Dictionary<String, PdfIndirectReference> assetsmap = null;
        /** an array with configurations (will be added to the RichMediaContent). */
        protected PdfArray configurations = null;
        /** an array of views (will be added to the RichMediaContent) */
        protected PdfArray views = null;

        /**
         * Creates a RichMediaAnnotation.
         * @param   writer  the PdfWriter to which the annotation will be added.
         * @param   rect    the rectangle where the annotation will be added.
         */
        public RichMediaAnnotation(PdfWriter writer, Rectangle rect) {
            this.writer = writer;
            annot = new PdfAnnotation(writer, rect);
            annot.Put(PdfName.SUBTYPE, PdfName.RICHMEDIA);
            richMediaContent = new PdfDictionary(PdfName.RICHMEDIACONTENT);
            assetsmap = new Dictionary<string,PdfIndirectReference>();
            configurations = new PdfArray();
            views = new PdfArray();
        }

        /**
         * Creates a RichMediaAnnotation using rich media content that has already
         * been added to the writer. Note that assets, configurations, views added
         * to a RichMediaAnnotation created like this will be ignored.
         * @param   writer  the PdfWriter to which the annotation will be added.
         * @param   rect    the rectangle where the annotation will be added.
         * @param   richMediaContentReference   reused rich media content.
         */
        public RichMediaAnnotation(PdfWriter writer, Rectangle rect, PdfIndirectReference richMediaContentReference) {
            this.richMediaContentReference = richMediaContentReference;
            richMediaContent = null;
            this.writer = writer;
            annot = new PdfAnnotation(writer, rect);
            annot.Put(PdfName.SUBTYPE, PdfName.RICHMEDIA);
        }

        /**
         * Gets a reference to the RichMediaContent for reuse of the
         * rich media content. Returns null if the content hasn't been
         * added to the Stream yet.
         * @return  a PdfDictionary with RichMediaContent
         */
        public PdfIndirectReference RichMediaContentReference {
            get {
                return richMediaContentReference;
            }
        }

        /**
         * Adds an embedded file.
         * (Part of the RichMediaContent.)
         * @param   name    a name for the name tree
         * @param   fs      a file specification for an embedded file.
         */
        public PdfIndirectReference AddAsset(String name, PdfFileSpecification fs) {
            if (assetsmap == null)
                throw new IllegalPdfSyntaxException("You can't add assets to reused RichMediaContent.");
            PdfIndirectReference refi = writer.AddToBody(fs).IndirectReference;
            assetsmap[name] = refi;
            return refi;
        }

        /**
         * Adds a reference to an embedded file.
         * (Part of the RichMediaContent.)
         * @param   ref a reference to a PdfFileSpecification
         */
        public PdfIndirectReference AddAsset(String name, PdfIndirectReference refi) {
            if (views == null)
                throw new IllegalPdfSyntaxException("You can't add assets to reused RichMediaContent.");
            assetsmap[name] = refi;
            return refi;
        }

        /**
         * Adds a RichMediaConfiguration.
         * (Part of the RichMediaContent.)
         * @param   configuration   a configuration dictionary
         */
        public PdfIndirectReference AddConfiguration(RichMediaConfiguration configuration) {
            if (configurations == null)
                throw new IllegalPdfSyntaxException("You can't add configurations to reused RichMediaContent.");
            PdfIndirectReference refi = writer.AddToBody(configuration).IndirectReference;
            configurations.Add(refi);
            return refi;
        }

        /**
         * Adds a reference to a RichMediaConfiguration.
         * (Part of the RichMediaContent.)
         * @param   ref     a reference to a RichMediaConfiguration
         */
        public PdfIndirectReference AddConfiguration(PdfIndirectReference refi) {
            if (configurations == null)
                throw new IllegalPdfSyntaxException("You can't add configurations to reused RichMediaContent.");
            configurations.Add(refi);
            return refi;
        }

        /**
         * Adds a view dictionary.
         * (Part of the RichMediaContent.)
         * @param   view    a view dictionary
         */
        public PdfIndirectReference AddView(PdfDictionary view) {
            if (views == null)
                throw new IllegalPdfSyntaxException( "You can't add views to reused RichMediaContent.");
            PdfIndirectReference refi = writer.AddToBody(view).IndirectReference;
            views.Add(refi);
            return refi;
        }

        /**
         * Adds a reference to a view dictionary.
         * (Part of the RichMediaContent.)
         * @param   ref a reference to a view dictionary
         */
        public PdfIndirectReference AddView(PdfIndirectReference refi) {
            if (views == null)
                throw new IllegalPdfSyntaxException("You can't add views to reused RichMediaContent.");
            views.Add(refi);
            return refi;
        }

        /**
         * Sets the RichMediaActivation dictionary specifying the style of
         * presentation, default script behavior, default view information,
         * and animation style when the annotation is activated.
         * (Part of the RichMediaSettings.)
         * @param   richMediaActivation
         */
        public RichMediaActivation Activation {
            set {
                richMediaSettings.Put(PdfName.ACTIVATION, value);
            }
        }

        /**
         * Sets the RichMediaDeactivation dictionary specifying the condition
         * that causes deactivation of the annotation.
         * (Part of the RichMediaSettings.)
         * @param   richMediaDeactivation
         */
        public RichMediaDeactivation Deactivation {
            set {
                richMediaSettings.Put(PdfName.DEACTIVATION, value);
            }
        }

        /**
         * Creates the actual annotation and adds different elements to the
         * PdfWriter while doing so.
         * @return  a PdfAnnotation
         */
        public PdfAnnotation CreateAnnotation() {
            if (richMediaContent != null) {
                if (assetsmap.Count > 0) {
                    PdfDictionary assets = PdfNameTree.WriteTree(assetsmap, writer);
                    richMediaContent.Put(PdfName.ASSETS, writer.AddToBody(assets).IndirectReference);
                }
                if (configurations.Size > 0) {
                    richMediaContent.Put(PdfName.CONFIGURATION, writer.AddToBody(configurations).IndirectReference);
                }
                if (views.Size > 0) {
                    richMediaContent.Put(PdfName.VIEWS, writer.AddToBody(views).IndirectReference);
                }
                richMediaContentReference = writer.AddToBody(richMediaContent).IndirectReference;
            }
            writer.AddDeveloperExtension(PdfDeveloperExtension.ADOBE_1_7_EXTENSIONLEVEL3);
            annot.Put(PdfName.RICHMEDIACONTENT, richMediaContentReference);
            annot.Put(PdfName.RICHMEDIASETTINGS, writer.AddToBody(richMediaSettings).IndirectReference);
            return annot;
        }
    }
}