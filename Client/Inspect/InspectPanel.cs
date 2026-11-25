using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.WeaponModding;
using HarmonyLib;
using BetterTacticalDevicesHandling.Client.Devices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterTacticalDevicesHandling.Client.Inspect;

// Device chips in the top-left corner of the inspect window's 3D preview. One panel per preview panel,
// rebuilt whenever the preview model is (re)built, e.g. after a mod is attached in the window.
internal sealed class InspectPanel : MonoBehaviour
{
    private const string FieldCachedLightMasks = "_cachedLightMasks";
    private const string FieldPositionedObject = "_positionedObject";
    private const string ObjectPanel = Plugin.Name + " Panel";
    private const string ObjectRow = "Row";
    private const string ObjectChip = "Chip";
    private const string ObjectText = "Text";
    private const string ChipAllOff = "ALL OFF";
    private const string LabelLockedHint = "Take the weapon in hands to switch";

    private static readonly Color PanelBackground = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color ChipOff = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    private static readonly Color ChipOn = new Color(0.36f, 0.4f, 0.3f, 0.95f);
    private static readonly Color TextOn = new Color(0.92f, 0.92f, 0.88f, 1f);
    private static readonly Color TextOff = new Color(0.55f, 0.55f, 0.52f, 1f);
    private static readonly Color TextLocked = new Color(0.4f, 0.4f, 0.38f, 1f);

    private static readonly Dictionary<WeaponPreview, InspectPanel> Panels = new Dictionary<WeaponPreview, InspectPanel>();

    private static readonly AccessTools.FieldRef<WeaponPreview, List<(Light, int)>> CachedLightMasks =
        AccessTools.FieldRefAccess<WeaponPreview, List<(Light, int)>>(FieldCachedLightMasks);

    // The model root; it is parented under Rotator only after SetPreviewMaskToChildrenLights, where the panel rebuilds.
    private static readonly AccessTools.FieldRef<WeaponPreview, Transform> PositionedObject =
        AccessTools.FieldRefAccess<WeaponPreview, Transform>(FieldPositionedObject);

    private readonly List<Chip> _chips = new List<Chip>();
    private List<TacticalDevice> _devices = new List<TacticalDevice>();
    private WeaponPreview _preview;
    private Weapon _weapon;
    private InspectAccess _access;

    private sealed class Chip
    {
        public DeviceAction? Action;
        public Image Background;
        public TextMeshProUGUI Label;
    }

    public static void Attach(GameObject previewPanel, WeaponPreview preview, Weapon weapon)
    {
        InspectPanel panel = previewPanel.GetComponentInChildren<InspectPanel>(true);
        if (panel == null)
        {
            panel = Create(previewPanel.transform);
        }

        panel._preview = preview;
        panel._weapon = weapon;
        panel.Clear();
        Panels[preview] = panel;
    }

    public static void Detach(WeaponPreview preview)
    {
        if (preview != null && Panels.TryGetValue(preview, out InspectPanel panel))
        {
            Panels.Remove(preview);
            if (panel != null)
            {
                panel.Clear();
            }
        }
    }

    public static void OnModelBuilt(WeaponPreview preview)
    {
        if (Panels.TryGetValue(preview, out InspectPanel panel) && panel != null)
        {
            panel.Rebuild();
        }
    }

    // F12 toggle while inspect windows are open.
    public static void RefreshAll()
    {
        foreach (InspectPanel panel in Panels.Values)
        {
            if (panel != null)
            {
                panel.Rebuild();
            }
        }
    }

    private static InspectPanel Create(Transform parent)
    {
        GameObject go = new GameObject(ObjectPanel, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -10f);
        Image background = go.GetComponent<Image>();
        background.color = PanelBackground;
        background.raycastTarget = false;
        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.spacing = 3f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go.AddComponent<InspectPanel>();
    }

    private void Rebuild()
    {
        Clear();
        _access = InspectAccessRules.For(_weapon);
        if (_access == InspectAccess.Hidden || !Plugin.InspectPanelEnabled.Value)
        {
            return;
        }

        Transform model = PositionedObject(_preview);
        if (model == null)
        {
            return;
        }

        _devices = TacticalDevice.Collect(_weapon, model);
        if (_devices.Count == 0)
        {
            return;
        }

        List<DeviceAction> actions = new List<DeviceAction>();
        foreach (TacticalDevice device in _devices)
        {
            RectTransform row = Row();
            Text(row, device.Name, 12f, TextOn).gameObject.AddComponent<LayoutElement>().minWidth = 90f;
            actions.Clear();
            DeviceAction.AddFor(device, actions);
            foreach (DeviceAction action in actions)
            {
                AddChip(row, action);
            }
        }

        if (_devices.Count > 1)
        {
            AddChip(Row(), null);
        }

        if (_access == InspectAccess.Locked)
        {
            Text(Row(), LabelLockedHint, 11f, TextOff);
        }

        gameObject.SetActive(true);
        RefreshChips();
    }

    private void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _chips.Clear();
        _devices = new List<TacticalDevice>();
        gameObject.SetActive(false);
    }

    private void RefreshChips()
    {
        foreach (Chip chip in _chips)
        {
            bool lit = chip.Action?.IsLit ?? false;
            chip.Label.text = chip.Action?.ChipTitle ?? ChipAllOff;
            chip.Label.color = _access == InspectAccess.Locked ? TextLocked : lit ? TextOn : TextOff;
            chip.Background.color = lit ? ChipOn : ChipOff;
        }
    }

    private void OnChip(Chip chip)
    {
        // The window can stay open while the weapon is drawn or put away.
        if (InspectAccessRules.For(_weapon) != _access)
        {
            Rebuild();
            return;
        }

        if (_access == InspectAccess.Locked)
        {
            return;
        }

        List<LightsState> targets = new List<LightsState>();
        if (chip.Action is DeviceAction action)
        {
            if (action.TryGetTarget(out LightsState target))
            {
                targets.Add(target);
            }
        }
        else
        {
            targets.AddRange(_devices.Where(d => d.Light.IsActive).Select(d => d.TurnedOff()));
        }

        if (targets.Count == 0)
        {
            return;
        }

        if (_access == InspectAccess.InHands)
        {
            ApplyInHands(targets);
        }
        else
        {
            ApplyInStash(targets);
        }

        RefreshModel(targets);
        RefreshChips();
    }

    // Same call the tactical key makes, so Fika sends it to the other players.
    private void ApplyInHands(List<LightsState> targets)
    {
        if (!(Singleton<GameWorld>.Instance.MainPlayer.HandsController is Player.FirearmController firearm))
        {
            return;
        }

        LightsState[] states = _devices.Select(d => d.Current()).ToArray();
        for (int i = 0; i < states.Length; i++)
        {
            foreach (LightsState target in targets)
            {
                if (states[i].Id == target.Id)
                {
                    states[i] = target;
                }
            }
        }

        firearm.SetLightsState(states, false, true);
    }

    private async void ApplyInStash(List<LightsState> targets)
    {
        List<(LightComponent light, LightsState previous)> undo = new List<(LightComponent, LightsState)>();
        foreach (LightsState target in targets)
        {
            TacticalDevice device = _devices.First(d => d.Light.Item.Id == target.Id);
            undo.Add((device.Light, device.Current()));
            device.Light.SetLightState(target);
        }

        HashSet<string> failed = await LightStateStore.Save(targets);
        if (failed.Count == 0)
        {
            return;
        }

        List<LightsState> reverted = new List<LightsState>();
        foreach ((LightComponent light, LightsState previous) in undo)
        {
            if (failed.Contains(previous.Id))
            {
                light.SetLightState(previous);
                reverted.Add(previous);
            }
        }

        Plugin.Log.LogWarning($"Inspect: {reverted.Count} light state change(s) not saved by the server, reverted");
        if (this != null && gameObject.activeInHierarchy)
        {
            RefreshModel(reverted);
            RefreshChips();
        }
    }

    // The preview model is separate from the hands model, so its beams are switched here. Lights the switch creates
    // (laser dots) get the preview-only culling mask, recorded like WeaponPreview.SetPreviewMaskToChildrenLights does
    // so the pooled model gets its masks back when the preview is released.
    private void RefreshModel(List<LightsState> changed)
    {
        TacticalComboVisualController[] visuals = PositionedObject(_preview).GetComponentsInChildren<TacticalComboVisualController>(true);
        List<(Light, int)> cachedMasks = CachedLightMasks(_preview);
        foreach (LightsState state in changed)
        {
            TacticalComboVisualController visual = TacticalDevice.FindVisual(visuals, state.Id);
            if (visual == null)
            {
                continue;
            }

            visual.UpdateBeams(false);
            foreach (Light light in visual.GetComponentsInChildren<Light>(true))
            {
                if (light.cullingMask != LayersMaskController.WeaponPreview)
                {
                    cachedMasks.Add((light, light.cullingMask));
                    light.cullingMask = LayersMaskController.WeaponPreview;
                }
            }
        }
    }

    private RectTransform Row()
    {
        GameObject go = new GameObject(ObjectRow, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(transform, false);
        HorizontalLayoutGroup layout = go.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        return rect;
    }

    private void AddChip(RectTransform row, DeviceAction? action)
    {
        GameObject go = new GameObject(ObjectChip, typeof(RectTransform), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup));
        ((RectTransform)go.transform).SetParent(row, false);
        HorizontalLayoutGroup layout = go.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 2, 2);
        layout.childControlWidth = layout.childControlHeight = true;
        Chip chip = new Chip
        {
            Action = action,
            Background = go.GetComponent<Image>(),
            Label = Text(go.transform, string.Empty, 11f, TextOff)
        };
        Button button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = chip.Background;
        button.onClick.AddListener(() => OnChip(chip));
        _chips.Add(chip);
    }

    private static TextMeshProUGUI Text(Transform parent, string text, float fontSize, Color color)
    {
        GameObject go = new GameObject(ObjectText, typeof(RectTransform), typeof(TextMeshProUGUI));
        ((RectTransform)go.transform).SetParent(parent, false);
        TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UiFont.Get();
        if (font != null)
        {
            label.font = font;
        }

        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        return label;
    }
}
