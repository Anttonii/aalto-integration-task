using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

public class Rating
{
    public double rate { get; set; }
    public int count { get; set; }
}

public class Item
{
    public int id { get; set; }
    public string? title { get; set; }
    public double price { get; set; }
    public string? description { get; set; }
    public string? category { get; set; }
    public string? image { get; set; }
}

/// <summary>
/// Auxiliary class for handling Http requests.
/// </summary>
public class RequestHandler
{
    private readonly HttpClient _client;
    private ILogger _logger;

    // Amount of time the request handler will retry before returning failure.
    private const int MAX_RETRIES = 5;

    public RequestHandler()
    {
        // Initialize logging for logging errors.
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Program");
        _logger = logger;

        _client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Sends a get request to the given URL and returns a list of items that are parsed from the received json.
    /// 
    /// If the request fails or the json is invalid, returns an empty list.
    /// </summary>
    /// <param name="url">The URL to retrieve the content from.</param>
    /// <returns></returns>
    public async Task<List<Item>> GetContent(string url)
    {
        string content = await GetContentAsync(url);

        if (content == "")
        {
            return new List<Item>();
        }

        return ParseItems(content);
    }

    /// <summary>
    /// Parse the json into a List<Item> if possible, otherwise returns an empty list.
    /// </summary>
    /// <param name="content">The json converted into a string.</param>
    /// <returns></returns>
    private List<Item> ParseItems(string content)
    {
        List<Item>? items = null;
        try
        {
            items = JsonConvert.DeserializeObject<List<Item>>(content);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to parse json with exception: {e.Message}");
        }

        if (items is null)
        {
            return new List<Item>();
        }

        return items;
    }
    private async Task<string> GetContentAsync(string url)
    {
        return await GetContentAsync(url, 0);
    }

    /// <summary>
    /// Sends a get request and returns the retrieved content as a string.
    /// 
    /// The returned string will be empty if the retrieved content is not a valid json.
    /// </summary>
    /// <param name="url">The URL to send the get request to.</param>
    /// <param name="attempt">The number of the the current attempt.</param>
    /// <returns></returns>
    private async Task<string> GetContentAsync(string url, int attempt)
    {
        if (attempt == 0)
        {
            _logger.LogInformation($"Sending get request to {url}");
        }
        else if (attempt > MAX_RETRIES)
        {
            _logger.LogError("Max retry attempts exceeded, returning empty string.");
            return "";
        }
        else
        {
            _logger.LogInformation($"Retrying the connection, attempt #{attempt}");
        }

        HttpResponseMessage? response = null;

        try
        {
            response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Unsuccessful request, status code: {response.StatusCode}");
                return "";
            }

            // In case of server error
            if ((int)response.StatusCode >= 500)
            {
                _logger.LogInformation("Response received a server error status code, retrying..");
                return await GetContentAsync(url, attempt + 1);
            }

            if (response.Content?.Headers?.ContentType?.MediaType != "application/json")
            {
                _logger.LogWarning("Request was successful but the result is not a valid json, returning empty string.");
                return "";
            }
        }
        catch (TimeoutException)
        {
            _logger.LogInformation("Connection was timed out, reattempting connection..");
            return await GetContentAsync(url, attempt + 1);
        }
        catch (Exception e)
        {
            _logger.LogError($"Exception caught when sending request: {e.Message}");
        }

        if (response is null)
        {
            _logger.LogError($"Failed to retrieve content from url: {url}");
            return "";
        }

        _logger.LogInformation("Request was successful.");
        return await response.Content.ReadAsStringAsync();
    }
}