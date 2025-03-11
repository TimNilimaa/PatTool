using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PatTool.Exceptions;
using PatTool.Helpers;

const string azureDevOpsResourceId = "499b84ac-1321-427f-aa17-267ca6975798"; //Azure DevOps

using var serviceProvider = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

if (!Validators.IsAzureCliInstalled())
{
    logger.LogError("Install Azure CLI first. https://aka.ms/installazurecli");
    return;
}

if (!Validators.IsAzureCliLoggedIn())
{
    logger.LogError("You must be logged in to Azure CLI first. Run: az login");
    return;
}

string? organization = Environment.GetEnvironmentVariable("ADO_PatTool_Org");

if (string.IsNullOrEmpty(organization))
{
    logger.LogError("Environment variables ADO_PatTool_Org must be set.");
    return;
}

int daysUntilExpiry = 1;

string? ttlEnvVar = Environment.GetEnvironmentVariable("ADO_PatTool_TTL");
if (!string.IsNullOrEmpty(ttlEnvVar) && int.TryParse(ttlEnvVar, out int ttlFromEnv))
{
    daysUntilExpiry = ttlFromEnv;
}

logger.LogDebug("Using Organization: {Organization}", organization);
logger.LogDebug("Token will expire in {DaysUntilExpiry} day(s).", daysUntilExpiry);

string token = await GetAccessToken(azureDevOpsResourceId);

var headers = new Dictionary<string, string>
{
    { "Authorization", "Bearer " + token },
};

string pipelineRunUrl = $"https://vssps.dev.azure.com/{organization}/_apis/tokens/pats?api-version=7.2-preview.1";

var adoToken = await CallAzureDevOpsApi(pipelineRunUrl, headers, daysUntilExpiry);

logger.LogTrace("Your token has been generated");
Console.WriteLine(adoToken);

static async Task<string> GetAccessToken(string azureDevOpsResourceId)
{
    var processStartInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "az",
        Arguments = $"account get-access-token --resource {azureDevOpsResourceId}",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    var output = await ProcessManagement.StartProcessWithRetryAsync(processStartInfo);

    var jsonDoc = JsonDocument.Parse(output) ?? throw new NoOutputFromExternalProgramException();

    return jsonDoc.RootElement.GetProperty("accessToken").GetString() ?? "";
}



static async Task<string> CallAzureDevOpsApi(string url, Dictionary<string, string> headers, int daysUntilExpiry)
{
    using var client = new HttpClient();

    foreach (var header in headers)
    {
        client.DefaultRequestHeaders.Add(header.Key, header.Value);
    }

    var body = new
    {
        scope = "vso.packaging",
        allOrgs = false,
        displayName = "Auto generated PAT by PatTool",
        validTo = DateTime.Now.AddDays(daysUntilExpiry)
    };

    HttpResponseMessage response = await client.PostAsJsonAsync(url, body);
    response.EnsureSuccessStatusCode();

    string responseContent = await response.Content.ReadAsStringAsync();
    var jsonDoc = JsonDocument.Parse(responseContent);

    return jsonDoc.RootElement.GetProperty("patToken").GetProperty("token").GetString() ?? "";
}
