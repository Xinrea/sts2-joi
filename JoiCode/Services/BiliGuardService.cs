using System.Net.Http;
using System.Text.Json;

namespace Joi.JoiCode.Services;

public static class BiliGuardService
{
    private const string ApiUrl =
        "https://api.live.bilibili.com/xlive/app-room/v2/guardTab/topList?roomid=21484828&page=1&page_size=29&ruid=61639371";

    private static readonly List<string> _guardNames = [];
    private static bool _loaded;

    public static IReadOnlyList<string> GuardNames => _guardNames;

    public static async void Initialize()
    {
        if (_loaded) return;
        _loaded = true;

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var json = await client.GetStringAsync(ApiUrl);
            var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            foreach (var item in data.GetProperty("top3").EnumerateArray())
            {
                if (item.TryGetProperty("username", out var name))
                    _guardNames.Add(name.GetString()!);
            }

            foreach (var item in data.GetProperty("list").EnumerateArray())
            {
                if (item.TryGetProperty("username", out var name))
                    _guardNames.Add(name.GetString()!);
            }

            MainFile.Logger?.Info($"BiliGuardService: loaded {_guardNames.Count} guard names");
        }
        catch (Exception ex)
        {
            MainFile.Logger?.Warn($"BiliGuardService: failed to load guard names: {ex.Message}");
        }
    }

    public static string? GetRandomGuardName()
    {
        if (_guardNames.Count == 0) return null;
        return _guardNames[Random.Shared.Next(_guardNames.Count)];
    }
}
