using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("denyscrasav4ik.basicallyukrainian.cursorrotator", "Cursor Rotator", "1.0.2")]
public class CursorRotatorPlugin : BaseUnityPlugin
{
    internal static ConfigEntry<float> MaxDistanceConfig;

    private void Awake()
    {
        MaxDistanceConfig = Config.Bind(
            "General",
            "MaxDistance",
            100f,
            "Maximum distance at which the cursor rotates toward a button."
        );

        var harmony = new Harmony("denyscrasav4ik.basicallyukrainian.cursorrotator");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(CursorController), "Update")]
class CursorController_Update_Patch
{
    static void Postfix(CursorController __instance)
    {
        RotateCursorTowardsClosestButton(__instance);
    }

    private static void RotateCursorTowardsClosestButton(CursorController cursor)
    {
        var buttons = GameObject
            .FindObjectsOfType<StandardMenuButton>()
            .Where(b => b.gameObject.activeInHierarchy && b.enabled)
            .ToArray();

        if (buttons.Length == 0)
            return;

        float maxDistance = CursorRotatorPlugin.MaxDistanceConfig.Value;

        Vector3 cursorPos = cursor.cursorTransform.position;

        StandardMenuButton closest = null;
        float closestDist = float.MaxValue;

        foreach (var btn in buttons)
        {
            float dist = Vector3.Distance(cursorPos, btn.transform.position);

            if (dist > maxDistance)
                continue;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = btn;
            }
        }

        if (closest == null)
        {
            cursor.cursorTransform.localRotation = Quaternion.identity;
            return;
        }

        RectTransform rect = closest.GetComponent<RectTransform>();
        Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
        Vector3 direction = worldCenter - cursorPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float worldY = cursor.cursorTransform.eulerAngles.y;
        if (worldY <= 90f || worldY >= 270f)
        {
            angle -= 90f;
        }
        else
        {
            angle += 90f;
        }

        cursor.cursorTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
