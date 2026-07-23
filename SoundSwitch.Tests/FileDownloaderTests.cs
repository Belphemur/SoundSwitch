using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Updater;

namespace SoundSwitch.Tests;

[TestFixture]
public class FileDownloaderTests
{
    private bool IsMP4(Stream stream)
    {
        // Check if the stream has enough bytes for the magic number
        if (stream.Length < 8)
            return false;

        // Read the first 8 bytes from the stream
        var buffer = new byte[8];
        stream.Position = 0;
        stream.ReadExactly(buffer, 0, 8);

        // Check if the byte sequence matches the MP4 magic number
        return buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x00 && buffer[3] == 0x14 && buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70;
    }

    /// <summary>
    /// Creates a fake MP4 file: the 8-byte magic number followed by padding,
    /// sized larger than the downloader's read buffer to trigger multiple progress callbacks.
    /// </summary>
    private static byte[] CreateMp4Content()
    {
        var content = new byte[8192];
        content[3] = 0x14; // ftyp box size
        content[4] = 0x66; // f
        content[5] = 0x74; // t
        content[6] = 0x79; // y
        content[7] = 0x70; // p
        return content;
    }

    private static bool HasHeaderEnd(byte[] buffer, int length)
    {
        for (var i = 3; i < length; i++)
        {
            if (buffer[i - 3] == (byte)'\r' && buffer[i - 2] == (byte)'\n' && buffer[i - 1] == (byte)'\r' && buffer[i] == (byte)'\n')
                return true;
        }

        return false;
    }

    [Test]
    public async Task DownloadTest()
    {
        // Arrange: serve the file from a local web server, so the test doesn't rely on an external resource
        var content = CreateMp4Content();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var uri = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/BigBuckBunny_320x180.mp4");

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            await using var network = client.GetStream();

            // Read the request headers before answering
            var request = new byte[4096];
            var requestLength = 0;
            int read;
            while ((read = await network.ReadAsync(request.AsMemory(requestLength, request.Length - requestLength))) > 0)
            {
                requestLength += read;
                if (HasHeaderEnd(request, requestLength))
                    break;
            }

            var header = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: video/mp4\r\nContent-Length: {content.Length}\r\nConnection: close\r\n\r\n");
            await network.WriteAsync(header);
            await network.WriteAsync(content);
        });

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

        await serverTask;

        //Assert
        downloaded.Should().Be(fileSize);
        fileSize.Should().Be(content.Length);
        stream.Should().Match(memStream => IsMP4(memStream));
    }
}
