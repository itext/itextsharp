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
using System.util;
using iTextSharp.tool.xml.svg.exceptions;

namespace iTextSharp.tool.xml.svg.utils {

    public class TransformationMatrix
    {

        public static Matrix GetTransformationMatrix(String transform)
        {
            //matrix, translate, scale, rotate, skewX, skewY
            Matrix matrix = null;

            IList<String> listWithTransformations = SplitString(transform);
            foreach (String str in listWithTransformations)
            {
                Matrix newMatrix = StrToMatrix(str);
                if (newMatrix != null)
                {
                    if (matrix == null) {
                        matrix = newMatrix;
                    } else {
                        matrix.Multiply(newMatrix);
                    }
                }
            }
            return matrix;
        }

        static IList<String> SplitString(String transform)
        {
            List<String> list = new List<String>();
            StringTokenizer tokenizer = new StringTokenizer(transform, ")", false);
            while (tokenizer.HasMoreTokens())
            {
                list.Add(tokenizer.NextToken().Trim() + ")");
            }
            return list;
        }

        static Matrix StrToMatrix(String str)
        {
            try
            {
                //matrix, translate, scale, rotate, skewX, skewY
                if (str.StartsWith("matrix"))
                {
                    return CreateWithMatrix(GetValuesFromStr(str));
                } else if (str.StartsWith("translate")) {
                    return CreateForTranslate(GetValuesFromStr(str));
                } else if (str.StartsWith("scale")) {
                    return CreateForScale(GetValuesFromStr(str));
                } else if (str.StartsWith("rotate")) {
                    return CreateForRotate(GetValuesFromStr(str));
                } else if (str.StartsWith("skewX")) {
                    return CreateForSkewX(GetValuesFromStr(str));
                } else if (str.StartsWith("skewY")) {
                    return CreateForSkewY(GetValuesFromStr(str));
                } else {
                    //no transformation at all
                    return null;
                }
            }
            catch (SvgParseException)
            {
                //when a mistake happens, do nothing
                return null;
            }
        }

        //if only one coordinate the second one is zero
        static Matrix CreateForSkewY(IList<float> values)
        {
            if (values.Count != 1) {
                throw new SvgParseException("Could not parse the transform");
            }
            Matrix skewing = new Matrix(1, (float)Math.Tan(values[0] * Math.PI / 180), 0, 1, 0, 0);
            return skewing;
        }

        //if only one coordinate the second one is zero
        static Matrix CreateForSkewX(IList<float> values)
        {
            if (values.Count != 1) {
                throw new SvgParseException("Could not parse the transform");
            }
            Matrix skewing = new Matrix(1, 0, (float) Math.Tan(values[0] * Math.PI / 180), 1, 0, 0);
            return skewing;
        }

        //if only one coordinate the second one is zero
        static Matrix CreateForRotate(IList<float> values)
        {
            if (values.Count != 1) {
                throw new SvgParseException("Could not parse the transform");
            }
            Matrix rotation = new Matrix();
            rotation.Rotate(values[0]);
            return rotation;
        }

        //if only one coordinate the second one is zero
        static Matrix CreateForScale(IList<float> values)
        {
            Matrix scaling = new Matrix();
            if (values.Count == 1) {
                scaling.Scale(values[0], values[0]);
                return scaling;
            }
            if (values.Count != 2) {
                throw new SvgParseException("Could not parse the transform");
            }
            scaling.Scale(values[0], values[1]);
            return scaling;
        }

        //if only one coordinate the second one is zero
        static Matrix CreateForTranslate(IList<float> values)
        {
            Matrix translation = new Matrix();
            if (values.Count == 1) {
                translation.Translate(values[0], 0);
                return translation;
            }
            if (values.Count != 2) {
                throw new SvgParseException("Could not parse the transform");
            }
            translation.Translate(values[0], values[1]);

            return translation;
        }

        static Matrix CreateWithMatrix(IList<float> values)
        {
            if (values.Count != 6) {
                throw new SvgParseException("Could not parse the transform");
            }
            return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
        }

        static IList<float> GetValuesFromStr(String str)
        {
            try
            {
                int index = str.IndexOf('(') + 1;
                int count = str.IndexOf(')') - index;
                String numbers = str.Substring(index, count);
                IList<float> result = new List<float>();
                StringTokenizer tokenizer = new StringTokenizer(numbers, ",");
                while (tokenizer.HasMoreTokens())
                {
                    result.Add(float.Parse(tokenizer.NextToken()));
                }
                return result;
            }
            catch 
            {
                throw new SvgParseException("Could not parse the transform");
            }
        }
    }	
}
