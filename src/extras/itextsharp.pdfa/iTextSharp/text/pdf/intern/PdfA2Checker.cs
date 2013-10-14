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
    public class PdfA2Checker : PdfAChecker {

        static public readonly HashSet2<PdfName> allowedBlendModes = new HashSet2<PdfName>(new PdfName[]{PdfGState.BM_NORMAL, PdfGState.BM_COMPATIBLE,
                PdfGState.BM_MULTIPLY, PdfGState.BM_SCREEN, PdfGState.BM_OVERLAY, PdfGState.BM_DARKEN, PdfGState.BM_LIGHTEN, PdfGState.BM_COLORDODGE,
                PdfGState.BM_COLORBURN, PdfGState.BM_HARDLIGHT, PdfGState.BM_SOFTLIGHT, PdfGState.BM_DIFFERENCE, PdfGState.BM_EXCLUSION});

        static public readonly HashSet2<PdfName> restrictedActions = new HashSet2<PdfName>(new PdfName[] {PdfName.LAUNCH, PdfName.SOUND,
                PdfName.MOVIE, PdfName.RESETFORM, PdfName.IMPORTDATA, PdfName.HIDE, PdfName.SETOCGSTATE, PdfName.RENDITION, PdfName.TRANS, PdfName.GOTO3DVIEW, PdfName.JAVASCRIPT});

        static public readonly PdfName DIGESTLOCATION = new PdfName("DigestLocation");
        static public readonly PdfName DIGESTMETHOD = new PdfName("DigestMethod");
        static public readonly PdfName DIGESTVALUE = new PdfName("DigestValue");

        static readonly int maxPageSize = 14400;
        static readonly int minPageSize = 3;
        protected int gsStackDepth = 0;

        internal PdfA2Checker(PdfAConformanceLevel conformanceLevel)
            :base(conformanceLevel) {
        }

        protected override void CheckFont(PdfWriter writer, int key, Object obj1) {
            BaseFont bf = (BaseFont) obj1;
            if (bf.FontType == BaseFont.FONT_TYPE_DOCUMENT) {
                PdfStream prs = null;
                PdfDictionary fontDictionary = ((DocumentFont) bf).FontDictionary;
                PdfDictionary fontDescriptor = fontDictionary.GetAsDict(PdfName.FONTDESCRIPTOR);
                if (fontDescriptor != null) {
                    prs = fontDescriptor.GetAsStream(PdfName.FONTFILE);
                    if (prs == null) {
                        prs = fontDescriptor.GetAsStream(PdfName.FONTFILE2);
                    }
                    if (prs == null) {
                        prs = fontDescriptor.GetAsStream(PdfName.FONTFILE3);
                    }
                }
                if (prs == null) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1",
                            ((BaseFont) obj1).PostscriptFontName));
                }
            }
            else {
                if (!bf.IsEmbedded())
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1",
                            ((BaseFont) obj1).PostscriptFontName));
            }
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
            if (pdfImage.Contains(PdfName.OPI)) {
                throw new PdfAConformanceException(obj1,
                    MessageLocalization.GetComposedMessage("an.image.dictionary.shall.not.contain.opi.key"));
            }
            PdfBoolean interpolate = pdfImage.GetAsBoolean(PdfName.INTERPOLATE);
            if (interpolate != null && interpolate.BooleanValue) {
                throw new PdfAConformanceException(obj1,
                    MessageLocalization.GetComposedMessage("the.value.of.interpolate.key.shall.not.be.true"));
            }
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

        protected override void CheckInlineImage(PdfWriter writer, int key, Object obj1) {
            PdfImage pdfImage = (PdfImage) obj1;
            PdfBoolean interpolate = pdfImage.GetAsBoolean(PdfName.INTERPOLATE);
            if (interpolate != null && interpolate.BooleanValue) {
                throw new PdfAConformanceException(obj1,
                    MessageLocalization.GetComposedMessage("the.value.of.interpolate.key.shall.not.be.true"));
            }

            PdfObject filter = pdfImage.GetDirectObject(PdfName.FILTER);
            if (filter is PdfName) {
                if (filter.Equals(PdfName.LZWDECODE))
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                if (filter.Equals(PdfName.CRYPT)) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("crypt.filter.is.not.permitted.inline.image"));
                }
            }
            else if (filter is PdfArray) {
                for (int i = 0; i < ((PdfArray) filter).Size; i++) {
                    PdfName f = ((PdfArray) filter).GetAsName(i);
                    if (f.Equals(PdfName.LZWDECODE))
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                    if (f.Equals(PdfName.CRYPT)) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("crypt.filter.is.not.permitted.inline.image"));
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
                        FillOrderRecursively(orderArray, order);
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

        protected override void CheckTrailer(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfWriter.PdfTrailer) {
                PdfWriter.PdfTrailer trailer = (PdfWriter.PdfTrailer) obj1;
                if (trailer.Get(PdfName.ENCRYPT) != null) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("keyword.encrypt.shall.not.be.used.in.the.trailer.dictionary"));
                }
            }
        }

        protected override void CheckStream(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfStream) {
                PdfStream stream = (PdfStream) obj1;
                if (stream.Contains(PdfName.F) || stream.Contains(PdfName.FFILTER) || stream.Contains(PdfName.FDECODEPARMS)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("stream.object.dictionary.shall.not.contain.the.f.ffilter.or.fdecodeparams.keys"));
                }

                PdfObject filter = stream.GetDirectObject(PdfName.FILTER);
                if (filter is PdfName) {
                    if (filter.Equals(PdfName.LZWDECODE))
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                    if (filter.Equals(PdfName.CRYPT)) {
                        PdfDictionary decodeParams = stream.GetAsDict(PdfName.DECODEPARMS);
                        if (decodeParams != null) {
                            PdfString cryptFilterName = decodeParams.GetAsString(PdfName.NAME);
                            if (cryptFilterName != null && !cryptFilterName.Equals(PdfName.IDENTITY)) {
                                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("not.identity.crypt.filter.is.not.permitted"));
                            }
                        }
                    }
                } else if (filter is PdfArray) {
                    for (int i = 0; i < ((PdfArray) filter).Size; i++) {
                        PdfName f = ((PdfArray) filter).GetAsName(i);
                        if (f.Equals(PdfName.LZWDECODE))
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                        if (f.Equals(PdfName.CRYPT)) {
                            PdfArray decodeParams = stream.GetAsArray(PdfName.DECODEPARMS);
                            if (decodeParams != null && i < decodeParams.Size) {
                                PdfDictionary decodeParam = decodeParams.GetAsDict(i);
                                PdfString cryptFilterName = decodeParam.GetAsString(PdfName.NAME);
                                if (cryptFilterName != null && !cryptFilterName.Equals(PdfName.IDENTITY)) {
                                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("not.identity.crypt.filter.is.not.permitted"));
                                }
                            }
                        }
                    }
                }

                if (PdfName.FORM.Equals(stream.GetAsName(PdfName.SUBTYPE))) {
                    if (stream.Contains(PdfName.OPI)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.form.xobject.dictionary.shall.not.contain.opi.key"));
                    }
                    if (stream.Contains(PdfName.PS)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.form.xobject..dictionary.shall.not.contain.ps.key"));
                    }
                }

                if (PdfName.PS.Equals(stream.GetAsName(PdfName.SUBTYPE))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("postscript.xobjects.are.not.allowed"));
                }
            }
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfFileSpecification) {
                PdfDictionary fileSpec = (PdfFileSpecification)obj1;
                if (fileSpec.Contains(PdfName.EF) &&
                        (!fileSpec.Contains(PdfName.UF) || !fileSpec.Contains(PdfName.F))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("file.specification.dictionary.shall.contain.f.uf.and.desc.entries"));
                }
            }
        }

        protected override void CheckPdfObject(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfNumber) {
                PdfNumber number = (PdfNumber) obj1;
                if (Math.Abs(number.DoubleValue) > PdfA1Checker.maxRealValue && number.ToString().Contains(".")) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("real.number.is.out.of.range"));
                }
            } else if (obj1 is PdfString) {
                PdfString str = (PdfString) obj1;
                if (str.GetBytes().Length > PdfA1Checker.maxStringLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.string.is.too.long"));
                }
            } else if (obj1 is PdfArray) {
                PdfArray array = (PdfArray) obj1;
                if (array.Size > PdfA1Checker.maxArrayLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.array.is.out.of.bounds"));
                }
            }  else if (obj1 is PdfDictionary) {
                PdfDictionary dictionary = (PdfDictionary) obj1;
                PdfName type = dictionary.GetAsName(PdfName.TYPE);
                if (PdfName.CATALOG.Equals(type)) {
                    if (dictionary.Contains(PdfName.AA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.an.aa.entry"));
                    }

                    if (dictionary.Contains(PdfName.REQUIREMENTS)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.a.requirements.entry"));
                    }

                    if (dictionary.Contains(PdfName.NEEDRENDERING)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.a.needrendering.entry"));
                    }

                    if (dictionary.Contains(PdfName.ACROFORM)) {
                        PdfDictionary acroForm = dictionary.GetAsDict(PdfName.ACROFORM);
                        if (acroForm != null && acroForm.Contains(PdfName.XFA)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.acroform.xfa.entry"));
                        }
                    }

                    if (dictionary.Contains(PdfName.NAMES)) {
                        PdfDictionary names = dictionary.GetAsDict(PdfName.NAMES);
                        if (names != null && names.Contains(PdfName.ALTERNATEPRESENTATION)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.alternatepresentation.names.entry"));
                        }
                    }


                    PdfDictionary permissions = dictionary.GetAsDict(PdfName.PERMS);
                    if (permissions != null) {
                        foreach (PdfName dictKey in permissions.Keys) {
                            if (PdfName.DOCMDP.Equals(dictKey)) {
                                PdfDictionary signatureDict = permissions.GetAsDict(PdfName.DOCMDP);
                                if (signatureDict != null) {
                                    PdfArray references = signatureDict.GetAsArray(PdfName.REFERENCE);
                                    if (references != null) {
                                        for (int i = 0; i < references.Length; i++) {
                                            PdfDictionary referenceDict = references.GetAsDict(i);
                                            if (referenceDict.Contains(DIGESTLOCATION)
                                                    || referenceDict.Contains(DIGESTMETHOD)
                                                        || referenceDict.Contains(DIGESTVALUE)) {
                                                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("signature.references.dictionary.shall.not.contain.digestlocation.digestmethod.digestvalue"));
                                            }
                                        }
                                    }
                                }
                            } else if (PdfName.UR3.Equals(dictKey)){}
                            else {
                                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("no.keys.other.than.UR3.and.DocMDP.shall.be.present.in.a.permissions.dictionary"));
                            }
                        }
                    }

                    if (CheckStructure(conformanceLevel)) {
                        PdfDictionary markInfo = dictionary.GetAsDict(PdfName.MARKINFO);
                        if (markInfo == null || markInfo.GetAsBoolean(PdfName.MARKED) == null || markInfo.GetAsBoolean(PdfName.MARKED).BooleanValue == false) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("document.catalog.dictionary.shall.include.a.markinfo.dictionary.whose.entry.marked.shall.have.a.value.of.true"));
                        }
                        if (!dictionary.Contains(PdfName.LANG)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("document.catalog.dictionary.should.contain.lang.entry"));
                        }
                    }

                }
                if (PdfName.PAGE.Equals(type)) {
                    PdfName[] boxNames = new PdfName[] {PdfName.MEDIABOX, PdfName.CROPBOX, PdfName.TRIMBOX, PdfName.ARTBOX, PdfName.BLEEDBOX};
                    foreach (PdfName boxName in boxNames) {
                        PdfObject box = dictionary.GetDirectObject(boxName);
                        if (box is PdfRectangle) {
                            float width = ((PdfRectangle)box).Width;
                            float height = ((PdfRectangle)box).Height;
                            if (width < minPageSize || width > maxPageSize || height < minPageSize || height > maxPageSize)
                                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.page.less.3.units.nor.greater.14400.in.either.direction"));
                        }
                    }
                    if (dictionary.Contains(PdfName.AA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("page.dictionary.shall.not.include.aa.entry"));
                    }

                    if (dictionary.Contains(PdfName.PRESSTEPS)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("page.dictionary.shall.not.include.pressteps.entry"));
                    }
                }
            }
        }

        protected override void CheckCanvas(PdfWriter writer, int key, Object obj1) {
            if (obj1 is String) {
                if ("q".Equals(obj1)) {
                    if (++gsStackDepth > PdfA1Checker.maxGsStackDepth)
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("graphics.state.stack.depth.is.greater.than.28"));
                } else if ("Q".Equals(obj1)) {
                    gsStackDepth--;
                }
            }
        }

        protected override void CheckColor(PdfWriter writer, int key, Object obj1) {
            //To change body of implemented methods use File | Settings | File Templates.
        }

        protected override void CheckAnnotation(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfFormField) {
                PdfFormField field = (PdfFormField) obj1;
                if (!field.Contains(PdfName.SUBTYPE))
                    return;
                if (field.Contains(PdfName.AA) || field.Contains(PdfName.A)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("widget.annotation.dictionary.or.field.dictionary.shall.not.include.a.or.aa.entry"));
                }
            }
            if (obj1 is PdfAnnotation) {
                PdfAnnotation annot = (PdfAnnotation) obj1;
                PdfObject subtype = annot.Get(PdfName.SUBTYPE);
                if (PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE)) && (annot.Contains(PdfName.AA) || annot.Contains(PdfName.A))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("widget.annotation.dictionary.or.field.dictionary.shall.not.include.a.or.aa.entry"));
                }

                if (CheckStructure(conformanceLevel)) {
                    if (subtype is PdfName && PdfA1Checker.contentAnnotations.Contains((PdfName)subtype) && !annot.Contains(PdfName.CONTENTS))
                    {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("annotation.of.type.1.should.have.contents.key", subtype.ToString()));
                    }
                }
            }
        }

        protected override void CheckAction(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfAction) {
                PdfAction action = (PdfAction) obj1;
                PdfName s = action.GetAsName(PdfName.S);
                if (PdfA1Checker.setState.Equals(s) || PdfA1Checker.noOp.Equals(s)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("deprecated.setstate.and.noop.actions.are.not.allowed"));
                }
                if (restrictedActions.Contains(s)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("launch.sound.movie.resetform.importdata.and.javascript.actions.are.not.allowed"));
                }
                if (PdfName.NAMED.Equals(s)) {
                    PdfName n = action.GetAsName(PdfName.N);
                    if (n != null && !PdfA1Checker.allowedNamedActions.Contains(n)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("named.action.type.1.not.allowed", n.ToString()));
                    }
                }
            }
        }

        protected override void CheckForm(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfAcroForm) {
                PdfAcroForm form = (PdfAcroForm) obj1;
                PdfBoolean needAppearances = form.GetAsBoolean(PdfName.NEEDAPPEARANCES);
                if (needAppearances != null && needAppearances.BooleanValue) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("needappearances.flag.of.the.interactive.form.dictionary.shall.either.not.be.present.or.shall.be.false"));
                }
            }
        }

        protected override void CheckStructElem(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfStructureElement) {
                PdfStructureElement structElem = (PdfStructureElement) obj1;
                PdfName role = structElem.StructureType;
                if (PdfName.FIGURE.Equals(role) || PdfName.FORMULA.Equals(role) || PdfName.FORM.Equals(role)) {
                    PdfObject o = structElem.Get(PdfName.ALT);
                    if (o is PdfString && o.ToString().Length > 0) {

                    } else {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("alt.entry.should.specify.alternate.description.for.1.element", role.ToString()));
                    }
                }
            }
        }

        private void FillOrderRecursively(PdfArray orderArray, HashSet2<PdfObject> order) {
            for (int i = 0; i < orderArray.Size; i++) {
                PdfArray orderChild = orderArray.GetAsArray(i);
                if (orderChild == null) {
                    order.Add(orderArray[i]);
                } else {
                    FillOrderRecursively(orderChild, order);
                }
            }
        }

    }
}