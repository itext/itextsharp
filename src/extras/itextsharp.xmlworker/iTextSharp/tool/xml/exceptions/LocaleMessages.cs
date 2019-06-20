/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Globalization;
using System.Reflection;
using iTextSharp.text.error_messages;
namespace iTextSharp.tool.xml.exceptions {

    /**
     * @author Emiel Ackermann
     *
     */
    public class LocaleMessages {

	    public const String UNSUPPORTED_CHARSET = "unsupported.charset";
	    public const String INVALID_NESTED_TAG = "tag.invalidnesting";
	    public const String NO_CUSTOM_CONTEXT = "customcontext.404";
	    public const String CUSTOMCONTEXT_404_CONTINUE = "customcontext.404.continue";
	    public const String UNSUPPORTED_CLONING = "unsupported.clone";
	    public const String NO_TAGPROCESSOR = "tag.noprocessor";
	    public const String NO_SIBLING = "tag.nosibling";
	    public const String PIPELINE_AUTODOC = "pipeline.autodoc.missingdep";
	    public const String STACK_404 = "pipeline.html.missingstack";
	    public const String OWN_CONTEXT_404 = "pipeline.owncontextmissing";
	    public const String ELEMENT_NOT_ADDED = "pipeline.pdfwriter.elemnotadded";
	    public const String ELEMENT_NOT_ADDED_EXC = "pipeline.pdfwriter.elemnotaddedexc";
	    public const String IMG_SRC_NOTCONVERTED = "exc.img.notconverted";
	    public const String HTML_IMG_USE = "html.tag.img.try";
	    public const String HTML_IMG_RETRIEVE_FAIL = "html.tag.img.failedretrieve";
	    public const String ADD_HEADER = "html.tag.h.create";
	    public const String HEADER_BM_DISABLED = "html.tag.h.disabled";
	    public const String A_LOCALGOTO = "html.tag.a.local";
	    public const String A_EXTERNAL = "html.tag.a.external";
	    public const String A_SETLOCALGOTO = "html.tag.a.setlocal";
	    public const String SPACEHACK = "html.tag.a.spacehack";
	    public const String COLSPAN = "html.tag.table.colspan";
	    public const String LINK_404 = "html.tag.link.404";
	    public const String META_CC = "html.tag.meta.cc";
	    public const String META_404 = "html.tag.meta.404";
	    public const String STYLE_NOTPARSED = "html.tag.style.notparsed";

	    private static LocaleMessages myself = new LocaleMessages();
	    private LocalizedResource bundle;

	    /**
	     * Returns the {@link LocaleMessages} with as Locale the default jvm locale.
	     * @return a singleton instance of LocaleMessages
	     */
	    public static LocaleMessages GetInstance() {
		    return myself;
	    }

	    /**
	     *
	     */
	    public LocaleMessages() {
		    bundle = new LocalizedResource("iTextSharp.errors.errors", CultureInfo.CurrentCulture, Assembly.GetExecutingAssembly());
	    }
	    /**
	     * @param locale the {@link Locale} to use.
	     *
	     */
	    public LocaleMessages(CultureInfo locale) {
		    bundle = new LocalizedResource("iTextSharp.errors.errors", locale, Assembly.GetExecutingAssembly());
	    }

	    /**
	     * Fetches the message belonging to the key.
	     * @param key the key for the message
	     * @return the message
	     */
	    virtual public String GetMessage(String key) {
		    return bundle.GetMessage(key);
	    }
    }
}
