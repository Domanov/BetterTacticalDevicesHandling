using System;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using BetterTacticalDevicesHandling.Client.Devices;
using static BetterTacticalDevicesHandling.Client.Wheel.Constants.WheelTile;

namespace BetterTacticalDevicesHandling.Client.Wheel;

internal sealed class WheelTile
{
    private static readonly Vector2 Size = new Vector2(170f, 110f);
    private static readonly Color Background = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color BackgroundSelected = new Color(0.3f, 0.3f, 0.28f, 0.85f);

    public readonly DeviceAction? Content;

    private readonly Image _background;
    private readonly TextMeshProUGUI _title;
    private readonly TextMeshProUGUI _footer;
    private readonly Action _unbindIcon;
    private bool _selected;

    public WheelTile(RectTransform parent, Vector2 position, DeviceAction? action, TMP_FontAsset font)
    {
        Content = action;
        _background = CreateImage(parent, position, Size, Background);
        if (action == null)
        {
            CreateCross(_background.rectTransform);
            return;
        }

        _title = CreateText(_background.rectTransform, font, 17f, 40f, true, new Vector2(0f, 1f), new Vector2(10f, -8f));
        _title.fontStyle = FontStyles.Bold;
        _footer = CreateText(_background.rectTransform, font, 11f, 14f, false, new Vector2(0f, 0f), new Vector2(10f, 8f));
        _footer.color = UiPalette.TextOff;
        Image icon = CreateImage(_background.rectTransform, new Vector2(0f, -10f), new Vector2(120f, 36f), Color.white);
        icon.preserveAspect = true;
        ItemIcon itemIcon = ItemViewFactory.LoadItemIcon(action.Value.Device.Light.Item, 1, false);
        _unbindIcon = itemIcon.Changed.Bind(() => icon.sprite = itemIcon.Sprite);
        Refresh();
    }

    public bool IsEmpty => Content == null;

    // The wheel canvas is screen-space overlay, so no camera.
    public bool Contains(Vector2 screenPoint) =>
        RectTransformUtility.RectangleContainsScreenPoint(_background.rectTransform, screenPoint, null);

    public void SetSelected(bool selected)
    {
        _selected = selected;
        UpdateBackground();
    }

    public void Refresh()
    {
        if (Content is DeviceAction action)
        {
            _title.text = action.Title;
            _title.color = action.IsOn ? UiPalette.TextActive : action.IsLit ? UiPalette.TextOn : UiPalette.TextOff;
            _footer.text = action.Footer;
            UpdateBackground();
        }
    }

    private void UpdateBackground()
    {
        bool on = Content is DeviceAction action && action.IsOn;
        _background.color = on
            ? _selected ? UiPalette.ActiveBackgroundSelected : UiPalette.ActiveBackground
            : _selected ? BackgroundSelected : Background;
    }

    public void Release()
    {
        _unbindIcon?.Invoke();
    }

    private static Image CreateImage(RectTransform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(ObjectImage, typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void CreateCross(RectTransform parent)
    {
        foreach (float angle in new[] { 45f, -45f })
        {
            Image line = CreateImage(parent, Vector2.zero, new Vector2(44f, 1.5f), UiPalette.TextOff);
            line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, TMP_FontAsset font, float fontSize, float height, bool wrap, Vector2 corner, Vector2 offset)
    {
        GameObject go = new GameObject(ObjectText, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = corner;
        rect.anchorMax = corner;
        rect.pivot = corner;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(Size.x - 20f, height);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            text.font = font;
        }

        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = 7f;
        text.enableWordWrapping = wrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = corner.y > 0.5f ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.BottomLeft;
        text.raycastTarget = false;
        return text;
    }
}
