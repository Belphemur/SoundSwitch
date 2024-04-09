using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Serilog;

namespace SoundSwitch.Framework.Updater
{
    public class FileDownloader
    {
        private static readonly ResiliencePipeline<HttpResponseMessage> RetryPipeline;
        private const int MAX_RETRY_ATTEMPTS = 5;

        static FileDownloader()
        {
            var retryOption = new RetryStrategyOptions<HttpResponseMessage>
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = MAX_RETRY_ATTEMPTS,
                UseJitter = true,
                OnRetry = arguments =>
                {
                    Log.Warning("Failed to download retrying {RetryCount}/{MaxRetries}", arguments.AttemptNumber, MAX_RETRY_ATTEMPTS);
                    return ValueTask.CompletedTask;
                },
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => !response.IsSuccessStatusCode) // Handle results
                    .Handle<HttpRequestException>() // Or handle exception
                    .Handle<TimeoutRejectedException>() // Chaining is supported
                    .Handle<HttpIOException>()
            };
            RetryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(retryOption).Build();
        }

        /// <summary>
        /// Downloads a file from the specified Uri into the specified stream.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="toStream"></param>
        /// <param name="progressCallback">If not null, will be called as the download progress. The first parameter will be the number of bytes downloaded so far, and the second the total size of the expected file after download.</param>
        /// <param name="cancellationToken">An optional CancellationToken that can be used to cancel the in-progress download.</param>
        /// <returns>A task that is completed once the download is complete.</returns>
        public static async Task DownloadFileAsync(Uri uri, Stream toStream, Action<long, long> progressCallback = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(toStream);

            if (uri.IsFile)
            {
                await ProcessFile(uri, toStream, progressCallback, cancellationToken);
                return;
            }

            await DownloadFileFromUrl(uri, toStream, progressCallback, cancellationToken);
        }

        private static async Task DownloadFileFromUrl(Uri uri, Stream toStream, Action<long, long> progressCallback, CancellationToken cancellationToken)
        {
            using var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            });
            client.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.ProductValue);
            client.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.CommentValue);
            
            using var response = await RetryPipeline.ExecuteAsync(async token =>
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                if (progressCallback != null)
                {
                    var length = response.Content.Headers.ContentLength ?? -1;
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = new byte[4096];
                    int read;
                    var totalRead = 0;
                    while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        await toStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                        totalRead += read;
                        progressCallback(totalRead, length);
                    }

                    Debug.Assert(totalRead == length || length == -1);
                    return response;
                }

                await response.Content.CopyToAsync(toStream, cancellationToken).ConfigureAwait(false);
                return response;
            }, cancellationToken);
        }

        private static async Task ProcessFile(Uri uri, Stream toStream, Action<long, long> progressCallback, CancellationToken cancellationToken)
        {
            await using var file = File.OpenRead(uri.LocalPath);

            if (progressCallback != null)
            {
                var length = file.Length;
                var buffer = new byte[4096];
                int read;
                var totalRead = 0;
                while ((read = await file.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await toStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    totalRead += read;
                    progressCallback(totalRead, length);
                }

                Debug.Assert(totalRead == length || length == -1);
                return;
            }

            await file.CopyToAsync(toStream, cancellationToken).ConfigureAwait(false);


            return;
        }
    }
}