using System;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using BetterTacticalDevicesHandling.Client.Devices;

namespace BetterTacticalDevicesHandling.Client.Wheel;

internal sealed class WheelTile
{
    private const string ObjectImage = "Image";
    private const string ObjectText = "Text";

    private static readonly Vector2 Size = new Vector2(170f, 110f);
    private static readonly Color Background = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color BackgroundSelected = new Color(0.3f, 0.3f, 0.28f, 0.85f);
    private static readonly Color TextOn = new Color(0.92f, 0.92f, 0.88f, 1f);
    private static readonly Color TextOff = new Color(0.55f, 0.55f, 0.52f, 1f);

    public readonly DeviceAction? Content;

    private readonly Image _background;
    private readonly TextMeshProUGUI _title;
    private readonly TextMeshProUGUI _footer;
    private readonly Action _unbindIcon;

    public WheelTile(RectTransform parent, Vector2 position, DeviceAction? action, TMP_FontAsset font)
    {
        Content = action;
        _background = CreateImage(parent, position, Size, Background);
        if (action == null)
        {
            CreateCross(_background.rectTransform);
            return;
        }

        _title = CreateText(_background.rectTransform, font, 13f, 30f, true, new Vector2(0f, 1f), new Vector2(10f, -8f));
        _footer = CreateText(_background.rectTransform, font, 11f, 14f, false, new Vector2(0f, 0f), new Vector2(10f, 8f));
        _footer.color = TextOff;
        Image icon = CreateImage(_background.rectTransform, new Vector2(0f, -2f), new Vector2(120f, 44f), Color.white);
        icon.preserveAspect = true;
        ItemIcon itemIcon = ItemViewFactory.LoadItemIcon(action.Value.Device.Light.Item, 1, false);
        _unbindIcon = itemIcon.Changed.Bind(() => icon.sprite = itemIcon.Sprite);
        Refresh();
    }

    public bool IsEmpty => Content == null;

    public void SetSelected(bool selected)
    {
        _background.color = selected ? BackgroundSelected : Background;
    }

    public void Refresh()
    {
        if (Content is DeviceAction action)
        {
            _title.text = action.Title;
            _title.color = action.IsLit ? TextOn : TextOff;
            _footer.text = action.Footer;
        }
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
            Image line = CreateImage(parent, Vector2.zero, new Vector2(44f, 1.5f), TextOff);
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
