using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

/*
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

namespace iTextSharp.text.pdf {
/** This class takes any PDF and returns exactly the same but
 * encrypted. All the content, links, outlines, etc, are kept.
 * It is also possible to change the info dictionary.
 */
public sealed class PdfEncryptor {
    
    private PdfEncryptor(){
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * @param strength128Bits <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
     * @throws DocumentException on error
     * @throws IOException on error */
    public static void Encrypt(PdfReader reader, Stream os, byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits);
        stamper.Close();
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * @param strength128Bits <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
     * @param newInfo an optional <CODE>String</CODE> map to add or change
     * the info dictionary. Entries with <CODE>null</CODE>
     * values delete the key in the original info dictionary
     * @throws DocumentException on error
     * @throws IOException on error
     */
    public static void Encrypt(PdfReader reader, Stream os, byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits, Dictionary<string,string> newInfo) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits);
        stamper.MoreInfo = newInfo;
        stamper.Close();
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param strength <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * @throws DocumentException on error
     * @throws IOException on error */
    public static void Encrypt(PdfReader reader, Stream os, bool strength, String userPassword, String ownerPassword, int permissions) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(strength, userPassword, ownerPassword, permissions);
        stamper.Close();
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param strength <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * @param newInfo an optional <CODE>String</CODE> map to add or change
     * the info dictionary. Entries with <CODE>null</CODE>
     * values delete the key in the original info dictionary
     * @throws DocumentException on error
     * @throws IOException on error
     */
    public static void Encrypt(PdfReader reader, Stream os, bool strength, String userPassword, String ownerPassword, int permissions, Dictionary<string,string> newInfo) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(strength, userPassword, ownerPassword, permissions);
        stamper.MoreInfo = newInfo;
        stamper.Close();
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param type the type of encryption. It can be one of STANDARD_ENCRYPTION_40, STANDARD_ENCRYPTION_128 or ENCRYPTION_AES128.
     * Optionally DO_NOT_ENCRYPT_METADATA can be ored to output the metadata in cleartext
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * @param newInfo an optional <CODE>String</CODE> map to add or change
     * the info dictionary. Entries with <CODE>null</CODE>
     * values delete the key in the original info dictionary
     * @throws DocumentException on error
     * @throws IOException on error
     */
    public static void Encrypt(PdfReader reader, Stream os, int type, String userPassword, String ownerPassword, int permissions, Dictionary<string,string> newInfo) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(type, userPassword, ownerPassword, permissions);
        stamper.MoreInfo = newInfo;
        stamper.Close();
    }
    
    /** Entry point to encrypt a PDF document. The encryption parameters are the same as in
     * <code>PdfWriter</code>. The userPassword and the
     *  ownerPassword can be null or have zero length. In this case the ownerPassword
     *  is replaced by a random string. The open permissions for the document can be
     *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
     *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
     *  The permissions can be combined by ORing them.
     * @param reader the read PDF
     * @param os the output destination
     * @param type the type of encryption. It can be one of STANDARD_ENCRYPTION_40, STANDARD_ENCRYPTION_128 or ENCRYPTION_AES128.
     * Optionally DO_NOT_ENCRYPT_METADATA can be ored to output the metadata in cleartext
     * @param userPassword the user password. Can be null or empty
     * @param ownerPassword the owner password. Can be null or empty
     * @param permissions the user permissions
     * values delete the key in the original info dictionary
     * @throws DocumentException on error
     * @throws IOException on error
     */
    public static void Encrypt(PdfReader reader, Stream os, int type, String userPassword, String ownerPassword, int permissions) {
        PdfStamper stamper = new PdfStamper(reader, os);
        stamper.SetEncryption(type, userPassword, ownerPassword, permissions);
        stamper.Close();
    }

    /**
     * Give you a verbose analysis of the permissions.
     * @param permissions the permissions value of a PDF file
     * @return a String that explains the meaning of the permissions value
     */
    public static String GetPermissionsVerbose(int permissions) {
        StringBuilder buf = new StringBuilder("Allowed:");
        if ((PdfWriter.ALLOW_PRINTING & permissions) == PdfWriter.ALLOW_PRINTING) buf.Append(" Printing");
        if ((PdfWriter.ALLOW_MODIFY_CONTENTS & permissions) == PdfWriter.ALLOW_MODIFY_CONTENTS) buf.Append(" Modify contents");
        if ((PdfWriter.ALLOW_COPY & permissions) == PdfWriter.ALLOW_COPY) buf.Append(" Copy");
        if ((PdfWriter.ALLOW_MODIFY_ANNOTATIONS & permissions) == PdfWriter.ALLOW_MODIFY_ANNOTATIONS) buf.Append(" Modify annotations");
        if ((PdfWriter.ALLOW_FILL_IN & permissions) == PdfWriter.ALLOW_FILL_IN) buf.Append(" Fill in");
        if ((PdfWriter.ALLOW_SCREENREADERS & permissions) == PdfWriter.ALLOW_SCREENREADERS) buf.Append(" Screen readers");
        if ((PdfWriter.ALLOW_ASSEMBLY & permissions) == PdfWriter.ALLOW_ASSEMBLY) buf.Append(" Assembly");
        if ((PdfWriter.ALLOW_DEGRADED_PRINTING & permissions) == PdfWriter.ALLOW_DEGRADED_PRINTING) buf.Append(" Degraded printing");
        return buf.ToString();
    }
    
    /**
     * Tells you if printing is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if printing is allowed
     *
     * @since 2.0.7
     */
    public static bool IsPrintingAllowed(int permissions) {
        return (PdfWriter.ALLOW_PRINTING & permissions) == PdfWriter.ALLOW_PRINTING;
    }
    
    /**
     * Tells you if modifying content is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if modifying content is allowed
     *
     * @since 2.0.7
     */
    public static bool IsModifyContentsAllowed(int permissions) {
        return (PdfWriter.ALLOW_MODIFY_CONTENTS & permissions) == PdfWriter.ALLOW_MODIFY_CONTENTS;
    } 
    
    /**
     * Tells you if copying is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if copying is allowed
     *
     * @since 2.0.7
     */
    public static bool IsCopyAllowed(int permissions) {
        return (PdfWriter.ALLOW_COPY & permissions) == PdfWriter.ALLOW_COPY;
    }
    
    /**
     * Tells you if modifying annotations is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if modifying annotations is allowed
     *
     * @since 2.0.7
     */
    public static bool IsModifyAnnotationsAllowed(int permissions) {
        return (PdfWriter.ALLOW_MODIFY_ANNOTATIONS & permissions) == PdfWriter.ALLOW_MODIFY_ANNOTATIONS;
    }
    
    /**
     * Tells you if filling in fields is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if filling in fields is allowed
     *
     * @since 2.0.7
     */
    public static bool IsFillInAllowed(int permissions) {
        return (PdfWriter.ALLOW_FILL_IN & permissions) == PdfWriter.ALLOW_FILL_IN;
    }
    
    /**
     * Tells you if repurposing for screenreaders is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if repurposing for screenreaders is allowed
     *
     * @since 2.0.7
     */
    public static bool IsScreenReadersAllowed(int permissions) {
        return (PdfWriter.ALLOW_SCREENREADERS & permissions) == PdfWriter.ALLOW_SCREENREADERS;
    }
    
    /**
     * Tells you if document assembly is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if document assembly is allowed
     *
     * @since 2.0.7
     */
    public static bool IsAssemblyAllowed(int permissions) {
        return (PdfWriter.ALLOW_ASSEMBLY & permissions) == PdfWriter.ALLOW_ASSEMBLY;
    }
    
    /**
     * Tells you if degraded printing is allowed.
     * @param permissions the permissions value of a PDF file
     * @return  true if degraded printing is allowed
     *
     * @since 2.0.7
     */
    public static bool IsDegradedPrintingAllowed(int permissions) {
        return (PdfWriter.ALLOW_DEGRADED_PRINTING & permissions) == PdfWriter.ALLOW_DEGRADED_PRINTING;
    }
}
}
