﻿using MicroEngineer.Utilities;
using Newtonsoft.Json;

namespace MicroEngineer.Windows;

[JsonObject(MemberSerialization.OptIn)]
public class SettingsWindow : BaseWindow
{
    [JsonProperty]
    public Theme ActiveTheme { get; set; } // Not used anymore with UITK. Might reuse later
    [JsonProperty]
    private bool snapWindows = true;
    public bool SnapWindows { get => snapWindows; set => snapWindows = value; }
    [JsonProperty]
    private float snapDistance = 20f;
    public float SnapDistance { get => snapDistance; set => snapDistance = value; }

    public void LoadSettings()
    {
        // Load any settings that will be stored in the SettingsWindow (none so far)
    }
}