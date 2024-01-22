using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace LM.WEB.Services;

public class CliServiceBase
{
    private readonly IHttpClientFactory _factory;
    public readonly ILogger<CliServiceBase> _logger;
    public readonly HttpClient _httpClient;
    public CliServiceBase(IHttpClientFactory factory, ILogger<CliServiceBase> logger)
    {
        _logger = logger;
        _factory = factory;
        _httpClient = factory.CreateClient("api");
    }

    /// <summary>
    /// REST CLIENT Methode Get
    /// </summary>
    /// <param name="pEnpoint"></param>
    /// <param name="pParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>HttpResponseMessage</returns>
    public async Task<HttpResponseMessage> GetAsync(string pEnpoint, Dictionary<string, object?>? pParams = null, CancellationToken? cancellationToken = null)
    {
        try
        {
            string queryString = "";
            if (pParams != null && pParams.Any()) queryString = "?" + string.Join("&", pParams.Select(m => $"{m.Key}={m.Value}"));
            Debug.Print(queryString);
            HttpResponseMessage response = await _httpClient.GetAsync($"api/{pEnpoint}{queryString}");
            Debug.Print(queryString);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAsync", pEnpoint);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string pEnpoint, object? pParams = null, CancellationToken? cancellationToken = null)
    {
        try
        {
            string jsonBody = string.Empty;
            if (pParams != null) jsonBody = JsonConvert.SerializeObject(pParams);
            HttpResponseMessage response = await _httpClient.PostAsync($"api/{pEnpoint}", new StringContent(jsonBody, UnicodeEncoding.UTF8, "application/json"));
            Debug.Print(jsonBody);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostAsync", pEnpoint);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PutAsync(string pEnpoint, object? pParams = null, CancellationToken? cancellationToken = null)
    {
        try
        {
            string jsonBody = string.Empty;
            if (pParams != null) jsonBody = JsonConvert.SerializeObject(pParams);
            HttpResponseMessage response = await _httpClient.PutAsync($"api/{pEnpoint}", new StringContent(jsonBody, UnicodeEncoding.UTF8, "application/json"));
            Debug.Print(jsonBody);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PutAsync", pEnpoint);
            throw;
        }
    }

    public async Task<HttpResponseMessage?> DeleteAsync(string pEnpoint, Dictionary<string, object?>? pParams = null, CancellationToken? cancellationToken = null)
    {
        HttpResponseMessage? response = null;
        try
        {
            string queryString = "";
            if (pParams != null && pParams.Any()) queryString = "?" + string.Join("&", pParams.Select(m => $"{m.Key}={m.Value}"));
            response = await _httpClient.DeleteAsync($"api/{pEnpoint}{queryString}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync", pEnpoint);
            throw;
        }
        return response;
    }

    public bool ValidateJsonContent(HttpContent content)
    {
        var mediaType = content?.Headers.ContentType?.MediaType;
        return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
