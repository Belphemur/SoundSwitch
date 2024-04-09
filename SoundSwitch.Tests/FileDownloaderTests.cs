using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Framework.Updater;

namespace SoundSwitch.Tests;

[TestFixture]
public class FileDownloaderTests
{
    [Test]
    public async Task DownloadTest()
    {
        // Arrange
        var uri = new Uri("https://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4");
        using var stream = new MemoryStream();
        var cancellationToken = default(CancellationToken);
        long downloaded = 0;
        long fileSize = 0;
        
        // Act
        await FileDownloader.DownloadFileAsync(uri, stream, (l, l1) =>
        {
            downloaded = l;
            fileSize = l1;
        } , cancellationToken);
        
        //Assert
        downloaded.Should().Be(fileSize);
    }
}