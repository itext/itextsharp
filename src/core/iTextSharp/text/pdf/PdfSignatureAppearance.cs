using System;
using System.IO;
using Org.BouncyCastle.X509;
using iTextSharp.text.error_messages;
using System.Text;
using System.Collections.Generic;
using iTextSharp.text.pdf.security;
using iTextSharp.text.io;
/*
 * $Id$
 *
 * This file is part of the iText (R) project.
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
namespace iTextSharp.text.pdf {

    /**
     * Class that takes care of the cryptographic options
     * and appearances that form a signature.
     */
    public class PdfSignatureAppearance {

        /**
         * Constructs a PdfSignatureAppearance object.
         * @param writer    the writer to which the signature will be written.
         */
        public PdfSignatureAppearance(PdfStamperImp writer) {
            this.writer = writer;
            signDate = DateTime.Now;
            fieldName = GetNewSigName();
            signatureCreator = Version.GetInstance().GetVersion;
        }
        
        /*
         * SIGNATURE
         */

        // signature types
        
        /** Approval signature */
        public const int NOT_CERTIFIED = 0;
        
        /** Author signature, no changes allowed */
        public const int CERTIFIED_NO_CHANGES_ALLOWED = 1;
        
        /** Author signature, form filling allowed */
        public const int CERTIFIED_FORM_FILLING = 2;
        
        /** Author signature, form filling and annotations allowed */
        public const int CERTIFIED_FORM_FILLING_AND_ANNOTATIONS = 3;

        /** The certification level */
        private int certificationLevel = NOT_CERTIFIED;

        /**
         * Sets the document type to certified instead of simply signed.
         * @param certificationLevel the values can be: <code>NOT_CERTIFIED</code>, <code>CERTIFIED_NO_CHANGES_ALLOWED</code>,
         * <code>CERTIFIED_FORM_FILLING</code> and <code>CERTIFIED_FORM_FILLING_AND_ANNOTATIONS</code>
         */
        virtual public int CertificationLevel {
            get {
                return certificationLevel;
            }
            set {
                certificationLevel = value;
            }
        }
        
        // signature info

        /** The caption for the reason for signing. */
        private String reasonCaption = "Reason: ";

        /** The caption for the location of signing. */
        private String locationCaption = "Location: ";


        /** The reason for signing. */
        private String reason;

        /** Holds value of property location. */
        private String location;

        /** Holds value of property signDate. */
        private DateTime signDate;
        
        /**
         * Gets and setsthe signing reason.
         * @return the signing reason
         */
        virtual public string Reason {
            get {
                return reason;
            }
            set {
                reason = value;
            }
        }

        /**
         * Sets the caption for signing reason.
         * @param reasonCaption the signing reason caption
         */

        virtual public string ReasonCaption
        {
            set { reasonCaption = value; }
        }

        /**
         * Gets and sets the signing location.
         * @return the signing location
         */
        virtual public string Location {
            get {
                return location;
            }
            set {
                location = value;
            }
        }

        /**
         * Sets the caption for the signing location.
         * @param locationCaption the signing location caption
         */

        virtual public string LocationCaption {
            set { locationCaption = value; }
        }

        /** Holds value of the application that creates the signature */
        private String signatureCreator;

        /**
         * Gets the signature creator.
         * @return the signature creator
         *
         * Sets the name of the application used to create the signature.
         * @param signatureCreator the name of the signature creating application
         */
        virtual public string SignatureCreator {
            get { return signatureCreator; }
            set { this.signatureCreator = value; }
        }

        /** The contact name of the signer. */
        private String contact;
        
        /**
         * Gets the signing contact.
         * @return the signing contact
         */
        virtual public string Contact {
            get {
                return contact;
            }
            set {
                contact = value;
            }
        }

        /**
         * Gets the signature date.
         * @return the signature date
         */
        virtual public DateTime SignDate {
            get {
                return signDate;
            }
            set {
                signDate = value;
            }
        }

        // the PDF file
        
        /** The file right before the signature is added (can be null). */
        private FileStream raf;
        /** The bytes of the file right before the signature is added (if raf is null) */
        private byte[] bout;
        /** Array containing the byte positions of the bytes that need to be hashed. */
        private long[] range;
        
        /**
         * Gets the document bytes that are hashable when using external signatures. The general sequence is:
         * preClose(), getRangeStream() and close().
         * <p>
         * @return the document bytes that are hashable
         */
        virtual public Stream GetRangeStream() {
            RandomAccessSourceFactory fac = new RandomAccessSourceFactory();
            return new RASInputStream(fac.CreateRanged(GetUnderlyingSource(), range));
        }

        /**
         * @return the underlying source
         * @throws IOException
         */
        private IRandomAccessSource GetUnderlyingSource() {
            //TODO: get rid of separate byte[] and RandomAccessFile objects and just store a RandomAccessSource
            RandomAccessSourceFactory fac = new RandomAccessSourceFactory();
            return raf == null ? fac.CreateSource(bout) : fac.CreateSource(raf);
        }
        
        /** The signing certificate */
        private X509Certificate signCertificate;

        // Developer extenstion
    
        /**
         * Adds the appropriate developer extension.
         */
	    virtual public void AddDeveloperExtension(PdfDeveloperExtension de) {
		    writer.AddDeveloperExtension(de);
	    }
        

        // Crypto dictionary
        
        /** The crypto dictionary */
        private PdfDictionary cryptoDictionary;
        /**
         * Gets the user made signature dictionary. This is the dictionary at the /V key.
         * @return the user made signature dictionary
         */
        virtual public PdfDictionary CryptoDictionary {
            get {
                return cryptoDictionary;
            }
            set {
                cryptoDictionary = value;
            }
        }
        
        /**
         * Sets the certificate used to provide the text in the appearance.
         * This certificate doesn't take part in the actual signing process.
         * @param signCertificate the certificate 
         */
        virtual public X509Certificate Certificate {
            get {
                return signCertificate;
            }
            set {
                signCertificate = value;
            }
        }

        // Signature event
        
        /**
        * An interface to retrieve the signature dictionary for modification.
        */    
        public interface ISignatureEvent {
            /**
            * Allows modification of the signature dictionary.
            * @param sig the signature dictionary
            */        
            void GetSignatureDictionary(PdfDictionary sig);
        }
        
        /**
         * Holds value of property signatureEvent.
         */
        private ISignatureEvent signatureEvent;

        /**
        * Sets the signature event to allow modification of the signature dictionary.
        * @param signatureEvent the signature event
        */
        virtual public ISignatureEvent SignatureEvent {
            get {
                return signatureEvent;
            }
            set {
                signatureEvent = value;
            }
        }
        
        /*
         * SIGNATURE FIELD
         */
        
        /** The name of the field */
        private String fieldName;

        /**
         * Gets the field name.
         * @return the field name
         */
        virtual public String FieldName {
            get {
                return fieldName;
            }
        }

        /**
         * Gets a new signature field name that
         * doesn't clash with any existing name.
         * @return a new signature field name
         */
        virtual public String GetNewSigName() {
            AcroFields af = writer.GetAcroFields();
            String name = "Signature";
            int step = 0;
            bool found = false;
            while (!found) {
                ++step;
                String n1 = name + step;
                if (af.GetFieldItem(n1) != null)
                    continue;
                n1 += ".";
                found = true;
                foreach (String fn in af.Fields.Keys) {
                    if (fn.StartsWith(n1)) {
                        found = false;
                        break;
                    }
                }
            }
            name += step;
            return name;
        }
        
        /**
         * The page where the signature will appear.
         */
        private int page = 1;

        /**
         * Gets the page number of the field.
         * @return the page number of the field
         */
        virtual public int Page {
            get {
                return page;
            }
        }
        
        /**
         * The coordinates of the rectangle for a visible signature,
         * or a zero-width, zero-height rectangle for an invisible signature.
         */
        private Rectangle rect;
        
        /**
         * Gets the rectangle representing the signature dimensions.
         * @return the rectangle representing the signature dimensions. It may be <CODE>null</CODE>
         * or have zero width or height for invisible signatures
         */
        virtual public Rectangle Rect {
            get {
                return rect;
            }
        }
        
        /** rectangle that represent the position and dimension of the signature in the page. */
        private Rectangle pageRect;
        
        /**
         * Gets the rectangle that represent the position and dimension of the signature in the page.
         * @return the rectangle that represent the position and dimension of the signature in the page
         */
        virtual public Rectangle PageRect {
            get {
                return pageRect;
            }
        }

        /**
         * Gets the visibility status of the signature.
         * @return the visibility status of the signature
         */
        virtual public bool IsInvisible() {
            return (rect == null || rect.Width == 0 || rect.Height == 0);
        }

        /**
         * Sets the signature to be visible. It creates a new visible signature field.
         * @param pageRect the position and dimension of the field in the page
         * @param page the page to place the field. The fist page is 1
         * @param fieldName the field name or <CODE>null</CODE> to generate automatically a new field name
         */
        virtual public void SetVisibleSignature(Rectangle pageRect, int page, String fieldName) {
            if (fieldName != null) {
                if (fieldName.IndexOf('.') >= 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("field.names.cannot.contain.a.dot"));
                AcroFields af = writer.GetAcroFields();
                AcroFields.Item item = af.GetFieldItem(fieldName);
                if (item != null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.already.exists", fieldName));
                this.fieldName = fieldName;
            }
            if (page < 1 || page > writer.reader.NumberOfPages)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.page.number.1", page));
            this.pageRect = new Rectangle(pageRect);
            this.pageRect.Normalize();
            rect = new Rectangle(this.pageRect.Width, this.pageRect.Height);
            this.page = page;
        }

        /**
         * Sets the signature to be visible. An empty signature field with the same name must already exist.
         * @param fieldName the existing empty signature field name
         */
        virtual public void SetVisibleSignature(String fieldName) {
            AcroFields af = writer.GetAcroFields();
            AcroFields.Item item = af.GetFieldItem(fieldName);
            if (item == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.does.not.exist", fieldName));
            PdfDictionary merged = item.GetMerged(0);
            if (!PdfName.SIG.Equals(PdfReader.GetPdfObject(merged.Get(PdfName.FT))))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.is.not.a.signature.field", fieldName));
            this.fieldName = fieldName;
            PdfArray r = merged.GetAsArray(PdfName.RECT);
            float llx = r.GetAsNumber(0).FloatValue;
            float lly = r.GetAsNumber(1).FloatValue;
            float urx = r.GetAsNumber(2).FloatValue;
            float ury = r.GetAsNumber(3).FloatValue;
            pageRect = new Rectangle(llx, lly, urx, ury);
            pageRect.Normalize();
            page = item.GetPage(0);
            int rotation = writer.reader.GetPageRotation(page);
            Rectangle pageSize = writer.reader.GetPageSizeWithRotation(page);
            switch (rotation) {
                case 90:
                    pageRect = new Rectangle(
                    pageRect.Bottom,
                    pageSize.Top - pageRect.Left,
                    pageRect.Top,
                    pageSize.Top - pageRect.Right);
                    break;
                case 180:
                    pageRect = new Rectangle(
                    pageSize.Right - pageRect.Left,
                    pageSize.Top - pageRect.Bottom,
                    pageSize.Right - pageRect.Right,
                    pageSize.Top - pageRect.Top);
                    break;
                case 270:
                    pageRect = new Rectangle(
                    pageSize.Right - pageRect.Bottom,
                    pageRect.Left,
                    pageSize.Right - pageRect.Top,
                    pageRect.Right);
                    break;
            }
            if (rotation != 0)
                pageRect.Normalize();
            rect = new Rectangle(this.pageRect.Width, this.pageRect.Height);
        }

        /*
         * SIGNATURE APPEARANCE
         */
        
        /**
         * Signature rendering modes
         * @since 5.0.1
         */
        public enum RenderingMode {
            /**
             * The rendering mode is just the description.
             */
            DESCRIPTION,
            /**
             * The rendering mode is the name of the signer and the description.
             */
            NAME_AND_DESCRIPTION,
            /**
             * The rendering mode is an image and the description.
             */
            GRAPHIC_AND_DESCRIPTION,
            /**
             * The rendering mode is just an image.
             */
            GRAPHIC
        }

        /** The rendering mode chosen for visible signatures */
        private RenderingMode renderingMode = RenderingMode.DESCRIPTION;

        /**
        * Gets the rendering mode for this signature.
        * @return the rendering mode for this signature
        * @since 5.0.1
        */
        virtual public RenderingMode SignatureRenderingMode {
            get {
                return renderingMode;
            }
            set {
                renderingMode = value;
            }
        }

        /** The image that needs to be used for a visible signature */
        private Image signatureGraphic = null;

        /**
         * Sets the Image object to render when Render is set to <CODE>RenderingMode.GRAPHIC</CODE>
         * or <CODE>RenderingMode.GRAPHIC_AND_DESCRIPTION</CODE>.
         * @param signatureGraphic image rendered. If <CODE>null</CODE> the mode is defaulted
         * to <CODE>RenderingMode.DESCRIPTION</CODE>
         */
        virtual public Image SignatureGraphic {
            get {
                return signatureGraphic;
            }
            set {
                signatureGraphic = value;
            }
        }
        
        /** Appearance compliant with the recommendations introduced in Acrobat 6? */
        private bool acro6Layers = true;

        /**
         * Acrobat 6.0 and higher recommends that only layer n0 and n2 be present.
         * Use this method with value <code>false</code> if you want to ignore this recommendation.
         * @param acro6Layers if <code>true</code> only the layers n0 and n2 will be present
         * @deprecated Adobe no longer supports Adobe Acrobat / Reader versions older than 9
         */
        virtual public bool Acro6Layers {
            get {
                return acro6Layers;
            }
            set {
                acro6Layers = value;
            }
        }
        
        /** Layers for a visible signature. */
        private PdfTemplate[] app = new PdfTemplate[5];

        /**
         * Gets a template layer to create a signature appearance. The layers can go from 0 to 4,
         * but only layer 0 and 2 will be used if acro6Layers is true.
         * <p>
         * Consult <A HREF="http://partners.adobe.com/asn/developer/pdfs/tn/PPKAppearances.pdf">PPKAppearances.pdf</A>
         * for further details.
         * @param layer the layer
         * @return a template
         */
        virtual public PdfTemplate GetLayer(int layer) {
            if (layer < 0 || layer >= app.Length)
                return null;
            PdfTemplate t = app[layer];
            if (t == null) {
                t = app[layer] = new PdfTemplate(writer);
                t.BoundingBox = rect;
                writer.AddDirectTemplateSimple(t, new PdfName("n" + layer));
            }
            return t;
        }

        /** Indicates if we need to reuse the existing appearance as layer 0. */
        private bool reuseAppearance = false;

        /**
         * Indicates that the existing appearances needs to be reused as layer 0.
         */
        virtual public bool ReuseAppearance
        {
            set { reuseAppearance = value; }
        }

        // layer 1
        
        /** An appearance that can be used for layer 1 (if acro6Layers is false). */
        public const String questionMark =
            "% DSUnknown\n" +
            "q\n" +
            "1 G\n" +
            "1 g\n" +
            "0.1 0 0 0.1 9 0 cm\n" +
            "0 J 0 j 4 M []0 d\n" +
            "1 i \n" +
            "0 g\n" +
            "313 292 m\n" +
            "313 404 325 453 432 529 c\n" +
            "478 561 504 597 504 645 c\n" +
            "504 736 440 760 391 760 c\n" +
            "286 760 271 681 265 626 c\n" +
            "265 625 l\n" +
            "100 625 l\n" +
            "100 828 253 898 381 898 c\n" +
            "451 898 679 878 679 650 c\n" +
            "679 555 628 499 538 435 c\n" +
            "488 399 467 376 467 292 c\n" +
            "313 292 l\n" +
            "h\n" +
            "308 214 170 -164 re\n" +
            "f\n" +
            "0.44 G\n" +
            "1.2 w\n" +
            "1 1 0.4 rg\n" +
            "287 318 m\n" +
            "287 430 299 479 406 555 c\n" +
            "451 587 478 623 478 671 c\n" +
            "478 762 414 786 365 786 c\n" +
            "260 786 245 707 239 652 c\n" +
            "239 651 l\n" +
            "74 651 l\n" +
            "74 854 227 924 355 924 c\n" +
            "425 924 653 904 653 676 c\n" +
            "653 581 602 525 512 461 c\n" +
            "462 425 441 402 441 318 c\n" +
            "287 318 l\n" +
            "h\n" +
            "282 240 170 -164 re\n" +
            "B\n" +
            "Q\n";

        // layer 2
        
        /** A background image for the text in layer 2. */
        private Image image;
        
        /**
         * Gets the background image for the layer 2.
         * @return the background image for the layer 2
         */
        virtual public Image Image {
            get {
                return image;
            }
            set {
                image = value;
            }
        }

        /** the scaling to be applied to the background image.t  */
        private float imageScale;

        /**
         * Sets the scaling to be applied to the background image. If it's zero the image
         * will fully fill the rectangle. If it's less than zero the image will fill the rectangle but
         * will keep the proportions. If it's greater than zero that scaling will be applied.
         * In any of the cases the image will always be centered. It's zero by default.
         * @param imageScale the scaling to be applied to the background image
         */
        virtual public float ImageScale {
            get {
                return imageScale;
            }
            set {
                imageScale = value;
            }
        }
        
        /** The text that goes in Layer 2 of the signature appearance. */
        private String layer2Text;
        
        /**
         * Sets the signature text identifying the signer.
         * @param text the signature text identifying the signer. If <CODE>null</CODE> or not set
         * a standard description will be used
         */
        virtual public string Layer2Text {
            get {
                return layer2Text;
            }
            set {
                layer2Text = value;
            }
        }
        
        /** Font for the text in Layer 2. */
        private Font layer2Font;
        
        /**
         * Sets the n2 and n4 layer font. If the font size is zero, auto-fit will be used.
         * @param layer2Font the n2 and n4 font
         */
        virtual public Font Layer2Font {
            get {
                return layer2Font;
            }
            set {
                layer2Font = value;
            }
        }

        /** Run direction for the text in layers 2 and 4. */
        private int runDirection = PdfWriter.RUN_DIRECTION_NO_BIDI;
        
        /** Sets the run direction in the n2 and n4 layer.
         * @param runDirection the run direction
         */
        virtual public int RunDirection {
            set {
                if (value < PdfWriter.RUN_DIRECTION_DEFAULT || value > PdfWriter.RUN_DIRECTION_RTL)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.run.direction.1", runDirection));
                this.runDirection = value;
            }
            get {
                return runDirection;
            }
        }
        
        // layer 4
        
        /** The text that goes in Layer 4 of the appearance. */
        private String layer4Text;
        
        /**
         * Sets the text identifying the signature status. Will be ignored if acro6Layers is true.
         * @param text the text identifying the signature status. If <CODE>null</CODE> or not set
         * the description "Signature Not Verified" will be used
         */
        virtual public string Layer4Text {
            get {
                return layer4Text;
            }
            set {
                layer4Text = value;
            }
        }

        // all layers
        
        /** Template containing all layers drawn on top of each other. */
        private PdfTemplate frm;
        
        /**
         * Gets the template that aggregates all appearance layers. This corresponds to the /FRM resource.
         * <p>
         * Consult <A HREF="http://partners.adobe.com/asn/developer/pdfs/tn/PPKAppearances.pdf">PPKAppearances.pdf</A>
         * for further details.
         * @return the template that aggregates all appearance layers
         */
        virtual public PdfTemplate GetTopLayer() {
            if (frm == null) {
                frm = new PdfTemplate(writer);
                frm.BoundingBox = rect;
                writer.AddDirectTemplateSimple(frm, new PdfName("FRM"));
            }
            return frm;
        }
        
        // creating the appearance
        
        /** extra space at the top. */
        private const float TOP_SECTION = 0.3f;
        
        /** margin for the content inside the signature rectangle. */
        private const float MARGIN = 2;
        
        /**
         * Gets the main appearance layer.
         * <p>
         * Consult <A HREF="http://partners.adobe.com/asn/developer/pdfs/tn/PPKAppearances.pdf">PPKAppearances.pdf</A>
         * for further details.
         * @return the main appearance layer
         * @throws DocumentException on error
         */
        virtual public PdfTemplate GetAppearance() {
            if (IsInvisible()) {
                PdfTemplate t = new PdfTemplate(writer);
                t.BoundingBox = new Rectangle(0, 0);
                writer.AddDirectTemplateSimple(t, null);
                return t;
            }
            if (app[0] == null && !reuseAppearance)
                CreateBlankN0();            
            if (app[1] == null && !acro6Layers) {
                PdfTemplate t = app[1] = new PdfTemplate(writer);
                t.BoundingBox = new Rectangle(100, 100);
                writer.AddDirectTemplateSimple(t, new PdfName("n1"));
                t.SetLiteral(questionMark);
            }
            if (app[2] == null) {
                String text;
                if (layer2Text == null) {
                    StringBuilder buf = new StringBuilder();
                    buf.Append("Digitally signed by ");
                    String name = null;
                    CertificateInfo.X509Name x500name = CertificateInfo.GetSubjectFields((X509Certificate)signCertificate);
                    if (x500name != null) {
                        name = x500name.GetField("CN");
                        if (name == null)
                            name = x500name.GetField("E");
                    }
                    if (name == null)
                        name = "";
                    buf.Append(name).Append('\n');
                    buf.Append("Date: ").Append(signDate.ToString("yyyy.MM.dd HH:mm:ss zzz"));
                    if (reason != null)
                        buf.Append('\n').Append(reasonCaption).Append(reason);
                    if (location != null)
                        buf.Append('\n').Append(locationCaption).Append(location);
                    text = buf.ToString();
                }
                else
                    text = layer2Text;
                PdfTemplate t = app[2] = new PdfTemplate(writer);
                t.BoundingBox = rect;
                writer.AddDirectTemplateSimple(t, new PdfName("n2"));
                if (image != null) {
                    if (imageScale == 0) {
                        t.AddImage(image, rect.Width, 0, 0, rect.Height, 0, 0);
                    }
                    else {
                        float usableScale = imageScale;
                        if (imageScale < 0)
                            usableScale = Math.Min(rect.Width / image.Width, rect.Height / image.Height);
                        float w = image.Width * usableScale;
                        float h = image.Height * usableScale;
                        float x = (rect.Width - w) / 2;
                        float y = (rect.Height - h) / 2;
                        t.AddImage(image, w, 0, 0, h, x, y);
                    }
                }
                Font font;
                if (layer2Font == null)
                    font = new Font();
                else
                    font = new Font(layer2Font);
                float size = font.Size;

                Rectangle dataRect = null;
                Rectangle signatureRect = null;

                if (renderingMode == RenderingMode.NAME_AND_DESCRIPTION || 
                    (renderingMode == RenderingMode.GRAPHIC_AND_DESCRIPTION && this.SignatureGraphic != null)) {
                    // origin is the bottom-left
                    signatureRect = new Rectangle(
                        MARGIN, 
                        MARGIN, 
                        rect.Width / 2 - MARGIN,
                        rect.Height - MARGIN);
                    dataRect = new Rectangle(
                        rect.Width / 2 +  MARGIN / 2, 
                        MARGIN, 
                        rect.Width - MARGIN / 2,
                        rect.Height - MARGIN);

                    if (rect.Height > rect.Width) {
                        signatureRect = new Rectangle(
                            MARGIN, 
                            rect.Height / 2, 
                            rect.Width - MARGIN,
                            rect.Height);
                        dataRect = new Rectangle(
                            MARGIN, 
                            MARGIN, 
                            rect.Width - MARGIN,
                            rect.Height / 2 - MARGIN);
                    }
                }
                else if (renderingMode == RenderingMode.GRAPHIC) {
                    if (signatureGraphic == null) {
                        throw new InvalidOperationException(MessageLocalization.GetComposedMessage("a.signature.image.should.be.present.when.rendering.mode.is.graphic.only"));
                    }
                    signatureRect = new Rectangle(
                            MARGIN,
                            MARGIN,
                            rect.Width - MARGIN, // take all space available
                            rect.Height - MARGIN);
                }
                else {
                    dataRect = new Rectangle(
                        MARGIN, 
                        MARGIN, 
                        rect.Width - MARGIN,
                        rect.Height * (1 - TOP_SECTION) - MARGIN);
                }

                if (renderingMode == RenderingMode.NAME_AND_DESCRIPTION) {
                    string signedBy = CertificateInfo.GetSubjectFields(signCertificate).GetField("CN");
                    if (signedBy == null)
                        signedBy = CertificateInfo.GetSubjectFields(signCertificate).GetField("E");
                    if (signedBy == null)
                        signedBy = "";
                    Rectangle sr2 = new Rectangle(signatureRect.Width - MARGIN, signatureRect.Height - MARGIN );
                    float signedSize = ColumnText.FitText(font, signedBy, sr2, -1, runDirection);

                    ColumnText ct2 = new ColumnText(t);
                    ct2.RunDirection = runDirection;
                    ct2.SetSimpleColumn(new Phrase(signedBy, font), signatureRect.Left, signatureRect.Bottom, signatureRect.Right, signatureRect.Top, signedSize, Element.ALIGN_LEFT);

                    ct2.Go();
                }
                else if (renderingMode == RenderingMode.GRAPHIC_AND_DESCRIPTION) {
                    if (signatureGraphic == null) {
                        throw new InvalidOperationException(MessageLocalization.GetComposedMessage("a.signature.image.should.be.present.when.rendering.mode.is.graphic.and.description"));
                    }
                    ColumnText ct2 = new ColumnText(t);
                    ct2.RunDirection = runDirection;
                    ct2.SetSimpleColumn(signatureRect.Left, signatureRect.Bottom, signatureRect.Right, signatureRect.Top, 0, Element.ALIGN_RIGHT);

                    Image im = Image.GetInstance(SignatureGraphic);
                    im.ScaleToFit(signatureRect.Width, signatureRect.Height);

                    Paragraph p = new Paragraph();
                    // must calculate the point to draw from to make image appear in middle of column
                    float x = 0;
                    // experimentation found this magic number to counteract Adobe's signature graphic, which
                    // offsets the y co-ordinate by 15 units
                    float y = -im.ScaledHeight + 15;

                    x = x + (signatureRect.Width - im.ScaledWidth) / 2;
                    y = y - (signatureRect.Height - im.ScaledHeight) / 2;
                    p.Add(new Chunk(im, x + (signatureRect.Width - im.ScaledWidth) / 2, y, false));
                    ct2.AddElement(p);
                    ct2.Go();
                }

                else if (renderingMode == RenderingMode.GRAPHIC) {
                    ColumnText ct2 = new ColumnText(t);
                    ct2.RunDirection = runDirection;
                    ct2.SetSimpleColumn(signatureRect.Left, signatureRect.Bottom, signatureRect.Right, signatureRect.Top, 0, Element.ALIGN_RIGHT);

                    Image im = Image.GetInstance(signatureGraphic);
                    im.ScaleToFit(signatureRect.Width, signatureRect.Height);

                    Paragraph p = new Paragraph(signatureRect.Height);
                    // must calculate the point to draw from to make image appear in middle of column
                    float x = (signatureRect.Width - im.ScaledWidth) / 2;
                    float y = (signatureRect.Height - im.ScaledHeight) / 2;
                    p.Add(new Chunk(im, x, y, false));
                    ct2.AddElement(p);
                    ct2.Go();
                }
                
                if (renderingMode != RenderingMode.GRAPHIC) {
                    if (size <= 0) {
                        Rectangle sr = new Rectangle(dataRect.Width, dataRect.Height);
                        size = ColumnText.FitText(font, text, sr, 12, runDirection);
                    }
                    ColumnText ct = new ColumnText(t);
                    ct.RunDirection = runDirection;
                    ct.SetSimpleColumn(new Phrase(text, font), dataRect.Left, dataRect.Bottom, dataRect.Right, dataRect.Top, size, Element.ALIGN_LEFT);
                    ct.Go();
                }
            }
            if (app[3] == null && !acro6Layers) {
                PdfTemplate t = app[3] = new PdfTemplate(writer);
                t.BoundingBox = new Rectangle(100, 100);
                writer.AddDirectTemplateSimple(t, new PdfName("n3"));
                t.SetLiteral("% DSBlank\n");
            }
            if (app[4] == null && !acro6Layers) {
                PdfTemplate t = app[4] = new PdfTemplate(writer);
                t.BoundingBox = new Rectangle(0, rect.Height * (1 - TOP_SECTION), rect.Right, rect.Top);
                writer.AddDirectTemplateSimple(t, new PdfName("n4"));
                Font font;
                if (layer2Font == null)
                    font = new Font();
                else
                    font = new Font(layer2Font);
                float size = font.Size;
                String text = "Signature Not Verified";
                if (layer4Text != null)
                    text = layer4Text;
                Rectangle sr = new Rectangle(rect.Width - 2 * MARGIN, rect.Height * TOP_SECTION - 2 * MARGIN);
                size = ColumnText.FitText(font, text, sr, 15, runDirection);
                ColumnText ct = new ColumnText(t);
                ct.RunDirection = runDirection;
                ct.SetSimpleColumn(new Phrase(text, font), MARGIN, 0, rect.Width - MARGIN, rect.Height - MARGIN, size, Element.ALIGN_LEFT);
                ct.Go();
            }
            int rotation = writer.reader.GetPageRotation(page);
            Rectangle rotated = new Rectangle(rect);
            int n = rotation;
            while (n > 0) {
                rotated = rotated.Rotate();
                n -= 90;
            }
            if (frm == null) {
                frm = new PdfTemplate(writer);
                frm.BoundingBox = rotated;
                writer.AddDirectTemplateSimple(frm, new PdfName("FRM"));
                float scale = Math.Min(rect.Width, rect.Height) * 0.9f;
                float x = (rect.Width - scale) / 2;
                float y = (rect.Height - scale) / 2;
                scale /= 100;
                if (rotation == 90)
                    frm.ConcatCTM(0, 1, -1, 0, rect.Height, 0);
                else if (rotation == 180)
                    frm.ConcatCTM(-1, 0, 0, -1, rect.Width, rect.Height);
                else if (rotation == 270)
                    frm.ConcatCTM(0, -1, 1, 0, 0, rect.Width);
                if (reuseAppearance) {
                    AcroFields af = writer.GetAcroFields();
                    PdfIndirectReference refe = af.GetNormalAppearance(FieldName);
                    if (refe != null) {
                	    frm.AddTemplateReference(refe, new PdfName("n0"), 1, 0, 0, 1, 0, 0);
                    }
                    else {
                	    reuseAppearance = false;
                        if (app[0] == null) {
                            CreateBlankN0();
                        }
                    }
                }
                if (!reuseAppearance) {
            	    frm.AddTemplate(app[0], 0, 0);
                }
                if (!acro6Layers)
                    frm.AddTemplate(app[1], scale, 0, 0, scale, x, y);
                frm.AddTemplate(app[2], 0, 0);
                if (!acro6Layers) {
                    frm.AddTemplate(app[3], scale, 0, 0, scale, x, y);
                    frm.AddTemplate(app[4], 0, 0);
                }
            }
            PdfTemplate napp = new PdfTemplate(writer);
            napp.BoundingBox = rotated;
            writer.AddDirectTemplateSimple(napp, null);
            napp.AddTemplate(frm, 0, 0);
            return napp;
        }

        private void CreateBlankN0() {
            PdfTemplate t = app[0] = new PdfTemplate(writer);
            t.BoundingBox = new Rectangle(100, 100);
            writer.AddDirectTemplateSimple(t, new PdfName("n0"));
            t.SetLiteral("% DSBlank\n");
        }

        /*
         * Creating the signed file.
         */

        /** The PdfStamper that creates the signed PDF. */
        private PdfStamper stamper;

        /**
         * Gets the <CODE>PdfStamper</CODE> associated with this instance.
         * @return the <CODE>PdfStamper</CODE> associated with this instance
         */
        virtual public PdfStamper Stamper {
            get {
                return stamper;
            }
        }

        /**
         * Sets the PdfStamper
         * @param stamper PdfStamper
         */        
        virtual public void SetStamper(PdfStamper stamper) {
            this.stamper = stamper;
        }

        /** The PdfStamperImp object corresponding with the stamper. */
        private PdfStamperImp writer;
        
        /** A byte buffer containing the bytes of the Stamper. */
        private ByteBuffer sigout;
        
        /**
         * Getter for the byte buffer.
         */
        virtual public ByteBuffer Sigout {
            get {
                return sigout;
            }
            set {
                sigout = value;
            }
        }

        /** OutputStream for the bytes of the stamper. */
        private Stream originalout;
        
        virtual public Stream Originalout {
            get {
                return originalout;
            }
            set {
                originalout = value;
            }
        }

        /** Temporary file in case you don't want to sign in memory. */
        private string tempFile;
        
        /**
        * Gets the temporary file.
        * @return the temporary file or <CODE>null</CODE> is the document is created in memory
        */    
        virtual public string TempFile {
            get {
                return tempFile;
            }
        }

        virtual public void SetTempFile(string tempFile) {
            this.tempFile = tempFile;
        }

        /** Name and content of keys that can only be added in the close() method. */
        private Dictionary<PdfName, PdfLiteral> exclusionLocations;
        
        /** Length of the output. */
        private int boutLen;
        
        /** Indicates if the stamper has already been pre-closed. */
        private bool preClosed = false;

        /// <summary>
        /// Signature field lock dictionary.
        /// </summary>
        private PdfSigLockDictionary fieldLock;

        /// <summary>
        /// Signature field lock dictionary.
        /// </summary>
        /// <remarks>
        /// If a signature is created on an existing signature field, then its /Lock dictionary 
        /// takes the precedence (if it exists).
        /// </remarks>
        public virtual PdfSigLockDictionary FieldLockDict {
            get { return fieldLock; }
            set { fieldLock = value; }
        }

        /**
         * Checks if the document is in the process of closing.
         * @return <CODE>true</CODE> if the document is in the process of closing,
         * <CODE>false</CODE> otherwise
         */
        virtual public bool IsPreClosed() {
            return preClosed;
        }
        
        /**
         * This is the first method to be called when using external signatures. The general sequence is:
         * preClose(), getDocumentBytes() and close().
         * <p>
         * If calling preClose() <B>dont't</B> call PdfStamper.close().
         * <p>
         * <CODE>exclusionSizes</CODE> must contain at least
         * the <CODE>PdfName.CONTENTS</CODE> key with the size that it will take in the
         * document. Note that due to the hex string coding this size should be
         * byte_size*2+2.
         * @param exclusionSizes a <CODE>HashMap</CODE> with names and sizes to be excluded in the signature
         * calculation. The key is a <CODE>PdfName</CODE> and the value an
         * <CODE>Integer</CODE>. At least the <CODE>PdfName.CONTENTS</CODE> must be present
         * @throws IOException on error
         * @throws DocumentException on error
         */
        virtual public void PreClose(Dictionary<PdfName, int> exclusionSizes) {
            if (preClosed)
                throw new DocumentException(MessageLocalization.GetComposedMessage("document.already.pre.closed"));
            stamper.MergeVerification();
            preClosed = true;
            AcroFields af = writer.GetAcroFields();
            String name = FieldName;
            bool fieldExists = af.DoesSignatureFieldExist(name);
            PdfIndirectReference refSig = writer.PdfIndirectReference;
            writer.SigFlags = 3;
            PdfDictionary fieldLock = null;
            if (fieldExists) {
                PdfDictionary merged = af.GetFieldItem(name).GetMerged(0);
                writer.MarkUsed(merged);
                fieldLock = merged.GetAsDict(PdfName.LOCK);

                if (fieldLock == null && FieldLockDict != null) {
                    merged.Put(PdfName.LOCK, writer.AddToBody(FieldLockDict).IndirectReference);
                    fieldLock = FieldLockDict;
                }

                merged.Put(PdfName.P, writer.GetPageReference(Page));
                merged.Put(PdfName.V, refSig);
                PdfObject obj = PdfReader.GetPdfObjectRelease(merged.Get(PdfName.F));
                int flags = 0;
                if (obj != null && obj.IsNumber())
                    flags = ((PdfNumber)obj).IntValue;
                flags |= PdfAnnotation.FLAGS_LOCKED;
                merged.Put(PdfName.F, new PdfNumber(flags));
                PdfDictionary ap = new PdfDictionary();
                ap.Put(PdfName.N, GetAppearance().IndirectReference);
                merged.Put(PdfName.AP, ap);
            }
            else {
                PdfFormField sigField = PdfFormField.CreateSignature(writer);
                sigField.FieldName = name;
                sigField.Put(PdfName.V, refSig);
                sigField.Flags = PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_LOCKED;

                if (FieldLockDict != null) {
                    sigField.Put(PdfName.LOCK, writer.AddToBody(FieldLockDict).IndirectReference);
                    fieldLock = FieldLockDict;
                }

                int pagen = Page;
                if (!IsInvisible())
                    sigField.SetWidget(PageRect, null);
                else
                    sigField.SetWidget(new Rectangle(0, 0), null);
                sigField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, GetAppearance());
                sigField.Page = pagen;
                writer.AddAnnotation(sigField, pagen);
            }

            exclusionLocations = new Dictionary<PdfName,PdfLiteral>();
            if (cryptoDictionary == null) {
                throw new DocumentException("No crypto dictionary defined.");
            }
            else {
                PdfLiteral lit = new PdfLiteral(80);
                exclusionLocations[PdfName.BYTERANGE] = lit;
                cryptoDictionary.Put(PdfName.BYTERANGE, lit);
                foreach (KeyValuePair<PdfName,int> entry in exclusionSizes) {
                    PdfName key = entry.Key;
                    int v = entry.Value;
                    lit = new PdfLiteral(v);
                    exclusionLocations[key] = lit;
                    cryptoDictionary.Put(key, lit);
                }
                if (certificationLevel > 0)
                    AddDocMDP(cryptoDictionary);
                if (fieldLock != null)
                    AddFieldMDP(cryptoDictionary, fieldLock);
                if (signatureEvent != null)
                    signatureEvent.GetSignatureDictionary(cryptoDictionary);
                writer.AddToBody(cryptoDictionary, refSig, false);
            }
            if (certificationLevel > 0) {
                // add DocMDP entry to root
                PdfDictionary docmdp = new PdfDictionary();
                docmdp.Put(new PdfName("DocMDP"), refSig);
                writer.reader.Catalog.Put(new PdfName("Perms"), docmdp);
            }
            writer.Close(stamper.MoreInfo);
            
            range = new long[exclusionLocations.Count * 2];
            long byteRangePosition = exclusionLocations[PdfName.BYTERANGE].Position;
            exclusionLocations.Remove(PdfName.BYTERANGE);
            int idx = 1;
            foreach (PdfLiteral lit in exclusionLocations.Values) {
                long n = lit.Position;
                range[idx++] = n;
                range[idx++] = lit.PosLength + n;
            }
            Array.Sort(range, 1, range.Length - 2);
            for (int k = 3; k < range.Length - 2; k += 2)
                range[k] -= range[k - 1];
            
            if (tempFile == null) {
                bout = sigout.Buffer;
                boutLen = sigout.Size;
                range[range.Length - 1] = boutLen - range[range.Length - 2];
                ByteBuffer bf = new ByteBuffer();
                bf.Append('[');
                for (int k = 0; k < range.Length; ++k)
                    bf.Append(range[k]).Append(' ');
                bf.Append(']');
                Array.Copy(bf.Buffer, 0, bout, byteRangePosition, bf.Size);
            }
            else {
                try {
                    raf = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite);
                    long len = raf.Length;
                    range[range.Length - 1] = len - range[range.Length - 2];
                    ByteBuffer bf = new ByteBuffer();
                    bf.Append('[');
                    for (int k = 0; k < range.Length; ++k)
                        bf.Append(range[k]).Append(' ');
                    bf.Append(']');
                    raf.Seek(byteRangePosition, SeekOrigin.Begin);
                    raf.Write(bf.Buffer, 0, bf.Size);
                }
                catch (IOException e) {
                    try{raf.Close();}catch{}
                    try{File.Delete(tempFile);}catch{}
                    throw e;
                }
            }
        }

        /**
         * Adds keys to the signature dictionary that define
         * the certification level and the permissions.
         * This method is only used for Certifying signatures.
         * @param crypto the signature dictionary
         */
        private void AddDocMDP(PdfDictionary crypto) {
            PdfDictionary reference = new PdfDictionary();
            PdfDictionary transformParams = new PdfDictionary();
            transformParams.Put(PdfName.P, new PdfNumber(certificationLevel));
            transformParams.Put(PdfName.V, new PdfName("1.2"));
            transformParams.Put(PdfName.TYPE, PdfName.TRANSFORMPARAMS);
            reference.Put(PdfName.TRANSFORMMETHOD, PdfName.DOCMDP);
            reference.Put(PdfName.TYPE, PdfName.SIGREF);
            reference.Put(PdfName.TRANSFORMPARAMS, transformParams);
            if (writer.GetPdfVersion().Version < PdfWriter.VERSION_1_6) {
                reference.Put(new PdfName("DigestValue"), new PdfString("aa"));
                PdfArray loc = new PdfArray();
                loc.Add(new PdfNumber(0));
                loc.Add(new PdfNumber(0));
                reference.Put(new PdfName("DigestLocation"), loc);
                reference.Put(new PdfName("DigestMethod"), new PdfName("MD5"));
            }
            reference.Put(PdfName.DATA, writer.reader.Trailer.Get(PdfName.ROOT));
            PdfArray types = new PdfArray();
            types.Add(reference);
            crypto.Put(PdfName.REFERENCE, types);
        }

        /**
         * Adds keys to the signature dictionary that define
         * the field permissions.
         * This method is only used for signatures that lock fields.
         * @param crypto the signature dictionary
         */
        private void AddFieldMDP(PdfDictionary crypto, PdfDictionary fieldLock) {
            PdfDictionary reference = new PdfDictionary();
            PdfDictionary transformParams = new PdfDictionary();
            transformParams.Merge(fieldLock);
            transformParams.Put(PdfName.TYPE, PdfName.TRANSFORMPARAMS);
            transformParams.Put(PdfName.V, new PdfName("1.2"));
            reference.Put(PdfName.TRANSFORMMETHOD, PdfName.FIELDMDP);
            reference.Put(PdfName.TYPE, PdfName.SIGREF);
            reference.Put(PdfName.TRANSFORMPARAMS, transformParams);
            reference.Put(new PdfName("DigestValue"), new PdfString("aa"));
            PdfArray loc = new PdfArray();
            loc.Add(new PdfNumber(0));
            loc.Add(new PdfNumber(0));
            reference.Put(new PdfName("DigestLocation"), loc);
            reference.Put(new PdfName("DigestMethod"), new PdfName("MD5"));
            reference.Put(PdfName.DATA, writer.reader.Trailer.Get(PdfName.ROOT));
            PdfArray types = crypto.GetAsArray(PdfName.REFERENCE);
            if (types == null)
        	    types = new PdfArray();
            types.Add(reference);
            crypto.Put(PdfName.REFERENCE, types);
        }

        /**
         * This is the last method to be called when using external signatures. The general sequence is:
         * preClose(), getDocumentBytes() and close().
         * <p>
         * <CODE>update</CODE> is a <CODE>PdfDictionary</CODE> that must have exactly the
         * same keys as the ones provided in {@link #preClose(HashMap)}.
         * @param update a <CODE>PdfDictionary</CODE> with the key/value that will fill the holes defined
         * in {@link #preClose(HashMap)}
         * @throws DocumentException on error
         * @throws IOException on error
         */
        virtual public void Close(PdfDictionary update) {
            try {
                if (!preClosed)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("preclose.must.be.called.first"));
                ByteBuffer bf = new ByteBuffer();
                foreach (PdfName key in update.Keys) {
                    PdfObject obj = update.Get(key);
                    PdfLiteral lit = exclusionLocations[key];
                    if (lit == null)
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("the.key.1.didn.t.reserve.space.in.preclose", key.ToString()));
                    bf.Reset();
                    obj.ToPdf(null, bf);
                    if (bf.Size > lit.PosLength)
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("the.key.1.is.too.big.is.2.reserved.3", key.ToString(), bf.Size, lit.PosLength));
                    if (tempFile == null)
                        Array.Copy(bf.Buffer, 0, bout, lit.Position, bf.Size);
                    else {
                        raf.Seek(lit.Position, SeekOrigin.Begin);
                        raf.Write(bf.Buffer, 0, bf.Size);
                    }
                }
                if (update.Size != exclusionLocations.Count)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.update.dictionary.has.less.keys.than.required"));
                if (tempFile == null) {
                    originalout.Write(bout, 0, boutLen);
                }
                else {
                    if (originalout != null) {
                        raf.Seek(0, SeekOrigin.Begin);
                        long length = raf.Length;
                        byte[] buf = new byte[8192];
                        while (length > 0) {
                            int r = raf.Read(buf, 0, (int)Math.Min((long)buf.Length, length));
                            if (r < 0)
                                throw new EndOfStreamException(MessageLocalization.GetComposedMessage("unexpected.eof"));
                            originalout.Write(buf, 0, r);
                            length -= r;
                        }
                    }
                }
            }
            finally {
                writer.reader.Close();
                if (tempFile != null) {
                    try{raf.Close();}catch{}
                    if (originalout != null)
                        try{File.Delete(tempFile);}catch{}
                }
                if (originalout != null)
                    try{originalout.Close();}catch{}
            }
        }
    }
}
