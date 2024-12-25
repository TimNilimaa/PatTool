using System.Diagnostics;

namespace PatTool.Helpers;

public static partial class Validators
{
    internal static bool IsAzureCliLoggedIn()
    {
        try
        {
            // Run 'az account show' to check if the user is logged in
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "az",
                Arguments = "account show", // This command shows details of the logged-in account
                RedirectStandardOutput = true, // Capture the output
                RedirectStandardError = true,  // Capture any error messages
                UseShellExecute = false,      // We need to handle standard output/error manually
                CreateNoWindow = true         // Don't show a console window for the process
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null) return false;

                // Read the standard output and error
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // If an error occurred or no output was returned, the user is not logged in
                if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                {
                    return false;
                }

                // If account info is returned, the user is logged in
                return !string.IsNullOrEmpty(output);
            }
        }
        catch (Exception)
        {
            // If an exception occurs (e.g., az is not found or access issues), assume user is not logged in
            return false;
        }
    }
}