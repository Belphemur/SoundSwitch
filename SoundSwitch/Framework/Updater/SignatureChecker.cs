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

using System.Security.Cryptography.X509Certificates;

namespace SoundSwitch.Framework.Updater
{
    public static class SignatureChecker
    {

        private static string _publicKey = "3082020A0282020100C9EAE3B1E873CE4F55AE1319171EED0B6B9D62DA5BC505BE864A98DC7C70C8B0AB2AD75644FB0E32176196F40A73CC2D1709F9F8C1FDF87732661D02C0C1F94956A57FA20A84408FA26BB46F59D581137257D3804EFBFA6B63CF7DF0F94E1060137FBC278D38EBCE304B9D73DF5D412DFF5BA2457130DD091C7059F73B3FC236BDB2F0A43DCD2F79843096D100CD7FFD35C282AE2AC0F716006EEB37BA832DF9758B7C2E1DCFC9D533472F01833E1C7AB46550B4C33D63A490E14FB5E3DDD1C505F590724FCA714E45DFE005CA52DBDF4272E54A3D27CDF4C0E08B4C991F48C77166197A44F1DB1A81C3D89E47F67E74C4265663BDC1332E438559EEB61056E57759F7A3D6576D6E9186C923E629B88D0C558C43ADA0C7CFB79E29993768CA9B6E81BACE389380E367E46C8E28077DA2C1FD02C50CF541ADBF05C170A13AFA80F66E8C7775D741C8D4F20FB990B934B9668BBE791F72367DB5D12E072BC692DAFDCF49327962B3E7904107EF3AA975C8524B3EB9F6F61E2B94F30292847559DD8A235DA7FF88F7F0B3CFF21D5BFD7452580FC8E24780C7EAB0E2B37791844522561392BA0F268641D25BF98E4EAA492B7563A1714505FEEDE82A560774A55E1C7D69B91328184607C3867E2E27D00E417CF6B8BC8B3A3B21CC1E775F7B655C39541342D492071AFBE5E2B89A136055429EA0A5E1C762AA5DE75A2625F78C4CD50203010001";
        private static string _serialNumber = "942A37BCA9A9889442F6710533CB5548";
        /// <summary>
        /// Does the given file have the right signature
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsValid(string filename)
        {
            var certificate = X509Certificate.CreateFromSignedFile(filename);
            return certificate.GetPublicKeyString() == _publicKey
                   && certificate.Issuer.Contains("CN=aaflalo.me")
                   && certificate.GetSerialNumberString() == _serialNumber;
        }
    }
}