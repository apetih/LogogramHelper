using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using LogogramHelper.Classes;

namespace LogogramHelper.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin { get; }
    private List<LogosAction> LogosActions { get; }

    private readonly ConcurrentDictionary<uint, TextureWrap> TextureStorage = new();

    public MainWindow(Plugin plugin) : base(
        "Logos Actions", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(528, 322),
            MaximumSize = new Vector2(528, 322)
        };
        this.Plugin = plugin;
        this.LogosActions = plugin.LogosActions;
        this.ShowCloseButton = false;
        LoadIcon(60861);
    }

    public void Dispose(){
        TextureStorage.Values.ToList().ForEach(v => v?.Dispose());
        TextureStorage.Clear();
    }
    private TextureWrap GetTex(uint id)
    {
        if (TextureStorage.TryGetValue(id, out var tex) && tex?.ImGuiHandle != IntPtr.Zero)
            return tex;

        LoadIcon(id);
        tex = TextureStorage[60861]; 

        if (tex?.ImGuiHandle == IntPtr.Zero)
            throw new NullReferenceException("Texture failed");

        return tex;
    }

    private async void LoadIcon(uint iconID)
    {
        if (iconID <= 0)
            return;

        var iconTex = await Task.Run(() => Plugin.DataManager.GetImGuiTextureIcon(iconID));
        TextureStorage[iconID] = iconTex;
    }



    public unsafe override void Draw()
    {
        for (var i = 0; i < 56; i++)
        {
            var action = LogosActions[i];
            var padding = 2;
            var bg = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            var tint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            if (ImGui.ImageButton(GetTex(action.IconID).ImGuiHandle, new Vector2(40, 40), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), padding, bg, tint))
                Plugin.DrawLogosDetailUI(action, GetTex(action.IconID));
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{action.Name}");
            if ((i+1) % 10 != 0) ImGui.SameLine();
        }
        
    }
}
