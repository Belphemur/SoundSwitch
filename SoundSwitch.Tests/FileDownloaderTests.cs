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
    private bool IsMP4(MemoryStream stream)
    {
        // Check if the stream has enough bytes for the magic number
        if (stream.Length < 8)
            return false;

        // Read the first 8 bytes from the stream
        var buffer = new byte[8];
        stream.Position = 0;
        stream.Read(buffer, 0, 8);

        // Check if the byte sequence matches the MP4 magic number
        return buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x00 && buffer[3] == 0x14 && buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70;
    }
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
        }, cancellationToken);

        //Assert
        downloaded.Should().Be(fileSize);
        IsMP4(stream).Should().BeTrue();
    }
}