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

using AuthenticodeExaminer;
using RailSharp;
using RailSharp.Internal.Result;

namespace SoundSwitch.Framework.Updater
{
    public static class SignatureChecker
    {
        /// <summary>
        /// Does the given file have the right signature
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Result<SignatureCheckResult, VoidSuccess> IsValid(string filename)
        {
            var inspector = new FileInspector(filename);
            var result = inspector.Validate(RevocationChecking.Online);
            if (result != SignatureCheckResult.Valid)
            {
                return result;
            }

            return Result.Success();
        }
    }
}