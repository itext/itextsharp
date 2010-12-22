using System;

namespace Org.BouncyCastle.Asn1.Icao
{
	public abstract class IcaoObjectIdentifiers
	{
		//
		// base id
		//
		public const string IdIcao = "2.23.136";

		public static readonly DerObjectIdentifier IdIcaoMrtd				= new DerObjectIdentifier(IdIcao + ".1");
		public static readonly DerObjectIdentifier IdIcaoMrtdSecurity		= new DerObjectIdentifier(IdIcaoMrtd + ".1");

		// LDS security object, see ICAO Doc 9303-Volume 2-Section IV-A3.2
		public static readonly DerObjectIdentifier IdIcaoLdsSecurityObject	= new DerObjectIdentifier(IdIcaoMrtdSecurity + ".1");

		// CSCA master list, see TR CSCA Countersigning and Master List issuance
		public static readonly DerObjectIdentifier IdIcaoCscaMasterList     = new DerObjectIdentifier(IdIcaoMrtdSecurity + ".2");
		public static readonly DerObjectIdentifier IdIcaoCscaMasterListSigningKey = new DerObjectIdentifier(IdIcaoMrtdSecurity + ".3");

		// document type list, see draft TR LDS and PKI Maintenance, par. 3.2.1
		public static readonly DerObjectIdentifier IdIcaoDocumentTypeList  = new DerObjectIdentifier(IdIcaoMrtdSecurity + ".4");

		// Active Authentication protocol, see draft TR LDS and PKI Maintenance,
		// par. 5.2.2
		public static readonly DerObjectIdentifier IdIcaoAAProtocolObject  = new DerObjectIdentifier(IdIcaoMrtdSecurity + ".5");

		// CSCA name change and key reoll-over, see draft TR LDS and PKI
		// Maintenance, par. 3.2.1
		public static readonly DerObjectIdentifier IdIcaoExtensions         = new DerObjectIdentifier(IdIcaoMrtdSecurity + ".6");
		public static readonly DerObjectIdentifier IdIcaoExtensionsNamechangekeyrollover = new DerObjectIdentifier(IdIcaoExtensions + ".1");
	}
}
