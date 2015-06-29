using System;
using System.Collections.Generic;
using System.Text;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
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
namespace iTextSharp.text.pdf.parser {


    /**
     * <b>Development preview</b> - this class (and all of the parser classes) are still experiencing
     * heavy development, and are subject to change both behavior and interface.
     * <br>
     * A text extraction renderer that keeps track of relative position of text on page
     * The resultant text will be relatively consistent with the physical layout that most
     * PDF files have on screen.
     * <br>
     * This renderer keeps track of the orientation and distance (both perpendicular
     * and parallel) to the unit vector of the orientation.  Text is ordered by
     * orientation, then perpendicular, then parallel distance.  Text with the same
     * perpendicular distance, but different parallel distance is treated as being on
     * the same line.
     * <br>
     * This renderer also uses a simple strategy based on the font metrics to determine if
     * a blank space should be inserted into the output.
     *
     * @since   5.0.2
     */
    public class LocationTextExtractionStrategy : ITextExtractionStrategy {

        /** set to true for debugging */
        public static bool DUMP_STATE = false;
        
        /** a summary of all found text */
        private List<TextChunk> locationalResult = new List<TextChunk>();

        /**
         * Creates a new text extraction renderer.
         */
        public LocationTextExtractionStrategy() {
        }

        /**
         * @see com.itextpdf.text.pdf.parser.RenderListener#beginTextBlock()
         */
        public virtual void BeginTextBlock(){
        }

        /**
         * @see com.itextpdf.text.pdf.parser.RenderListener#endTextBlock()
         */
        public virtual void EndTextBlock(){
        }

        /**
         * @param str
         * @return true if the string starts with a space character, false if the string is empty or starts with a non-space character
         */
        private bool StartsWithSpace(String str){
            if (str.Length == 0) return false;
            return str[0] == ' ';
        }
        
        /**
         * @param str
         * @return true if the string ends with a space character, false if the string is empty or ends with a non-space character
         */
        private bool EndsWithSpace(String str){
            if (str.Length == 0) return false;
            return str[str.Length-1] == ' ';
        }

        /**
         * Determines if a space character should be inserted between a previous chunk and the current chunk.
         * This method is exposed as a callback so subclasses can fine time the algorithm for determining whether a space should be inserted or not.
         * By default, this method will insert a space if the there is a gap of more than half the font space character width between the end of the
         * previous chunk and the beginning of the current chunk.  It will also indicate that a space is needed if the starting point of the new chunk 
         * appears *before* the end of the previous chunk (i.e. overlapping text).
         * @param chunk the new chunk being evaluated
         * @param previousChunk the chunk that appeared immediately before the current chunk
         * @return true if the two chunks represent different words (i.e. should have a space between them).  False otherwise.
         */
        virtual protected bool IsChunkAtWordBoundary(TextChunk chunk, TextChunk previousChunk) {
            /**
             * Here we handle a very specific case which in PDF may look like:
             * -.232 Tc [( P)-226.2(r)-231.8(e)-230.8(f)-238(a)-238.9(c)-228.9(e)]TJ
             * The font's charSpace width is 0.232 and it's compensated with charSpacing of 0.232.
             * And a resultant TextChunk.charSpaceWidth comes to TextChunk constructor as 0.
             * In this case every chunk is considered as a word boundary and space is added.
             * We should consider charSpaceWidth equal (or close) to zero as a no-space.
             */
            if (chunk.CharSpaceWidth < 0.1f)
                return false;

            float dist = chunk.DistanceFromEndOf(previousChunk);
            if(dist < -chunk.CharSpaceWidth || dist > chunk.CharSpaceWidth / 2.0f)
                return true;

            return false;
        }

        /**
         * Filters the provided list with the provided filter
         * @param textChunks a list of all TextChunks that this strategy found during processing
         * @param filter the filter to apply.  If null, filtering will be skipped.
         * @return the filtered list
         * @since 5.3.3
         */
        private List<TextChunk> filterTextChunks(List<TextChunk> textChunks, ITextChunkFilter filter){
            if (filter == null) {
                return textChunks;
            }

            List<TextChunk> filtered = new List<TextChunk>();

            foreach (TextChunk textChunk in textChunks) {
                if (filter.Accept(textChunk)) {
                    filtered.Add(textChunk);
                }
            }

    	    return filtered;
        }

        /**
         * Gets text that meets the specified filter
         * If multiple text extractions will be performed for the same page (i.e. for different physical regions of the page), 
         * filtering at this level is more efficient than filtering using {@link FilteredRenderListener} - but not nearly as powerful
         * because most of the RenderInfo state is not captured in {@link TextChunk}
         * @param chunkFilter the filter to to apply
         * @return the text results so far, filtered using the specified filter
         */
        public virtual String GetResultantText(ITextChunkFilter chunkFilter){
            if (DUMP_STATE) {
                DumpState();
            }

            List<TextChunk> filteredTextChunks = filterTextChunks(locationalResult, chunkFilter);
            filteredTextChunks.Sort();

            StringBuilder sb = new StringBuilder();
            TextChunk lastChunk = null;
            foreach (TextChunk chunk in filteredTextChunks) {

                if (lastChunk == null){
                    sb.Append(chunk.Text);
                } else {
                    if (chunk.SameLine(lastChunk)){
                        // we only insert a blank space if the trailing character of the previous string wasn't a space, and the leading character of the current string isn't a space
                        if(IsChunkAtWordBoundary(chunk, lastChunk) && !StartsWithSpace(chunk.Text) && !EndsWithSpace(lastChunk.Text))
                            sb.Append(' ');

                        sb.Append(chunk.Text);
                    } else {
                        sb.Append('\n');
                        sb.Append(chunk.Text);
                    }
                }
                lastChunk = chunk;
            }

            return sb.ToString();
        }

        /**
         * Returns the result so far.
         * @return  a String with the resulting text.
         */
        public virtual String GetResultantText() {
            return GetResultantText(null);
        }

        /** Used for debugging only */
        private void DumpState(){
            foreach (TextChunk location in locationalResult) {
                
                location.PrintDiagnostics();
                
                Console.Out.WriteLine();
            }
            
        }
        
        /**
         * 
         * @see com.itextpdf.text.pdf.parser.RenderListener#renderText(com.itextpdf.text.pdf.parser.TextRenderInfo)
         */
        public virtual void RenderText(TextRenderInfo renderInfo) {
            LineSegment segment = renderInfo.GetBaseline();
            if (renderInfo.GetRise() != 0)
            { // remove the rise from the baseline - we do this because the text from a super/subscript render operations should probably be considered as part of the baseline of the text the super/sub is relative to 
                Matrix riseOffsetTransform = new Matrix(0, -renderInfo.GetRise());
                segment = segment.TransformBy(riseOffsetTransform);
            }
            TextChunk location = new TextChunk(renderInfo.GetText(), segment.GetStartPoint(), segment.GetEndPoint(), renderInfo.GetSingleSpaceWidth());
            locationalResult.Add(location);        
        }
        


        /**
         * Represents a chunk of text, it's orientation, and location relative to the orientation vector
         */
        public class TextChunk : IComparable<TextChunk>{
            /** the text of the chunk */
            private readonly String text;
            /** the starting location of the chunk */
            private readonly Vector startLocation;
            /** the ending location of the chunk */
            private readonly Vector endLocation;
            /** unit vector in the orientation of the chunk */
            private readonly Vector orientationVector;
            /** the orientation as a scalar for quick sorting */
            private readonly int orientationMagnitude;
            /** perpendicular distance to the orientation unit vector (i.e. the Y position in an unrotated coordinate system)
             * we round to the nearest integer to handle the fuzziness of comparing floats */
            private readonly int distPerpendicular;
            /** distance of the start of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system) */
            private readonly float distParallelStart;
            /** distance of the end of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system) */
            private readonly float distParallelEnd;
            /** the width of a single space character in the font of the chunk */
            private readonly float charSpaceWidth;
            
            public TextChunk(String str, Vector startLocation, Vector endLocation, float charSpaceWidth) {
                this.text = str;
                this.startLocation = startLocation;
                this.endLocation = endLocation;
                this.charSpaceWidth = charSpaceWidth;
                
                Vector oVector = endLocation.Subtract(startLocation);
                if (oVector.Length == 0) {
                    oVector = new Vector(1, 0, 0);
                }
                orientationVector = oVector.Normalize();
                orientationMagnitude = (int)(Math.Atan2(orientationVector[Vector.I2], orientationVector[Vector.I1])*1000);

                // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
                // the two vectors we are crossing are in the same plane, so the result will be purely
                // in the z-axis (out of plane) direction, so we just take the I3 component of the result
                Vector origin = new Vector(0,0,1);
                distPerpendicular = (int)(startLocation.Subtract(origin)).Cross(orientationVector)[Vector.I3];

                distParallelStart = orientationVector.Dot(startLocation);
                distParallelEnd = orientationVector.Dot(endLocation);
            }


            /**
             * @return the text captured by this chunk
             */
            virtual public String Text {
                get { return text; }
            }

            /**
             * @return the width of a single space character as rendered by this chunk
             */
            virtual public float CharSpaceWidth {
                get { return charSpaceWidth; }
            }

            /**
             * @return the start location of the text
             */
            virtual public Vector StartLocation {
                get { return startLocation; }
            }

            /**
             * @return the end location of the text
             */
            virtual public Vector EndLocation {
                get { return endLocation; }
            }


            virtual public void PrintDiagnostics(){
                Console.Out.WriteLine("Text (@" + startLocation + " -> " + endLocation + "): " + text);
                Console.Out.WriteLine("orientationMagnitude: " + orientationMagnitude);
                Console.Out.WriteLine("distPerpendicular: " + distPerpendicular);
                Console.Out.WriteLine("distParallel: " + distParallelStart);
            }
            
            /**
             * @param as the location to compare to
             * @return true is this location is on the the same line as the other
             */
            virtual public bool SameLine(TextChunk a){
                if (orientationMagnitude != a.orientationMagnitude) return false;
                if (distPerpendicular != a.distPerpendicular) return false;
                return true;
            }

            /**
             * Computes the distance between the end of 'other' and the beginning of this chunk
             * in the direction of this chunk's orientation vector.  Note that it's a bad idea
             * to call this for chunks that aren't on the same line and orientation, but we don't
             * explicitly check for that condition for performance reasons.
             * @param other
             * @return the number of spaces between the end of 'other' and the beginning of this chunk
             */
            virtual public float DistanceFromEndOf(TextChunk other){
                float distance = distParallelStart - other.distParallelEnd;
                return distance;
            }
            
            /**
             * Compares based on orientation, perpendicular distance, then parallel distance
             * @see java.lang.Comparable#compareTo(java.lang.Object)
             */
            virtual public int CompareTo(TextChunk rhs) {
                if (this == rhs) return 0; // not really needed, but just in case
                
                int rslt;
                rslt = CompareInts(orientationMagnitude, rhs.orientationMagnitude);
                if (rslt != 0) return rslt;

                rslt = CompareInts(distPerpendicular, rhs.distPerpendicular);
                if (rslt != 0) return rslt;

                // note: it's never safe to check floating point numbers for equality, and if two chunks
                // are truly right on top of each other, which one comes first or second just doesn't matter
                // so we arbitrarily choose this way.
                rslt = distParallelStart < rhs.distParallelStart ? -1 : 1;

                return rslt;
            }

            /**
             *
             * @param int1
             * @param int2
             * @return comparison of the two integers
             */
            private static int CompareInts(int int1, int int2){
                return int1 == int2 ? 0 : int1 < int2 ? -1 : 1;
            }

            
        }

        /**
         * no-op method - this renderer isn't interested in image events
         * @see com.itextpdf.text.pdf.parser.RenderListener#renderImage(com.itextpdf.text.pdf.parser.ImageRenderInfo)
         * @since 5.0.1
         */
        public virtual void RenderImage(ImageRenderInfo renderInfo) {
            // do nothing
        }

        /**
         * Specifies a filter for filtering {@link TextChunk} objects during text extraction 
         * @see LocationTextExtractionStrategy#getResultantText(TextChunkFilter)
         * @since 5.3.3
         */
        public interface ITextChunkFilter {
            /**
             * @param textChunk the chunk to check
             * @return true if the chunk should be allowed
             */
            bool Accept(TextChunk textChunk);
        }
    }
}
