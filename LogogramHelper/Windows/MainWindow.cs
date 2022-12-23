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
using LogogramHelper.Util;

namespace LogogramHelper.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin { get; }
    private List<LogosAction> LogosActions { get; }

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
    }

    public void Dispose(){
    }


    public override void Draw()
    {
        for (var i = 0; i < 56; i++)
        {
            var action = LogosActions[i];
            var padding = 2;
            var bg = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            var tint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            if (ImGui.ImageButton(TextureManager.GetTex(action.IconID).ImGuiHandle, new Vector2(40, 40), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), padding, bg, tint)) {
                var roleTextures = new Dictionary<uint, TextureWrap>();
                action.Roles.ForEach(role => {
                    var tex = TextureManager.GetTex(role);
                    roleTextures.Add(role, tex);
                });
                Plugin.DrawLogosDetailUI(action);
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{action.Name}");
            if ((i+1) % 10 != 0) ImGui.SameLine();
        }
        
    }
}
