using System;
using System.IO;

namespace iTextSharp.text.pdf
{


    /**
     * Extension of PdfStamper that will attempt to keep a file
     * in conformance with the PDF/A standard.
     * @see PdfStamper
     */
    public class PdfAStamper : PdfStamper {

        /**
         * Starts the process of adding extra content to an existing PDF document keeping the document PDF/A conformant.
         * @param reader the original document. It cannot be reused
         * @param os the output stream
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @throws DocumentException on error
         * @throws IOException or error
         */
        public PdfAStamper(PdfReader reader, Stream os, PdfAConformanceLevel conformanceLevel) {
            stamper = new PdfAStamperImp(reader, os, '\0', false, conformanceLevel);
        }

        /**
         * Starts the process of adding extra content to an existing PDF document keeping the document PDF/A conformant.
         * @param reader the original document. It cannot be reused
         * @param os the output stream
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original document
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @throws DocumentException on error
         * @throws IOException or error
         */
        public PdfAStamper(PdfReader reader, Stream os, char pdfVersion, PdfAConformanceLevel conformanceLevel) {
            stamper = new PdfAStamperImp(reader, os, pdfVersion, false, conformanceLevel);
        }

        /**
         * Starts the process of adding extra content to an existing PDF document keeping the document PDF/A conformant.
         * @param reader the original document. It cannot be reused
         * @param os the output stream
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original document
         * @param append if <CODE>true</CODE> appends the document changes as a new revision. This is only useful for multiple signatures as nothing is gained in speed or memory
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @throws DocumentException on error
         * @throws IOException or error
         */
        public PdfAStamper(PdfReader reader, Stream os, char pdfVersion, bool append, PdfAConformanceLevel conformanceLevel) {
            stamper = new PdfAStamperImp(reader, os, pdfVersion, append, conformanceLevel);
        }

        /**
         * Applies a digital signature to a document, possibly as a new revision, making
         * possible multiple signatures. The returned PdfStamper
         * can be used normally as the signature is only applied when closing.
         * <p>
         * A possible use for adding a signature without invalidating an existing one is:
         * <p>
         * <pre>
         * KeyStore ks = KeyStore.getInstance("pkcs12");
         * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
         * String alias = (String)ks.aliases().nextElement();
         * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
         * Certificate[] chain = ks.getCertificateChain(alias);
         * PdfReader reader = new PdfReader("original.pdf");
         * FileOutputStream fout = new FileOutputStream("signed.pdf");
         * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0', new
         * File("/temp"), true);
         * PdfSignatureAppearance sap = stp.getSignatureAppearance();
         * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
         * sap.setReason("I'm the author");
         * sap.setLocation("Lisbon");
         * // comment next line to have an invisible signature
         * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
         * stp.close();
         * </pre>
         * @param reader the original document
         * @param os the output stream or <CODE>null</CODE> to keep the document in the temporary file
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
         * document
         * @param tempFile location of the temporary file. If it's a directory a temporary file will be created there.
         *     If it's a file it will be used directly. The file will be deleted on exit unless <CODE>os</CODE> is null.
         *     In that case the document can be retrieved directly from the temporary file. If it's <CODE>null</CODE>
         *     no temporary file will be created and memory will be used
         * @param append if <CODE>true</CODE> the signature and all the other content will be added as a
         * new revision thus not invalidating existing signatures
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @return a <CODE>PdfAStamper</CODE>
         * @throws DocumentException on error
         * @throws IOException on error
         */
        public static PdfAStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, String tempFile, bool append, PdfAConformanceLevel conformanceLevel) {
            PdfAStamper stp;
            if (tempFile == null) {
                ByteBuffer bout = new ByteBuffer();
                stp = new PdfAStamper(reader, bout, pdfVersion, append, conformanceLevel);
                stp.sigApp = new PdfSignatureAppearance(stp.stamper);
                stp.sigApp.Sigout = bout;
            } else {
                if (Directory.Exists(tempFile)) {
                    tempFile = Path.Combine(tempFile, Path.GetTempFileName());
                    tempFile += ".pdf";
                }
                FileStream fout = new FileStream(tempFile, FileMode.Create);
                stp = new PdfAStamper(reader, fout, pdfVersion, append, conformanceLevel);
                stp.sigApp = new PdfSignatureAppearance(stp.stamper);
                stp.sigApp.SetTempFile(tempFile);
            }
            stp.sigApp.Originalout = os;
            stp.sigApp.SetStamper(stp);
            stp.hasSignature = true;
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acroForm = (PdfDictionary) PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), catalog);
            if (acroForm != null) {
                acroForm.Remove(PdfName.NEEDAPPEARANCES);
                stp.stamper.MarkUsed(acroForm);
            }
            return stp;
        }

        /**
         * Applies a digital signature to a document. The returned PdfStamper
         * can be used normally as the signature is only applied when closing.
         * <p>
         * Note that the pdf is created in memory.
         * <p>
         * A possible use is:
         * <p>
         * <pre>
         * KeyStore ks = KeyStore.getInstance("pkcs12");
         * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
         * String alias = (String)ks.aliases().nextElement();
         * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
         * Certificate[] chain = ks.getCertificateChain(alias);
         * PdfReader reader = new PdfReader("original.pdf");
         * FileOutputStream fout = new FileOutputStream("signed.pdf");
         * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0');
         * PdfSignatureAppearance sap = stp.getSignatureAppearance();
         * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
         * sap.setReason("I'm the author");
         * sap.setLocation("Lisbon");
         * // comment next line to have an invisible signature
         * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
         * stp.close();
         * </pre>
         * @param reader the original document
         * @param os the output stream
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
         * document
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @throws DocumentException on error
         * @throws IOException on error
         * @return a <CODE>PdfAStamper</CODE>
         */
        public static PdfAStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, PdfAConformanceLevel conformanceLevel) {
            return CreateSignature(reader, os, pdfVersion, null, false, conformanceLevel);
        }

        /**
         * Applies a digital signature to a document. The returned PdfStamper
         * can be used normally as the signature is only applied when closing.
         * <p>
         * A possible use is:
         * <p>
         * <pre>
         * KeyStore ks = KeyStore.getInstance("pkcs12");
         * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
         * String alias = (String)ks.aliases().nextElement();
         * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
         * Certificate[] chain = ks.getCertificateChain(alias);
         * PdfReader reader = new PdfReader("original.pdf");
         * FileOutputStream fout = new FileOutputStream("signed.pdf");
         * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0', new File("/temp"));
         * PdfSignatureAppearance sap = stp.getSignatureAppearance();
         * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
         * sap.setReason("I'm the author");
         * sap.setLocation("Lisbon");
         * // comment next line to have an invisible signature
         * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
         * stp.close();
         * </pre>
         * @param reader the original document
         * @param os the output stream or <CODE>null</CODE> to keep the document in the temporary file
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
         * document
         * @param tempFile location of the temporary file. If it's a directory a temporary file will be created there.
         *     If it's a file it will be used directly. The file will be deleted on exit unless <CODE>os</CODE> is null.
         *     In that case the document can be retrieved directly from the temporary file. If it's <CODE>null</CODE>
         *     no temporary file will be created and memory will be used
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @return a <CODE>PdfAStamper</CODE>
         * @throws DocumentException on error
         * @throws IOException on error
         */
        public static PdfAStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, String tempFile, PdfAConformanceLevel conformanceLevel) {
            return CreateSignature(reader, os, pdfVersion, tempFile, false, conformanceLevel);
        }

    }

}
