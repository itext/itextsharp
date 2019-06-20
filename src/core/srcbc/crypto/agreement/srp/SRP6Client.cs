/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement.Srp
{
	/**
	 * Implements the client side SRP-6a protocol. Note that this class is stateful, and therefore NOT threadsafe.
	 * This implementation of SRP is based on the optimized message sequence put forth by Thomas Wu in the paper
	 * "SRP-6: Improvements and Refinements to the Secure Remote Password Protocol, 2002"
	 */
	public class Srp6Client
	{
	    protected BigInteger N;
	    protected BigInteger g;

	    protected BigInteger privA;
	    protected BigInteger pubA;

	    protected BigInteger B;

	    protected BigInteger x;
	    protected BigInteger u;
	    protected BigInteger S;

	    protected IDigest digest;
	    protected SecureRandom random;

	    public Srp6Client()
	    {
	    }

	    /**
	     * Initialises the client to begin new authentication attempt
	     * @param N The safe prime associated with the client's verifier
	     * @param g The group parameter associated with the client's verifier
	     * @param digest The digest algorithm associated with the client's verifier
	     * @param random For key generation
	     */
	    public virtual void Init(BigInteger N, BigInteger g, IDigest digest, SecureRandom random)
	    {
	        this.N = N;
	        this.g = g;
	        this.digest = digest;
	        this.random = random;
	    }

	    /**
	     * Generates client's credentials given the client's salt, identity and password
	     * @param salt The salt used in the client's verifier.
	     * @param identity The user's identity (eg. username)
	     * @param password The user's password
	     * @return Client's public value to send to server
	     */
	    public virtual BigInteger GenerateClientCredentials(byte[] salt, byte[] identity, byte[] password)
	    {
	        this.x = Srp6Utilities.CalculateX(digest, N, salt, identity, password);
	        this.privA = SelectPrivateValue();
	        this.pubA = g.ModPow(privA, N);

	        return pubA;
	    }

	    /**
	     * Generates client's verification message given the server's credentials
	     * @param serverB The server's credentials
	     * @return Client's verification message for the server
	     * @throws CryptoException If server's credentials are invalid
	     */
	    public virtual BigInteger CalculateSecret(BigInteger serverB)
	    {
	        this.B = Srp6Utilities.ValidatePublicValue(N, serverB);
	        this.u = Srp6Utilities.CalculateU(digest, N, pubA, B);
	        this.S = CalculateS();

	        return S;
	    }

	    protected virtual BigInteger SelectPrivateValue()
	    {
	    	return Srp6Utilities.GeneratePrivateValue(digest, N, g, random);    	
	    }

	    private BigInteger CalculateS()
	    {
	        BigInteger k = Srp6Utilities.CalculateK(digest, N, g);
	        BigInteger exp = u.Multiply(x).Add(privA);
	        BigInteger tmp = g.ModPow(x, N).Multiply(k).Mod(N);
	        return B.Subtract(tmp).Mod(N).ModPow(exp, N);
	    }
	}
}
