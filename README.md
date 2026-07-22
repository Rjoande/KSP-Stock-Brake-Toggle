# StockBrakeToggle

A tiny KSP plugin that binds a configurable key combo to **toggling** the stock *Brakes* action group. This is the exact same effect as clicking the brake icon in the flight UI, status light included.

## The problem

In stock KSP, `B` only holds the brakes on for as long as it's pressed. There is no keyboard shortcut to *latch* them on or off (a "parking brake"
toggle), unlike SAS, RCS, Gear or Lights, which do behave as proper toggles. The lack of this has been a [known, long-standing request](https://forum.kerbalspaceprogram.com/topic/180598-a-shortcut-to-lock-brakes/) that stock KSP never implemented.

## The fix

StockBrakeToggle adds nothing but that missing toggle. On a configurable key press it calls:

```csharp
vessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
```

the same API the stock UI brake button itself calls. No custom braking logic, no physics tricks, no Harmony dependency, no other GameData
dependency at all. Since it drives the actual stock action group, the PAW, the UI status light, and any other mod/module reading `ActionGroups[Brakes]` all stay perfectly in sync.

## Installation

Drop the `StockBrakeToggle` folder into your `GameData` folder, so you end up with `GameData/StockBrakeToggle/StockBrakeToggle.dll`.

## Configuration

`PluginData/StockBrakeToggle.cfg` re-read every time you enter a flight scene (no game restart needed, just re-enter from the VAB/Space Center):

| Key | Default | Meaning |
|---|---|---|
| `enabled` | `true` | Master on/off switch |
| `key` | `Alt+Y` | Toggle combo. Optional modifiers (`Alt`, `Ctrl`/`Control`, `Shift`) separated by `+`, followed by the main key (a `UnityEngine.KeyCode` name, e.g. `Y`, `F5`, `Keypad0`, `Mouse2`) |

The plugin won't fire the combo while a text field has keyboard focus (e.g. renaming a vessel), using the same input lock (`ControlTypes.KEYBOARDINPUT`) the stock game uses for that.

### Why not `B` (or `Alt+B`)

**Don't use whatever key is currently bound to the stock *Brakes* action** (`Settings > Input > BRAKES`, `B` by default) **as the main key here**, not even with a modifier. Even with `Alt+B` (assuming `B` is your Brakes key, the default): on key-down the stock handler forces the group to `true` and the plugin's toggle flips it back to `false`; on key-up the stock handler forces it to `false` again regardless. Net effect: the combo can only ever *disengage* the brakes, never re-engage them. A key with no stock binding at all (like `Y`) sidesteps the conflict entirely, since no stock code ever touches it. The plugin checks your actual current `GameSettings.BRAKES` binding at runtime and logs a warning to `KSP.log` if the configured main key collides with it.

## Compatibility

Built and tested against KSP 1.12.5 (build `03190`). No dependency on any other mod. Should work on any 1.x version where `ActionGroups`,
`KSPActionGroup.Brakes` and `InputLockManager` have kept their current shape (essentially all of KSP's lifetime).

## Building from source

`Source/build.bat` rebuilds the DLL with the C# compiler bundled in the .NET Framework, against KSP's own managed assemblies
(`KSP_x64_Data/Managed`). No SDK, NuGet package, or Harmony required. Just point the `KSP` variable at the top of the script to your KSP install.

## License

MIT — see [LICENSE](LICENSE).
