using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using ImGuiScene;
using LogogramHelper.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LogogramHelper.Windows
{
    public class LogosWindow : Window, IDisposable
    {
        private Plugin Plugin { get; }
        private LogosAction Action { get; set; }
        private IDictionary<int, int> LogogramStock { get; set; }
        private IDictionary<int, Logogram> Logograms { get; }
        private TextureWrap Texture { get; set; } = null!;
        public LogosWindow(Plugin plugin) : base(
        "Logos Details", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.Plugin = plugin;
            this.Action = plugin.LogosActions[0];
            this.LogogramStock = plugin.LogogramStock;
            this.Logograms = plugin.Logograms;
        }

        public void Dispose()
        {
        }
        private unsafe void ObtainLogograms()
        {
            var arrayData = Framework.Instance()->GetUiModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;
            for (var i = 1; i <= arrayData.NumberArrays[133]->IntArray[0]; i++)
            {
                var id = arrayData.NumberArrays[133]->IntArray[(4 * i) + 1];
                var stock = arrayData.NumberArrays[133]->IntArray[4 * i];
                if (!LogogramStock.ContainsKey(id))
                {
                    LogogramStock.Add(id, stock);
                    continue;
                }
                if (LogogramStock[id] != stock)
                    LogogramStock[id] = stock;
            }
        }
        public void SetDetails(LogosAction action, TextureWrap texture) {
            this.Action = action;
            this.Texture = texture;
        }
        public override void Draw()
        {
            var addonShardListPtr = this.Plugin.GameGui.GetAddonByName("EurekaMagiciteItemShardList", 1);
            if (addonShardListPtr != IntPtr.Zero)
            {
                ObtainLogograms();
            }
            if (Texture == null)
                return;
            ImGui.PushTextWrapPos(540.0f);
            ImGui.BeginGroup();
            ImGui.Image(Texture.ImGuiHandle, new Vector2(40, 40), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            ImGui.SameLine();
            ImGui.BeginGroup();
            ImGui.Text(Action.Name);
            var details = Action.Type.ToUpper();
            if (Action.Duration != null)
                details += $" · DURATION: {Action.Duration}";
            if (Action.Cast != null)
                details += $" · CAST: {Action.Cast}";
            if (Action.Recast != null)
                details += $" · RECAST: {Action.Recast}";
            ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), details);
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Spacing();
            ImGui.Text($"{Action.Description}");
            ImGui.Spacing();
            ImGui.Text("Combinations:");
            ImGui.BeginChild($"combinations{Action.Name}", new Vector2(540.0f, (ImGui.GetFontSize() + 4) * Action.Recipes.Count), false, ImGuiWindowFlags.NoScrollbar);
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 40f);
            ImGui.SetColumnWidth(1, 500f);
            Action.Recipes.ForEach(recipe => {
                var total = new List<int>();
                var logosNames = new List<string>();
                recipe.ForEach(item => {
                    if (!LogogramStock.ContainsKey(item.LogogramID))
                        LogogramStock.Add(item.LogogramID, 0);
                    total.Add(LogogramStock[item.LogogramID] / item.Quantity);
                    for (var j = 0; j < item.Quantity; j++) logosNames.Add(Logograms[item.LogogramID].Name);
                });
                if (total.Min() > 0)
                    ImGui.Text($"{total.Min()}");
                else
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), $"{total.Min()}");
                ImGui.NextColumn();
                ImGui.Text(string.Join(" + ", logosNames));
                ImGui.NextColumn();
            });
            ImGui.EndChild();
            ImGui.PopTextWrapPos();
        }
    }
}
