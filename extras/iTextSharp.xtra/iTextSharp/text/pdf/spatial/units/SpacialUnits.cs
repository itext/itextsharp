using System;
using iTextSharp.text.pdf;
/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Bruno Lowagie, Balder Van Camp, et al.
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
namespace iTextSharp.text.pdf.spatial.units {

/**
 * Angular display units for use in a Preferred Display Units (PDU) array.
 * @since 5.1.0
 */
    public enum Angular {
        /** a degree */
        DEGREE,
        /** a grad (1/400 of a circle, or 0.9 degrees) */
        GRAD}

    /**
     * Indicates whether and in what manner to display a fractional value
     * from the result of converting to the units
     * @since 5.1.0
     */
    public enum Fraction {
        /**
         * Show as decimal to the precision specified by D
         */
        DECIMAL,
        /**
         * Show as a fraction with denominator specified by D
         */
        FRACTION,
        /**
         * No fractional part; round to the nearest whole unit.
         */
        ROUND,
        /**
         * No fractional part; truncate to achieve whole units.
         */
        TRUNCATE}

    /**
     * Linear display units for use in a Preferred Display Units (PDU) array.
     * @since 5.1.0
     */
    public enum Linear {
        /** a meter */
        METER,
        /** a kilometer */
        KILOMETER,
        /** an international foot */
        INTERNATIONAL_FOOT,
        /** a U.S. survey foot */
        US_SURVEY_FOOT,
        /** an international mile */
        INTERNATIONAL_MILE,
        /** an international nautical mil*/
        INTERNATIONAL_NAUTICAL_MILE}

    /**
     * Identifier for use in the Names array that identifies the
     * internal data elements of the individual point arrays in the XPTS array
     * @since 5.1.0
     */
    public enum PtIdentifier {
        /** Latitude in degrees */
        LATITUDE,
        /** Longitude in degrees */
        LONGITUDE,
        /** Altitude in meters*/
        ALTITUDE}

    /**
     * Area display units for use in a Preferred Display Units (PDU) array.
     * @since 5.1.0
     */
    public enum Square {
        /** a square meter */
        SQUARE_METER,
        /** a hectare (10,000 square meters) */
        HECTARE,
        /** a square kilometer */
        SQUARE_KILOMETER,
        /** a square foot */
        SQUARE_FOOT,
        /** an acre */
        ACRE,
        /** a square mile */
        SQUARE_MILE}

    public static class DecodeUnits {
        public static PdfName Decode(Angular e) {
            switch (e) {
                case Angular.DEGREE:
                    return new PdfName("DEG");
                case Angular.GRAD:
                    return new PdfName("GRD");
                default:
                    return null;
            }
        }

        public static PdfName Decode(Fraction f) {
            switch (f) {
                case Fraction.DECIMAL:
                    return new PdfName("D");
                case Fraction.FRACTION:
                    return new PdfName("F");
                case Fraction.ROUND:
                    return new PdfName("R");
                case Fraction.TRUNCATE:
                    return new PdfName("T");
                default:
                    return null;
            }
        }

        public static PdfName Decode(Linear l) {
            switch (l) {
                case Linear.INTERNATIONAL_FOOT:
                    return new PdfName("FT");
                case Linear.INTERNATIONAL_MILE:
                    return new PdfName("MI");
                case Linear.INTERNATIONAL_NAUTICAL_MILE:
                    return new PdfName("NM");
                case Linear.KILOMETER:
                    return new PdfName("KM");
                case Linear.METER:
                    return new PdfName("M");
                case Linear.US_SURVEY_FOOT:
                    return new PdfName("USFT");
                default:
                    return null;
            }
        }

        public static PdfName Decode(PtIdentifier p) {
            switch (p) {
                case PtIdentifier.ALTITUDE:
                    return new PdfName("ALT");
                case PtIdentifier.LATITUDE:
                    return new PdfName("LAT");
                case PtIdentifier.LONGITUDE:
                    return new PdfName("LON");
                default:
                    return null;
            }
        }

        public static PdfName Decode(Square s) {
            switch (s) {
                case Square.ACRE:
                    return new PdfName("A");
                case Square.HECTARE:
                    return new PdfName("HA");
                case Square.SQUARE_FOOT:
                    return new PdfName("SQFT");
                case Square.SQUARE_KILOMETER:
                    return new PdfName("SQKM");
                case Square.SQUARE_METER:
                    return new PdfName("SQM");
                case Square.SQUARE_MILE:
                    return new PdfName("SQMI");
                default:
                    return null;
            }
        }
    }
}