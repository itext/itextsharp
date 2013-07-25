using System;
using System.Collections.Generic;
using System.util.collections;
using iTextSharp.text.error_messages;
/*
 * $Id: PdfA2Checker.java 5827 2013-05-31 08:56:23Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Alexander Chingarev, Bruno Lowagie, et al.
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

namespace iTextSharp.text.pdf.intern
{
public class PdfA2Checker : PdfA1Checker {

    private static readonly HashSet2<PdfName> allowedBlendModes;

    internal PdfA2Checker(PdfAConformanceLevel conformanceLevel)
        :base(conformanceLevel) {
    }


    static PdfA2Checker() {
        allowedBlendModes = new HashSet2<PdfName>();
        allowedBlendModes.Add(PdfGState.BM_NORMAL);
        allowedBlendModes.Add(PdfGState.BM_COMPATIBLE);
        allowedBlendModes.Add(PdfGState.BM_MULTIPLY);
        allowedBlendModes.Add(PdfGState.BM_SCREEN);
        allowedBlendModes.Add(PdfGState.BM_OVERLAY);
        allowedBlendModes.Add(PdfGState.BM_DARKEN);
        allowedBlendModes.Add(PdfGState.BM_LIGHTEN);
        allowedBlendModes.Add(PdfGState.BM_COLORDODGE);
        allowedBlendModes.Add(PdfGState.BM_COLORBURN);
        allowedBlendModes.Add(PdfGState.BM_HARDLIGHT);
        allowedBlendModes.Add(PdfGState.BM_SOFTLIGHT);
        allowedBlendModes.Add(PdfGState.BM_DIFFERENCE);
        allowedBlendModes.Add(PdfGState.BM_EXCLUSION);
    }

    protected override void CheckGState(PdfWriter writer, int key, Object obj1) {
        PdfDictionary gs = (PdfDictionary) obj1;
        PdfObject obj = gs.Get(PdfName.BM);
        if (obj != null && !allowedBlendModes.Contains((PdfName)obj)) {
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", obj.ToString()));
        }
    }

    protected override void CheckImage(PdfWriter writer, int key, Object obj1) {
        PdfImage pdfImage = (PdfImage) obj1;
        if (pdfImage != null && (pdfImage.Image is Jpeg2000)) {
            Jpeg2000 jpeg2000 = (Jpeg2000) pdfImage.Image;
            if (!jpeg2000.IsJp2()) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.jpx.baseline.set.of.features.shall.be.used"));
            }
            if (jpeg2000.GetNumOfComps() != 1 && jpeg2000.GetNumOfComps() != 3 && jpeg2000.GetNumOfComps() != 4) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.number.of.colour.channels.in.the.jpeg2000.data.shall.be.123"));
            }
            if (jpeg2000.Bpc < 1 || jpeg2000.Bpc > 38) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.bit-depth.of.the.jpeg2000.data.shall.have.a.value.in.the.range.1to38"));
            }
            if (jpeg2000.GetBpcBoxData() != null) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.colour.channels.in.the.jpeg2000.data.shall.have.the.same.bit-depth"));
            }
            List<Jpeg2000.ColorSpecBox> colorSpecBoxes = jpeg2000.GetColorSpecBoxes();
            if (colorSpecBoxes != null) {
                if (colorSpecBoxes.Count > 1) {
                    int approx0x01 = 0;
                    foreach (Jpeg2000.ColorSpecBox colorSpecBox in colorSpecBoxes) {
                        if (colorSpecBox.GetApprox() == 1)
                            approx0x01++;
                    }
                    if (approx0x01 != 1) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("exactly.one.colour.space.specification.shall.have.the.value.0x01.in.the.approx.field"));
                    }
                }
                foreach (Jpeg2000.ColorSpecBox colorSpecBox in colorSpecBoxes) {
                    if (colorSpecBox.GetMeth() != 1 && colorSpecBox.GetMeth() != 2 && colorSpecBox.GetMeth() != 3) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.value.of.the.meth.entry.in.colr.box.shall.be.123"));
                    }
                    if (colorSpecBox.GetEnumCs() == 19) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("jpeg2000.enumerated.colour.space.19.(CIEJab).shall.not.be.used"));
                    }
                    byte[] colorProfileBytes = colorSpecBox.GetColorProfile();
                    if (colorProfileBytes != null) {
                        //ICC profile verification should follow here.
                    }
                }

            }
        }
    }

    protected override void CheckLayer(PdfWriter writer, int key, Object obj1) {
        if (obj1 is IPdfOCG) {

        } else if (obj1 is PdfOCProperties) {
            PdfOCProperties properties = (PdfOCProperties)obj1;
            List<PdfDictionary> configsList = new List<PdfDictionary>();
            PdfDictionary d = properties.GetAsDict(PdfName.D);
            if (d != null)
                configsList.Add(d);
            PdfArray configs = properties.GetAsArray(PdfName.CONFIGS);
            if (configs != null) {
                for (int i = 0; i < configs.Size; i++) {
                    PdfDictionary config = configs.GetAsDict(i);
                    if (config != null)
                        configsList.Add(config);
                }
            }
            HashSet2<PdfObject> ocgs = new HashSet2<PdfObject>();
            PdfArray ocgsArray = properties.GetAsArray(PdfName.OCGS);
            if (ocgsArray != null)
                for (int i = 0; i < ocgsArray.Size; i++)
                    ocgs.Add(ocgsArray[i]);
            HashSet2<String> names = new HashSet2<String>();
            HashSet2<PdfObject> order = new HashSet2<PdfObject>();
            foreach (PdfDictionary config in configsList) {
                PdfString name = config.GetAsString(PdfName.NAME);
                if (name == null) {
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("optional.content.configuration.dictionary.shall.contain.name.entry"));
                }
                String name1 = name.ToUnicodeString();
                if (names.Contains(name1)) {
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("value.of.name.entry.shall.be.unique.amongst.all.optional.content.configuration.dictionaries"));
                }
                names.Add(name1);
                if (config.Contains(PdfName.AS)) {
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.as.key.shall.not.appear.in.any.optional.content.configuration.dictionary"));
                }
                PdfArray orderArray = config.GetAsArray(PdfName.ORDER);
                if (orderArray != null)
                    fillOrderRecursively(orderArray, order);
            }
            if (order.Count != ocgs.Count) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("order.array.shall.contain.references.to.all.ocgs"));
            }
            ocgs.RetainAll(order);
            if (order.Count != ocgs.Count) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("order.array.shall.contain.references.to.all.ocgs"));
            }
        } else {

        }
    }

    private void fillOrderRecursively(PdfArray orderArray, HashSet2<PdfObject> order) {
        for (int i = 0; i < orderArray.Size; i++) {
            PdfArray orderChild = orderArray.GetAsArray(i);
            if (orderChild == null) {
                order.Add(orderArray[i]);
            } else {
                fillOrderRecursively(orderChild, order);
            }
        }
    }

}

}