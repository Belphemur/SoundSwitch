/********************************************************************
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using NUnit.Framework;
using FluentAssertions;
using SoundSwitch.Framework.Banner;

namespace SoundSwitch.Tests;

[TestFixture]
public class ExclusiveFullscreenDetectorTests
{
    [Test]
    public void TestIsForegroundInExclusiveFullscreen_DoesNotThrow()
    {
        // Act
        Action act = () => ExclusiveFullscreenDetector.IsForegroundInExclusiveFullscreen();

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void TestIsForegroundInExclusiveFullscreen_ReturnsBool()
    {
        // Act — in the test environment there's no FSE window, so it should return false
        var result = ExclusiveFullscreenDetector.IsForegroundInExclusiveFullscreen();

        // Assert
        result.Should().BeFalse("no fullscreen application is running in the test environment");
    }
}
