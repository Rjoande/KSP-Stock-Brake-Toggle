// ToggleStockBrake — binds a configurable key combo to toggling the stock
// "Brakes" action group, i.e. the exact same effect as clicking the brake
// icon in the flight UI (status light included). Stock KSP's "B" key only
// holds the brakes while pressed; there is no built-in way to latch/toggle
// them from the keyboard. This plugin adds nothing but that missing toggle:
// it simply calls ActionGroups.ToggleGroup(KSPActionGroup.Brakes).
//
// IMPORTANT: the configured main key must NOT be whatever key is currently
// bound to the stock "Brakes" action (Settings > Input > BRAKES, default:
// B — see GameSettings.BRAKES). FlightInputHandler.Update() unconditionally
// forces the Brakes group to true on that physical key's KeyDown and to
// false on its KeyUp, regardless of any modifier held. So a combo built on
// that key (e.g. "Alt+B") always gets forced back off the instant the key
// is released, making it impossible to re-engage the brakes with the
// toggle. Default here is Alt+Y, since Y has no stock keybinding at all.
//
// Config: GameData/ToggleStockBrake/PluginData/ToggleStockBrake.cfg

using System;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyTitle("ToggleStockBrake")]
[assembly: AssemblyDescription("Configurable keyboard toggle for the stock Brakes action group")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: KSPAssembly("ToggleStockBrake", 1, 0)]

namespace ToggleStockBrake
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ToggleStockBrakeAddon : MonoBehaviour
    {
        public const string Version = "1.0.0";

        private static bool enabled_ = true;
        private static bool requireAlt;
        private static bool requireCtrl;
        private static bool requireShift;
        private static KeyCode mainKey = KeyCode.Y;
        private static string keySpec = "Alt+Y";

        private string configPath;

        public void Awake()
        {
            configPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Path.Combine("PluginData", "ToggleStockBrake.cfg"));
            LoadConfig();
        }

        public void Update()
        {
            if (!enabled_)
                return;

            if (!HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null)
                return;

            // Skip while a text field has keyboard focus (e.g. renaming a
            // vessel/maneuver) — the same lock the stock game itself uses.
            if (InputLockManager.IsLocked(ControlTypes.KEYBOARDINPUT))
                return;

            if (!Input.GetKeyDown(mainKey))
                return;

            if (requireAlt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                return;
            if (requireCtrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                return;
            if (requireShift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return;

            FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
        }

        private void LoadConfig()
        {
            enabled_ = true;
            keySpec = "Alt+Y";
            ApplyKeySpec(keySpec);

            ConfigNode root = ConfigNode.Load(configPath);
            ConfigNode node = root == null ? null : root.GetNode("TOGGLESTOCKBRAKE");
            if (node == null)
            {
                Debug.LogWarning("[ToggleStockBrake] config not found or missing TOGGLESTOCKBRAKE node ("
                    + configPath + "), using defaults (" + keySpec + ")");
                return;
            }

            bool b;
            if (bool.TryParse(node.GetValue("enabled"), out b)) enabled_ = b;

            string k = node.GetValue("key");
            if (!string.IsNullOrEmpty(k) && !ApplyKeySpec(k))
            {
                Debug.LogWarning("[ToggleStockBrake] invalid 'key' value: '" + k
                    + "', reverting to default Alt+Y");
                ApplyKeySpec("Alt+Y");
            }

            KeyCode brakesPrimary = GameSettings.BRAKES.primary.code;
            KeyCode brakesSecondary = GameSettings.BRAKES.secondary.code;
            if (mainKey == brakesPrimary || (brakesSecondary != KeyCode.None && mainKey == brakesSecondary))
            {
                Debug.LogWarning("[ToggleStockBrake] configured main key ('" + keySpec
                    + "') matches your current stock Brakes keybinding (Settings > Input > BRAKES): "
                    + "the game unconditionally forces the brake state on that physical key's "
                    + "release, regardless of modifiers, so the toggle will never be able to "
                    + "re-engage them. Pick a different main key.");
            }

            Debug.Log("[ToggleStockBrake] v" + Version + ": enabled=" + enabled_ + " key=" + keySpec);
        }

        // Expected format: '+'-separated tokens, the last one is the main key
        // (a UnityEngine.KeyCode name, e.g. "Y", "F5", "Mouse2", "Backspace"),
        // any preceding tokens are modifiers among Alt/Ctrl(or Control)/Shift.
        // Examples: "Y", "Alt+Y", "Ctrl+Shift+Y". Returns false (leaving the
        // current state untouched) if the format is invalid.
        private static bool ApplyKeySpec(string spec)
        {
            if (string.IsNullOrEmpty(spec))
                return false;

            string[] tokens = spec.Split('+');
            string keyToken = tokens[tokens.Length - 1].Trim();

            KeyCode parsedKey;
            try
            {
                parsedKey = (KeyCode)Enum.Parse(typeof(KeyCode), keyToken, true);
            }
            catch (ArgumentException)
            {
                return false;
            }

            bool alt = false, ctrl = false, shift = false;
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                string t = tokens[i].Trim();
                if (t.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    alt = true;
                else if (t.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)
                    || t.Equals("Control", StringComparison.OrdinalIgnoreCase))
                    ctrl = true;
                else if (t.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    shift = true;
                else
                    return false;
            }

            mainKey = parsedKey;
            requireAlt = alt;
            requireCtrl = ctrl;
            requireShift = shift;
            keySpec = spec.Trim();
            return true;
        }
    }
}
