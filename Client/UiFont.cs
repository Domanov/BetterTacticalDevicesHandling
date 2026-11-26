using System.Linq;
using TMPro;
using UnityEngine;
using static BetterTacticalDevicesHandling.Client.Constants.UiFont;

namespace BetterTacticalDevicesHandling.Client;

internal static class UiFont
{
    private static TMP_FontAsset _font;

    // The game's UI font (Bender); TMP has no usable default in the game build.
    public static TMP_FontAsset Get()
    {
        if (_font == null)
        {
            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            _font = fonts.FirstOrDefault(f => f.name.Contains(MatchGameFont)) ?? fonts.FirstOrDefault();
        }

        return _font;
    }
}
