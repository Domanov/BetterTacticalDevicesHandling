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
    // Route and JSON keys must match Server/LightStateRouter.cs.
    private const string Route = "/dmnv/bettertacticaldeviceshandling/lights";
    private const string ItemsKey = "items";
    private const string IdKey = "id";
    private const string IsActiveKey = "isActive";
    private const string ModeKey = "mode";
    private const string FailedKey = "failed";

    // Returns the ids the server did not save; all ids when the request itself failed.
    public static async Task<HashSet<string>> Save(IReadOnlyList<LightsState> states)
    {
        JObject request = new JObject
        {
            [ItemsKey] = new JArray(states.Select(s => new JObject
            {
                [IdKey] = s.Id,
                [IsActiveKey] = s.IsActive,
                [ModeKey] = s.LightMode
            }))
        };
        try
        {
            string response = await RequestHandler.PostJsonAsync(Route, request.ToString());
            JArray failed = JObject.Parse(response)[FailedKey] as JArray;
            return new HashSet<string>(failed?.Select(t => (string)t) ?? Enumerable.Empty<string>());
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Inspect: saving light state failed (is the server part of {Plugin.Name} installed?): {e.Message}");
            return new HashSet<string>(states.Select(s => s.Id));
        }
    }
}
