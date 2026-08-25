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

#if NIGHTLY
using System;
using System.IO;
using System.Security.Cryptography;

using RailSharp;
using RailSharp.Internal.Result;

using Serilog;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Verifies the integrity of a downloaded nightly installer by computing its
/// SHA-512 checksum and comparing it against the expected value.
/// </summary>
public static class UpdateVerifier
{
    /// <summary>
    /// Verify that the file at <paramref name="filename"/> matches <paramref name="expectedSha512"/>.
    /// </summary>
    public static Result<string, VoidSuccess> Verify(string filename, string expectedSha512)
    {
        if (string.IsNullOrWhiteSpace(expectedSha512))
        {
            return "No SHA-512 checksum is available for this update.";
        }

        try
        {
            string actual;
            using (var stream = File.OpenRead(filename))
            using (var sha512 = SHA512.Create())
            {
                var hash = sha512.ComputeHash(stream);
                actual = Convert.ToHexString(hash).ToLowerInvariant();
            }

            if (string.Equals(actual, expectedSha512.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success();
            }

            Log.Error("SHA-512 mismatch for {File}: expected {Expected}, got {Actual}", filename, expectedSha512, actual);
            return "The downloaded file does not match its expected SHA-512 checksum.";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to verify SHA-512 checksum for {File}", filename);
            return ex.Message;
        }
    }
}
#endif
