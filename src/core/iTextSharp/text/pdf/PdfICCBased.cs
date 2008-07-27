using System;

namespace iTextSharp.text.pdf {

    /**
     * A <CODE>PdfICCBased</CODE> defines a ColorSpace
     *
     * @see        PdfStream
     */

    public class PdfICCBased : PdfStream {
    
        /**
        * Creates an ICC stream.
        * @param   profile an ICC profile
        */
        public PdfICCBased(ICC_Profile profile) : this(profile, DEFAULT_COMPRESSION) {
            ;
        }
        
        /**
        * Creates an ICC stream.
        *
        * @param   compressionLevel    the compressionLevel
        *
        * @param   profile an ICC profile
        * @since   2.1.3   (replacing the constructor without param compressionLevel)
        */
        public PdfICCBased(ICC_Profile profile, int compressionLevel) {
            int numberOfComponents = profile.NumComponents;
            switch (numberOfComponents) {
                case 1:
                    Put(PdfName.ALTERNATE, PdfName.DEVICEGRAY);
                    break;
                case 3:
                    Put(PdfName.ALTERNATE, PdfName.DEVICERGB);
                    break;
                case 4:
                    Put(PdfName.ALTERNATE, PdfName.DEVICECMYK);
                    break;
                default:
                    throw new PdfException(numberOfComponents + " Component(s) is not supported in iText");
            }
            Put(PdfName.N, new PdfNumber(numberOfComponents));
            bytes = profile.Data;
            FlateCompress(compressionLevel);
        }
    }
}