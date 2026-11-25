using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InputSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using BetterTacticalDevicesHandling.Client.Devices;

namespace BetterTacticalDevicesHandling.Client.Wheel;

// Modelled on the vanilla gesture menu: a child input node of GamePlayerOwner is translated before the owner,
// so blocking commands, eating axes and showing the cursor here keeps the player from shooting or looking around.
internal sealed class DeviceWheel : InputNode
{
    private const int MinSlots = 6;
    private const float RadiusX = 250f;
    private const float RadiusY = 220f;
    private const float DeadZone = 60f;

    private static DeviceWheel _open;

    private FirearmHandsInputTranslator _translator;
    private GamePlayerOwner _owner;
    private Canvas _canvas;
    private readonly List<WheelTile> _tiles = new List<WheelTile>();
    private WheelTile _selected;
    private KeyCode _closingKey = KeyCode.None;
    private int _releasedFrame = -1;

    public static void Toggle()
    {
        if (_open != null)
        {
            _open.CloseOnRelease(Plugin.WheelKey.Value.MainKey);
            return;
        }

        // A locked cursor means no screen (inventory, map, pause menu) is open over the raid.
        if (Cursor.lockState != CursorLockMode.Locked || !Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        Player player = Singleton<GameWorld>.Instance.MainPlayer;
        GamePlayerOwner owner = player != null ? player.GetComponent<GamePlayerOwner>() : null;
        if (owner == null || !(owner.HandsInputTranslator is FirearmHandsInputTranslator translator) || !player.HealthController.IsAlive)
        {
            return;
        }

        List<TacticalDevice> devices = TacticalDevice.Collect(translator._controller.Item, player.HandsController.ControllerGameObject.transform);
        if (devices.Count == 0)
        {
            return;
        }

        GameObject go = new GameObject("Dmnv-BetterTacticalDevicesHandling Wheel", typeof(RectTransform));
        _open = go.AddComponent<DeviceWheel>();
        _open.Build(translator, owner, devices);
    }

    public override ETranslateResult TranslateCommand(ECommand command)
    {
        if (command == ECommand.Escape)
        {
            CloseOnRelease(KeyCode.Escape);
        }

        return GetDefaultBlockResult(command);
    }

    public override void TranslateAxes(ref float[] axes)
    {
        axes = null;
    }

    public override ECursorResult ShouldLockCursor()
    {
        return _closingKey == KeyCode.None ? ECursorResult.ShowCursor : ECursorResult.Ignore;
    }

    private void Build(FirearmHandsInputTranslator translator, GamePlayerOwner owner, List<TacticalDevice> devices)
    {
        _translator = translator;
        _owner = owner;

        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 30000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;

        List<DeviceAction> actions = new List<DeviceAction>();
        foreach (TacticalDevice device in devices)
        {
            DeviceAction.AddFor(device, actions);
        }

        int slots = Mathf.Max(MinSlots, actions.Count + actions.Count % 2);
        RectTransform root = (RectTransform)transform;
        TMP_FontAsset font = UiFont.Get();
        for (int i = 0; i < slots; i++)
        {
            float angle = SlotAngle(i, slots) * Mathf.Deg2Rad;
            Vector2 position = new Vector2(Mathf.Cos(angle) * RadiusX, Mathf.Sin(angle) * RadiusY);
            _tiles.Add(new WheelTile(root, position, i < actions.Count ? actions[i] : (DeviceAction?)null, font));
        }

        _owner.Add(this);
    }

    private void Update()
    {
        if (_open != this)
        {
            return;
        }

        Player player = _translator._player;
        if (player == null || !ReferenceEquals(player.HandsController, _translator._controller) || !player.HealthController.IsAlive)
        {
            Close();
            return;
        }

        if (_closingKey != KeyCode.None)
        {
            FinishClosing();
            return;
        }

        KeyCode closeKey = Input.GetKeyDown(KeyCode.Escape) ? KeyCode.Escape
            : Input.GetKeyDown(KeyCode.Mouse1) ? KeyCode.Mouse1
            : KeyCode.None;
        if (closeKey != KeyCode.None)
        {
            CloseOnRelease(closeKey);
            return;
        }

        Select(PointedTile());
        if (_selected != null && Input.GetMouseButtonDown(0))
        {
            Apply(_selected);
        }
    }

    private WheelTile PointedTile()
    {
        Vector2 offset = ((Vector2)Input.mousePosition - new Vector2(Screen.width, Screen.height) * 0.5f) / _canvas.scaleFactor;
        if (offset.magnitude < DeadZone)
        {
            return null;
        }

        float angle = Mathf.Atan2(offset.y / RadiusY, offset.x / RadiusX) * Mathf.Rad2Deg;
        int nearest = 0;
        float nearestDelta = float.MaxValue;
        for (int i = 0; i < _tiles.Count; i++)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(angle, SlotAngle(i, _tiles.Count)));
            if (delta < nearestDelta)
            {
                nearest = i;
                nearestDelta = delta;
            }
        }

        WheelTile tile = _tiles[nearest];
        return tile.IsEmpty ? null : tile;
    }

    private void Select(WheelTile tile)
    {
        if (tile == _selected)
        {
            return;
        }

        _selected?.SetSelected(false);
        _selected = tile;
        _selected?.SetSelected(true);
    }

    private void Apply(WheelTile tile)
    {
        if (!(tile?.Content is DeviceAction action) || !action.TryGetTarget(out LightsState target))
        {
            return;
        }

        LightsState[] states = _translator.GetCurrentLightStates();
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].Id == target.Id)
            {
                states[i] = target;
            }
        }

        _translator._controller.SetLightsState(states, false, true);
        foreach (WheelTile t in _tiles)
        {
            t.Refresh();
        }
    }

    // The game reads the same key press/release; staying in the input tree (hidden) until a frame after the release keeps
    // it from aiming (right mouse), opening the pause menu (Esc) or recentring the free look (middle mouse) as the wheel closes.
    private void CloseOnRelease(KeyCode key)
    {
        if (_closingKey != KeyCode.None)
        {
            return;
        }

        _closingKey = key;
        _canvas.enabled = false;
        Select(null);
    }

    private void FinishClosing()
    {
        if (Input.GetKey(_closingKey))
        {
            return;
        }

        if (_releasedFrame < 0)
        {
            _releasedFrame = Time.frameCount;
        }
        else if (Time.frameCount > _releasedFrame)
        {
            Close();
        }
    }

    private void Close()
    {
        if (_open != this)
        {
            return;
        }

        _open = null;
        _owner.Remove(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        foreach (WheelTile tile in _tiles)
        {
            tile.Release();
        }

        if (_open == this)
        {
            _open = null;
            _owner.Remove(this);
        }
    }

    // Slot 0 at the top, then clockwise.
    private static float SlotAngle(int index, int count) => 90f - index * 360f / count;
}
