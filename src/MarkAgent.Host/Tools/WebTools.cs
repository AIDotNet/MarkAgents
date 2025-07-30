using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using MarkAgent.Host.Tools.Models;
using ModelContextProtocol.Server;

namespace MarkAgent.Host.Tools;

[McpServerToolType]
public class WebTools(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache memoryCache)
{
    private string? Key => configuration["TavilyKey"];

    private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(2);

    [McpServerTool, Description(Prompts.WebQueryPrompt)]
    public async Task<string> WebQuery(
        [Description(
            "The query to search for on the web. This should be a string that represents the search terms or question you want to ask.")]
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "<error>查询不能为空</error>";
        }

        // 生成缓存键
        var cacheKey = $"webquery:{query.ToLowerInvariant().GetHashCode()}";

        // 尝试从缓存获取结果
        if (memoryCache.TryGetValue(cacheKey, out string cachedResult))
        {
            return $"<cached_result>\n{cachedResult}\n</cached_result>";
        }

        var client = httpClientFactory.CreateClient();

        var sendMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.tavily.com/search")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                query
            }), Encoding.UTF8, "application/json"),
            Headers =
            {
                { "Authorization", $"Bearer {Key}" },
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
            }
        };

        var httpResponseMessage = await client.SendAsync(sendMessage);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return $"<error>搜索失败: {httpResponseMessage.StatusCode} - {httpResponseMessage.ReasonPhrase}</error>";
        }

        var result = await httpResponseMessage.Content.ReadFromJsonAsync<WebQueryResult>();

        if (result?.results is { Length: > 0 })
        {
            var response = GenerateResponse(result);

            // 缓存结果
            memoryCache.Set(cacheKey, response, _cacheExpiration);

            return response;
        }

        return $"<no_results>未找到相关搜索结果: {query}</no_results>";
    }

    private string GenerateResponse(WebQueryResult result)
    {
        var response = new StringBuilder();
        for (int i = 0; i < Math.Min(result.results.Length, 5); i++)
        {
            var searchResult = result.results[i];
            response.AppendLine($"## {searchResult.title}");
            response.AppendLine($"<url>{searchResult.url}</url>");
            response.AppendLine();
            if (!string.IsNullOrEmpty(searchResult.content))
            {
                response.AppendLine($"<content>{searchResult.content}</content>");
            }

            response.AppendLine();
        }

        return response.ToString();
    }
}