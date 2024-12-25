using System.Diagnostics;

namespace PatTool.Helpers;

public static partial class Validators
{
    internal static bool IsAzureCliInstalled()
    {
        try
        {
            // Start a process to execute 'az --version' command
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "az",
                Arguments = "--version",  // 'az --version' will return version info if installed
                RedirectStandardOutput = true,  // To capture the output
                RedirectStandardError = true,   // To capture any error messages
                UseShellExecute = false,  // We need to handle standard output/error manually
                CreateNoWindow = true    // Don't show a console window for the process
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null) return false;

                // Read the standard output (this will contain version info if `az` is installed)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // If there was an error in executing `az`, or no output was returned, then `az` might not be installed
                if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                {
                    return false;
                }

                // If version info is found, `az` is installed
                return !string.IsNullOrEmpty(output);
            }
        }
        catch (Exception)
        {
            // If an exception occurs (e.g., file not found or access issue), assume `az` is not installed
            return false;
        }
    }
}