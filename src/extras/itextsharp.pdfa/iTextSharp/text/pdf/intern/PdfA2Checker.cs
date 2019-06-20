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
using System.Text;
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.intern
{
    public class PdfA2Checker : PdfAChecker {
        public static readonly HashSet2<PdfName> allowedBlendModes = new HashSet2<PdfName>(new PdfName[] {PdfGState.BM_NORMAL,
            PdfGState.BM_COMPATIBLE, PdfGState.BM_MULTIPLY, PdfGState.BM_SCREEN, PdfGState.BM_OVERLAY,
            PdfGState.BM_DARKEN, PdfGState.BM_LIGHTEN, PdfGState.BM_COLORDODGE, PdfGState.BM_COLORBURN,
            PdfGState.BM_HARDLIGHT, PdfGState.BM_SOFTLIGHT, PdfGState.BM_DIFFERENCE, PdfGState.BM_EXCLUSION});

        public static readonly HashSet2<PdfName> restrictedActions = new HashSet2<PdfName>(new PdfName[] {PdfName.LAUNCH,
            PdfName.SOUND, PdfName.MOVIE, PdfName.RESETFORM, PdfName.IMPORTDATA, PdfName.HIDE, PdfName.SETOCGSTATE,
            PdfName.RENDITION, PdfName.TRANS, PdfName.GOTO3DVIEW, PdfName.JAVASCRIPT});

        private static HashSet2<PdfName> allowedAnnotTypes = new HashSet2<PdfName>(new PdfName[] {PdfName.TEXT, PdfName.LINK,
            PdfName.FREETEXT, PdfName.LINE, PdfName.SQUARE, PdfName.CIRCLE, PdfName.POLYGON, PdfName.POLYLINE, PdfName.HIGHLIGHT, PdfName.UNDERLINE,
            PdfName.SQUIGGLY, PdfName.STRIKEOUT, PdfName.STAMP, PdfName.CARET, PdfName.INK, PdfName.POPUP, PdfName.FILEATTACHMENT, PdfName.WIDGET,
            PdfName.PRINTERMARK, PdfName.TRAPNET, PdfName.WATERMARK, PdfName.REDACT});

        public static readonly HashSet2<PdfName> contentAnnotations = new HashSet2<PdfName>(new PdfName[] {PdfName.TEXT,
            PdfName.FREETEXT, PdfName.LINE, PdfName.SQUARE, PdfName.CIRCLE, PdfName.STAMP, PdfName.INK, PdfName.POPUP});

        private static readonly HashSet2<PdfName> keysForCheck = new HashSet2<PdfName>(new PdfName[] {PdfName.AP, PdfName.N,
            PdfName.R, PdfName.D, PdfName.FONTFILE, PdfName.FONTFILE2, PdfName.FONTFILE3, PdfName.NAME, PdfName.XFA,
            PdfName.ALTERNATEPRESENTATION, PdfName.DOCMDP, PdfName.REFERENCE, new PdfName("DigestLocation"),
            new PdfName("DigestMethod"), new PdfName("DigestValue"), PdfName.MARKED, PdfName.S, PdfName.SUBTYPE,
            PdfName.F});

        static public readonly PdfName DIGESTLOCATION = new PdfName("DigestLocation");
        static public readonly PdfName DIGESTMETHOD = new PdfName("DigestMethod");
        static public readonly PdfName DIGESTVALUE = new PdfName("DigestValue");

        static readonly int maxPageSize = 14400;
        static readonly int minPageSize = 3;
        protected int gsStackDepth = 0;
        protected bool rgbUsed = false;
        protected bool cmykUsed = false;
        protected bool grayUsed = false;
        protected bool transparencyWithoutPageGroupDetected = false;
        protected bool transparencyDetectedOnThePage = false;

        
        static public readonly int maxStringLength = 32767;


        internal PdfA2Checker(PdfAConformanceLevel conformanceLevel)
            :base(conformanceLevel) {
        }

        protected override HashSet2<PdfName> InitKeysForCheck() {
            return keysForCheck;
        }

        protected override void CheckFont(PdfWriter writer, int key, Object obj1) {
            BaseFont bf = (BaseFont) obj1;
            if (bf.FontType == BaseFont.FONT_TYPE_DOCUMENT) {
                PdfStream prs = null;
                PdfDictionary fontDictionary = ((DocumentFont) bf).FontDictionary;
                PdfDictionary fontDescriptor = GetDirectDictionary(fontDictionary.Get(PdfName.FONTDESCRIPTOR));
                if (fontDescriptor != null) {
                    prs = GetDirectStream(fontDescriptor.Get(PdfName.FONTFILE));
                    if (prs == null) {
                        prs = GetDirectStream(fontDescriptor.Get(PdfName.FONTFILE2));
                    }
                    if (prs == null) {
                        prs = GetDirectStream(fontDescriptor.Get(PdfName.FONTFILE3));
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
            if (obj1 is PdfObject) {
                PdfDictionary gs = GetDirectDictionary((PdfObject) obj1);
                PdfObject obj = gs.Get(PdfName.BM);
                if (obj != null) {
                    if (!allowedBlendModes.Contains((PdfName)obj))
                        throw new PdfAConformanceException(
                            MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", obj.ToString()));
                    if (!PdfGState.BM_NORMAL.Equals(obj))
                        transparencyDetectedOnThePage = true;
                }

                PdfNumber ca = gs.GetAsNumber(PdfName.ca);
                if (ca != null && ca.FloatValue < 1f) {
                    transparencyDetectedOnThePage = true;
                }

                ca = gs.GetAsNumber(PdfName.CA);
                if (ca != null && ca.FloatValue < 1f) {
                    transparencyDetectedOnThePage = true;
                }

                PdfDictionary smask = GetDirectDictionary(gs.Get(PdfName.SMASK));
                if (smask != null) {
                    transparencyDetectedOnThePage = true;
                }

                if (gs.Contains(PdfName.TR)) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("an.extgstate.dictionary.shall.not.contain.the.tr.key"));
                }

                PdfName tr2 = gs.GetAsName(PdfName.TR2);
                if (tr2 != null && !tr2.Equals(PdfName.DEFAULT)) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage(
                            "an.extgstate.dictionary.shall.not.contain.the.TR2.key.with.a.value.other.than.default"));
                }

                if (gs.Contains(PdfName.HTP)) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("an.extgstate.dictionary.shall.not.contain.the.htp.key"));
                }

                PdfDictionary halfTone = GetDirectDictionary(gs.Get(PdfName.HT));
                if (halfTone != null) {
                    if (halfTone.Contains(PdfName.HALFTONENAME))
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("halftones.shall.not.contains.halftonename"));

                    PdfNumber halftoneType = halfTone.GetAsNumber(PdfName.HALFTONETYPE);
                    if (halftoneType == null || (halftoneType.IntValue != 1 && halftoneType.IntValue != 5))
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("all.halftones.shall.have.halftonetype.1.or.5"));
                }
                PdfName ri = gs.GetAsName(PdfName.RI);
                if (ri != null &&
                    !(PdfName.RELATIVECOLORIMETRIC.Equals(ri) || PdfName.ABSOLUTECOLORIMETRIC.Equals(ri) ||
                      PdfName.PERCEPTUAL.Equals(ri) || PdfName.SATURATION.Equals(ri))) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("1.value.of.ri.key.is.not.allowed", ri.ToString()));
                }
            }
        }

        protected override void CheckImage(PdfWriter writer, int key, Object obj1) {
            PdfImage pdfImage = (PdfImage) obj1;
            if (GetDirectStream(pdfImage.Get(PdfName.SMASK)) != null) {
                transparencyDetectedOnThePage = true;
            }
            PdfNumber smaskInData = pdfImage.GetAsNumber(PdfName.SMASKINDATA);
            if (smaskInData != null && smaskInData.FloatValue > 0) {
                transparencyDetectedOnThePage = true;
            }
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

        protected override void CheckFormXObj(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfTemplate) {
                if (((PdfTemplate) obj1).Group != null) {
                    transparencyDetectedOnThePage = true;
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
                PdfDictionary d = GetDirectDictionary(properties.Get(PdfName.D));
                if (d != null)
                    configsList.Add(d);
                PdfArray configs = GetDirectArray(properties.Get(PdfName.CONFIGS));
                if (configs != null) {
                    for (int i = 0; i < configs.Size; i++) {
                        PdfDictionary config = GetDirectDictionary(configs[i]);
                        if (config != null)
                            configsList.Add(config);
                    }
                }
                HashSet2<PdfObject> ocgs = new HashSet2<PdfObject>();
                PdfArray ocgsArray = GetDirectArray(properties.Get(PdfName.OCGS));
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
                    PdfArray orderArray = GetDirectArray(config.Get(PdfName.ORDER));
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
                        PdfDictionary decodeParams = GetDirectDictionary(stream.Get(PdfName.DECODEPARMS));
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
                            PdfArray decodeParams = GetDirectArray(stream.Get(PdfName.DECODEPARMS));
                            if (decodeParams != null && i < decodeParams.Size) {
                                PdfDictionary decodeParam = GetDirectDictionary(decodeParams[i]);
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
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.form.xobject.dictionary.shall.not.contain.ps.key"));
                    }
                }

                if (PdfName.PS.Equals(stream.GetAsName(PdfName.SUBTYPE))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("postscript.xobjects.are.not.allowed"));
                }
            }
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfFileSpecification) {
                PdfDictionary fileSpec = (PdfFileSpecification) obj1;
                if (!fileSpec.Contains(PdfName.UF) || !fileSpec.Contains(PdfName.F) || !fileSpec.Contains(PdfName.DESC)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("file.specification.dictionary.shall.contain.f.uf.and.desc.entries"));
                }

                if (fileSpec.Contains(PdfName.EF)) {
                    PdfDictionary dict = GetDirectDictionary(fileSpec.Get(PdfName.EF));
                    if (dict == null || !dict.Contains(PdfName.F)) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("ef.key.of.file.specification.dictionary.shall.contain.dictionary.with.valid.f.key"));
                    }

                    PdfDictionary embeddedFile = GetDirectDictionary(dict.Get(PdfName.F));
                    if (embeddedFile == null) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("ef.key.of.file.specification.dictionary.shall.contain.dictionary.with.valid.f.key"));
                    }

                    CheckEmbeddedFile(embeddedFile);
                }
            }
        }

        private static PdfName MimeTypePdf = new PdfName(PdfAWriter.MimeTypePdf);

        protected virtual void CheckEmbeddedFile(PdfDictionary embeddedFile) {
            PdfName subtype = embeddedFile.GetAsName(PdfName.SUBTYPE);
            if (subtype == null || !MimeTypePdf.Equals(subtype)) {
                throw new PdfAConformanceException(embeddedFile, MessageLocalization.GetComposedMessage("embedded.file.shall.contain.pdf.mime.type"));
            }
        }

        protected override void CheckPdfObject(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfNumber) {
                PdfNumber number = (PdfNumber) obj1;
                if (Math.Abs(number.DoubleValue) > float.MaxValue && number.ToString().Contains(".")) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("real.number.is.out.of.range"));
                }
            } else if (obj1 is PdfString) {
                PdfString str = (PdfString) obj1;
                if (str.GetBytes().Length > maxStringLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.string.is.too.long"));
                }
            }  else if (obj1 is PdfDictionary) {
                PdfDictionary dictionary = (PdfDictionary) obj1;
                PdfName type = dictionary.GetAsName(PdfName.TYPE);
                if (PdfName.CATALOG.Equals(type)) {
                    if (!dictionary.Contains(PdfName.METADATA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.contain.metadata"));
                    }
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
                        PdfDictionary acroForm = GetDirectDictionary(dictionary.Get(PdfName.ACROFORM));
                        if (acroForm != null && acroForm.Contains(PdfName.XFA)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.acroform.xfa.entry"));
                        }
                    }

                    if (dictionary.Contains(PdfName.NAMES)) {
                        PdfDictionary names = GetDirectDictionary(dictionary.Get(PdfName.NAMES));
                        if (names != null && names.Contains(PdfName.ALTERNATEPRESENTATION)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.alternatepresentation.names.entry"));
                        }
                    }


                    PdfDictionary permissions = GetDirectDictionary(dictionary.Get(PdfName.PERMS));
                    if (permissions != null) {
                        foreach (PdfName dictKey in permissions.Keys) {
                            if (PdfName.DOCMDP.Equals(dictKey)) {
                                PdfDictionary signatureDict = GetDirectDictionary(permissions.Get(PdfName.DOCMDP));
                                if (signatureDict != null) {
                                    PdfArray references = GetDirectArray(signatureDict.Get(PdfName.REFERENCE));
                                    if (references != null) {
                                        for (int i = 0; i < references.Size; i++) {
                                            PdfDictionary referenceDict = GetDirectDictionary(references[i]);
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
                        PdfDictionary markInfo = GetDirectDictionary(dictionary.Get(PdfName.MARKINFO));
                        if (markInfo == null || markInfo.GetAsBoolean(PdfName.MARKED) == null || markInfo.GetAsBoolean(PdfName.MARKED).BooleanValue == false) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("document.catalog.dictionary.shall.include.a.markinfo.dictionary.whose.entry.marked.shall.have.a.value.of.true"));
                        }
                        if (!dictionary.Contains(PdfName.LANG)) {
                            if (LOGGER.IsLogging(Level.WARN)) {
                                LOGGER.Warn(MessageLocalization.GetComposedMessage("document.catalog.dictionary.should.contain.lang.entry"));
                            }
                        }
                    }
                } else if (PdfName.PAGE.Equals(type)) {
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

                    if (transparencyDetectedOnThePage) {
                        PdfDictionary group = GetDirectDictionary(dictionary.Get(PdfName.GROUP));
                        if (group == null || !PdfName.TRANSPARENCY.Equals(group.GetAsName(PdfName.S)) ||
                            !group.Contains(PdfName.CS)) {
                            transparencyWithoutPageGroupDetected = true;
                        }
                        else {
                            PdfName csName = group.GetAsName(PdfName.CS);
                            if (PdfName.DEVICERGB.Equals(csName))
                                rgbUsed = true;
                            if (PdfName.DEVICEGRAY.Equals(csName))
                                grayUsed = true;
                            if (PdfName.DEVICECMYK.Equals(csName))
                                cmykUsed = true;
                        }
                    }
                    transparencyDetectedOnThePage = false;
                } else if (PdfName.OUTPUTINTENT.Equals(type)) {
                    isCheckOutputIntent = true;
                    PdfObject destOutputIntent = dictionary.Get(PdfName.DESTOUTPUTPROFILE);
                    if (destOutputIntent != null && pdfaDestOutputIntent != null) {
                        if (pdfaDestOutputIntent.IndRef != destOutputIntent.IndRef)
                            throw new PdfAConformanceException(obj1,
                                MessageLocalization.GetComposedMessage(
                                    "if.outputintents.array.more.than.one.entry.the.same.indirect.object"));
                    } else {
                        pdfaDestOutputIntent = destOutputIntent;
                    }

                    PdfName gts = dictionary.GetAsName(PdfName.S);
                    if (pdfaDestOutputIntent != null) {
                        if (PdfName.GTS_PDFA1.Equals(gts)) {
                            if (pdfaOutputIntentColorSpace != null)
                                throw new PdfAConformanceException(obj1,
                                    MessageLocalization.GetComposedMessage("a.pdfa.file.may.have.only.one.pdfa.outputintent"));
                            pdfaOutputIntentColorSpace = "";
                        }

                        String deviceClass = "";
                        ICC_Profile icc_profile = writer.ColorProfile;
                        if (PdfName.GTS_PDFA1.Equals(gts))
                            pdfaOutputIntentColorSpace = Encoding.GetEncoding("US-ASCII").GetString(icc_profile.Data, 16, 4);
                        deviceClass = Encoding.GetEncoding("US-ASCII").GetString(icc_profile.Data, 12, 4);
                        if (!"prtr".Equals(deviceClass) && !"mntr".Equals(deviceClass))
                            throw new PdfAConformanceException(obj1,
                                MessageLocalization.GetComposedMessage("outputintent.shall.be.prtr.or.mntr"));
                    } else {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("outputintent.shall.have.gtspdfa1.and.destoutputintent"));
                    }
                } else if (PdfName.EMBEDDEDFILE.Equals(type)) {
                    CheckEmbeddedFile(dictionary);
                }
                PdfObject obj2 = dictionary.Get(PdfName.HALFTONETYPE);
                if (obj2 != null && obj2.IsNumber()) {
                    PdfNumber number = (PdfNumber) obj2;
                    if (number.IntValue != 1 || number.IntValue != 5) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("an.extgstate.dictionary.shall.contain.the.halftonetype.key.of.value.1.or.5"));
                    }

                    if (dictionary.Contains(PdfName.HALFTONENAME)) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("an.extgstate.dictionary.shall.not.contain.the.halftonename.key"));
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
            switch (key) {
                case PdfIsoKeys.PDFISOKEY_COLOR:
                    if (obj1 is ExtendedColor) {
                        ExtendedColor ec = (ExtendedColor) obj1;
                        switch (ec.Type) {
                            case ExtendedColor.TYPE_CMYK:
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_CMYK, obj1);
                                break;
                            case ExtendedColor.TYPE_GRAY:
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_GRAY, obj1);
                                return;
                            case ExtendedColor.TYPE_RGB:
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_RGB, obj1);
                                break;
                            case ExtendedColor.TYPE_SEPARATION:
                                SpotColor sc = (SpotColor) ec;
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_COLOR, sc.PdfSpotColor.AlternativeCS);
                                break;
                            case ExtendedColor.TYPE_SHADING:
                                ShadingColor xc = (ShadingColor) ec;
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_COLOR, xc.PdfShadingPattern.Shading.ColorSpace);
                                break;
                            case ExtendedColor.TYPE_PATTERN:
                                PatternColor pc = (PatternColor) ec;
                                CheckColor(writer, PdfIsoKeys.PDFISOKEY_COLOR, pc.Painter.DefaultColor);
                                break;
                        }
                    }
                    else if (obj1 is BaseColor)
                        CheckColor(writer, PdfIsoKeys.PDFISOKEY_RGB, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_CMYK:
                    cmykUsed = true;
                    break;
                case PdfIsoKeys.PDFISOKEY_RGB:
                    rgbUsed = true;
                    break;
                case PdfIsoKeys.PDFISOKEY_GRAY:
                    grayUsed = true;
                    break;
            }
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
                if (subtype == null) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("annotation.type.1.not.allowed", "null"));
                }
                if (subtype != null && !allowedAnnotTypes.Contains((PdfName)subtype)) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("annotation.type.1.not.allowed", subtype.ToString()));
                }

                if (!PdfName.POPUP.Equals(annot.GetAsName(PdfName.SUBTYPE))) {
                    PdfNumber f = annot.GetAsNumber(PdfName.F);
                    if (f == null) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("an.annotation.dictionary.shall.contain.the.f.key"));
                    }
                    int flags = f.IntValue;
                    if (CheckFlag(flags, PdfAnnotation.FLAGS_PRINT) == false
                        || CheckFlag(flags, PdfAnnotation.FLAGS_HIDDEN) == true
                        || CheckFlag(flags, PdfAnnotation.FLAGS_INVISIBLE) == true
                        || CheckFlag(flags, PdfAnnotation.FLAGS_NOVIEW) == true
                        || CheckFlag(flags, PdfAnnotation.FLAGS_TOGGLENOVIEW) == true) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage(
                                "the.f.keys.print.flag.bit.shall.be.set.to.1.and.its.hidden.invisible.noview.and.togglenoview.flag.bits.shall.be.set.to.0"));
                    }
                    if (PdfName.TEXT.Equals(annot.GetAsName(PdfName.SUBTYPE))) {
                        if (CheckFlag(flags, PdfAnnotation.FLAGS_NOZOOM) == false ||
                            CheckFlag(flags, PdfAnnotation.FLAGS_NOROTATE) == false) {
                            throw new PdfAConformanceException(obj1,
                                MessageLocalization.GetComposedMessage(
                                    "text.annotations.should.set.the.nozoom.and.norotate.flag.bits.of.the.f.key.to.1"));
                        }
                    }
                }

                if (PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE)) && (annot.Contains(PdfName.AA) || annot.Contains(PdfName.A))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("widget.annotation.dictionary.or.field.dictionary.shall.not.include.a.or.aa.entry"));
                }

                if (CheckStructure(conformanceLevel)) {
                    if (subtype is PdfName && contentAnnotations.Contains((PdfName)subtype) && !annot.Contains(PdfName.CONTENTS))
                    {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("annotation.of.type.1.should.have.contents.key", subtype.ToString()));
                    }
                }

                PdfDictionary ap = GetDirectDictionary(annot.Get(PdfName.AP));
                if (ap != null) {
                    if (ap.Contains(PdfName.R) || ap.Contains(PdfName.D)) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage(
                                "appearance.dictionary.shall.contain.only.the.n.key.with.stream.value"));
                    }
                    PdfObject n = GetDirectObject(ap.Get(PdfName.N));
                    if (PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE)) &&
                        new PdfName("Btn").Equals(annot.GetAsName(PdfName.FT))) {
                        if (n == null || (!n.IsDictionary() && n.Type != 0))
                            throw new PdfAConformanceException(obj1,
                                MessageLocalization.GetComposedMessage(
                                    "appearance.dictionary.of.widget.subtype.and.btn.field.type.shall.contain.only.the.n.key.with.dictionary.value"));
                    }
                    else {
                        if (n == null || (!n.IsStream() && n.Type != 0))
                            throw new PdfAConformanceException(obj1,
                                MessageLocalization.GetComposedMessage(
                                    "appearance.dictionary.shall.contain.only.the.n.key.with.stream.value"));
                    }
                }
                else {
                    bool isCorrectRect = false;
                    PdfArray rect = GetDirectArray(annot.Get(PdfName.RECT));
                    if (rect != null && rect.Size == 4) {
                        PdfNumber index0 = rect.GetAsNumber(0);
                        PdfNumber index1 = rect.GetAsNumber(1);
                        PdfNumber index2 = rect.GetAsNumber(2);
                        PdfNumber index3 = rect.GetAsNumber(3);
                        if (index0 != null && index1 != null && index2 != null && index3 != null &&
                            index0.DoubleValue == index2.DoubleValue && index1.DoubleValue == index3.DoubleValue)
                            isCorrectRect = true;
                    }
                    if (!PdfName.POPUP.Equals(annot.GetAsName(PdfName.SUBTYPE)) &&
                        !PdfName.LINK.Equals(annot.GetAsName(PdfName.SUBTYPE)) &&
                        !isCorrectRect)
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage(
                                "every.annotation.shall.have.at.least.one.appearance.dictionary"));
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

        protected override void CheckOutputIntent(PdfWriter writer, int key, Object obj1) {
            if (writer is PdfAStamperImp && writer.ColorProfile != null)
                throw new PdfAConformanceException(obj1,
                    MessageLocalization.GetComposedMessage("outputintent.shall.not.be.updated"));
        }

        private void FillOrderRecursively(PdfArray orderArray, HashSet2<PdfObject> order) {
            for (int i = 0; i < orderArray.Size; i++) {
                PdfArray orderChild = GetDirectArray(orderArray[i]);
                if (orderChild == null) {
                    order.Add(orderArray[i]);
                } else {
                    FillOrderRecursively(orderChild, order);
                }
            }
        }

        public override void Close(PdfWriter writer) {
            CheckOutputIntentsInStamperMode(writer);
            if (pdfaOutputIntentColorSpace != null) {
                if ("RGB ".Equals(pdfaOutputIntentColorSpace)) {
                    if (cmykUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTCMYK) == null)
                        throw new PdfAConformanceException(null,
                            MessageLocalization.GetComposedMessage(
                                "devicecmyk.shall.only.be.used.if.defaultcmyk.pdfa.or.outputintent"));
                } else if ("CMYK".Equals(pdfaOutputIntentColorSpace)) {
                    if (rgbUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTRGB) == null)
                        throw new PdfAConformanceException(null,
                            MessageLocalization.GetComposedMessage(
                                "devicergb.shall.only.be.used.if.defaultrgb.pdfa.or.outputintent"));
                } else if ("GRAY".Equals(pdfaOutputIntentColorSpace)) {
                    if (rgbUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTRGB) == null)
                        throw new PdfAConformanceException(null,
                            MessageLocalization.GetComposedMessage(
                                "devicergb.shall.only.be.used.if.defaultrgb.pdfa.or.outputintent"));
                    if (cmykUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTCMYK) == null)
                        throw new PdfAConformanceException(null,
                            MessageLocalization.GetComposedMessage(
                                "devicecmyk.shall.only.be.used.if.defaultcmyk.pdfa.or.outputintent"));
                } else {
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage("outputintent.shall.have.colourspace.gray.rgb.or.cmyk"));
                }
            } else {
                if (rgbUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTRGB) == null) {
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage("devicergb.shall.only.be.used.if.defaultrgb.pdfa.or.outputintent"));
                }
                if (cmykUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTCMYK) == null) {
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage("devicecmyk.shall.only.be.used.if.defaultcmyk.pdfa.or.outputintent"));
                }
                if (grayUsed && writer.DefaultColorspace.Get(PdfName.DEFAULTGRAY) == null) {
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage("devicegray.shall.only.be.used.if.defaultgray.pdfa.or.outputintent"));
                }
                if (transparencyWithoutPageGroupDetected) {
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage(
                            "if.the.document.not.contain.outputintent.transparencygroup.shall.comtain.cs.key"));
                }
            }
        }
    }
}
