using System;
using System.Text;
using System.Collections.Generic;
using iTextSharp.text.error_messages;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.pdf.interfaces;

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
    /// This is the smallest significant part of text that can be added to a document.
    /// </summary>
    /// <remarks>
    /// Most elements can be divided in one or more Chunks.
    /// A chunk is a string with a certain Font.
    /// all other layoutparameters should be defined in the object to which
    /// this chunk of text is added.
    /// </remarks>
    /// <example>
    /// <code>
    /// <strong>Chunk chunk = new Chunk("Hello world", FontFactory.GetFont(FontFactory.COURIER, 20, Font.ITALIC, new BaseColor(255, 0, 0)));</strong>
    /// document.Add(chunk);
    /// </code>
    /// </example>
    public class Chunk : IElement, IAccessibleElement {

        // public static membervariables

        /** The character stand in for an image or a separator. */
        public const string OBJECT_REPLACEMENT_CHARACTER = "\ufffc";

        ///<summary> This is a Chunk containing a newline. </summary>
        public static readonly Chunk NEWLINE = new Chunk("\n");

    /** This is a Chunk containing a newpage. */
        public static readonly Chunk NEXTPAGE = new Chunk("");
        static Chunk() {
            NEXTPAGE.SetNewPage();
            NEWLINE.Role = PdfName.P;
        }

            
        public static readonly Chunk TABBING = new Chunk(float.NaN, false);

        public static readonly Chunk SPACETABBING = new Chunk(float.NaN, true);

        // member variables

        ///<summary> This is the content of this chunk of text. </summary>
        protected StringBuilder content = null;

        ///<summary> This is the Font of this chunk of text. </summary>
        protected Font font = null;

        ///<summary> Contains some of the attributes for this Chunk. </summary>
        protected Dictionary<string,object> attributes = null;

        protected internal PdfName role = null;
        protected internal Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = null;

        // constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        /// <overloads>
        /// Has six overloads.
        /// </overloads>
	    public Chunk() {
		    this.content = new StringBuilder();
            this.font = new Font();
            this.role = PdfName.SPAN;
	    }

        /**
        * A <CODE>Chunk</CODE> copy constructor.
        * @param ck the <CODE>Chunk</CODE> to be copied
        */    
        public Chunk(Chunk ck) {
            if (ck.content != null) {
                content = new StringBuilder(ck.content.ToString());
            }
            if (ck.font != null) {
                font = new Font(ck.font);
            }
            if (ck.attributes != null) {
                attributes = new Dictionary<String,object>(ck.attributes);
            }
            role = ck.role;
            if (ck.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(ck.accessibleAttributes);
            }
            id = ck.ID;
        }
        
        /// <summary>
        /// Constructs a chunk of text with a certain content and a certain Font.
        /// </summary>
        /// <param name="content">the content</param>
        /// <param name="font">the font</param>
        public Chunk(string content, Font font) {
            this.content = new StringBuilder(content);
            this.font = font;
            this.role = PdfName.SPAN;
        }

        /// <summary>
        /// Constructs a chunk of text with a certain content, without specifying a Font.
        /// </summary>
        /// <param name="content">the content</param>
        public Chunk(string content) : this(content, new Font()) {}

        /**
        * Constructs a chunk of text with a char and a certain <CODE>Font</CODE>.
        *
        * @param    c        the content
        * @param    font        the font
        */
        public Chunk(char c, Font font) {
            this.content = new StringBuilder();
            this.content.Append(c);
            this.font = font;
            this.role = PdfName.SPAN;
        }
            
        /**
        * Constructs a chunk of text with a char, without specifying a <CODE>Font</CODE>.
        *
        * @param    c        the content
        */
        public Chunk(char c) : this(c, new Font()) {
        }

        /// <summary>
        /// Constructs a chunk containing an Image.
        /// </summary>
        /// <param name="image">the image</param>
        /// <param name="offsetX">the image offset in the x direction</param>
        /// <param name="offsetY">the image offset in the y direction</param>
        public Chunk(Image image, float offsetX, float offsetY) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            Image copyImage = Image.GetInstance(image);
            copyImage.SetAbsolutePosition(float.NaN, float.NaN);
            SetAttribute(IMAGE, new Object[]{copyImage, offsetX, offsetY, false});
            this.role = null;
        }

        /**
        * Key for drawInterface of the Separator.
        * @since   2.1.2
        */
        public const string SEPARATOR = "SEPARATOR";
        
        /**
        * Creates a separator Chunk.
        * Note that separator chunks can't be used in combination with tab chunks!
        * @param   separator   the drawInterface to use to draw the separator.
        * @since   2.1.2
        */
        public Chunk(IDrawInterface separator) : this(separator, false) {
        }   
        
        /**
        * Creates a separator Chunk.
        * Note that separator chunks can't be used in combination with tab chunks!
        * @param   separator   the drawInterface to use to draw the separator.
        * @param   vertical    true if this is a vertical separator
        * @since   2.1.2
        */
        public Chunk(IDrawInterface separator, bool vertical) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            SetAttribute(SEPARATOR, new Object[] {separator, vertical});
            this.role = null;
        }

        /**
        * Key for drawInterface of the tab.
        * @since   2.1.2
        */
        public const string TAB = "TAB";
        /**
         * Key for tab stops of the tab.
         * @since	5.4.1
         */
        public const string TABSETTINGS = "TABSETTINGS";
        private string contentWithNoTabs = null;

        /**
        * Creates a tab Chunk.
        * Note that separator chunks can't be used in combination with tab chunks!
        * @param   separator   the drawInterface to use to draw the tab.
        * @param   tabPosition an X coordinate that will be used as start position for the next Chunk.
        * @since   2.1.2
        */
        [Obsolete]
        public Chunk(IDrawInterface separator, float tabPosition) : this(separator, tabPosition, false) {
        }
        
        /**
        * Creates a tab Chunk.
        * Note that separator chunks can't be used in combination with tab chunks!
        * @param   separator   the drawInterface to use to draw the tab.
        * @param   tabPosition an X coordinate that will be used as start position for the next Chunk.
        * @param   newline     if true, a newline will be added if the tabPosition has already been reached.
        * @since   2.1.2
        */
        [Obsolete]
        public Chunk(IDrawInterface separator, float tabPosition, bool newline) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            if (tabPosition < 0) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tab.position.may.not.be.lower.than.0.yours.is.1", tabPosition));
            }
            SetAttribute(TAB, new Object[] { separator, tabPosition, newline, 0 });
            this.role = PdfName.ARTIFACT;
        }

        /**
         * Creates a tab Chunk.
         *
         * @param   tabInterval     an interval that will be used if tab stops are omitted.
         * @param   isWhitespace    if true, the current tab is treated as white space.
         * @since 5.4.1
         */
        private Chunk(float tabInterval, bool isWhitespace) : this(OBJECT_REPLACEMENT_CHARACTER, new Font())
        {
            if (tabInterval < 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tab.position.may.not.be.lower.than.0.yours.is.1", tabInterval));
            SetAttribute(TAB, new Object[] {tabInterval, isWhitespace});
            SetAttribute(SPLITCHARACTER, TabSplitCharacter.TAB);

            SetAttribute(TABSETTINGS, null);
            this.role = PdfName.ARTIFACT;
        }


        /// <summary>
        /// Constructs a chunk containing an Image.
        /// </summary>
        /// <param name="image">the image</param>
        /// <param name="offsetX">the image offset in the x direction</param>
        /// <param name="offsetY">the image offset in the y direction</param>
        /// <param name="changeLeading">true if the leading has to be adapted to the image</param>
        public Chunk(Image image, float offsetX, float offsetY, bool changeLeading) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            SetAttribute(IMAGE, new Object[]{image, offsetX, offsetY, changeLeading});
            this.role = PdfName.ARTIFACT;
        }

        // implementation of the Element-methods

        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        virtual public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException) {
                return false;
            }
        }

        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        virtual public int Type {
            get {
                return Element.CHUNK;
            }
        }

        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        virtual public IList<Chunk> Chunks {
            get {
                List<Chunk> tmp = new List<Chunk>();
                tmp.Add(this);
                return tmp;
            }
        }

        // methods

        /// <summary>
        /// appends some text to this Chunk.
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>a StringBuilder</returns>
        virtual public StringBuilder Append(string str) {
            contentWithNoTabs = null;
            return content.Append(str);
        }

        // methods to retrieve information

        /// <summary>
        /// Get/set the font of this Chunk.
        /// </summary>
        /// <value>a Font</value>
        public virtual Font Font {
            get {
                return font;
            }

            set {
                this.font = value;
            }
        }


        /// <summary>
        /// Returns the content of this Chunk.
        /// </summary>
        /// <value>a string</value>
        public virtual string Content {
            get {
                if (contentWithNoTabs == null)
                    contentWithNoTabs = content.ToString().Replace("\t", "");
                return contentWithNoTabs;
            }
        }

        public override string ToString() {
            return Content;
        }


        /// <summary>
        /// Checks is this Chunk is empty.
        /// </summary>
        /// <returns>false if the Chunk contains other characters than space.</returns>
        public virtual bool IsEmpty() {
            return (content.ToString().Trim().Length == 0) && (content.ToString().IndexOf("\n") == -1) && (attributes == null);
        }

        /**
        * Gets the width of the Chunk in points.
        * @return a width in points
        */
        virtual public float GetWidthPoint() {
            if (GetImage() != null) {
                return GetImage().ScaledWidth;
            }
            return font.GetCalculatedBaseFont(true).GetWidthPoint(Content, font.CalculatedSize) * HorizontalScaling;
        }
    
        /// <summary>
        /// Checks the attributes of this Chunk.
        /// </summary>
        /// <returns>false if there aren't any.</returns>
        virtual public bool HasAttributes() {
            return attributes != null && attributes.Count > 0;
        }

        /**
         * Checks  the accessible attributes of this <CODE>Chunk</CODE>.
         *
         * @return false if there aren't any.
         */
        public bool HasAccessibleAttributes() {
            return accessibleAttributes != null && accessibleAttributes.Count > 0;
        }

        /// <summary>
        /// Sets/Gets the attributes for this Chunk.
        /// </summary>
        /// <remarks>
        /// It may be null.
        /// </remarks>
        /// <value>a Hashtable</value>
        virtual public Dictionary<string,object> Attributes {
            get {
                return attributes;
            }
            set {
                attributes = value;
            }
        }

        /// <summary>
        /// Sets an arbitrary attribute.
        /// </summary>
        /// <param name="name">the key for the attribute</param>
        /// <param name="obj">the value of the attribute</param>
        /// <returns>this Chunk</returns>
        private Chunk SetAttribute(string name, Object obj) {
            if (attributes == null)
                attributes = new Dictionary<string,object>();
            attributes[name] = obj;
            return this;
        }

        /** Key for text horizontal scaling. */
        public const string HSCALE = "HSCALE";

        /**
        * Sets the text horizontal scaling. A value of 1 is normal and a value of 0.5f
        * shrinks the text to half it's width.
        * @param scale the horizontal scaling factor
        * @return this <CODE>Chunk</CODE>
        */    
        virtual public Chunk SetHorizontalScaling(float scale) {
            return SetAttribute(HSCALE, scale);
        }
        
        /**
        * Gets the horizontal scaling.
        * @return a percentage in float
        */
        virtual public float HorizontalScaling {
            get {
                if (attributes != null && attributes.ContainsKey(HSCALE))
                    return (float)attributes[HSCALE];
                else
                    return 1f;
            }
        }
        
        ///<summary> Key for underline. </summary>
        public const string UNDERLINE = "UNDERLINE";

        /**
        * Sets an horizontal line that can be an underline or a strikethrough.
        * Actually, the line can be anywhere vertically and has always the
        * <CODE>Chunk</CODE> width. Multiple call to this method will
        * produce multiple lines.
        * @param thickness the absolute thickness of the line
        * @param yPosition the absolute y position relative to the baseline
        * @return this <CODE>Chunk</CODE>
        */    
        virtual public Chunk SetUnderline(float thickness, float yPosition) {
            return SetUnderline(null, thickness, 0f, yPosition, 0f, PdfContentByte.LINE_CAP_BUTT);
        }

        /**
        * Sets an horizontal line that can be an underline or a strikethrough.
        * Actually, the line can be anywhere vertically and has always the
        * <CODE>Chunk</CODE> width. Multiple call to this method will
        * produce multiple lines.
        * @param color the color of the line or <CODE>null</CODE> to follow
        * the text color
        * @param thickness the absolute thickness of the line
        * @param thicknessMul the thickness multiplication factor with the font size
        * @param yPosition the absolute y position relative to the baseline
        * @param yPositionMul the position multiplication factor with the font size
        * @param cap the end line cap. Allowed values are
        * PdfContentByte.LINE_CAP_BUTT, PdfContentByte.LINE_CAP_ROUND and
        * PdfContentByte.LINE_CAP_PROJECTING_SQUARE
        * @return this <CODE>Chunk</CODE>
        */    
        virtual public Chunk SetUnderline(BaseColor color, float thickness, float thicknessMul, float yPosition, float yPositionMul, int cap) {
            if (attributes == null)
                attributes = new Dictionary<string,object>();
            Object[] obj = {color, new float[]{thickness, thicknessMul, yPosition, yPositionMul, (float)cap}};
            Object[][] old = null;
            if (attributes.ContainsKey(UNDERLINE))
                old = (Object[][])attributes[UNDERLINE];
            Object[][] unders = Utilities.AddToArray(old, obj);
            return SetAttribute(UNDERLINE, unders);
        }
        
        ///<summary> Key for sub/basescript. </summary>
        public const string SUBSUPSCRIPT = "SUBSUPSCRIPT";

        /// <summary>
        /// Sets the text displacement relative to the baseline. Positive values rise the text,
        /// negative values lower the text.
        /// </summary>
        /// <remarks>
        /// It can be used to implement sub/basescript.
        /// </remarks>
        /// <param name="rise">the displacement in points</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetTextRise(float rise) {
            return SetAttribute(SUBSUPSCRIPT, rise);
        }

        virtual public float GetTextRise() {
            if (attributes != null && attributes.ContainsKey(SUBSUPSCRIPT)) {
                return (float)attributes[SUBSUPSCRIPT];
            }
            return 0.0f;
        }

        ///<summary> Key for text skewing. </summary>
        public const string SKEW = "SKEW";

        /**
        * Skews the text to simulate italic and other effects.
        * Try <CODE>alpha=0</CODE> and <CODE>beta=12</CODE>.
        * @param alpha the first angle in degrees
        * @param beta the second angle in degrees
        * @return this <CODE>Chunk</CODE>
        */    
        virtual public Chunk SetSkew(float alpha, float beta) {
            alpha = (float)Math.Tan(alpha * Math.PI / 180);
            beta = (float)Math.Tan(beta * Math.PI / 180);
            return SetAttribute(SKEW, new float[]{alpha, beta});
        }

        ///<summary> Key for background. </summary>
        public const string BACKGROUND = "BACKGROUND";

        /// <summary>
        /// Sets the color of the background Chunk.
        /// </summary>
        /// <param name="color">the color of the background</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetBackground(BaseColor color) {
            return SetBackground(color, 0, 0, 0, 0);
        }

        /** Sets the color and the size of the background <CODE>Chunk</CODE>.
        * @param color the color of the background
        * @param extraLeft increase the size of the rectangle in the left
        * @param extraBottom increase the size of the rectangle in the bottom
        * @param extraRight increase the size of the rectangle in the right
        * @param extraTop increase the size of the rectangle in the top
        * @return this <CODE>Chunk</CODE>
        */
        virtual public Chunk SetBackground(BaseColor color, float extraLeft, float extraBottom, float extraRight, float extraTop) {
            return SetAttribute(BACKGROUND, new Object[]{color, new float[]{extraLeft, extraBottom, extraRight, extraTop}});
        }

        ///<summary> Key for text rendering mode.</summary>
        public const string TEXTRENDERMODE = "TEXTRENDERMODE";

        /** Sets the text rendering mode. It can outline text, simulate bold and make
        * text invisible.
        * @param mode the text rendering mode. It can be <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL</CODE>,
        * <CODE>PdfContentByte.TEXT_RENDER_MODE_STROKE</CODE>, <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE</CODE>
        * and <CODE>PdfContentByte.TEXT_RENDER_MODE_INVISIBLE</CODE>.
        * @param strokeWidth the stroke line width for the modes <CODE>PdfContentByte.TEXT_RENDER_MODE_STROKE</CODE> and
        * <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE</CODE>.
        * @param strokeColor the stroke color or <CODE>null</CODE> to follow the text color
        * @return this <CODE>Chunk</CODE>
        */    
        virtual public Chunk SetTextRenderMode(int mode, float strokeWidth, BaseColor strokeColor) {
            return SetAttribute(TEXTRENDERMODE, new Object[]{mode, strokeWidth, strokeColor});
        }

        ///<summary> Key for split character. </summary>
        public const string SPLITCHARACTER = "SPLITCHARACTER";

        /// <summary>
        /// Sets the split characters.
        /// </summary>
        /// <param name="splitCharacter">the SplitCharacter interface</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetSplitCharacter(ISplitCharacter splitCharacter) {
            return SetAttribute(SPLITCHARACTER, splitCharacter);
        }

        ///<summary> Key for hyphenation. </summary>
        public const string HYPHENATION = "HYPHENATION";

        /// <summary>
        /// sets the hyphenation engine to this Chunk.
        /// </summary>
        /// <param name="hyphenation">the hyphenation engine</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetHyphenation(IHyphenationEvent hyphenation) {
            return SetAttribute(HYPHENATION, hyphenation);
        }

        ///<summary> Key for remote goto. </summary>
        public const string REMOTEGOTO = "REMOTEGOTO";

        /// <summary>
        /// Sets a goto for a remote destination for this Chunk.
        /// </summary>
        /// <param name="filename">the file name of the destination document</param>
        /// <param name="name">the name of the destination to go to</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetRemoteGoto(string filename, string name) {
            return SetAttribute(REMOTEGOTO, new Object[]{filename, name});
        }

        /// <summary>
        /// Sets a goto for a remote destination for this Chunk.
        /// </summary>
        /// <param name="filename">the file name of the destination document</param>
        /// <param name="page">the page of the destination to go to. First page is 1</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetRemoteGoto(string filename, int page) {
            return SetAttribute(REMOTEGOTO, new Object[]{filename, page});
        }

        ///<summary> Key for local goto. </summary>
        public const string LOCALGOTO = "LOCALGOTO";

        /// <summary>
        /// Sets a local goto for this Chunk.
        /// </summary>
        /// <remarks>
        /// There must be a local destination matching the name.
        /// </remarks>
        /// <param name="name">the name of the destination to go to</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetLocalGoto(string name) {
            return SetAttribute(LOCALGOTO, name);
        }

        ///<summary> Key for local destination. </summary>
        public const string LOCALDESTINATION = "LOCALDESTINATION";

        /// <summary>
        /// Sets a local destination for this Chunk.
        /// </summary>
        /// <param name="name">the name for this destination</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetLocalDestination(string name) {
            return SetAttribute(LOCALDESTINATION, name);
        }

        ///<summary> Key for generic tag. </summary>
        public const string GENERICTAG = "GENERICTAG";

        /// <summary>
        /// Sets the generic tag Chunk.
        /// </summary>
        /// <remarks>
        /// The text for this tag can be retrieved with PdfPageEvent.
        /// </remarks>
        /// <param name="text">the text for the tag</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetGenericTag(string text) {
            return SetAttribute(GENERICTAG, text);
        }

        /** Key for line-height (alternative for leading in Phrase). */
	    public const string LINEHEIGHT = "LINEHEIGHT";

	    /**
	     * Sets a line height tag.
	     *
	     * @return this <CODE>Chunk</CODE>
	     */

	    virtual public Chunk setLineHeight(float lineheight) {
		    return SetAttribute(LINEHEIGHT, lineheight);
	    }

        ///<summary> Key for image. </summary>
        public const string IMAGE = "IMAGE";

        /// <summary>
        /// Returns the image.
        /// </summary>
        /// <value>an Image</value>
        virtual public Image GetImage() {
            if (attributes != null && attributes.ContainsKey(IMAGE)) 
                return (Image)((Object[])attributes[Chunk.IMAGE])[0];
            else
                return null;
        }

        ///<summary> Key for Action. </summary>
        public const string ACTION = "ACTION";

        /// <summary>
        /// Sets an action for this Chunk.
        /// </summary>
        /// <param name="action">the action</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetAction(PdfAction action) {
            Role = PdfName.LINK;
            return SetAttribute(ACTION, action);
        }

        /// <summary>
        /// Sets an anchor for this Chunk.
        /// </summary>
        /// <param name="url">the Uri to link to</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetAnchor(Uri url) {
            Role = PdfName.LINK;
            String urlStr = url.AbsoluteUri;
            SetAccessibleAttribute(PdfName.ALT, new PdfString(urlStr));
            return SetAttribute(ACTION, new PdfAction(urlStr));
        }

        /// <summary>
        /// Sets an anchor for this Chunk.
        /// </summary>
        /// <param name="url">the url to link to</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetAnchor(string url) {
            Role = PdfName.LINK;
            SetAccessibleAttribute(PdfName.ALT, new PdfString(url));
            return SetAttribute(ACTION, new PdfAction(url));
        }

        ///<summary> Key for newpage. </summary>
        public const string NEWPAGE = "NEWPAGE";

        /// <summary>
        /// Sets a new page tag.
        /// </summary>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetNewPage() {
            return SetAttribute(NEWPAGE, null);
        }

        ///<summary> Key for annotation. </summary>
        public const string PDFANNOTATION = "PDFANNOTATION";

        /// <summary>
        /// Sets a generic annotation to this Chunk.
        /// </summary>
        /// <param name="annotation">the annotation</param>
        /// <returns>this Chunk</returns>
        virtual public Chunk SetAnnotation(PdfAnnotation annotation) {
            return SetAttribute(PDFANNOTATION, annotation);
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

        /**
        * Returns the hyphenation (if present).
        * @param    hyphenation a HyphenationEvent instance
        * @since    2.1.2
        */
        virtual public IHyphenationEvent GetHyphenation() {
            if (attributes != null && attributes.ContainsKey(HYPHENATION))
                return (IHyphenationEvent)attributes[HYPHENATION];
            else
                return null;
        }

        // keys used in PdfChunk

        ///<summary> Key for color. </summary>
        public const string COLOR = "COLOR";

        ///<summary> Key for encoding. </summary>
        public const string ENCODING = "ENCODING";

	    /**
	    * Key for character spacing.
	    */
	    public const string CHAR_SPACING = "CHAR_SPACING";

	    /**
	    * Sets the character spacing.
	    * 
	    * @param charSpace the character spacing value
	    * @return this <CODE>Chunk</CODE>
	    */
	    virtual public Chunk SetCharacterSpacing(float charSpace) {
		    return SetAttribute(CHAR_SPACING, charSpace);
	    }
    	
	    /**
	    * Gets the character spacing.
	    * 
	    * @return a value in float
	    */
	    virtual public float GetCharacterSpacing() {
		    if (attributes != null && attributes.ContainsKey(CHAR_SPACING))
			    return (float)attributes[CHAR_SPACING];
            else
		        return 0f;
	    }
        
      	/**
	     *  Key for word spacing.
	     */
	    public const string WORD_SPACING = "WORD_SPACING";

	    /**
	     * Sets the word spacing.
	     *
	     * @param wordSpace the word spacing value
	     * @return this <CODE>Chunk</CODE>
	     */	
	    virtual public Chunk SetWordSpacing(float wordSpace) {
		    return SetAttribute(WORD_SPACING, wordSpace);
	    }
	
	    /**
	     * Gets the word spacing.
	     *
	     * @return a value in float
	     */	
	    virtual public float GetWordSpacing() {
		    if (attributes != null && attributes.ContainsKey(WORD_SPACING)) {
			    float f = (float)attributes[WORD_SPACING];
			    return f;
		    }
		    return 0.0f;		
	    }



        public const string WHITESPACE = "WHITESPACE";

        public static Chunk CreateWhitespace(String content) {
            return CreateWhitespace(content, false);
        }

        public static Chunk CreateWhitespace(String content, bool preserve) {
            Chunk whitespace = null;
            if (!preserve) {
                whitespace = new Chunk(' ');
                whitespace.SetAttribute(WHITESPACE, content);
            } else {
                 whitespace = new Chunk(content);
            }

            return whitespace;
        }

        virtual public bool IsWhitespace() {
            return attributes != null && attributes.ContainsKey(WHITESPACE);
        }

        [Obsolete]
        public static Chunk CreateTabspace() {
            return CreateTabspace(60);
        }

        [Obsolete]
        public static Chunk CreateTabspace(float spacing)
        {
            Chunk tabspace = new Chunk(spacing, true);
            return tabspace;
        }

        [Obsolete]
        virtual public bool IsTabspace()
        {
            return attributes != null && attributes.ContainsKey(TAB);
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (GetImage() != null) {
                return GetImage().GetAccessibleAttribute(key);
            } else if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            }
            else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (GetImage() != null) {
                GetImage().SetAccessibleAttribute(key, value);
            } else {
                if (accessibleAttributes == null)
                    accessibleAttributes = new Dictionary<PdfName, PdfObject>();
                accessibleAttributes[key] = value;
            }
        }

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            if (GetImage() != null)
                return GetImage().GetAccessibleAttributes();
            else
                return accessibleAttributes;
        }

        virtual public PdfName Role{
            get
            {
                if (GetImage() != null)
                    return GetImage().Role;
                else
                    return role;
            }
            set
            {
                if (GetImage() != null)
                    GetImage().Role = value;
                else
                    role = value;
            }
        }

        virtual public AccessibleElementId ID {
            get
            {
                if (id == null)
                    id = new AccessibleElementId();
                return id;
            }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return true; }
        }

        virtual public String GetTextExpansion() {
            PdfObject o = GetAccessibleAttribute(PdfName.E);
            if (o is PdfString)
                return ((PdfString)o).ToUnicodeString();
            return null;
        }

        /**
         * Sets the textual expansion of the abbreviation or acronym.
         * It is highly recommend to set textuual expansion when generating PDF/UA documents.
         * @param value
         */
        virtual public void SetTextExpansion(String value) {
            SetAccessibleAttribute(PdfName.E, new PdfString(value));
        }
    }
}
