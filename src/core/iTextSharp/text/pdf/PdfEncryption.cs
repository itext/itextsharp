using System;
using System.Collections;
using System.Text;
using System.IO;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf.crypto;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using iTextSharp.text.error_messages;

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

namespace iTextSharp.text.pdf {

/**
 *
 * @author  Paulo Soares
 */
public class PdfEncryption {

    public const int STANDARD_ENCRYPTION_40 = 2;
    public const int STANDARD_ENCRYPTION_128 = 3;
    public const int AES_128 = 4;
    public const int AES_256 = 5;

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
    internal byte[] mkey = new byte[0];
    /** Work area to prepare the object/generation bytes */
    internal byte[] extra = new byte[5];
    /** The message digest algorithm MD5 */
    internal IDigest md5;
    /** The encryption key for the owner */
    internal byte[] ownerKey = new byte[32];
    /** The encryption key for the user */
    internal byte[] userKey = new byte[32];
    internal byte[] oeKey;
    internal byte[] ueKey;
    internal byte[] perms;
    /** The public key security handler for certificate encryption */
    protected PdfPublicKeySecurityHandler publicKeyHandler = null;
    internal long permissions;
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
        if (enc.key != null)
            key = (byte[]) enc.key.Clone();
        keySize = enc.keySize;
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

    virtual public void SetCryptoMode(int mode, int kl) {
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
            case PdfWriter.ENCRYPTION_AES_256:
                keyLength = 256;
                keySize = 32;
                revision = AES_256;
                break;
            default:
                throw new ArgumentException(MessageLocalization.GetComposedMessage("no.valid.encryption.mode"));
        }
    }
    
    virtual public int GetCryptoMode() {
        return cryptoMode;
    }
    
    virtual public bool IsMetadataEncrypted() {
        return encryptMetadata;
    }

    virtual public long GetPermissions() {
        return permissions;
    }

    /**
     * Indicates if only the embedded files have to be encrypted.
     * @return  if true only the embedded files will be encrypted
     * @since   2.1.3
     */
    virtual public bool IsEmbeddedFilesOnly() {
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

        byte[] digest = DigestAlgorithms.Digest("MD5", ownerPad);
        if (revision == STANDARD_ENCRYPTION_128 || revision == AES_128) {
            byte[] mkey = new byte[keyLength / 8];
            // only use for the input as many bit as the key consists of
            for (int k = 0; k < 50; ++k)
                Array.Copy(DigestAlgorithms.Digest("MD5", digest, 0, mkey.Length), 0, digest, 0, mkey.Length);
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
    private void SetupGlobalEncryptionKey(byte[] documentID, byte[] userPad, byte[] ownerKey, long permissions) {
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
                Array.Copy(DigestAlgorithms.Digest("MD5", digest), 0, digest, 0, mkey.Length);
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
    virtual public void SetupAllKeys(byte[] userPassword, byte[] ownerPassword, int permissions) {
        if (ownerPassword == null || ownerPassword.Length == 0)
            ownerPassword = DigestAlgorithms.Digest("MD5", CreateDocumentId());
        md5.Reset();
        permissions |= (int)((revision == STANDARD_ENCRYPTION_128 || revision == AES_128 || revision == AES_256) ? (uint)0xfffff0c0 : (uint)0xffffffc0);
        permissions &= unchecked((int)0xfffffffc);
        this.permissions = permissions;
        if (revision == AES_256) {
            if (userPassword == null)
                userPassword = new byte[0];
            documentID = CreateDocumentId();
            byte[] uvs = IVGenerator.GetIV(8);
            byte[] uks = IVGenerator.GetIV(8);
            key = IVGenerator.GetIV(32);
            // Algorithm 3.8.1
            IDigest md = DigestUtilities.GetDigest("SHA-256");
            md.BlockUpdate(userPassword, 0, Math.Min(userPassword.Length, 127));
            md.BlockUpdate(uvs, 0, uvs.Length);
            userKey = new byte[48];
            md.DoFinal(userKey, 0);
            System.Array.Copy(uvs, 0, userKey, 32, 8);
            System.Array.Copy(uks, 0, userKey, 40, 8);
            // Algorithm 3.8.2
            md.BlockUpdate(userPassword, 0, Math.Min(userPassword.Length, 127));
            md.BlockUpdate(uks, 0, uks.Length);
            byte[] tempDigest = new byte[32];
            md.DoFinal(tempDigest, 0);
            AESCipherCBCnoPad ac = new AESCipherCBCnoPad(true, tempDigest);
            ueKey = ac.ProcessBlock(key, 0, key.Length);
            // Algorithm 3.9.1
            byte[] ovs = IVGenerator.GetIV(8);
            byte[] oks = IVGenerator.GetIV(8);
            md.BlockUpdate(ownerPassword, 0, Math.Min(ownerPassword.Length, 127));
            md.BlockUpdate(ovs, 0, ovs.Length);
            md.BlockUpdate(userKey, 0, userKey.Length);
            ownerKey = new byte[48];
            md.DoFinal(ownerKey, 0);
            System.Array.Copy(ovs, 0, ownerKey, 32, 8);
            System.Array.Copy(oks, 0, ownerKey, 40, 8);
            // Algorithm 3.9.2
            md.BlockUpdate(ownerPassword, 0, Math.Min(ownerPassword.Length, 127));
            md.BlockUpdate(oks, 0, oks.Length);
            md.BlockUpdate(userKey, 0, userKey.Length);
            md.DoFinal(tempDigest, 0);
            ac = new AESCipherCBCnoPad(true, tempDigest);
            oeKey = ac.ProcessBlock(key, 0, key.Length);
            // Algorithm 3.10
            byte[] permsp = IVGenerator.GetIV(16);
            permsp[0] = (byte)permissions;
            permsp[1] = (byte)(permissions >> 8);
            permsp[2] = (byte)(permissions >> 16);
            permsp[3] = (byte)(permissions >> 24);
            permsp[4] = (byte)(255);
            permsp[5] = (byte)(255);
            permsp[6] = (byte)(255);
            permsp[7] = (byte)(255);
            permsp[8] = encryptMetadata ? (byte)'T' : (byte)'F';
            permsp[9] = (byte)'a';
            permsp[10] = (byte)'d';
            permsp[11] = (byte)'b';
            ac = new AESCipherCBCnoPad(true, key);
            perms = ac.ProcessBlock(permsp, 0, permsp.Length);
        }
        else {
            //PDF refrence 3.5.2 Standard Security Handler, Algorithum 3.3-1
            //If there is no owner password, use the user password instead.
            byte[] userPad = PadPassword(userPassword);
            byte[] ownerPad = PadPassword(ownerPassword);

            this.ownerKey = ComputeOwnerKey(userPad, ownerPad);
            documentID = CreateDocumentId();
            SetupByUserPad(this.documentID, userPad, this.ownerKey, permissions);
        }
    }

    private const int VALIDATION_SALT_OFFSET = 32;
    private const int KEY_SALT_OFFSET = 40;
    private const int SALT_LENGHT = 8;
    private const int OU_LENGHT = 48;

    virtual public bool ReadKey(PdfDictionary enc, byte[] password) {
        if (password == null)
            password = new byte[0];
        byte[] oValue = DocWriter.GetISOBytes(enc.Get(PdfName.O).ToString());
        byte[] uValue = DocWriter.GetISOBytes(enc.Get(PdfName.U).ToString());
        byte[] oeValue = DocWriter.GetISOBytes(enc.Get(PdfName.OE).ToString());
        byte[] ueValue = DocWriter.GetISOBytes(enc.Get(PdfName.UE).ToString());
        byte[] perms = DocWriter.GetISOBytes(enc.Get(PdfName.PERMS).ToString());
        bool isUserPass = false;
        IDigest md = DigestUtilities.GetDigest("SHA-256");
        md.BlockUpdate(password, 0, Math.Min(password.Length, 127));
        md.BlockUpdate(oValue, VALIDATION_SALT_OFFSET, SALT_LENGHT);
        md.BlockUpdate(uValue, 0, OU_LENGHT);
        byte[] hash = DigestUtilities.DoFinal(md);
        bool isOwnerPass = CompareArray(hash, oValue, 32);
        AESCipherCBCnoPad ac;
        if (isOwnerPass) {
            md.BlockUpdate(password, 0, Math.Min(password.Length, 127));
            md.BlockUpdate(oValue, KEY_SALT_OFFSET, SALT_LENGHT);
            md.BlockUpdate(uValue, 0, OU_LENGHT);
            md.DoFinal(hash, 0);
            ac = new AESCipherCBCnoPad(false, hash);
            key = ac.ProcessBlock(oeValue, 0, oeValue.Length);
        }
        else {
            md.BlockUpdate(password, 0, Math.Min(password.Length, 127));
            md.BlockUpdate(uValue, VALIDATION_SALT_OFFSET, SALT_LENGHT);
            md.DoFinal(hash, 0);
            isUserPass = CompareArray(hash, uValue, 32);
            if (!isUserPass)
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("bad.user.password"));
            md.BlockUpdate(password, 0, Math.Min(password.Length, 127));
            md.BlockUpdate(uValue, KEY_SALT_OFFSET, SALT_LENGHT);
            md.DoFinal(hash, 0);
            ac = new AESCipherCBCnoPad(false, hash);
            key = ac.ProcessBlock(ueValue, 0, ueValue.Length);
        }
        ac = new AESCipherCBCnoPad(false, key);
        byte[] decPerms = ac.ProcessBlock(perms, 0, perms.Length);
        if (decPerms[9] != (byte)'a' || decPerms[10] != (byte)'d' || decPerms[11] != (byte)'b')
            throw new BadPasswordException(MessageLocalization.GetComposedMessage("bad.user.password"));
        permissions = (decPerms[0] & 0xff) | ((decPerms[1] & 0xff) << 8)
                | ((decPerms[2] & 0xff) << 16) | ((decPerms[2] & 0xff) << 24);
        encryptMetadata = decPerms[8] == (byte)'T';
        return isOwnerPass;
    }

    private static bool CompareArray(byte[] a, byte[] b, int len) {
        for (int k = 0; k < len; ++k) {
            if (a[k] != b[k]) {
                return false;
            }
        }
        return true;
    }

    public static byte[] CreateDocumentId() {
        long time = DateTime.Now.Ticks + Environment.TickCount;
        long mem = GC.GetTotalMemory(false);
        String s = time + "+" + mem + "+" + (seq++);
        byte[] b = Encoding.ASCII.GetBytes(s);            
        return DigestAlgorithms.Digest("MD5", b);
    }

    virtual public void SetupByUserPassword(byte[] documentID, byte[] userPassword, byte[] ownerKey, long permissions) {
        SetupByUserPad(documentID, PadPassword(userPassword), ownerKey, permissions);
    }

    /**
     */
    private void SetupByUserPad(byte[] documentID, byte[] userPad, byte[] ownerKey, long permissions) {
        SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions);
        SetupUserKey();
    }

    /**
     */
    virtual public void SetupByOwnerPassword(byte[] documentID, byte[] ownerPassword, byte[] userKey, byte[] ownerKey, long permissions) {
        SetupByOwnerPad(documentID, PadPassword(ownerPassword), userKey, ownerKey, permissions);
    }

    private void SetupByOwnerPad(byte[] documentID, byte[] ownerPad, byte[] userKey, byte[] ownerKey, long permissions) {
        byte[] userPad = ComputeOwnerKey(ownerKey, ownerPad); //userPad will be set in this.ownerKey
        SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions); //step 3
        SetupUserKey();
    }

	virtual public void SetKey(byte[] key) {
        this.key = key;
    }
    
    virtual public void SetupByEncryptionKey(byte[] key, int keylength) {
        mkey = new byte[keylength/8];
        System.Array.Copy(key, 0, mkey, 0, mkey.Length);
    }    

    virtual public void SetHashKey(int number, int generation) {
        if (revision == AES_256)
            return;
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

    public static PdfObject CreateInfoId(byte[] id, bool modified) {
        ByteBuffer buf = new ByteBuffer(90);
        buf.Append('[').Append('<');
        if(id.Length != 16)
            id = CreateDocumentId();
        for (int k = 0; k < 16; ++k)
            buf.AppendHex(id[k]);
        buf.Append('>').Append('<');
        if (modified)
            id = CreateDocumentId();
        for (int k = 0; k < 16; ++k)
            buf.AppendHex(id[k]);
        buf.Append('>').Append(']');
        buf.Close();
        return new PdfLiteral(buf.ToByteArray());
    }

    virtual public PdfDictionary GetEncryptionDictionary() {
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
                if (revision == AES_256) {
                    dic.Put(PdfName.R, new PdfNumber(AES_256));
                    dic.Put(PdfName.V, new PdfNumber(5));
                }
                else {
                    dic.Put(PdfName.R, new PdfNumber(AES_128));
                    dic.Put(PdfName.V, new PdfNumber(4));
                }
                dic.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S5);
                                
                PdfDictionary stdcf = new PdfDictionary();
                stdcf.Put(PdfName.RECIPIENTS, recipients);                                                                    
                if (!encryptMetadata)
                    stdcf.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                
                if (revision == AES_128) {
                    stdcf.Put(PdfName.CFM, PdfName.AESV2);
                    stdcf.Put(PdfName.LENGTH, new PdfNumber(128));
                }
                else if (revision == AES_256) {
                    stdcf.Put(PdfName.CFM, PdfName.AESV3);
                    stdcf.Put(PdfName.LENGTH, new PdfNumber(256));
                }
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
            
            IDigest sh;
            if (revision == AES_256)
			    sh = DigestUtilities.GetDigest("SHA-256");
            else
                sh = DigestUtilities.GetDigest("SHA-1");
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
            if (revision == AES_256)
                key = mdResult;
            else
                SetupByEncryptionKey(mdResult, keyLength);              
        } else {
            dic.Put(PdfName.FILTER, PdfName.STANDARD);
            dic.Put(PdfName.O, new PdfLiteral(StringUtils.EscapeString(ownerKey)));
            dic.Put(PdfName.U, new PdfLiteral(StringUtils.EscapeString(userKey)));
            dic.Put(PdfName.P, new PdfNumber(permissions));
            dic.Put(PdfName.R, new PdfNumber(revision));
            if (revision == STANDARD_ENCRYPTION_40) {
                dic.Put(PdfName.V, new PdfNumber(1));
            }
            else if (revision == STANDARD_ENCRYPTION_128 && encryptMetadata) {
                dic.Put(PdfName.V, new PdfNumber(2));
                dic.Put(PdfName.LENGTH, new PdfNumber(128));
                
            }
            else if (revision == AES_256) {
                if (!encryptMetadata)
                    dic.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                dic.Put(PdfName.OE, new PdfLiteral(StringUtils.EscapeString(oeKey)));
                dic.Put(PdfName.UE, new PdfLiteral(StringUtils.EscapeString(ueKey)));
                dic.Put(PdfName.PERMS, new PdfLiteral(StringUtils.EscapeString(perms)));
                dic.Put(PdfName.V, new PdfNumber(revision));
                dic.Put(PdfName.LENGTH, new PdfNumber(256));
                PdfDictionary stdcf = new PdfDictionary();
                stdcf.Put(PdfName.LENGTH, new PdfNumber(32));
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
                stdcf.Put(PdfName.CFM, PdfName.AESV3);
                PdfDictionary cf = new PdfDictionary();
                cf.Put(PdfName.STDCF, stdcf);
                dic.Put(PdfName.CF, cf);
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
    
    virtual public PdfObject GetFileID(bool modified) {
        return CreateInfoId(documentID, modified);
    }

    virtual public OutputStreamEncryption GetEncryptionStream(Stream os) {
        return new OutputStreamEncryption(os, key, 0, keySize, revision);
    }
    
    virtual public int CalculateStreamSize(int n) {
        if (revision == AES_128 || revision == AES_256)
            return (n & 0x7ffffff0) + 32;
        else
            return n;
    }
    
    virtual public byte[] EncryptByteArray(byte[] b) {
        MemoryStream ba = new MemoryStream();
        OutputStreamEncryption os2 = GetEncryptionStream(ba);
        os2.Write(b, 0, b.Length);
        os2.Finish();
        return ba.ToArray();
    }
    
    virtual public StandardDecryption GetDecryptor() {
        return new StandardDecryption(key, 0, keySize, revision);
    }
    
    virtual public byte[] DecryptByteArray(byte[] b) {
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

    virtual public void AddRecipient(X509Certificate cert, int permission) {
        documentID = CreateDocumentId();
        publicKeyHandler.AddRecipient(new PdfPublicKeyRecipient(cert, permission));
    }

    virtual public byte[] ComputeUserPassword(byte[] ownerPassword) {
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
}
}
