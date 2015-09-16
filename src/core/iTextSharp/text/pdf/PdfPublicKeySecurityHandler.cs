using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf.crypto;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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

/**
 *     The below 2 methods are from pdfbox.
 * 
 *     private DERObject CreateDERForRecipient(byte[] in, X509Certificate cert) ;
 *     private KeyTransRecipientInfo ComputeRecipientInfo(X509Certificate x509certificate, byte[] abyte0);
 *     
 *     2006-11-22 Aiken Sam.
 */

/**
 * Copyright (c) 2003-2006, www.pdfbox.org
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of pdfbox; nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * http://www.pdfbox.org
 *
 */

namespace iTextSharp.text.pdf {

    /**
    * @author Aiken Sam (aikensam@ieee.org)
    */
    public class PdfPublicKeySecurityHandler {
        
        private const int SEED_LENGTH = 20;
        
        private List<PdfPublicKeyRecipient> recipients = null;
        
        private byte[] seed;

        public PdfPublicKeySecurityHandler() {
            seed = IVGenerator.GetIV(SEED_LENGTH);
            recipients = new List<PdfPublicKeyRecipient>();
        }


        virtual public void AddRecipient(PdfPublicKeyRecipient recipient) {
            recipients.Add(recipient);
        }
        
        virtual protected internal byte[] GetSeed() {
            return (byte[])seed.Clone();
        }
        
        virtual public int GetRecipientsSize() {
            return recipients.Count;
        }
        
        virtual public byte[] GetEncodedRecipient(int index) {
            //Certificate certificate = recipient.GetX509();
            PdfPublicKeyRecipient recipient = recipients[index];
            byte[] cms = recipient.Cms;
            
            if (cms != null) return cms;
            
            X509Certificate certificate  = recipient.Certificate;
            int permission =  recipient.Permission;//PdfWriter.AllowCopy | PdfWriter.AllowPrinting | PdfWriter.AllowScreenReaders | PdfWriter.AllowAssembly;   
            int revision = 3;
            
            permission |= (int)(revision==3 ? (uint)0xfffff0c0 : (uint)0xffffffc0);
            permission &= unchecked((int)0xfffffffc);
            permission += 1;
          
            byte[] pkcs7input = new byte[24];
            
            byte one = (byte)(permission);
            byte two = (byte)(permission >> 8);
            byte three = (byte)(permission >> 16);
            byte four = (byte)(permission >> 24);

            System.Array.Copy(seed, 0, pkcs7input, 0, 20); // put this seed in the pkcs7 input
                                
            pkcs7input[20] = four;
            pkcs7input[21] = three;                
            pkcs7input[22] = two;
            pkcs7input[23] = one;

            Asn1Object obj = CreateDERForRecipient(pkcs7input, certificate);
                
            MemoryStream baos = new MemoryStream();
                
            DerOutputStream k = new DerOutputStream(baos);
                
            k.WriteObject(obj);  
            
            cms = baos.ToArray();

            recipient.Cms = cms;
            
            return cms;    
        }
        
        virtual public PdfArray GetEncodedRecipients() {
            PdfArray EncodedRecipients = new PdfArray();
            byte[] cms = null;
            for (int i=0; i<recipients.Count; i++) {
                try {
                    cms = GetEncodedRecipient(i);
                    EncodedRecipients.Add(new PdfLiteral(StringUtils.EscapeString(cms)));
                } catch {
                    EncodedRecipients = null;
                }
            }            
            return EncodedRecipients;
        }
        
        private Asn1Object CreateDERForRecipient(byte[] inp, X509Certificate cert) {
            
            String s = "1.2.840.113549.3.2";
            
            byte[] outp = new byte[100];
            DerObjectIdentifier derob = new DerObjectIdentifier(s);
            byte[] keyp = IVGenerator.GetIV(16);
            IBufferedCipher cf = CipherUtilities.GetCipher(derob);
            KeyParameter kp = new KeyParameter(keyp);
            byte[] iv = IVGenerator.GetIV(cf.GetBlockSize());
            ParametersWithIV piv = new ParametersWithIV(kp, iv);
            cf.Init(true, piv);
            int len = cf.DoFinal(inp, outp, 0);

            byte[] abyte1 = new byte[len];
            System.Array.Copy(outp, 0, abyte1, 0, len);
            DerOctetString deroctetstring = new DerOctetString(abyte1);
            KeyTransRecipientInfo keytransrecipientinfo = ComputeRecipientInfo(cert, keyp);
            DerSet derset = new DerSet(new RecipientInfo(keytransrecipientinfo));
            Asn1EncodableVector ev = new Asn1EncodableVector();
            ev.Add(new DerInteger(58));
            ev.Add(new DerOctetString(iv));
            DerSequence seq = new DerSequence(ev);
            AlgorithmIdentifier algorithmidentifier = new AlgorithmIdentifier(derob, seq);
            EncryptedContentInfo encryptedcontentinfo = 
                new EncryptedContentInfo(PkcsObjectIdentifiers.Data, algorithmidentifier, deroctetstring);
            Asn1Set set = null;
            EnvelopedData env = new EnvelopedData(null, derset, encryptedcontentinfo, set);
            Org.BouncyCastle.Asn1.Cms.ContentInfo contentinfo = 
                new Org.BouncyCastle.Asn1.Cms.ContentInfo(PkcsObjectIdentifiers.EnvelopedData, env);
            return contentinfo.ToAsn1Object();        
        }
        
        private KeyTransRecipientInfo ComputeRecipientInfo(X509Certificate x509certificate, byte[] abyte0) {
            Asn1InputStream asn1inputstream = 
                new Asn1InputStream(new MemoryStream(x509certificate.GetTbsCertificate()));
            TbsCertificateStructure tbscertificatestructure = 
                TbsCertificateStructure.GetInstance(asn1inputstream.ReadObject());
            AlgorithmIdentifier algorithmidentifier = tbscertificatestructure.SubjectPublicKeyInfo.AlgorithmID;
            Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber issuerandserialnumber = 
                new Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber(
                    tbscertificatestructure.Issuer, 
                    tbscertificatestructure.SerialNumber.Value);
            IBufferedCipher cipher = CipherUtilities.GetCipher(algorithmidentifier.ObjectID);
            cipher.Init(true, x509certificate.GetPublicKey());
            byte[] outp = new byte[10000];
            int len = cipher.DoFinal(abyte0, outp, 0);
            byte[] abyte1 = new byte[len];
            System.Array.Copy(outp, 0, abyte1, 0, len);
            DerOctetString deroctetstring = new DerOctetString(abyte1);
            RecipientIdentifier recipId = new RecipientIdentifier(issuerandserialnumber);
            return new KeyTransRecipientInfo( recipId, algorithmidentifier, deroctetstring);
        }        
    }
}
