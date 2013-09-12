using System;

namespace iTextSharp.text.pdf.mc
{
    public class StructureObject : StructureItem
    {
        /** The structure element of which the properties are stored. */
        protected PdfDictionary structElem;
        /** The reference of the structure element of which the properties are stored. */
        protected PdfIndirectReference refa;
        /** An object referred to by an OBJR dictionary. */
        protected PdfObject obj;
        /** The reference to the object. */
        protected PdfIndirectReference objref;
        /** The StructParent value of the object. */
        protected int structParent;

        /**
         * Creates a StructureObject for an OBJR dictionary.
         * @param structElem	the parent structure element
         * @param ref			the reference of the parent structure element
         * @param dict			the object reference dictionary
         */
        public StructureObject(PdfDictionary structElem, PdfIndirectReference refa, PdfDictionary dict) {
            this.structElem = structElem;
            this.refa = refa;
            this.obj = dict.GetDirectObject(PdfName.OBJ);
            this.objref = dict.GetAsIndirectObject(PdfName.OBJ);
            this.structParent = ((PdfDictionary) obj).GetAsNumber(PdfName.STRUCTPARENT).IntValue;
            PdfIndirectReference pg = dict.GetAsIndirectObject(PdfName.PG);
            if (pg == null)
                pg = structElem.GetAsIndirectObject(PdfName.PG);
            this.pageref = pg.Number;
        }

        /**
         * Returns the structure element.
         * @return a StructElem dictionary
         */
        public PdfDictionary GetStructElem() {
            return structElem;
        }

        /**
         * Returns the structure element's reference.
         * @return an indirect reference to a StructElem dictionary
         */
        public PdfIndirectReference GetRef() {
            return refa;
        }

        /**
         * Returns the object referred to by the OBJR dictionary.
         * Note that this method returns a dictionary which means
         * that in case the object is a stream, only the stream
         * dictionary will be returned.
         * @return the object referred to by OBJR as a dictionary
         */
        public PdfDictionary GetObjAsDict() {
            if (obj.IsDictionary())
                return (PdfDictionary) obj;
            return null;
        }

        /**
         * Returns the reference to the object.
         * @return	an object reference
         */
        public PdfIndirectReference GetObjRef() {
            return objref;
        }

        /**
         * Checks if a StructParent corresponds with the StructParent stored in the object.
         * @return  1 in case the StructParent matches,
         *         -1 in case there's no match.
         */
        public override int CheckStructParent(int pg, int sp) {
            if (pg == pageref && sp == structParent)
                return 1;
            return -1;
        }

        /**
         * Creates a String representation of the object.
         * @see java.lang.Object#toString()
         */
        public override String ToString() {
            return "StructParent " + structParent + " of object " + obj + " on page with reference " + pageref;
        }
    }
}