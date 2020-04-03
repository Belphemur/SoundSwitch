/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using AuthenticodeExaminer;

namespace SoundSwitch.Framework.Updater
{
    public static class SignatureChecker
    {
        private static readonly string _certumSubject = "E=soundswitch@aaflalo.me, CN=\"Open Source Developer, Antoine Aflalo\", O=Open Source Developer, S=Quebec, C=CA";

        /// <summary>
        /// Does the given file have the right signature
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsValid(string filename)
        {
            return IsCertumSigned(filename);
        }

        private static bool IsCertumSigned(string filename)
        {
            var inspector = new FileInspector(filename);
            return inspector.Validate() == SignatureCheckResult.Valid
                   && inspector.GetSignatures().FirstOrDefault(signature => signature.SigningCertificate.Subject == _certumSubject) != null;
        }
    }
}