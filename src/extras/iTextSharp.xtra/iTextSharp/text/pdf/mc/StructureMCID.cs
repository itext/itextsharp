using System;

namespace iTextSharp.text.pdf.mc
{
    /**
     * Object that stores an item that is part of the document structure.
     * It can refer to a marked-content sequence in a page or an object
     * reference (in this case the subclass StructureObject is used).
     */
    public class StructureMCID : StructureItem {
        /** The mcid of the structure element. */
        protected int mcid = -1;

        /**
         * Creates a StructureMCID using an MCID.
         * @param obj	an MCID
         */
        public StructureMCID(PdfIndirectReference pg, PdfNumber mcid) {
            this.pageref = pg.Number;
            this.mcid = mcid.IntValue;
        }

        /**
         * Creates a StructurItem using an MCR dictionary.
         * @param dict	an MCR dictionary
         */
        public StructureMCID(PdfDictionary mcr)
        {
            mcid = mcr.GetAsNumber(PdfName.MCID).IntValue;
            pageref = mcr.GetAsIndirectObject(PdfName.PG).Number;
        }

        /**
         * Checks if the MCID in this object corresponds with the stored number
         * @param mcid the MCID
         * @return 1 in case the MCIDs corresponds with obj,
         *         -1 in case the MCID doesn't correspond
         */
        public override int CheckMCID(int pg, int mcid) {
            if (pageref == -1)
                throw new InvalidOperationException();
            if (pg == pageref && this.mcid == mcid)
                return 1;
            return -1;
        }

        /**
         * Creates a String representation of the object.
         * @see java.lang.Object#toString()
         */
        public override String ToString() {
            return "MCID " + mcid + " on page with reference " + pageref;
        }
    }
}
