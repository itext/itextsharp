using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.xml.simpleparser;
/*
 * $Id$
 * 
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

namespace iTextSharp.text.xml.xmp {

    /**
    * With this class you can create an Xmp Stream that can be used for adding
    * Metadata to a PDF Dictionary. Remark that this class doesn't cover the
    * complete XMP specification. 
    */
    public class XmpWriter {

        /** A possible charset for the XMP. */
        public const String UTF8 = "UTF-8";
        /** A possible charset for the XMP. */
        public const String UTF16 = "UTF-16";
        /** A possible charset for the XMP. */
        public const String UTF16BE = "UTF-16BE";
        /** A possible charset for the XMP. */
        public const String UTF16LE = "UTF-16LE";
        
        /** String used to fill the extra space. */
        public const String EXTRASPACE = "                                                                                                   \n";
        
        /** You can add some extra space in the XMP packet; 1 unit in this variable represents 100 spaces and a newline. */
        protected int extraSpace;
        
        /** The writer to which you can write bytes for the XMP stream. */
        protected StreamWriter writer;
        
        /** The about string that goes into the rdf:Description tags. */
        protected String about;
        
        /**
        * Processing Instruction required at the start of an XMP stream
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_BEGIN = "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n";
        
        /**
        * Processing Instruction required at the end of an XMP stream for XMP streams that can be updated
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_END_W = "<?xpacket end=\"w\"?>";
        
        /**
        * Processing Instruction required at the end of an XMP stream for XMP streams that are read only
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_END_R = "<?xpacket end=\"r\"?>";
	        
        /** The end attribute. */
        protected char end = 'w';
        
        /**
        * Creates an XmpWriter. 
        * @param os
        * @param utfEncoding
        * @param extraSpace
        * @throws IOException
        */
        public XmpWriter(Stream os, string utfEncoding, int extraSpace) {
            this.extraSpace = extraSpace;
            writer = new StreamWriter(os, new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(utfEncoding)));
            writer.Write(XPACKET_PI_BEGIN);
            writer.Write("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\">\n");
            writer.Write("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n");
            about = "";
        }
        
        /**
        * Creates an XmpWriter.
        * @param os
        * @throws IOException
        */
        public XmpWriter(Stream os) : this(os, UTF8, 20) {
        }
        
        /** Sets the XMP to read-only */
        public void SetReadOnly() {
            end = 'r';
        }
        
        /**
        * @param about The about to set.
        */
        public String About {
            set {
                this.about = value;
            }
        }
        
        /**
        * Adds an rdf:Description.
        * @param xmlns
        * @param content
        * @throws IOException
        */
        public void AddRdfDescription(String xmlns, String content) {
            writer.Write("<rdf:Description rdf:about=\"");
            writer.Write(about);
            writer.Write("\" ");
            writer.Write(xmlns);
            writer.Write(">");
            writer.Write(content);
            writer.Write("</rdf:Description>\n");
        }
        
        /**
        * Adds an rdf:Description.
        * @param s
        * @throws IOException
        */
        public void AddRdfDescription(XmpSchema s) {
            writer.Write("<rdf:Description rdf:about=\"");
            writer.Write(about);
            writer.Write("\" ");
            writer.Write(s.Xmlns);
            writer.Write(">");
            writer.Write(s.ToString());
            writer.Write("</rdf:Description>\n");
        }
        
        /**
        * Flushes and closes the XmpWriter.
        * @throws IOException
        */
        public void Close() {
            writer.Write("</rdf:RDF>");
            writer.Write("</x:xmpmeta>\n");
            for (int i = 0; i < extraSpace; i++) {
                writer.Write(EXTRASPACE);
            }
            writer.Write(end == 'r' ? XPACKET_PI_END_R : XPACKET_PI_END_W);
            writer.Flush();
            writer.Close();
        }
        
        /**
        * @param os
        * @param info
        * @throws IOException
        */
        public XmpWriter(Stream os, PdfDictionary info, int PdfXConformance) : this(os,info) {
            if (info != null) {
        	    DublinCoreSchema dc = new DublinCoreSchema();
        	    PdfSchema p = new PdfSchema();
        	    XmpBasicSchema basic = new XmpBasicSchema();
        	    PdfName key;
        	    PdfObject obj;
        	    String value;
        	    foreach (PdfName pdfName in info.Keys) {
        		    key = pdfName;
        		    obj = info.Get(key);
        		    if (obj == null)
        			    continue;
        		    if (!obj.IsString())
        			    continue;
        		    value = ((PdfString)obj).ToUnicodeString();
        		    if (PdfName.TITLE.Equals(key)) {
        			    dc.AddTitle(value);
        		    }
        		    if (PdfName.AUTHOR.Equals(key)) {
        			    dc.AddAuthor(value);
        		    }
        		    if (PdfName.SUBJECT.Equals(key)) {
        			    dc.AddSubject(value);
        			    dc.AddDescription(value);
        		    }
        		    if (PdfName.KEYWORDS.Equals(key)) {
        			    p.AddKeywords(value);
        		    }
        		    if (PdfName.CREATOR.Equals(key)) {
        			    basic.AddCreatorTool(value);
        		    }
        		    if (PdfName.PRODUCER.Equals(key)) {
        			    p.AddProducer(value);
        		    }
        		    if (PdfName.CREATIONDATE.Equals(key)) {
        			    basic.AddCreateDate(PdfDate.GetW3CDate(obj.ToString()));
        		    }
        		    if (PdfName.MODDATE.Equals(key)) {
        			    basic.AddModDate(PdfDate.GetW3CDate(obj.ToString()));
        		    }
        	    }
        	    if (dc.Count > 0) AddRdfDescription(dc);
                if (p.Count > 0) AddRdfDescription(p);
                if (basic.Count > 0) AddRdfDescription(basic);
            }
    }

    /**
     * @param os
     * @param info
     * @throws IOException
     */
    public XmpWriter(Stream os, PdfDictionary info) : this(os) {            
        if (info != null) {
        	DublinCoreSchema dc = new DublinCoreSchema();
        	PdfSchema p = new PdfSchema();
        	XmpBasicSchema basic = new XmpBasicSchema();
        	PdfName key;
        	PdfObject obj;
        	String value;
        	foreach (PdfName pdfName in info.Keys) {
        		key = pdfName;
        		obj = info.Get(key);
        		if (obj == null)
        			continue;
        		if (!obj.IsString())
        			continue;
        		value = ((PdfString)obj).ToUnicodeString();
        		if (PdfName.TITLE.Equals(key)) {
        			dc.AddTitle(value);
        		}
        		if (PdfName.AUTHOR.Equals(key)) {
        			dc.AddAuthor(value);
        		}
        		if (PdfName.SUBJECT.Equals(key)) {
        			dc.AddSubject(value);
        			dc.AddDescription(value);
        		}
        		if (PdfName.KEYWORDS.Equals(key)) {
        			p.AddKeywords(value);
        		}
        		if (PdfName.CREATOR.Equals(key)) {
        			basic.AddCreatorTool(value);
        		}
        		if (PdfName.PRODUCER.Equals(key)) {
        			p.AddProducer(value);
        		}
        		if (PdfName.CREATIONDATE.Equals(key)) {
        			basic.AddCreateDate(PdfDate.GetW3CDate(obj.ToString()));
        		}
        		if (PdfName.MODDATE.Equals(key)) {
        			basic.AddModDate(PdfDate.GetW3CDate(obj.ToString()));
        		}
        	}
        	if (dc.Count > 0) AddRdfDescription(dc);
        	if (p.Count > 0) AddRdfDescription(p);
        	if (basic.Count > 0) AddRdfDescription(basic);
        }
        }
        
        /**
        * @param os
        * @param info
        * @throws IOException
        */
        public XmpWriter(Stream os, IDictionary<string,string> info) : this(os) {
            if (info != null) {
                DublinCoreSchema dc = new DublinCoreSchema();
                PdfSchema p = new PdfSchema();
                XmpBasicSchema basic = new XmpBasicSchema();
                String value;
                foreach (KeyValuePair<string,string> entry in info) {
                    String key = entry.Key;
                    value = entry.Value;
                    if (value == null)
                        continue;
                    if ("Title".Equals(key)) {
                        dc.AddTitle(value);
                    }
                    if ("Author".Equals(key)) {
                        dc.AddAuthor(value);
                    }
                    if ("Subject".Equals(key)) {
                        dc.AddSubject(value);
                        dc.AddDescription(value);
                    }
                    if ("Keywords".Equals(key)) {
                        p.AddKeywords(value);
                    }
                    if ("Creator".Equals(key)) {
                        basic.AddCreatorTool(value);
                    }
                    if ("Producer".Equals(key)) {
                        p.AddProducer(value);
                    }
                    if ("CreationDate".Equals(key)) {
                        basic.AddCreateDate(PdfDate.GetW3CDate(value));
                    }
                    if ("ModDate".Equals(key)) {
                        basic.AddModDate(PdfDate.GetW3CDate(value));
                    }
                }
                if (dc.Count > 0) AddRdfDescription(dc);
                if (p.Count > 0) AddRdfDescription(p);
                if (basic.Count > 0) AddRdfDescription(basic);
            }
        }
    }
}
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.xml.simpleparser;
/*
 * $Id$
 * 
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

namespace iTextSharp.text.xml.xmp {

    /**
    * With this class you can create an Xmp Stream that can be used for adding
    * Metadata to a PDF Dictionary. Remark that this class doesn't cover the
    * complete XMP specification. 
    */
    public class XmpWriter {

        /** A possible charset for the XMP. */
        public const String UTF8 = "UTF-8";
        /** A possible charset for the XMP. */
        public const String UTF16 = "UTF-16";
        /** A possible charset for the XMP. */
        public const String UTF16BE = "UTF-16BE";
        /** A possible charset for the XMP. */
        public const String UTF16LE = "UTF-16LE";
        
        /** String used to fill the extra space. */
        public const String EXTRASPACE = "                                                                                                   \n";
        
        /** You can add some extra space in the XMP packet; 1 unit in this variable represents 100 spaces and a newline. */
        protected int extraSpace;
        
        /** The writer to which you can write bytes for the XMP stream. */
        protected StreamWriter writer;
        
        /** The about string that goes into the rdf:Description tags. */
        protected String about;
        
        /**
        * Processing Instruction required at the start of an XMP stream
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_BEGIN = "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n";
        
        /**
        * Processing Instruction required at the end of an XMP stream for XMP streams that can be updated
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_END_W = "<?xpacket end=\"w\"?>";
        
        /**
        * Processing Instruction required at the end of an XMP stream for XMP streams that are read only
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_END_R = "<?xpacket end=\"r\"?>";
	        
        /** The end attribute. */
        protected char end = 'w';
        
        /**
        * Creates an XmpWriter. 
        * @param os
        * @param utfEncoding
        * @param extraSpace
        * @throws IOException
        */
        public XmpWriter(Stream os, string utfEncoding, int extraSpace) {
            this.extraSpace = extraSpace;
            writer = new StreamWriter(os, new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(utfEncoding)));
            writer.Write(XPACKET_PI_BEGIN);
            writer.Write("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\">\n");
            writer.Write("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n");
            about = "";
        }
        
        /**
        * Creates an XmpWriter.
        * @param os
        * @throws IOException
        */
        public XmpWriter(Stream os) : this(os, UTF8, 20) {
        }
        
        /** Sets the XMP to read-only */
        public void SetReadOnly() {
            end = 'r';
        }
        
        /**
        * @param about The about to set.
        */
        public String About {
            set {
                this.about = value;
            }
        }
        
        /**
        * Adds an rdf:Description.
        * @param xmlns
        * @param content
        * @throws IOException
        */
        public void AddRdfDescription(String xmlns, String content) {
            writer.Write("<rdf:Description rdf:about=\"");
            writer.Write(about);
            writer.Write("\" ");
            writer.Write(xmlns);
            writer.Write(">");
            writer.Write(content);
            writer.Write("</rdf:Description>\n");
        }
        
        /**
        * Adds an rdf:Description.
        * @param s
        * @throws IOException
        */
        public void AddRdfDescription(XmpSchema s) {
            writer.Write("<rdf:Description rdf:about=\"");
            writer.Write(about);
            writer.Write("\" ");
            writer.Write(s.Xmlns);
            writer.Write(">");
            writer.Write(s.ToString());
            writer.Write("</rdf:Description>\n");
        }
        
        /**
        * Flushes and closes the XmpWriter.
        * @throws IOException
        */
        public void Close() {
            writer.Write("</rdf:RDF>");
            writer.Write("</x:xmpmeta>\n");
            for (int i = 0; i < extraSpace; i++) {
                writer.Write(EXTRASPACE);
            }
            writer.Write(end == 'r' ? XPACKET_PI_END_R : XPACKET_PI_END_W);
            writer.Flush();
            writer.Close();
        }
        
        /**
        * @param os
        * @param info
        * @throws IOException
        */
        public XmpWriter(Stream os, PdfDictionary info, int PdfXConformance) : this(os) {
            if (info != null) {
                DublinCoreSchema dc = new DublinCoreSchema();
                PdfSchema p = new PdfSchema();
                XmpBasicSchema basic = new XmpBasicSchema();
                PdfObject obj;
                String value;
                foreach (PdfName key in info.Keys) {
                    obj = info.Get(key);
                    if (obj == null)
                        continue;
                    if (!obj.IsString())
                        continue;
                    value = ((PdfString)obj).ToUnicodeString();
                    if (PdfName.TITLE.Equals(key)) {
                        dc.AddTitle(value);
                    }
                    if (PdfName.AUTHOR.Equals(key)) {
                        dc.AddAuthor(value);
                    }
                    if (PdfName.SUBJECT.Equals(key)) {
                        dc.AddSubject(value);
                        dc.AddDescription(value);
                    }
                    if (PdfName.KEYWORDS.Equals(key)) {
                        p.AddKeywords(value);
                    }
                    if (PdfName.CREATOR.Equals(key)) {
                        basic.AddCreatorTool(value);
                    }
                    if (PdfName.PRODUCER.Equals(key)) {
                        p.AddProducer(value);
                    }
                    if (PdfName.CREATIONDATE.Equals(key)) {
                        basic.AddCreateDate(PdfDate.GetW3CDate(obj.ToString()));
                    }
                    if (PdfName.MODDATE.Equals(key)) {
                        basic.AddModDate(PdfDate.GetW3CDate(obj.ToString()));
                    }
                }
                if (dc.Count > 0) AddRdfDescription(dc);
                if (p.Count > 0) AddRdfDescription(p);
                if (basic.Count > 0) AddRdfDescription(basic);              
             }
        }
        
        
        /**
        * @param os
        * @param info
        * @throws IOException
        */
        public XmpWriter(Stream os, IDictionary<string,string> info) : this(os) {
            if (info != null) {
                DublinCoreSchema dc = new DublinCoreSchema();
                PdfSchema p = new PdfSchema();
                XmpBasicSchema basic = new XmpBasicSchema();
                String value;
                foreach (KeyValuePair<string,string> entry in info) {
                    String key = entry.Key;
                    value = entry.Value;
                    if (value == null)
                        continue;
                    if ("Title".Equals(key)) {
                        dc.AddTitle(value);
                    }
                    if ("Author".Equals(key)) {
                        dc.AddAuthor(value);
                    }
                    if ("Subject".Equals(key)) {
                        dc.AddSubject(value);
                        dc.AddDescription(value);
                    }
                    if ("Keywords".Equals(key)) {
                        p.AddKeywords(value);
                    }
                    if ("Creator".Equals(key)) {
                        basic.AddCreatorTool(value);
                    }
                    if ("Producer".Equals(key)) {
                        p.AddProducer(value);
                    }
                    if ("CreationDate".Equals(key)) {
                        basic.AddCreateDate(PdfDate.GetW3CDate(value));
                    }
                    if ("ModDate".Equals(key)) {
                        basic.AddModDate(PdfDate.GetW3CDate(value));
                    }
                }
                if (dc.Count > 0) AddRdfDescription(dc);
                if (p.Count > 0) AddRdfDescription(p);
                if (basic.Count > 0) AddRdfDescription(basic);
            }
        }

         /**
         * @param os
         * @param info
         * @throws IOException
         */
        public XmpWriter(Stream os, PdfDictionary info) : this(os) {
            
            if (info != null) {
        	    DublinCoreSchema dc = new DublinCoreSchema();
        	    PdfSchema p = new PdfSchema();
        	    XmpBasicSchema basic = new XmpBasicSchema();
        	    PdfName key;
        	    PdfObject obj;
        	    String value;
        	    foreach (PdfName pdfName in info.Keys) {
        		    key = pdfName;
        		    obj = info.Get(key);
        		    if (obj == null)
        			    continue;
        		    if (!obj.IsString())
        			    continue;
        		    value = ((PdfString)obj).ToUnicodeString();
        		    if (PdfName.TITLE.Equals(key)) {
        			    dc.AddTitle(value);
        		    }
        		    if (PdfName.AUTHOR.Equals(key)) {
        			    dc.AddAuthor(value);
        		    }
        		    if (PdfName.SUBJECT.Equals(key)) {
        			    dc.AddSubject(value);
        			    dc.AddDescription(value);
        		    }
        		    if (PdfName.KEYWORDS.Equals(key)) {
        			    p.AddKeywords(value);
        		    }
        		    if (PdfName.CREATOR.Equals(key)) {
        			    basic.AddCreatorTool(value);
        		    }
        		    if (PdfName.PRODUCER.Equals(key)) {
        			    p.AddProducer(value);
        		    }
        		    if (PdfName.CREATIONDATE.Equals(key)) {
        			    basic.AddCreateDate(PdfDate.GetW3CDate(obj.ToString()));
        		    }
        		    if (PdfName.MODDATE.Equals(key)) {
        			    basic.AddModDate(PdfDate.GetW3CDate(obj.ToString()));
        		    }
        	    }
        	    if (dc.Count > 0) AddRdfDescription(dc);
                if (p.Count > 0) AddRdfDescription(p);
                if (basic.Count > 0) AddRdfDescription(basic);
            }
        }
    }
}
