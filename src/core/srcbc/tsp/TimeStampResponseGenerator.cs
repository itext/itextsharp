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
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Tsp
{
    /**
     * Generator for RFC 3161 Time Stamp Responses.
     */
    public class TimeStampResponseGenerator
    {
        private PkiStatus status;

        private Asn1EncodableVector statusStrings;

        private int failInfo;
        private TimeStampTokenGenerator tokenGenerator;
        private IList acceptedAlgorithms;
        private IList acceptedPolicies;
        private IList acceptedExtensions;

        public TimeStampResponseGenerator(
            TimeStampTokenGenerator tokenGenerator,
            IList acceptedAlgorithms)
            : this(tokenGenerator, acceptedAlgorithms, null, null)
        {
        }

        public TimeStampResponseGenerator(
            TimeStampTokenGenerator tokenGenerator,
            IList acceptedAlgorithms,
            IList acceptedPolicy)
            : this(tokenGenerator, acceptedAlgorithms, acceptedPolicy, null)
        {
        }

        public TimeStampResponseGenerator(
            TimeStampTokenGenerator tokenGenerator,
            IList acceptedAlgorithms,
            IList acceptedPolicies,
            IList acceptedExtensions)
        {
            this.tokenGenerator = tokenGenerator;
            this.acceptedAlgorithms = acceptedAlgorithms;
            this.acceptedPolicies = acceptedPolicies;
            this.acceptedExtensions = acceptedExtensions;

            statusStrings = new Asn1EncodableVector();
        }

        private void AddStatusString(string statusString)
        {
            statusStrings.Add(new DerUtf8String(statusString));
        }

        private void SetFailInfoField(int field)
        {
            failInfo |= field;
        }

        private PkiStatusInfo GetPkiStatusInfo()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
                new DerInteger((int)status));

            if (statusStrings.Count > 0)
            {
                v.Add(new PkiFreeText(new DerSequence(statusStrings)));
            }

            if (failInfo != 0)
            {
                v.Add(new FailInfo(failInfo));
            }

            return new PkiStatusInfo(new DerSequence(v));
        }

        public TimeStampResponse Generate(
            TimeStampRequest request,
            BigInteger serialNumber,
            DateTime genTime)
        {
            return Generate(request, serialNumber, new DateTimeObject(genTime));
        }

        /**
         * Return an appropriate TimeStampResponse.
         * <p>
         * If genTime is null a timeNotAvailable error response will be returned.
         *
         * @param request the request this response is for.
         * @param serialNumber serial number for the response token.
         * @param genTime generation time for the response token.
         * @param provider provider to use for signature calculation.
         * @return
         * @throws NoSuchAlgorithmException
         * @throws NoSuchProviderException
         * @throws TSPException
         * </p>
         */
        public TimeStampResponse Generate(
            TimeStampRequest request,
            BigInteger serialNumber,
            DateTimeObject genTime)
        {
            TimeStampResp resp;

            try
            {
                if (genTime == null)
                    throw new TspValidationException("The time source is not available.",
                        PkiFailureInfo.TimeNotAvailable);

                request.Validate(acceptedAlgorithms, acceptedPolicies, acceptedExtensions);

                this.status = PkiStatus.Granted;
                this.AddStatusString("Operation Okay");

                PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();

                ContentInfo tstTokenContentInfo;
                try
                {
                    TimeStampToken token = tokenGenerator.Generate(request, serialNumber, genTime.Value);
                    byte[] encoded = token.ToCmsSignedData().GetEncoded();

                    tstTokenContentInfo = ContentInfo.GetInstance(Asn1Object.FromByteArray(encoded));
                }
                catch (IOException e)
                {
                    throw new TspException("Timestamp token received cannot be converted to ContentInfo", e);
                }

                resp = new TimeStampResp(pkiStatusInfo, tstTokenContentInfo);
            }
            catch (TspValidationException e)
            {
                status = PkiStatus.Rejection;

                this.SetFailInfoField(e.FailureCode);
                this.AddStatusString(e.Message);

                PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();

                resp = new TimeStampResp(pkiStatusInfo, null);
            }

            try
            {
                return new TimeStampResponse(resp);
            }
            catch (IOException e)
            {
                throw new TspException("created badly formatted response!", e);
            }
        }

        class FailInfo
            : DerBitString
        {
            internal FailInfo(
                int failInfoValue)
                : base(GetBytes(failInfoValue), GetPadBits(failInfoValue))
            {
            }
        }

        /**
         * Generate a TimeStampResponse with chosen status and FailInfoField.
         *
         * @param status the PKIStatus to set.
         * @param failInfoField the FailInfoField to set.
         * @param statusString an optional string describing the failure.
         * @return a TimeStampResponse with a failInfoField and optional statusString
         * @throws TSPException in case the response could not be created
         */
        public TimeStampResponse GenerateFailResponse(PkiStatus status, int failInfoField, string statusString)
        {
            this.status = status;

            this.SetFailInfoField(failInfoField);

            if (statusString != null)
            {
                this.AddStatusString(statusString);
            }

            PkiStatusInfo pkiStatusInfo = GetPkiStatusInfo();

            TimeStampResp resp = new TimeStampResp(pkiStatusInfo, null);

            try
            {
                return new TimeStampResponse(resp);
            }
            catch (IOException e)
            {
                throw new TspException("created badly formatted response!", e);
            }
        }
    }
}
