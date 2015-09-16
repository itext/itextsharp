using System.IO;
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
using System.Text;

namespace iTextSharp.text.pdf
{
    /**
     * <CODE>PdfIndirectObject</CODE> is the Pdf indirect object.
     * <P>
     * An <I>indirect object</I> is an object that has been labeled so that it can be referenced by
     * other objects. Any type of <CODE>PdfObject</CODE> may be labeled as an indirect object.<BR>
     * An indirect object consists of an object identifier, a direct object, and the <B>endobj</B>
     * keyword. The <I>object identifier</I> consists of an integer <I>object number</I>, an integer
     * <I>generation number</I>, and the <B>obj</B> keyword.<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 4.10 (page 53).
     *
     * @see        PdfObject
     * @see        PdfIndirectReference
     */
    public class PdfIndirectObject
    {

        // membervariables

        /** The object number */
        protected int number;

        /** the generation number */
        protected int generation = 0;

        internal static byte[] STARTOBJ = DocWriter.GetISOBytes(" obj\n");
        internal static byte[] ENDOBJ = DocWriter.GetISOBytes("\nendobj\n");
        internal static int SIZEOBJ = STARTOBJ.Length + ENDOBJ.Length;
        protected internal PdfObject objecti;
        protected internal PdfWriter writer;

        // constructors

        /**
         * Constructs a <CODE>PdfIndirectObject</CODE>.
         *
         * @param        number            the objecti number
         * @param        objecti            the direct objecti
         */
        public PdfIndirectObject(int number, PdfObject objecti, PdfWriter writer)
            : this(number, 0, objecti, writer)
        {
        }

        public PdfIndirectObject(PdfIndirectReference refi, PdfObject objecti, PdfWriter writer)
            : this(refi.Number, refi.Generation, objecti, writer)
        {
        }

        /**
         * Constructs a <CODE>PdfIndirectObject</CODE>.
         *
         * @param        number            the objecti number
         * @param        generation        the generation number
         * @param        objecti            the direct objecti
         */
        public PdfIndirectObject(int number, int generation, PdfObject objecti, PdfWriter writer)
        {
            this.writer = writer;
            this.number = number;
            this.generation = generation;
            this.objecti = objecti;
            PdfEncryption crypto = null;
            if (writer != null)
                crypto = writer.Encryption;
            if (crypto != null)
                crypto.SetHashKey(number, generation);
        }

        virtual public int Number
        {
            get { return number; }
        }

        virtual public int Generation
        {
            get { return generation; }
        }


        // methods

        /**
         * Returns a <CODE>PdfIndirectReference</CODE> to this <CODE>PdfIndirectObject</CODE>.
         *
         * @return        a <CODE>PdfIndirectReference</CODE>
         */
        public virtual PdfIndirectReference IndirectReference
        {
            get { return new PdfIndirectReference(objecti.Type, number, generation); }
        }

        /**
         * Writes eficiently to a stream
         *
         * @param os the stream to write to
         * @throws IOException on write error
         */
        virtual public void WriteTo(Stream os)
        {
            byte[] tmp = DocWriter.GetISOBytes(number.ToString());
            os.Write(tmp, 0, tmp.Length);
            os.WriteByte((byte)' ');
            tmp = DocWriter.GetISOBytes(generation.ToString());
            os.Write(tmp, 0, tmp.Length);
            os.Write(STARTOBJ, 0, STARTOBJ.Length);
            objecti.ToPdf(writer, os);
            os.Write(ENDOBJ, 0, ENDOBJ.Length);
        }

        public override string ToString() {
            return new StringBuilder().Append(number).Append(' ').Append(generation).Append(" R: ").Append(objecti != null ? objecti.ToString(): "null").ToString();
        }
    }
}
