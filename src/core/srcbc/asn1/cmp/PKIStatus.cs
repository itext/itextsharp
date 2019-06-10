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

namespace Org.BouncyCastle.Asn1.Cmp
{
	public enum PkiStatus
	{
		Granted					= 0,
		GrantedWithMods			= 1,
		Rejection				= 2,
		Waiting					= 3,
		RevocationWarning		= 4,
		RevocationNotification	= 5,
    	KeyUpdateWarning		= 6,
	}

	public class PkiStatusEncodable
		: Asn1Encodable
	{
		public static readonly PkiStatusEncodable granted = new PkiStatusEncodable(PkiStatus.Granted);
		public static readonly PkiStatusEncodable grantedWithMods = new PkiStatusEncodable(PkiStatus.GrantedWithMods);
		public static readonly PkiStatusEncodable rejection = new PkiStatusEncodable(PkiStatus.Rejection);
		public static readonly PkiStatusEncodable waiting = new PkiStatusEncodable(PkiStatus.Waiting);
		public static readonly PkiStatusEncodable revocationWarning = new PkiStatusEncodable(PkiStatus.RevocationWarning);
		public static readonly PkiStatusEncodable revocationNotification = new PkiStatusEncodable(PkiStatus.RevocationNotification);
		public static readonly PkiStatusEncodable keyUpdateWaiting = new PkiStatusEncodable(PkiStatus.KeyUpdateWarning);

		private readonly DerInteger status;

		private PkiStatusEncodable(PkiStatus status)
			: this(new DerInteger((int)status))
		{
		}

		private PkiStatusEncodable(DerInteger status)
		{
			this.status = status;
		}

		public static PkiStatusEncodable GetInstance(object obj)
		{
			if (obj is PkiStatusEncodable)
				return (PkiStatusEncodable)obj;

			if (obj is DerInteger)
				return new PkiStatusEncodable((DerInteger)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public virtual BigInteger Value
		{
			get { return status.Value; }
		}

		public override Asn1Object ToAsn1Object()
		{
			return status;
		}
	}
}
