/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.Text;
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.intern
{
    public class PdfA1Checker : PdfAChecker {
    
        public static readonly PdfName setState = new PdfName("SetState");
        public static readonly PdfName noOp = new PdfName("NoOp");

        private static HashSet2<PdfName> allowedAnnotTypes = new HashSet2<PdfName>(new PdfName[] {PdfName.TEXT, PdfName.LINK,
            PdfName.FREETEXT, PdfName.LINE, PdfName.SQUARE, PdfName.CIRCLE, PdfName.HIGHLIGHT, PdfName.UNDERLINE, PdfName.SQUIGGLY, PdfName.STRIKEOUT, PdfName.STAMP, 
            PdfName.INK, PdfName.POPUP, PdfName.WIDGET, PdfName.PRINTERMARK, PdfName.TRAPNET});

        public static readonly HashSet2<PdfName> allowedNamedActions = new HashSet2<PdfName>(new PdfName[] {
            PdfName.NEXTPAGE, PdfName.PREVPAGE, PdfName.FIRSTPAGE, PdfName.LASTPAGE});

        private static HashSet2<PdfName> restrictedActions = new HashSet2<PdfName>(new PdfName[] {PdfName.LAUNCH, PdfName.SOUND,
            PdfName.MOVIE, PdfName.RESETFORM, PdfName.IMPORTDATA, PdfName.JAVASCRIPT});

        public static readonly HashSet2<PdfName> contentAnnotations = new HashSet2<PdfName>(new PdfName[] {PdfName.TEXT,
            PdfName.FREETEXT, PdfName.LINE, PdfName.SQUARE, PdfName.CIRCLE, PdfName.STAMP, PdfName.INK, PdfName.POPUP});

        private static readonly HashSet2<PdfName> keysForCheck = new HashSet2<PdfName>(new PdfName[] {PdfName.FONTDESCRIPTOR,
            PdfName.FONTFILE, PdfName.FONTFILE2, PdfName.FONTFILE3, PdfName.FILTER, PdfName.GROUP, PdfName.S,
            PdfName.EMBEDDEDFILES, PdfName.MARKED, PdfName.DESTOUTPUTPROFILE});

        public const double maxRealValue = 32767;
        public const int maxStringLength = 65535;
        public const int maxArrayLength = 8191;
        public const int maxDictionaryLength = 4095;
        public const int maxGsStackDepth = 28;
        protected int gsStackDepth = 0;
        protected bool rgbUsed = false;
        protected bool cmykUsed = false;
        protected bool grayUsed = false;
       

        internal PdfA1Checker(PdfAConformanceLevel conformanceLevel)
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
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont) obj1).PostscriptFontName));
                }
            } else {
                if (!bf.IsEmbedded())
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont) obj1).PostscriptFontName));
            }
        }

        protected override void CheckImage(PdfWriter writer, int key, Object obj1) {
            PdfImage image = (PdfImage) obj1;
            if(image.Get(PdfName.SMASK) != null && !PdfName.NONE.Equals(image.GetAsName(PdfName.SMASK)))
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.smask.key.is.not.allowed.in.images"));
            if(image.Contains(PdfName.ALTERNATES)) {
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("an.image.dictionary.shall.not.contain.alternates.key"));
            }
            if(image.Contains(PdfName.OPI)) {
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("an.image.dictionary.shall.not.contain.opi.key"));
            }
            PdfBoolean interpolate = image.GetAsBoolean(PdfName.INTERPOLATE);
            if(interpolate != null && interpolate.BooleanValue) {
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.value.of.interpolate.key.shall.not.be.true"));
            }
            PdfName intent = image.GetAsName(PdfName.INTENT);
            if(intent != null && !(PdfName.RELATIVECOLORIMETRIC.Equals(intent) || PdfName.ABSOLUTECOLORIMETRIC.Equals(intent) || PdfName.PERCEPTUAL.Equals(intent) || PdfName.SATURATION.Equals(intent))) {
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("1.value.of.intent.key.is.not.allowed", intent.ToString()));
            }
        }

        protected override void CheckFormXObj(PdfWriter writer, int key, Object obj1) {
            //For PDF/A-1 it is enough to check a form xObject at the addToBody... only.
        }

        protected override void CheckInlineImage(PdfWriter writer, int key, Object obj1) {}

        protected override void CheckGState(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfDictionary) {
                PdfDictionary gs = (PdfDictionary) obj1;
                PdfObject obj = gs.Get(PdfName.BM);
                if (obj != null && !PdfGState.BM_NORMAL.Equals(obj) && !PdfGState.BM_COMPATIBLE.Equals(obj))
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", obj.ToString()));
                obj = gs.Get(PdfName.CA);
                double v = 0.0;
                if (obj != null && (v = ((PdfNumber) obj).DoubleValue) != 1.0)
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v.ToString()));
                obj = gs.Get(PdfName.ca);
                v = 0.0;
                if (obj != null && (v = ((PdfNumber) obj).DoubleValue) != 1.0)
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v.ToString()));

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
                PdfName ri = gs.GetAsName(PdfName.RI);
                if (ri != null &&
                    !(PdfName.RELATIVECOLORIMETRIC.Equals(ri) || PdfName.ABSOLUTECOLORIMETRIC.Equals(ri) ||
                      PdfName.PERCEPTUAL.Equals(ri) || PdfName.SATURATION.Equals(ri))) {
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("1.value.of.ri.key.is.not.allowed", ri.ToString()));
                }
                if (gs.Get(PdfName.SMASK) != null && !PdfName.NONE.Equals(gs.GetAsName(PdfName.SMASK)))
                    throw new PdfAConformanceException(obj1,
                        MessageLocalization.GetComposedMessage("the.smask.key.is.not.allowed.in.extgstate"));
            }
        }

        protected override void CheckLayer(PdfWriter writer, int key, Object obj1) {
            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("layers.are.not.allowed"));
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

                PdfObject filter = GetDirectObject(stream.Get(PdfName.FILTER));
                if (filter is PdfName) {
                    if (filter.Equals(PdfName.LZWDECODE))
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                }
                else if (filter is PdfArray) {
                    foreach (PdfObject f in ((PdfArray) filter)) {
                        if (f.Equals(PdfName.LZWDECODE))
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("lzwdecode.filter.is.not.permitted"));
                    }
                }

                if (PdfName.FORM.Equals(stream.GetAsName(PdfName.SUBTYPE))) {
                    if (stream.Contains(PdfName.OPI)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.form.xobject.dictionary.shall.not.contain.opi.key"));
                    }
                    if (stream.Contains(PdfName.PS)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.form.xobject.dictionary.shall.not.contain.ps.key"));
                    }
                    PdfDictionary group = GetDirectDictionary(stream.Get(PdfName.GROUP));
                    if (group != null) {
                        PdfName s = group.GetAsName(PdfName.S);
                        if (PdfName.TRANSPARENCY.Equals(s)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("a.group.object.with.an.s.key.with.a.value.of.transparency.shall.not.be.included.in.a.form.xobject"));
                        }
                    }
                }
                if (PdfName.PS.Equals(stream.GetAsName(PdfName.SUBTYPE))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("postscript.xobjects.are.not.allowed"));
                }
            }
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfFileSpecification) {
                PdfFileSpecification fileSpec = (PdfFileSpecification) obj1;
                if (fileSpec.Contains(PdfName.EF)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("embedded.files.are.not.permitted"));
                }
            }
        }

        protected override void CheckPdfObject(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfNumber) {
                PdfNumber number = (PdfNumber) obj1;
                if (Math.Abs(number.DoubleValue) > maxRealValue && number.ToString().Contains(".")) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("real.number.is.out.of.range"));
                }
            } else if (obj1 is PdfString) {
                PdfString str = (PdfString) obj1;
                if (str.GetBytes().Length > maxStringLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.string.is.too.long"));
                }
            } else if (obj1 is PdfArray) {
                PdfArray array = (PdfArray) obj1;
                if (array.Size > maxArrayLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.array.exceeds.length.set.by.PDFA1.standard", array.Length.ToString()));
                }
            } else if (obj1 is PdfDictionary) {
                PdfDictionary dictionary = (PdfDictionary) obj1;
                if (dictionary.Size > maxDictionaryLength) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("pdf.dictionary.is.out.of.bounds"));
                }
                PdfName type = dictionary.GetAsName(PdfName.TYPE);
                if (PdfName.CATALOG.Equals(type)) {
                    if (!dictionary.Contains(PdfName.METADATA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.contain.metadata"));
                    }

                    if (dictionary.Contains(PdfName.AA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.an.aa.entry"));
                    }

                    if (dictionary.Contains(PdfName.NAMES)) {
                        PdfDictionary names = GetDirectDictionary(dictionary.Get(PdfName.NAMES));
                        if (names != null && names.Contains(PdfName.EMBEDDEDFILES)) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.document.catalog.dictionary.shall.not.include.embeddedfiles.names.entry"));
                        }
                    }

                    if (CheckStructure(conformanceLevel)) {
                        PdfDictionary markInfo = GetDirectDictionary(dictionary.Get(PdfName.MARKINFO));
                        if (markInfo == null || markInfo.GetAsBoolean(PdfName.MARKED) == null || markInfo.GetAsBoolean(PdfName.MARKED).BooleanValue == false) {
                            throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("document.catalog.dictionary.shall.include.a.markinfo.dictionary.whose.entry.marked.shall.have.a.value.of.true"));
                        }
                        if (!dictionary.Contains(PdfName.LANG)) {
                           if(LOGGER.IsLogging(Level.WARN)){
                                LOGGER.Warn(MessageLocalization.GetComposedMessage("document.catalog.dictionary.should.contain.lang.entry"));
                            }
                        }
                    }
                }
                else if (PdfName.PAGE.Equals(type)) {
                    if (dictionary.Contains(PdfName.AA)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("page.dictionary.shall.not.include.aa.entry"));
                    }
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
                            ICC_Profile icc_profile = writer.ColorProfile;
                            pdfaOutputIntentColorSpace = Encoding.GetEncoding("US-ASCII").GetString(icc_profile.Data, 16, 4);
                        }
                    } else {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("outputintent.shall.have.gtspdfa1.and.destoutputintent"));
                    }
                }
            }
        }

        protected override void CheckCanvas(PdfWriter writer, int key, Object obj1) {
            if (obj1 is String) {
                if ("q".Equals(obj1)) {
                    if (++gsStackDepth > maxGsStackDepth)
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
                    } else if (obj1 is BaseColor)
                        CheckColor(writer, PdfIsoKeys.PDFISOKEY_RGB, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_CMYK:
                    if (rgbUsed) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("devicergb.and.devicecmyk.colorspaces.cannot.be.used.both.in.one.file"));
                    }
                    cmykUsed = true;
                    break;
                case PdfIsoKeys.PDFISOKEY_RGB:
                    if (cmykUsed) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("devicergb.and.devicecmyk.colorspaces.cannot.be.used.both.in.one.file"));
                    }
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
                PdfName subtype = annot.Get(PdfName.SUBTYPE) as PdfName;
                if (subtype != null && !allowedAnnotTypes.Contains(subtype))
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("annotation.type.1.not.allowed", subtype.ToString()));
                PdfNumber ca = annot.GetAsNumber(PdfName.CA);
                if (ca != null && ca.FloatValue != 1.0) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("an.annotation.dictionary.shall.not.contain.the.ca.key.with.a.value.other.than.1"));
                }
                PdfNumber f = annot.GetAsNumber(PdfName.F);
                if (f == null) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("an.annotation.dictionary.shall.contain.the.f.key"));
                }
                int flags = f.IntValue;
                if (CheckFlag(flags, PdfAnnotation.FLAGS_PRINT) == false || CheckFlag(flags, PdfAnnotation.FLAGS_HIDDEN) ||
                        CheckFlag(flags, PdfAnnotation.FLAGS_INVISIBLE) || CheckFlag(flags, PdfAnnotation.FLAGS_NOVIEW)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("the.f.keys.print.flag.bit.shall.be.set.to.1.and.its.hidden.invisible.and.noview.flag.bits.shall.be.set.to.0"));
                }
                if (PdfName.TEXT.Equals(annot.GetAsName(PdfName.SUBTYPE))) {
                    if (CheckFlag(flags, PdfAnnotation.FLAGS_NOZOOM) == false || CheckFlag(flags, PdfAnnotation.FLAGS_NOROTATE) == false) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("text.annotations.should.set.the.nozoom.and.norotate.flag.bits.of.the.f.key.to.1"));
                    }
                }
                if (annot.Contains(PdfName.C) || annot.Contains(PdfName.IC)) {
                    ICC_Profile colorProfile = ((PdfAWriter) writer).ColorProfile;
                    String cs = "";
                    cs = System.Text.Encoding.ASCII.GetString(colorProfile.Data, 16, 4);
                    cs = cs.Trim();
                    if (!"RGB".Equals(cs.ToUpper())) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("destoutputprofile.in.the.pdfa1.outputintent.dictionary.shall.be.rgb"));
                    }
                }
                PdfDictionary ap = GetDirectDictionary(annot.Get(PdfName.AP));
                if (ap != null) {
                    if (ap.Contains(PdfName.R) || ap.Contains(PdfName.D)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("appearance.dictionary.shall.contain.only.the.n.key.with.stream.value"));
                    }
                    PdfObject n = ap.Get(PdfName.N);
                    if (!(n is PdfIndirectReference)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("appearance.dictionary.shall.contain.only.the.n.key.with.stream.value"));
                    }

                }
                if (PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE)) && (annot.Contains(PdfName.AA) || annot.Contains(PdfName.A))) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("widget.annotation.dictionary.or.field.dictionary.shall.not.include.a.or.aa.entry"));
                }
                if (CheckStructure(conformanceLevel)) {
                    if (contentAnnotations.Contains(subtype) && !annot.Contains(PdfName.CONTENTS)) {
                        throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("annotation.of.type.1.should.have.contents.key", subtype.ToString()));
                    }
                }
            }
        }

        protected override void CheckAction(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfAction) {
                PdfAction action = (PdfAction) obj1;
                PdfName s = action.GetAsName(PdfName.S);
                if (setState.Equals(s) || noOp.Equals(s)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("deprecated.setstate.and.noop.actions.are.not.allowed"));
                }
                if (restrictedActions.Contains(s)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("launch.sound.movie.resetform.importdata.and.javascript.actions.are.not.allowed"));
                }
                if (PdfName.NAMED.Equals(s)) {
                    PdfName n = action.GetAsName(PdfName.N);
                    if (n != null && !allowedNamedActions.Contains(n)) {
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
                throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("outputintent.shall.not.be.updated"));
        }

        public override void Close(PdfWriter writer) {
            CheckOutputIntentsInStamperMode(writer);
                    if ((rgbUsed || cmykUsed || grayUsed) && pdfaOutputIntentColorSpace == null) {
            throw new PdfAConformanceException(null, MessageLocalization.GetComposedMessage("if.device.rgb.cmyk.gray.used.in.file.that.file.shall.contain.pdfa.outputintent"));
        }

            if ("RGB ".Equals(pdfaOutputIntentColorSpace)) {
                if (cmykUsed)
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage(
                            "devicecmyk.may.be.used.only.if.the.file.has.a.cmyk.pdfa.outputIntent"));
            } else if ("CMYK".Equals(pdfaOutputIntentColorSpace)) {
                if (rgbUsed)
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage(
                            "devicergb.may.be.used.only.if.the.file.has.a.rgb.pdfa.outputIntent"));
            } else {
                if (cmykUsed)
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage(
                            "devicecmyk.may.be.used.only.if.the.file.has.a.cmyk.pdfa.outputIntent"));
                if (rgbUsed)
                    throw new PdfAConformanceException(null,
                        MessageLocalization.GetComposedMessage(
                            "devicergb.may.be.used.only.if.the.file.has.a.rgb.pdfa.outputIntent"));
            }
        }
    }
}
