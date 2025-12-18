using System.IO.Compression;

namespace AI.Invoicer.Infrastructure.AIService
{
    public class DownloadModelService
    {
        public event EventHandler<double>? ProgressChanged;
        public event EventHandler<string>? DownloadCompleted;
        public event EventHandler<Exception>? DownloadFailed;
        public async Task DownloadFileAsync(string url, string destinationPath)
        {
            using var httpClient = new HttpClient();
            try
            {
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ;
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var totalReadBytes = 0L;
                var isMoreToRead = true;
                var buffer = new byte[8192];
                do
                {
                    var readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        isMoreToRead = false;
                        ProgressChanged?.Invoke(this, 100);
                        continue;
                    }
                    await fileStream.WriteAsync(buffer.AsMemory(0, readBytes));
                    totalReadBytes += readBytes;
                    if (totalBytes.HasValue)
                    {
                        var progress = (double)totalReadBytes / totalBytes.Value * 100;
                        ProgressChanged?.Invoke(this, progress);
                    }
                } while (isMoreToRead);

                DownloadCompleted?.Invoke(this, destinationPath);
            }
            catch (Exception ex)
            {
                DownloadFailed?.Invoke(this, ex);
                return;
            }
        }


        public Task DecompressZipFileAsync(string zipPath, string extractPath)
        {
            return Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            });
        }
    }
}
