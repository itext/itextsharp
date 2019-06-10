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

using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Tsp
{
	public class TimeStampTokenInfo
	{
		private TstInfo		tstInfo;
		private DateTime	genTime;

		public TimeStampTokenInfo(
			TstInfo tstInfo)
		{
			this.tstInfo = tstInfo;

			try
			{
				this.genTime = tstInfo.GenTime.ToDateTime();
			}
			catch (Exception e)
			{
				throw new TspException("unable to parse genTime field: " + e.Message);
			}
		}

		public bool IsOrdered
		{
			get { return tstInfo.Ordering.IsTrue; }
		}

		public Accuracy Accuracy
		{
			get { return tstInfo.Accuracy; }
		}

		public DateTime GenTime
		{
			get { return genTime; }
		}

		public GenTimeAccuracy GenTimeAccuracy
		{
			get
			{
				return this.Accuracy == null
					?	null
					:	new GenTimeAccuracy(this.Accuracy);
			}
		}

		public string Policy
		{
			get { return tstInfo.Policy.Id; }
		}

		public BigInteger SerialNumber
		{
			get { return tstInfo.SerialNumber.Value; }
		}

		public GeneralName Tsa
		{
			get { return tstInfo.Tsa; }
		}

		/**
		 * @return the nonce value, null if there isn't one.
		 */
		public BigInteger Nonce
		{
			get
			{
				return tstInfo.Nonce == null
					?	null
					:	tstInfo.Nonce.Value;
			}
		}

		public AlgorithmIdentifier HashAlgorithm
		{
			get { return tstInfo.MessageImprint.HashAlgorithm; }
		}

		public string MessageImprintAlgOid
		{
			get { return tstInfo.MessageImprint.HashAlgorithm.ObjectID.Id; }
		}

		public byte[] GetMessageImprintDigest()
		{
			return tstInfo.MessageImprint.GetHashedMessage();
		}

		public byte[] GetEncoded()
		{
			return tstInfo.GetEncoded();
		}

		public TstInfo TstInfo
		{
			get { return tstInfo; }
		}
	}
}
