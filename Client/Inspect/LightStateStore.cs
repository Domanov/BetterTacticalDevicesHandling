using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;

namespace BetterTacticalDevicesHandling.Client.Inspect;

// Out of raid the game has no inventory action for device state, so changes go to the mod's server route.
internal static class LightStateStore
{
    private const string Route = "/dmnv/bettertacticaldeviceshandling/lights";

    // Returns the ids the server did not save; all ids when the request itself failed.
    public static async Task<HashSet<string>> Save(IReadOnlyList<LightsState> states)
    {
        string json = JsonConvert.SerializeObject(new
        {
            items = states.Select(s => new { id = s.Id, isActive = s.IsActive, mode = s.LightMode })
        });
        try
        {
            string response = await RequestHandler.PostJsonAsync(Route, json);
            JArray failed = JObject.Parse(response)["failed"] as JArray;
            return new HashSet<string>(failed?.Select(t => (string)t) ?? Enumerable.Empty<string>());
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Inspect: saving light state failed (is the server part of {Plugin.Name} installed?): {e.Message}");
            return new HashSet<string>(states.Select(s => s.Id));
        }
    }
}
