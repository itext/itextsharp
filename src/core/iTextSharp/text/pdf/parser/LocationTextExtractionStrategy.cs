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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using iTextSharp.text.io;

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

        private readonly ITextChunkLocationStrategy tclStrat;

        /**
         * Creates a new text extraction renderer.
         */
        public LocationTextExtractionStrategy() : this(new TextChunkLocationStrategyDefaultImp()) {
        }

        /**
         * Creates a new text extraction renderer, with a custom strategy for
         * creating new TextChunkLocation objects based on the input of the
         * TextRenderInfo.
         * @param strat the custom strategy
         */
        public LocationTextExtractionStrategy(ITextChunkLocationStrategy strat) {
            tclStrat = strat;
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
         * Determines if a space character should be inserted between a previous chunk and the current chunk.
         * This method is exposed as a callback so subclasses can fine time the algorithm for determining whether a space should be inserted or not.
         * By default, this method will insert a space if the there is a gap of more than half the font space character width between the end of the
         * previous chunk and the beginning of the current chunk.  It will also indicate that a space is needed if the starting point of the new chunk 
         * appears *before* the end of the previous chunk (i.e. overlapping text).
         * @param chunk the new chunk being evaluated
         * @param previousChunk the chunk that appeared immediately before the current chunk
         * @return true if the two chunks represent different words (i.e. should have a space between them).  False otherwise.
         */
        protected virtual bool IsChunkAtWordBoundary(TextChunk chunk, TextChunk previousChunk)
        {
            return chunk.Location.IsAtWordBoundary(previousChunk.Location);
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
            TextChunk tc = new TextChunk(renderInfo.GetText(), tclStrat.CreateLocation(renderInfo, segment));
            locationalResult.Add(tc);        
        }

        public interface ITextChunkLocationStrategy
        {
            ITextChunkLocation CreateLocation(TextRenderInfo renderInfo, LineSegment baseline);
        }

        public interface ITextChunkLocation : IComparable<ITextChunkLocation> {

            /** the starting location of the chunk */
            Vector StartLocation { get; }
            /** the ending location of the chunk */
            Vector EndLocation { get; }
            /** the orientation as a scalar for quick sorting */
            int OrientationMagnitude { get; }
            /** perpendicular distance to the orientation unit vector (i.e. the Y position in an unrotated coordinate system)
             * we round to the nearest integer to handle the fuzziness of comparing floats */
            int DistPerpendicular { get; }
            /** distance of the start of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system) */
            float DistParallelStart { get; }
            /** distance of the end of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system) */
            float DistParallelEnd { get; }
            /** the width of a single space character in the font of the chunk */
            float CharSpaceWidth { get; }
            /**
             * @param comparedLine the location to compare to
             * @return true is this location is on the the same line as the other
             */
            bool SameLine(ITextChunkLocation other);
            /**
             * Computes the distance between the end of 'other' and the beginning of this chunk
             * in the direction of this chunk's orientation vector.  Note that it's a bad idea
             * to call this for chunks that aren't on the same line and orientation, but we don't
             * explicitly check for that condition for performance reasons.
             * @param other
             * @return the number of spaces between the end of 'other' and the beginning of this chunk
             */
            float DistanceFromEndOf(ITextChunkLocation other);

            bool IsAtWordBoundary(ITextChunkLocation previous);
        }

        public class TextChunkLocationStrategyDefaultImp : ITextChunkLocationStrategy {
            public ITextChunkLocation CreateLocation(TextRenderInfo renderInfo, LineSegment baseline) {
                return new TextChunkLocationDefaultImp(baseline.GetStartPoint(), baseline.GetEndPoint(), renderInfo.GetSingleSpaceWidth());
            }
        }

        public class TextChunkLocationDefaultImp : ITextChunkLocation{
            /** unit vector in the orientation of the chunk */
            private readonly Vector orientationVector;

            private readonly Vector startLocation;
            private readonly Vector endLocation;
            private readonly int orientationMagnitude;
            private readonly int distPerpendicular;
            private readonly float distParallelStart;
            private readonly float distParallelEnd;
            private readonly float charSpaceWidth;

            public TextChunkLocationDefaultImp(Vector startLocation, Vector endLocation, float charSpaceWidth) {
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

            public virtual Vector StartLocation {
                get { return startLocation; }
            }

            public virtual Vector EndLocation {
                get { return endLocation; }
            }

            public virtual int OrientationMagnitude {
                get { return orientationMagnitude; }
            }

            public virtual int DistPerpendicular {
                get { return distPerpendicular; }
            }

            public virtual float DistParallelStart {
                get { return distParallelStart; }
            }

            public virtual float DistParallelEnd {
                get { return distParallelEnd; }
            }

            public virtual float CharSpaceWidth {
                get { return charSpaceWidth; }
            }

            public virtual bool SameLine(ITextChunkLocation other) {
                return OrientationMagnitude == other.OrientationMagnitude &&
                       DistPerpendicular == other.DistPerpendicular;
            }

            public virtual float DistanceFromEndOf(ITextChunkLocation other){
                float distance = DistParallelStart - other.DistParallelEnd;
                return distance;
            }

            public virtual bool IsAtWordBoundary(ITextChunkLocation previous) {
                float dist = DistanceFromEndOf(previous);
                
                if (dist < 0) {
                    dist = previous.DistanceFromEndOf(this);

                    //The situation when the chunks intersect. We don't need to add space in this case
                    if (dist < 0) {
                        return false;
                    }
                }
                return dist > CharSpaceWidth / 2.0f;
            }

            /**
             * Compares based on orientation, perpendicular distance, then parallel distance
             * @see java.lang.Comparable#compareTo(java.lang.Object)
             */
            public virtual int CompareTo(ITextChunkLocation other) {
                if (this == other) return 0; // not really needed, but just in case
                
                int rslt;
                rslt = CompareInts(OrientationMagnitude, other.OrientationMagnitude);
                if (rslt != 0) return rslt;

                rslt = CompareInts(DistPerpendicular, other.DistPerpendicular);
                if (rslt != 0) return rslt;

                // note: it's never safe to check floating point numbers for equality, and if two chunks
                // are truly right on top of each other, which one comes first or second just doesn't matter
                // so we arbitrarily choose this way.
                rslt = DistParallelStart < other.DistParallelStart ? -1 : 1;

                return rslt;
            }  
        }

        /**
         * Represents a chunk of text, it's orientation, and location relative to the orientation vector
         */
        public class TextChunk : IComparable<TextChunk>
        {
            /** the text of the chunk */
            private readonly String text;
            private readonly ITextChunkLocation location;

            public TextChunk(String str, Vector startLocation, Vector endLocation, float charSpaceWidth) :
                this(str, new TextChunkLocationDefaultImp(startLocation, endLocation, charSpaceWidth)) {
            }

            public TextChunk(String str, ITextChunkLocation location) {
                this.text = str;
                this.location = location;
            }

            /**
                 * @return the start location of the text
                 */
            public virtual Vector StartLocation {
                get { return Location.StartLocation; }
            }

            /**
             * @return the end location of the text
             */
            public virtual Vector EndLocation {
                get { return Location.EndLocation; }
            }

            /**
             * @return the width of a single space character as rendered by this chunk
             */
            public virtual float CharSpaceWidth {
                get { return Location.CharSpaceWidth; }
            }

            public virtual String Text {
                get { return text; }
            }

            public virtual ITextChunkLocation Location {
                get { return location; }
            }

            public virtual void PrintDiagnostics()
            {
                Console.Out.WriteLine("Text (@" + StartLocation + " -> " + EndLocation + "): " + Text);
                Console.Out.WriteLine("orientationMagnitude: " + Location.OrientationMagnitude);
                Console.Out.WriteLine("distPerpendicular: " + Location.DistPerpendicular);
                Console.Out.WriteLine("distParallel: " + Location.DistParallelStart);
            }

            /**
             * Computes the distance between the end of 'other' and the beginning of this chunk
             * in the direction of this chunk's orientation vector.  Note that it's a bad idea
             * to call this for chunks that aren't on the same line and orientation, but we don't
             * explicitly check for that condition for performance reasons.
             * @param other
             * @return the number of spaces between the end of 'other' and the beginning of this chunk
             */
            public virtual float DistanceFromEndOf(TextChunk other) {
                return Location.DistanceFromEndOf(other.Location);
            }

            /**
             * Compares based on orientation, perpendicular distance, then parallel distance
             * @see java.lang.Comparable#compareTo(java.lang.Object)
             */
            public virtual int CompareTo(TextChunk other) {
                return Location.CompareTo(other.Location);
            }

            /**
             * @param as the location to compare to
             * @return true is this location is on the the same line as the other
             */
            public virtual bool SameLine(TextChunk lastChunk)
            {
                return Location.SameLine(lastChunk.Location);
            }
        }

        /**
         *
         * @param int1
         * @param int2
         * @return comparison of the two integers
         */
        private static int CompareInts(int int1, int int2)
        {
            return int1 == int2 ? 0 : int1 < int2 ? -1 : 1;
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
