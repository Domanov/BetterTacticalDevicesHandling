using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;

namespace BetterTacticalDevicesHandling.Client.Inspect;

// Out of raid the game has no inventory action for device state, so changes go to the mod's server route.
internal static class LightStateStore
{
    // Route and JSON keys must match LightStateApi in Server/LightStateRouter.cs.
    private const string RouteLights = "/dmnv/bettertacticaldeviceshandling/lights";
    private const string JsonItems = "items";
    private const string JsonId = "id";
    private const string JsonIsActive = "isActive";
    private const string JsonMode = "mode";
    private const string JsonFailed = "failed";

    // Returns the ids the server did not save; all ids when the request itself failed.
    public static async Task<HashSet<string>> Save(IReadOnlyList<LightsState> states)
    {
        JObject request = new JObject
        {
            [JsonItems] = new JArray(states.Select(s => new JObject
            {
                [JsonId] = s.Id,
                [JsonIsActive] = s.IsActive,
                [JsonMode] = s.LightMode
            }))
        };
        try
        {
            string response = await RequestHandler.PostJsonAsync(RouteLights, request.ToString());
            JArray failed = JObject.Parse(response)[JsonFailed] as JArray;
            return new HashSet<string>(failed?.Select(t => (string)t) ?? Enumerable.Empty<string>());
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Inspect: saving light state failed (is the server part of {Plugin.Name} installed?): {e.Message}");
            return new HashSet<string>(states.Select(s => s.Id));
        }
    }
}
