using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace BetterTacticalDevicesHandling.Server;

public record LightState
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    [JsonPropertyName("mode")]
    public int Mode { get; init; }
}

public record LightStateRequest : IRequestData
{
    [JsonPropertyName("items")]
    public List<LightState> Items { get; init; } = [];
}

public record LightStateResponse
{
    [JsonPropertyName("failed")]
    public List<string> Failed { get; init; } = [];
}

[Injectable]
public class LightStateRouter(JsonUtil jsonUtil, LightStateService service) : StaticRouter(jsonUtil, [
    new RouteAction<LightStateRequest>(
        LightStateService.Route,
        async (_, request, sessionId, _, cancellationToken) => await service.Apply(request, sessionId, cancellationToken))
]);

// Out of raid there is no inventory action for a device's on/off state and mode, so the client sends it here
// and it is written straight into the caller's own profile (PMC or scav stash).
[Injectable]
public class LightStateService(
    ProfileHelper profileHelper,
    ItemHelper itemHelper,
    SaveServer saveServer,
    HttpResponseUtil httpResponseUtil,
    ISptLogger<LightStateService> logger)
{
    public const string Route = "/dmnv/bettertacticaldeviceshandling/lights";

    public async ValueTask<string> Apply(LightStateRequest request, MongoId sessionId, CancellationToken cancellationToken)
    {
        List<string> failed = [];
        Dictionary<MongoId, LightState> pending = [];
        foreach (LightState state in request.Items)
        {
            if (MongoId.IsValidMongoId(state.Id))
            {
                pending[new MongoId(state.Id)] = state;
            }
            else
            {
                failed.Add(state.Id);
            }
        }

        // One pass over the inventories, stopping as soon as every requested item is found.
        int applied = 0;
        foreach (PmcData? profile in new[] { profileHelper.GetPmcProfile(sessionId), profileHelper.GetScavProfile(sessionId) })
        {
            foreach (Item item in profile?.Inventory?.Items ?? [])
            {
                if (pending.Count == 0)
                {
                    break;
                }

                if (!pending.Remove(item.Id, out LightState? state))
                {
                    continue;
                }

                if (ModesCount(item) is int modes)
                {
                    item.Upd ??= new Upd();
                    item.Upd.Light = new UpdLight { IsActive = state.IsActive, SelectedMode = ((state.Mode % modes) + modes) % modes };
                    applied++;
                }
                else
                {
                    failed.Add(state.Id);
                }
            }
        }

        failed.AddRange(pending.Values.Select(s => s.Id));
        if (applied > 0)
        {
            await saveServer.SaveProfileAsync(sessionId, cancellationToken);
        }

        if (failed.Count > 0)
        {
            logger.Warning($"[BetterTacticalDevicesHandling] light state not saved for {failed.Count} item(s): not a light in profile {sessionId}");
        }

        return httpResponseUtil.NoBody(new LightStateResponse { Failed = failed });
    }

    private int? ModesCount(Item item)
    {
        KeyValuePair<bool, TemplateItem?> template = itemHelper.GetItem(item.Template);
        int? count = template.Value?.Properties?.ModesCount?.Item;
        return template.Key && count > 0 ? count : null;
    }
}
