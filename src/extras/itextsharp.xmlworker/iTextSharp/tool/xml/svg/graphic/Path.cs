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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg.tags;
using iTextSharp.tool.xml.svg.utils;

namespace iTextSharp.tool.xml.svg.graphic {

    public class Path : Graphic
    {
        PathBean path;

        public Path(PathBean path, IDictionary<String, String> css) : base(css)
        {
            this.path = path;
        }



        protected override void Draw(PdfContentByte cb)
        {
            DrawElement(cb);
        }

        //just a method that cleans up the path before drawing it
        IList<PathItem> Translate(IList<PathItem> items)
        {
            IList<PathItem> result = new List<PathItem>();
            try
            {
                foreach (PathItem item in items)
                {
                    if (item.IsMoveTo() || item.IsLineTo())
                    {
                        TranslateForMoveAndLineTo(item, result);
                    }
                    if (item.IsHorizontalLineTo() || item.IsVerticalLineTo())
                    {
                        TranslateForHorizontalOrVerticalLine(item, result);
                    }

                    if (item.IsCubicBezier() || item.IsQuadraticBezier() || item.IsCubicBezierShorthand() || item.IsQuadraticBezierShorthand())
                    {
                        TranslateCurves(item, result);
                    }

                    if (item.IsArcTo())
                    {
                        TranslateArc(item, result);
                    }

                    if (item.IsClosePath())
                    {
                        result.Add(item);
                    }
                }
            }
            catch
            {
                //when an exception occurs in one of the translate methods, stop looking at the rest of the path and return the list
            }
            return result;
        }


        private float[] GetCurrentPoint(IList<PathItem> result)
        {
            float[] currentPoint = new float[2];
            if (result != null && result.Count != 0)
            {
                IList<float> coordinates = result[result.Count - 1].Coordinates;
                currentPoint[0] = coordinates[coordinates.Count - 2];
                currentPoint[1] = coordinates[coordinates.Count - 1];
            }
            return currentPoint;
        }

        private void TranslateArc(PathItem item, IList<PathItem> result)
        {
            //add the current point at the end

            int size = 7;
            //needs groups of 7 coordinates
            IList<float> numbers = item.Coordinates;
            for (int i = 0; i < (numbers.Count / size); i++)
            {
                //first add the current coordinates, copy all and if relative, change the last two coordinates
                IList<float> coordinates = new List<float>();
                float[] currentPoint = GetCurrentPoint(result);

                for (int j = 0; j < size; j++)
                {
                    if (j == size - 2 && item.IsRelative())
                    { //x
                        coordinates.Add(currentPoint[0] + (numbers[(i * size) + j]));
                    }
                    else if (j == size - 1 && item.IsRelative())
                    { //y
                        coordinates.Add(currentPoint[1] + (numbers[(i * size) + j]));
                    }
                    else
                    {
                        coordinates.Add(numbers[(i * size) + j]);
                    }
                }
                //this is a bit strange but easiest way to transfer the coordinates
                coordinates.Add(currentPoint[0]);
                coordinates.Add(currentPoint[1]);
                result.Add(new PathItem(coordinates, PathItem.ARC));
            }
            if (numbers.Count % size != 0)
            {
                throw new Exception("Something wrong with the number of coordinates in the path");
            }
        }

        private void TranslateCurves(PathItem item, IList<PathItem> result)
        {
            int size;
            if (item.IsCubicBezier())
            {
                size = 6;
            }
            else if (item.IsCubicBezierShorthand() || item.IsQuadraticBezier())
            {
                size = 4;
            }
            else
            {
                size = 2;
            }

            IList<float> numbers = item.Coordinates;

            for (int i = 0; i < (numbers.Count / size); i++)
            {
                IList<float> coordinates = new List<float>();
                //shorthand notations - get the coordinates of the first control point
                if (item.IsCubicBezierShorthand() || item.IsQuadraticBezierShorthand())
                {
                    //add the last control point of the previous pathItem
                    if (result != null && result.Count != 0)
                    {
                        PathItem previous = result[result.Count - 1];
                        if ((previous.IsCubicBezier() && item.IsCubicBezierShorthand())
                                || (previous.IsQuadraticBezier() && item.IsQuadraticBezierShorthand()))
                        {
                            IList<float> previousCoordinates = result[result.Count - 1].Coordinates;

                            float xPreviousControlPoint = previousCoordinates[previousCoordinates.Count - 4];
                            float yPreviousControlPoint = previousCoordinates[previousCoordinates.Count - 3];

                            //reflection if this point to the currentPoint
                            float[] current = GetCurrentPoint(result);
                            coordinates.Add(2 * current[0] - xPreviousControlPoint);
                            coordinates.Add(2 * current[1] - yPreviousControlPoint);
                        }
                    }
                    if (coordinates.Count == 0)
                    {
                        //add the currentPoint
                        float[] current = GetCurrentPoint(result);
                        coordinates.Add(current[0]);
                        coordinates.Add(current[1]);
                    }
                }

                for (int j = 0; j < size; j++)
                { //copy the rest of the coordinates
                    if (item.IsRelative())
                    {
                        float[] currentPoint = GetCurrentPoint(result);
                        if (j % 2 == 0)
                        {
                            coordinates.Add(currentPoint[0] + (numbers[(i * size) + j]));
                        }
                        else
                        {
                            coordinates.Add(currentPoint[1] + (numbers[(i * size) + j]));
                        }
                    }
                    else
                    {
                        coordinates.Add(numbers[(i * size) + j]);
                    }
                }
                if (item.IsCubicBezier() || item.IsCubicBezierShorthand())
                {
                    result.Add(new PathItem(coordinates, PathItem.CUBIC_BEZIER));
                }
                else
                {
                    result.Add(new PathItem(coordinates, PathItem.QUADRATIC_BEZIER));
                }
            }
            if (numbers.Count % size != 0)
            {
                throw new Exception("Something wrong with the number of coordinates in the path");
            }
        }

        private void TranslateForHorizontalOrVerticalLine(PathItem item, IList<PathItem> result)
        {
            //TODO check if this is correct
            IList<float> numbers = item.Coordinates;
            if (numbers.Count == 0)
            {
                numbers.Add(0f);
            }
            float[] currentPoint = GetCurrentPoint(result);

            if (!item.IsRelative())
            { //take the last command
                IList<float> coordinates = new List<float>();
                if (item.IsHorizontalLineTo())
                {
                    coordinates.Add(numbers[numbers.Count - 1]);
                    coordinates.Add(currentPoint[1]);
                }
                else
                {
                    coordinates.Add(currentPoint[0]);
                    coordinates.Add(numbers[numbers.Count - 1]);
                }

                result.Add(new PathItem(coordinates, PathItem.LINE));
            }
            else
            {
                float coordinate = 0f;
                for (int i = 0; i < numbers.Count; i++)
                {
                    coordinate = numbers[i];
                }
                IList<float> coordinates = new List<float>();
                if (item.IsHorizontalLineTo())
                {
                    coordinates.Add(coordinate + currentPoint[0]);
                    coordinates.Add(currentPoint[1]);
                }
                else
                {
                    coordinates.Add(currentPoint[0]);
                    coordinates.Add(coordinate + currentPoint[1]);
                }
                result.Add(new PathItem(coordinates, PathItem.LINE));
            }
        }

        private void TranslateForMoveAndLineTo(PathItem item, IList<PathItem> result)
        {
            IList<float> numbers = item.Coordinates;
            if (numbers.Count % 2 == 1) {
                numbers.Add(0f);
            }

            //for each pair
            for (int i = 0; i < (numbers.Count / 2); i++)
            {
                float x = numbers[i * 2];
                float y = numbers[(i * 2) + 1];

                if (item.IsRelative())
                {
                    float[] currentPoint = GetCurrentPoint(result);
                    x = x + currentPoint[0];
                    y = y + currentPoint[1];
                }

                IList<float> coordinates = new List<float>();
                coordinates.Add(x);
                coordinates.Add(y);

                if (item.IsMoveTo() && i == 0)
                {
                    result.Add(new PathItem(coordinates, PathItem.MOVE));
                }
                else
                {
                    result.Add(new PathItem(coordinates, PathItem.LINE));
                }
            }
        }

        private void DrawArc(PdfContentByte cb, IList<float> numbers)
        {
            EllipseArc ellipse = EllipseArc.CreateEllipseArc(numbers[7], numbers[8], numbers[5], numbers[6], numbers[0], numbers[1], numbers[4], numbers[3]);

            cb.SetColorFill(BaseColor.ORANGE);
            cb.Rectangle(numbers[7], numbers[8], 2, 2); //p1
            cb.Fill();
            cb.SetColorFill(BaseColor.GREEN);
            cb.Rectangle(numbers[5], numbers[6], 2, 2); //p2
            cb.Fill();

            cb.Arc(ellipse.Cx - numbers[0], ellipse.Cy - numbers[1], ellipse.Cx + numbers[0], ellipse.Cy + numbers[1],
                    ellipse.StartAng, ellipse.Extend);
        }

        public IList<PathItem> GetTranslatedPathItems()
        {
            return Translate(path.PathItems);
        }

        void DrawElement(PdfContentByte cb)
        {
            try
            {
                IList<PathItem> translatedItems = Translate(path.PathItems);

                //loop over the items in the path
                foreach (PathItem item in translatedItems)
                {
                    IList<float> numbers = item.Coordinates;

                    if (item.IsMoveTo())
                    {
                        cb.MoveTo(numbers[0], numbers[1]);
                    }
                    else if (item.IsLineTo())
                    {
                        cb.LineTo(numbers[0], numbers[1]);
                    }
                    else if (item.IsCubicBezier())
                    {
                        cb.CurveTo(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
                    }
                    else if (item.IsQuadraticBezier())
                    {
                        cb.CurveTo(numbers[0], numbers[1], numbers[2], numbers[3]);
                    }
                    else if (item.IsArcTo())
                    {
                        DrawArc(cb, numbers);
                    }
                    else if (item.IsClosePath())
                    {
                        cb.ClosePath();
                    }
                    else
                    {
                        //System.out.Println(item);
                    }
                }
            } catch {
                //TODO
            }
        }
    }
}
