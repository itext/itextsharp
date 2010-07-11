using System;
using System.Collections;
using System.Text;
using System.IO;
using iTextSharp.text.pdf.crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using iTextSharp.text.error_messages;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.pdf {

/**
 *
 * @author  Paulo Soares
 */
public class PdfEncryption {

    public const int STANDARD_ENCRYPTION_40 = 2;
    public const int STANDARD_ENCRYPTION_128 = 3;
    public const int AES_128 = 4;

    private static byte[] pad = {
        (byte)0x28, (byte)0xBF, (byte)0x4E, (byte)0x5E, (byte)0x4E, (byte)0x75,
        (byte)0x8A, (byte)0x41, (byte)0x64, (byte)0x00, (byte)0x4E, (byte)0x56,
        (byte)0xFF, (byte)0xFA, (byte)0x01, (byte)0x08, (byte)0x2E, (byte)0x2E,
        (byte)0x00, (byte)0xB6, (byte)0xD0, (byte)0x68, (byte)0x3E, (byte)0x80,
        (byte)0x2F, (byte)0x0C, (byte)0xA9, (byte)0xFE, (byte)0x64, (byte)0x53,
        (byte)0x69, (byte)0x7A};
        
    private static readonly byte[] salt = {(byte)0x73, (byte)0x41, (byte)0x6c, (byte)0x54};
    internal static readonly byte[] metadataPad = {(byte)255,(byte)255,(byte)255,(byte)255};
    /** The encryption key for a particular object/generation */
    internal byte[] key;
    /** The encryption key length for a particular object/generation */
    internal int keySize;
    /** The global encryption key */
    internal byte[] mkey;
    /** Work area to prepare the object/generation bytes */
    internal byte[] extra = new byte[5];
    /** The message digest algorithm MD5 */
    internal IDigest md5;
    /** The encryption key for the owner */
    internal byte[] ownerKey = new byte[32];
    /** The encryption key for the user */
    internal byte[] userKey = new byte[32];
    /** The public key security handler for certificate encryption */
    protected PdfPublicKeySecurityHandler publicKeyHandler = null;
    internal int permissions;
    internal byte[] documentID;
    internal static long seq = DateTime.Now.Ticks + Environment.TickCount;
    private int revision;
    private ARCFOUREncryption rc4 = new ARCFOUREncryption();
    /** The generic key length. It may be 40 or 128. */
    private int keyLength;
    private bool encryptMetadata;
    /**
     * Indicates if the encryption is only necessary for embedded files.
     * @since 2.1.3
     */
    private bool embeddedFilesOnly;

    private int cryptoMode;
    
    public PdfEncryption() {
        md5 = DigestUtilities.GetDigest("MD5");
        publicKeyHandler = new PdfPublicKeySecurityHandler();
    }

    public PdfEncryption(PdfEncryption enc) : this() {
        mkey = (byte[])enc.mkey.Clone();
        ownerKey = (byte[])enc.ownerKey.Clone();
        userKey = (byte[])enc.userKey.Clone();
        permissions = enc.permissions;
        if (enc.documentID != null)
            documentID = (byte[])enc.documentID.Clone();
        revision = enc.revision;
        keyLength = enc.keyLength;
        encryptMetadata = enc.encryptMetadata;
        embeddedFilesOnly = enc.embeddedFilesOnly;
        publicKeyHandler = enc.publicKeyHandler;
    }

    public void SetCryptoMode(int mode, int kl) {
        cryptoMode = mode;
		encryptMetadata = (mode & PdfWriter.DO_NOT_ENCRYPT_METADATA) != PdfWriter.DO_NOT_ENCRYPT_METADATA;
		embeddedFilesOnly = (mode & PdfWriter.EMBEDDED_FILES_ONLY) == PdfWriter.EMBEDDED_FILES_ONLY;
        mode &= PdfWriter.ENCRYPTION_MASK;
        switch (mode) {
            case PdfWriter.STANDARD_ENCRYPTION_40:
                encryptMetadata = true;
                embeddedFilesOnly = false;
                keyLength = 40;
                revision = STANDARD_ENCRYPTION_40;
                break;
            case PdfWriter.STANDARD_ENCRYPTION_128:
                embeddedFilesOnly = false;
                if (kl > 0)
                    keyLength = kl;
                else
                    keyLength = 128;
                revision = STANDARD_ENCRYPTION_128;
                break;
            case PdfWriter.ENCRYPTION_AES_128:
                keyLength = 128;
                revision = AES_128;
                break;
            default:
                throw new ArgumentException(MessageLocalization.GetComposedMessage("no.valid.encryption.mode"));
        }
    }
    
    public int GetCryptoMode() {
        return cryptoMode;
    }
    
    public bool IsMetadataEncrypted() {
        return encryptMetadata;
    }

    /**
     * Indicates if only the embedded files have to be encrypted.
     * @return  if true only the embedded files will be encrypted
     * @since   2.1.3
     */
    public bool IsEmbeddedFilesOnly() {
        return embeddedFilesOnly;
    }

    private byte[] PadPassword(byte[] userPassword) {
        byte[] userPad = new byte[32];
        if (userPassword == null) {
            Array.Copy(pad, 0, userPad, 0, 32);
        }
        else {
            Array.Copy(userPassword, 0, userPad, 0, Math.Min(userPassword.Length, 32));
            if (userPassword.Length < 32)
                Array.Copy(pad, 0, userPad, userPassword.Length, 32 - userPassword.Length);
        }

        return userPad;
    }

    /**
     */
    private byte[] ComputeOwnerKey(byte[] userPad, byte[] ownerPad) {
        byte[] ownerKey = new byte[32];

        byte[] digest = DigestComputeHash("MD5", ownerPad);
        if (revision == STANDARD_ENCRYPTION_128 || revision == AES_128) {
            byte[] mkey = new byte[keyLength / 8];
            // only use for the input as many bit as the key consists of
            for (int k = 0; k < 50; ++k)
                Array.Copy(DigestComputeHash("MD5", digest, 0, mkey.Length), 0, digest, 0, mkey.Length);
            Array.Copy(userPad, 0, ownerKey, 0, 32);
            for (int i = 0; i < 20; ++i) {
                for (int j = 0; j < mkey.Length ; ++j)
                    mkey[j] = (byte)(digest[j] ^ i);
                rc4.PrepareARCFOURKey(mkey);
                rc4.EncryptARCFOUR(ownerKey);
            }
        }
        else {
            rc4.PrepareARCFOURKey(digest, 0, 5);
            rc4.EncryptARCFOUR(userPad, ownerKey);
        }

        return ownerKey;
    }

    /**
     *
     * ownerKey, documentID must be setuped
     */
    private void SetupGlobalEncryptionKey(byte[] documentID, byte[] userPad, byte[] ownerKey, int permissions) {
        this.documentID = documentID;
        this.ownerKey = ownerKey;
        this.permissions = permissions;
        // use variable keylength
        mkey = new byte[keyLength / 8];

        //fixed by ujihara in order to follow PDF refrence
        md5.Reset();
        md5.BlockUpdate(userPad, 0, userPad.Length);
        md5.BlockUpdate(ownerKey, 0, ownerKey.Length);

        byte[] ext = new byte[4];
        ext[0] = (byte)permissions;
        ext[1] = (byte)(permissions >> 8);
        ext[2] = (byte)(permissions >> 16);
        ext[3] = (byte)(permissions >> 24);
        md5.BlockUpdate(ext, 0, 4);
        if (documentID != null) 
            md5.BlockUpdate(documentID, 0, documentID.Length);
        if (!encryptMetadata)
            md5.BlockUpdate(metadataPad, 0, metadataPad.Length);
        byte[] hash = new byte[md5.GetDigestSize()];
        md5.DoFinal(hash, 0);

        byte[] digest = new byte[mkey.Length];
        Array.Copy(hash, 0, digest, 0, mkey.Length);

        
        md5.Reset();
        // only use the really needed bits as input for the hash
        if (revision == STANDARD_ENCRYPTION_128 || revision == AES_128) {
            for (int k = 0; k < 50; ++k) {
                Array.Copy(DigestComputeHash("MD5", digest), 0, digest, 0, mkey.Length);
            }
        }
        Array.Copy(digest, 0, mkey, 0, mkey.Length);
    }

    /**
     *
     * mkey must be setuped
     */
    // use the revision to choose the setup method
    private void SetupUserKey() {
        if (revision == STANDARD_ENCRYPTION_128 || revision == AES_128) {
            md5.BlockUpdate(pad, 0, pad.Length);
            md5.BlockUpdate(documentID, 0, documentID.Length);
            byte[] digest = new byte[md5.GetDigestSize()];
            md5.DoFinal(digest, 0);
            md5.Reset();
            Array.Copy(digest, 0, userKey, 0, 16);
            for (int k = 16; k < 32; ++k)
                userKey[k] = 0;
            for (int i = 0; i < 20; ++i) {
                for (int j = 0; j < mkey.Length; ++j)
                    digest[j] = (byte)(mkey[j] ^ i);
                rc4.PrepareARCFOURKey(digest, 0, mkey.Length);
                rc4.EncryptARCFOUR(userKey, 0, 16);
            }
        }
        else {
            rc4.PrepareARCFOURKey(mkey);
            rc4.EncryptARCFOUR(pad, userKey);
        }
    }

    // gets keylength and revision and uses revison to choose the initial values for permissions
    public void SetupAllKeys(byte[] userPassword, byte[] ownerPassword, int permissions) {
        if (ownerPassword == null || ownerPassword.Length == 0)
            ownerPassword = DigestComputeHash("MD5", CreateDocumentId());
        md5.Reset();
        permissions |= (int)((revision == STANDARD_ENCRYPTION_128 || revision == AES_128) ? (uint)0xfffff0c0 : (uint)0xffffffc0);
        permissions &= unchecked((int)0xfffffffc);
        //PDF refrence 3.5.2 Standard Security Handler, Algorithum 3.3-1
        //If there is no owner password, use the user password instead.
        byte[] userPad = PadPassword(userPassword);
        byte[] ownerPad = PadPassword(ownerPassword);

        this.ownerKey = ComputeOwnerKey(userPad, ownerPad);
        documentID = CreateDocumentId();
        SetupByUserPad(this.documentID, userPad, this.ownerKey, permissions);
    }

    public static byte[] CreateDocumentId() {
        long time = DateTime.Now.Ticks + Environment.TickCount;
        long mem = GC.GetTotalMemory(false);
        String s = time + "+" + mem + "+" + (seq++);
        byte[] b = Encoding.ASCII.GetBytes(s);            
        return DigestComputeHash("MD5", b);
    }

    public void SetupByUserPassword(byte[] documentID, byte[] userPassword, byte[] ownerKey, int permissions) {
        SetupByUserPad(documentID, PadPassword(userPassword), ownerKey, permissions);
    }

    /**
     */
    private void SetupByUserPad(byte[] documentID, byte[] userPad, byte[] ownerKey, int permissions) {
        SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions);
        SetupUserKey();
    }

    /**
     */
    public void SetupByOwnerPassword(byte[] documentID, byte[] ownerPassword, byte[] userKey, byte[] ownerKey, int permissions) {
        SetupByOwnerPad(documentID, PadPassword(ownerPassword), userKey, ownerKey, permissions);
    }

    private void SetupByOwnerPad(byte[] documentID, byte[] ownerPad, byte[] userKey, byte[] ownerKey, int permissions) {
        byte[] userPad = ComputeOwnerKey(ownerKey, ownerPad); //userPad will be set in this.ownerKey
        SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions); //step 3
        SetupUserKey();
    }

    public void SetupByEncryptionKey(byte[] key, int keylength) {
        mkey = new byte[keylength/8];
        System.Array.Copy(key, 0, mkey, 0, mkey.Length);
    }    

    public void SetHashKey(int number, int generation) {
        md5.Reset();    //added by ujihara
        extra[0] = (byte)number;
        extra[1] = (byte)(number >> 8);
        extra[2] = (byte)(number >> 16);
        extra[3] = (byte)generation;
        extra[4] = (byte)(generation >> 8);
        md5.BlockUpdate(mkey, 0, mkey.Length);
        md5.BlockUpdate(extra, 0, extra.Length);
        if (revision == AES_128)
            md5.BlockUpdate(salt, 0, salt.Length);
        key = new byte[md5.GetDigestSize()];
        md5.DoFinal(key, 0);
        md5.Reset();
        keySize = mkey.Length + 5;
        if (keySize > 16)
            keySize = 16;
    }

    public static PdfObject CreateInfoId(byte[] id) {
        ByteBuffer buf = new ByteBuffer(90);
        buf.Append('[').Append('<');
        for (int k = 0; k < 16; ++k)
            buf.AppendHex(id[k]);
        buf.Append('>').Append('<');
        id = CreateDocumentId();
        for (int k = 0; k < 16; ++k)
            buf.AppendHex(id[k]);
        buf.Append('>').Append(']');
        return new PdfLiteral(buf.ToByteArray());
    }

    public PdfDictionary GetEncryptionDictionary() {
        PdfDictionary dic = new PdfDictionary();
        
        if (publicKeyHandler.GetRecipientsSize() > 0) {
            PdfArray recipients = null;
            
            dic.Put(PdfName.FILTER, PdfName.PUBSEC);  
            dic.Put(PdfName.R, new PdfNumber(revision));	

            recipients = publicKeyHandler.GetEncodedRecipients();
            
            if (revision == STANDARD_ENCRYPTION_40) {
                dic.Put(PdfName.V, new PdfNumber(1));
                dic.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S4);
                dic.Put(PdfName.RECIPIENTS, recipients);
            }
            else if (revision == STANDARD_ENCRYPTION_128 && encryptMetadata) {
                dic.Put(PdfName.V, new PdfNumber(2));
                dic.Put(PdfName.LENGTH, new PdfNumber(128));
                dic.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S4);
                dic.Put(PdfName.RECIPIENTS, recipients);
            }
            else {               
                dic.Put(PdfName.R, new PdfNumber(AES_128));
                dic.Put(PdfName.V, new PdfNumber(4));
                dic.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S5);
                                
                PdfDictionary stdcf = new PdfDictionary();
                stdcf.Put(PdfName.RECIPIENTS, recipients);                                                                    
                if (!encryptMetadata)
                    stdcf.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                
                if (revision == AES_128)
                    stdcf.Put(PdfName.CFM, PdfName.AESV2);
                else
                    stdcf.Put(PdfName.CFM, PdfName.V2);                  
                PdfDictionary cf = new PdfDictionary();
                cf.Put(PdfName.DEFAULTCRYPTFILTER, stdcf);                
                dic.Put(PdfName.CF, cf);
                if (embeddedFilesOnly) {
                    dic.Put(PdfName.EFF, PdfName.DEFAULTCRYPTFILTER);
                    dic.Put(PdfName.STRF, PdfName.IDENTITY);
                    dic.Put(PdfName.STMF, PdfName.IDENTITY);
                }
                else {
                    dic.Put(PdfName.STRF, PdfName.DEFAULTCRYPTFILTER);
                    dic.Put(PdfName.STMF, PdfName.DEFAULTCRYPTFILTER);
                }
            }
            
            IDigest sh = DigestUtilities.GetDigest("SHA1");
            byte[] encodedRecipient = null;
            byte[] seed = publicKeyHandler.GetSeed();
            sh.BlockUpdate(seed, 0, seed.Length);
            for (int i=0; i<publicKeyHandler.GetRecipientsSize(); i++)
            {
                encodedRecipient = publicKeyHandler.GetEncodedRecipient(i);
                sh.BlockUpdate(encodedRecipient, 0, encodedRecipient.Length);
            }
            if (!encryptMetadata)
                sh.BlockUpdate(metadataPad, 0, metadataPad.Length);
            byte[] mdResult = new byte[sh.GetDigestSize()];
            sh.DoFinal(mdResult, 0);            
            SetupByEncryptionKey(mdResult, keyLength);              
        } else {
            dic.Put(PdfName.FILTER, PdfName.STANDARD);
            dic.Put(PdfName.O, new PdfLiteral(PdfContentByte.EscapeString(ownerKey)));
            dic.Put(PdfName.U, new PdfLiteral(PdfContentByte.EscapeString(userKey)));
            dic.Put(PdfName.P, new PdfNumber(permissions));
            dic.Put(PdfName.R, new PdfNumber(revision));
            if (revision == STANDARD_ENCRYPTION_40) {
                dic.Put(PdfName.V, new PdfNumber(1));
            }
            else if (revision == STANDARD_ENCRYPTION_128 && encryptMetadata) {
                dic.Put(PdfName.V, new PdfNumber(2));
                dic.Put(PdfName.LENGTH, new PdfNumber(128));
                
            }
            else {
                if (!encryptMetadata)
                    dic.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                dic.Put(PdfName.R, new PdfNumber(AES_128));
                dic.Put(PdfName.V, new PdfNumber(4));
                dic.Put(PdfName.LENGTH, new PdfNumber(128));
                PdfDictionary stdcf = new PdfDictionary();
                stdcf.Put(PdfName.LENGTH, new PdfNumber(16));
                if (embeddedFilesOnly) {
                    stdcf.Put(PdfName.AUTHEVENT, PdfName.EFOPEN);
                    dic.Put(PdfName.EFF, PdfName.STDCF);
                    dic.Put(PdfName.STRF, PdfName.IDENTITY);
                    dic.Put(PdfName.STMF, PdfName.IDENTITY);
                }
                else {
                    stdcf.Put(PdfName.AUTHEVENT, PdfName.DOCOPEN);
                    dic.Put(PdfName.STRF, PdfName.STDCF);
                    dic.Put(PdfName.STMF, PdfName.STDCF);
                }
                if (revision == AES_128)
                    stdcf.Put(PdfName.CFM, PdfName.AESV2);
                else
                    stdcf.Put(PdfName.CFM, PdfName.V2);
                PdfDictionary cf = new PdfDictionary();
                cf.Put(PdfName.STDCF, stdcf);
                dic.Put(PdfName.CF, cf);
            }
        }
        return dic;
    }
    
    public PdfObject FileID {
        get {
            return CreateInfoId(documentID);
        }
    }
    public OutputStreamEncryption GetEncryptionStream(Stream os) {
        return new OutputStreamEncryption(os, key, 0, keySize, revision);
    }
    
    public int CalculateStreamSize(int n) {
        if (revision == AES_128)
            return (n & 0x7ffffff0) + 32;
        else
            return n;
    }
    
    public byte[] EncryptByteArray(byte[] b) {
        MemoryStream ba = new MemoryStream();
        OutputStreamEncryption os2 = GetEncryptionStream(ba);
        os2.Write(b, 0, b.Length);
        os2.Finish();
        return ba.ToArray();
    }
    
    public StandardDecryption GetDecryptor() {
        return new StandardDecryption(key, 0, keySize, revision);
    }
    
    public byte[] DecryptByteArray(byte[] b) {
        MemoryStream ba = new MemoryStream();
        StandardDecryption dec = GetDecryptor();
        byte[] b2 = dec.Update(b, 0, b.Length);
        if (b2 != null)
            ba.Write(b2, 0, b2.Length);
        b2 = dec.Finish();
        if (b2 != null)
            ba.Write(b2, 0, b2.Length);
        return ba.ToArray();
    }

    public void AddRecipient(X509Certificate cert, int permission) {
        documentID = CreateDocumentId();
        publicKeyHandler.AddRecipient(new PdfPublicKeyRecipient(cert, permission));
    }

	public byte[] ComputeUserPassword(byte[] ownerPassword) {
		byte[] userPad = ComputeOwnerKey(ownerKey, PadPassword(ownerPassword));
		for (int i = 0; i < userPad.Length; i++) {
			bool match = true;
			for (int j = 0; j < userPad.Length - i; j++) {
				if (userPad[i + j] != pad[j]) {
					match = false;
					break;
                }
			}
			if (!match) continue;
			byte[] userPassword = new byte[i];
			System.Array.Copy(userPad, 0, userPassword, 0, i);
			return userPassword;
		}
		return userPad;
	}

    public static byte[] DigestComputeHash(string algo, byte[] b, int offset, int len) {
        IDigest d = DigestUtilities.GetDigest(algo);
        d.BlockUpdate(b, offset, len);
        byte[] r = new byte[d.GetDigestSize()];
        d.DoFinal(r, 0);
        return r;
    }

    public static byte[] DigestComputeHash(string algo, byte[] b) {
        return DigestComputeHash(algo, b, 0, b.Length);
    }
}
}
