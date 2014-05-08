using System;
using System.Collections.Generic;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.intern;
using iTextSharp.text.xml.xmp;
using iTextSharp.xmp;

namespace itextsharp.pdfa.iTextSharp.text.pdf.intern {
    public class ZugferdChecker : PdfA3Checker {

        private IList<PdfFileSpecification> attachments = new List<PdfFileSpecification>();

        protected internal ZugferdChecker(PdfAConformanceLevel conformanceLevel) : base(conformanceLevel) {
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, object obj1) {
            base.CheckFileSpec(writer, key, obj1);
            attachments.Add((PdfFileSpecification)obj1);
        }

        public override void Close(PdfWriter writer) {
            base.Close(writer);
            bool ok = false;
            IXmpMeta xmpMeta = writer.XmpWriter.XmpMeta;
            try {
                String docFileName = xmpMeta.GetPropertyString(PdfAXmpWriter.zugferdSchemaNS,
                    PdfAXmpWriter.zugferdDocumentFileName);
                foreach (PdfFileSpecification attachment in attachments) {
                    if (docFileName.Equals(attachment.GetAsString(PdfName.UF).ToString())) {
                        PdfName relationship = attachment.GetAsName(PdfName.AFRELATIONSHIP);
                        if (!AFRelationshipValue.Alternative.Equals(relationship)) {
                            attachments.Clear();
                            throw new PdfAConformanceException(attachment,
                                MessageLocalization.GetComposedMessage("afrelationship.value.shall.be.alternative"));
                        }
                        ok = true;
                        break;
                    }
                }
            } catch (Exception e) {
                attachments.Clear();
                throw e;
            }
            attachments.Clear();
            if (!ok) {
                throw new PdfAConformanceException(xmpMeta,
                    MessageLocalization.GetComposedMessage("zugferd.xmp.schema.shall.contain.attachment.name"));
            }
        }
    }
}
