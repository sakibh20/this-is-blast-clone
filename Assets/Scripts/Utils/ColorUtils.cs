using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ColorUtils
{
    // Common color property names used by shaders (try in this order)
    private static readonly int[] ColorPropIDs = new int[]
    {
        Shader.PropertyToID("_Color"),
        Shader.PropertyToID("_BaseColor"),
        Shader.PropertyToID("_TintColor"),
        Shader.PropertyToID("_MainColor"),
        Shader.PropertyToID("_AlbedoColor"),
        Shader.PropertyToID("_BaseMap") // unlikely but safe to try
    };

    /// <summary>
    /// Blend that keeps white/brightness feel while applying a theme.
    /// result = Lerp(baseColor, (white * themeColor), strength)
    /// This keeps white highlights while tinting hue by themeColor.
    /// </summary>
    private static Color BlendPreserveWhite(Color baseColor, Color themeColor, float strength)
    {
        strength = Mathf.Clamp01(strength);

        // if base is pure white shortcut (so it's fully theme color at strength 1)
        Color whiteTint = Color.white * themeColor; // tint applied to white baseline
        Color result = Color.Lerp(baseColor, whiteTint, strength);

        // keep alpha unchanged
        result.a = baseColor.a;
        return result;
    }

    // --- Image (UI) ---
    public static void ApplyThemeTint(Image img, Color themeColor, float strength = 0.6f)
    {
        if (img == null) return;

        // Ensure we manipulate instance material if you have a custom material on the Image
        // NOTE: image.color always affects UI color if the shader uses vertex color; this handles most UI default cases
        Color baseColor = img.color;
        img.color = BlendPreserveWhite(baseColor, themeColor, strength);

        // If you use a custom UI material that ignores image.color, also try material path:
        if (img.material != null && img.material != img.sprite?.associatedAlphaSplitTexture)
        {
            ApplyThemeTint(img.material, themeColor, strength);
        }
    }

    // --- SpriteRenderer ---
    public static void ApplyThemeTint(SpriteRenderer sr, Color themeColor, float strength = 0.6f)
    {
        if (sr == null) return;

        // Read base from renderer.color (fast path) — most sprite shaders multiply by renderer.color
        Color baseColor = sr.color;
        Color newColor = BlendPreserveWhite(baseColor, themeColor, strength);
        sr.color = newColor;

        // Also attempt material property if shader uses another color property (TCP2 etc.)
        if (sr.sharedMaterial != null)
        {
            // instance material so we don't change shared asset
            Material inst = sr.material; // Unity will create instance if not already done
            ApplyThemeTint(inst, themeColor, strength);
        }
    }

    // --- Generic Renderer / Material ---
    public static void ApplyThemeTint(Renderer renderer, Color themeColor, float strength = 0.6f)
    {
        if (renderer == null) return;
        if (renderer.sharedMaterial == null) return;

        // Ensure instance
        Material mat = renderer.material; // this creates an instance for this renderer
        ApplyThemeTint(mat, themeColor, strength);
    }

    public static void ApplyThemeTint(Material mat, Color themeColor, float strength = 0.6f)
    {
        if (mat == null) return;
        themeColor = ClampColor(themeColor);

        // Try to find a supported color property on this shader
        foreach (int id in ColorPropIDs)
        {
            if (mat.HasProperty(id))
            {
                Color baseColor = mat.GetColor(id);
                Color result = BlendPreserveWhite(baseColor, themeColor, strength);
                mat.SetColor(id, result);
                return;
            }
        }

        // Fallback: if no standard color property, try material.color (some shaders still rely on this)
        try
        {
            Color baseColor = mat.color;
            Color result = BlendPreserveWhite(baseColor, themeColor, strength);
            mat.color = result;
            return;
        }
        catch
        {
            // final fallback - nothing to do; useful to debug
            Debug.LogWarning($"ApplyThemeTint: shader '{mat.shader.name}' has no recognised color property. Tint skipped.");
        }
    }

    public static void ResetToOriginal(Material mat, Color originalColor)
    {
        if (mat == null) return;

        foreach (int id in ColorPropIDs)
        {
            if (mat.HasProperty(id))
            {
                mat.SetColor(id, originalColor);
                return;
            }
        }

        mat.color = originalColor;
    }

    public static void ResetToOriginal(SpriteRenderer sr, Color original)
    {
        if (sr == null) return;
        sr.color = original;
    }

    public static void ResetToOriginal(Image img, Color original)
    {
        if (img == null) return;
        img.color = original;
    }

    private static float Clamp01(float v) => Mathf.Clamp01(v);

    private static Color ClampColor(Color c)
    {
        c.r = Mathf.Clamp01(c.r);
        c.g = Mathf.Clamp01(c.g);
        c.b = Mathf.Clamp01(c.b);
        c.a = Mathf.Clamp01(c.a);
        return c;
    }
}
