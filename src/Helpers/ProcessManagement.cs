using System.Diagnostics;

namespace PatTool.Helpers;

internal static class ProcessManagement
{
    internal static async Task<string> StartProcessWithRetryAsync(ProcessStartInfo processStartInfo)
    {
        Process? process = null;
        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        // Retry logic for process start
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            process = System.Diagnostics.Process.Start(processStartInfo);
            
            if (process != null)
            {
                break; // If the process started, exit the loop
            }

            // If process is null, wait and retry
            if (attempt < maxRetries)
            {
                await Task.Delay(delay * (int)Math.Pow(2, attempt)); // Exponential backoff
            }
        }

        // If the process is still null after retries, throw an exception
        if (process == null)
        {
            throw new InvalidOperationException("The process could not be started after multiple retries.");
        }

        // Proceed with reading the output after ensuring the process is not null
        using var reader = process.StandardOutput;
        return await reader.ReadToEndAsync();
    }
}
