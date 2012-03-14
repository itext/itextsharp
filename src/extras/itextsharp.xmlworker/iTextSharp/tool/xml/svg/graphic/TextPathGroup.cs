/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
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

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg.tags;
using iTextSharp.tool.xml.svg.utils;

namespace iTextSharp.tool.xml.svg.graphic {

    public class TextPathGroup : Graphic
    {
        IList<IElement> list;
        float width, height, x, y;
        Path path;
        static int fontsize = 40;

        public TextPathGroup(IList<IElement> list, float x, float y, float width, float height, IDictionary<String, String> css, Path path) : base(css)
        {
            this.list = list;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.path = path;
        }

        void DrawGroup(PdfContentByte cb)
        {
            Draw(cb, PathToLineSegment(), list);
        }

        public void Draw(PdfContentByte cb, IList<float[]> lines, IList<IElement> list)
        {
            if (lines.Count < 2)
            {
                return; //Do nothing
            }

            //TODO check all the style elements
            cb.SetLineWidth(3);
            //first set a letter type
            BaseFont bf = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED);
            cb.SetFontAndSize(bf, fontsize);

            float xPrevious = lines[0][0], yPrevious = lines[0][1];
            int indexOfTextElement = 0, indexOfCharacter = 0;

            String currentCharacter = GetCharacter(indexOfTextElement, indexOfCharacter);
            while (string.IsNullOrEmpty(currentCharacter))
            {
                if (indexOfCharacter >= GetLengthOfText(indexOfTextElement) - 1)
                {
                    //this was the last character of the text element
                    if (indexOfTextElement == list.Count - 1)
                    {
                        //this was the last text element; exit while loop
                        return; //stop
                    }
                    else
                    {
                        //goto first character of the next element
                        //TODO set the css
                        indexOfTextElement++;
                        indexOfCharacter = 0;
                    }
                }
                currentCharacter = GetCharacter(indexOfTextElement, indexOfCharacter);
            }

            double halfWidthOfCharacter = cb.GetEffectiveStringWidth(currentCharacter, true)/2.0;
            double totalLength = 0;

            bool lookForStart = true;

            for (int j = 1; j < lines.Count; j++)
            {

                float[] point = lines[j];

                double lengthLijnStuk = CalculateDistance(xPrevious, yPrevious, point[0], point[1]);
                //System.out.Println(lengthLijnStuk);
                totalLength = totalLength + lengthLijnStuk;
                //System.out.Println(totalLength);

                while (totalLength >= halfWidthOfCharacter)
                {
                    double tussen = totalLength - halfWidthOfCharacter;
                    double xyAlongThis = lengthLijnStuk - tussen - halfWidthOfCharacter;
                    double[] xy = GetPointOnLine(xPrevious, yPrevious, point[0], point[1], xyAlongThis);

                    if (lookForStart)
                    {
                        ShowText(cb, point[0], point[1], xPrevious, yPrevious, xy[0], xy[1], currentCharacter);
                        lookForStart = false;
                        totalLength = tussen; //distance to the end of the line segment

                    }
                    else
                    {
                        //look for the end point
                        lookForStart = true;
                        totalLength = tussen; //distance to the end of the line segment 

                        indexOfCharacter++;
                        currentCharacter = GetCharacter(indexOfTextElement, indexOfCharacter);

                        while (currentCharacter == null || currentCharacter.Length == 0)
                        {
                            if (indexOfCharacter >= GetLengthOfText(indexOfTextElement) - 1)
                            {
                                //this was the last character of the text element
                                if (indexOfTextElement == list.Count - 1)
                                {
                                    //this was the last text element; exit while loop
                                    return;
                                }
                                else
                                {
                                    //goto first character of the next element
                                    indexOfTextElement++;
                                    indexOfCharacter = 0;
                                }
                            }
                            currentCharacter = GetCharacter(indexOfTextElement, indexOfCharacter);
                        }
                        halfWidthOfCharacter = cb.GetEffectiveStringWidth(currentCharacter, true)/2.0;
                    }
                }

                xPrevious = point[0];
                yPrevious = point[1];
            }
        }

        private String GetCharacter(int indexOfTextElement, int indexOfCharacter)
        {
            Text text = (Text)list[indexOfTextElement];
            if (text != null && text.GetText().Length > indexOfCharacter)
            {
                return "" + text.GetText()[indexOfCharacter];
            }
            return null;
        }

        private int GetLengthOfText(int indexOfTextElement)
        {
            Text text = (Text)list[indexOfTextElement];
            return text.GetText().Length;
        }

        public void DrawLine(PdfContentByte cb, float x1, float y1, float x2, float y2, int step) {
    	    cb.MoveTo(x1, y1);
    	    cb.LineTo(x2, y2);
    	    cb.Stroke();
        	
    	    for (int i = 0; i < 10; i++) {
        	    double[] point = GetPointOnLine(x1, y1, x2, y2, i*step);
        	    cb.Rectangle((float)point[0], (float)point[1], 2, 2);
        	    cb.Stroke();
		    }
        }

        private void ShowText(PdfContentByte cb, float x, float y, float xPrevious, float yPrevious, double xmidden, double ymidden, String character)
        {
            double corner = CalculateCorner(x, y, xPrevious, yPrevious);
            cb.SaveState();
            PdfTemplate template2 = cb.CreateTemplate(1000, 1000);

            template2.BeginText();
            template2.SetColorFill(BaseColor.BLACK);
            BaseFont bf = BaseFont.CreateFont();
            template2.SetFontAndSize(bf, fontsize);
            //template2.SetTextRise(10);
            //double halfWidthOfCharacter = cb.GetEffectiveStringWidth(character+"", true) / 2.0;  
            template2.SetTextMatrix(0, 0);
            template2.ShowText(character + "");
            template2.EndText();
            Matrix translation = new Matrix();
            translation.Translate((float)xmidden, (float)ymidden);
            cb.ConcatCTM(translation);
            Matrix rotation = new Matrix();
            rotation.Rotate((float)corner);
            cb.ConcatCTM(rotation);
            cb.ConcatCTM(1, 0, 0, -1, 0, 0);
            cb.AddTemplate(template2, 0, 0);
            cb.RestoreState();
        }

        private double CalculateCorner(double x1, double y1, double x0, double y0)
        {
            //to center
            double x = x1 - x0;
            double y = y1 - y0;
            if (x >= 0 && y >= 0)
            {
                return (Math.Atan(y / x) * 180d / Math.PI);
            }
            if (x < 0 && y > 0)
            {
                return (Math.Atan(y / x) * 180d / Math.PI + 180);
            }
            if (x < 0 && y < 0)
            {
                return (Math.Atan(y / x) * 180d / Math.PI + 180);
            }

            if (x >= 0 && y < 0)
            {
                return (Math.Atan(y / x) + 180d / Math.PI + 360);
            }
            return 0;
        }

        private double[] GetPointOnLine(float x1, float y1, float x2, float y2, double length){
    	double corner = CalculateCorner(x2, y2, x1, y1);
    	//System.out.Println(corner);
    	double rad = corner * Math.PI / 180d;
    	double[] result = new double[2];
    	result[0] = x1 + length * Math.Cos(rad);
    	result[1] = y1 + length * Math.Sin(rad);
    	return result;
    }

        private double CalculateDistance(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private IList<float[]> PathToLineSegment()
        {
            return PathToLineSegment(path.GetTranslatedPathItems());
        }

        private IList<float[]> PathToLineSegment(IList<PathItem> pathItems){
    	    IList<float[]> coordinates = new List<float[]>();
        	
		    foreach (PathItem item in pathItems) {
			    IList<float> numbers = item.Coordinates;

			    if(item.IsMoveTo() || item.IsLineTo()){
				    float[] point = new float[2];
				    point[0] = numbers[0];
				    point[1] = numbers[1];
				    coordinates.Add(point);
			    }else if (item.IsCubicBezier() || item.IsQuadraticBezier()){
                    foreach (float[] point in BezierCurveToLines(coordinates, item)) {
			            coordinates.Add(point);    
			        }
			    }else if (item.IsArcTo()){
                    foreach (float[] point in ArcToLines(item)) {
			            coordinates.Add(point);    
			        }		
			    }
		    }
    		
    	    return coordinates;
        }

        private IList<float[]> ArcToLines(PathItem item) {
    	    IList<float> numbers = item.Coordinates;
		    EllipseArc ellipse = EllipseArc.CreateEllipseArc(numbers[7], numbers[8], numbers[5], numbers[6], numbers[0], numbers[1], numbers[4], numbers[3]);

		    IList<float[]> newCoordinates = PdfContentByte.BezierArc(ellipse.Cx - numbers[0], ellipse.Cy - numbers[1], ellipse.Cx + numbers[0], ellipse.Cy + numbers[1],
				    ellipse.StartAng, ellipse.Extend);
    		
		    IList<float[]> result = new List<float[]>();
    		
            if (newCoordinates.Count == 0) return result;
               
            float[] pt = newCoordinates[0];
            float x0 = pt[0];
            float y0 = pt[1];
            
            for (int k = 0; k < newCoordinates.Count; ++k) {
                pt = newCoordinates[k];
                foreach (float[] point in BezierCurveToLines(x0, y0, pt[2], pt[3], pt[4], pt[5], pt[6], pt[7], true))
                {
                    result.Add(point);    
                }
                x0 = pt[6];
                y0 = pt[7];
            }
            return result;
        }

        private IList<float[]> BezierCurveToLines(IList<float[]> coordinates, PathItem item){
    	IList<float> numbers = item.Coordinates;
    	
    	float[] start = coordinates[coordinates.Count - 1];
    	float x0 = start[0];
    	float y0 = start[1];
    	float x1 = numbers[0];
    	float y1 = numbers[1];
    	float x2 = numbers[2];
    	float y2 = numbers[3];
    	float x3 = 0;
    	float y3 = 0;
    	
    	if(item.IsCubicBezier()){
        	x3 = numbers[4];
        	y3 = numbers[5];    		
    	}
    	
    	return BezierCurveToLines(x0, y0, x1, y1, x2, y2, x3, y3, item.IsCubicBezier());
    }

        private IList<float[]> BezierCurveToLines(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3, bool cubic)
        {
            float A, B, C, D, E, F, G, H;

            if (cubic)
            {
                A = x3 - 3 * x2 + 3 * x1 - x0;
                B = 3 * x2 - 6 * x1 + 3 * x0;
                C = 3 * x1 - 3 * x0;
                D = x0;

                E = y3 - 3 * y2 + 3 * y1 - y0;
                F = 3 * y2 - 6 * y1 + 3 * y0;
                G = 3 * y1 - 3 * y0;
                H = y0;
            }
            else
            {
                A = 0;
                B = x2 - 2 * x1 + x0;
                C = 2 * x1 - 2 * x0;
                D = x0;
                E = 0;
                F = y2 - 2 * y1 + y0;
                G = 2 * y1 - 2 * y0;
                H = y0;
            }

            IList<float[]> result = new List<float[]>();
            float step = 0.005f;
            for (float t = step; t <= 1; t += step)
            {
                float[] point = new float[2];
                point[0] = A * t * t * t + B * t * t + C * t + D;
                point[1] = E * t * t * t + F * t * t + G * t + H;
                result.Add(point);
            }
            return result;
        }

        protected override void Draw(PdfContentByte cb)
        {
            try
            {
                DrawGroup(cb);
            }
            catch (Exception exp)
            {
                //System.out.Println(exp.GetMessage());
            }
        }
    }
}
