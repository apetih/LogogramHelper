using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using LogogramHelper.Windows;
using Dalamud.Game.Gui;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Data;
using LogogramHelper.Classes;
using System.Linq;
using System;
using ImGuiScene;

namespace LogogramHelper
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Logogram Helper";

        [PluginService]
        internal GameGui GameGui { get; init; }
        [PluginService]
        internal static DataManager DataManager { get; private set; } = null!;
        internal DalamudPluginInterface PluginInterface { get; init; }
        public WindowSystem WindowSystem = new("LogogramHelper");
        internal List<LogosAction> LogosActions;
        internal IDictionary<int, Logogram> Logograms;
        internal IDictionary<int, int> LogogramStock = new Dictionary<int, int>();

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;


            LoadData();

            WindowSystem.AddWindow(new MainWindow(this));
            WindowSystem.AddWindow(new LogosWindow(this));

            this.PluginInterface.UiBuilder.Draw += DrawUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
            var addonPtr = GameGui.GetAddonByName("EurekaMagiciteItemSynthesis", 1);
            if (addonPtr != IntPtr.Zero)
                WindowSystem.GetWindow("Logos Actions").IsOpen = true;
            else {
                if (WindowSystem.GetWindow("Logos Actions").IsOpen)
                    WindowSystem.GetWindow("Logos Actions").IsOpen = false;
                if (WindowSystem.GetWindow("Logos Details").IsOpen)
                    WindowSystem.GetWindow("Logos Details").IsOpen = false;
            }
            
        }

        private void LoadData()
        {
            using var logogramReader = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logograms.json"));
            var logogramJson = logogramReader.ReadToEnd();
            var Logos = JsonConvert.DeserializeObject<List<Logogram>>(logogramJson);
            Logograms = Logos.ToDictionary(keySelector: l => l.Id, elementSelector: l => l);
            logogramReader.Close();

            using var r = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logosActions.json"));
            var logosJson = r.ReadToEnd();
            LogosActions = JsonConvert.DeserializeObject<List<LogosAction>>(logosJson);
            r.Close();

        }

        public void DrawLogosDetailUI(LogosAction action, TextureWrap texture)
        {
            LogosWindow lWindow = (LogosWindow)WindowSystem.GetWindow("Logos Details");
            lWindow.SetDetails(action, texture);
            WindowSystem.GetWindow("Logos Details").IsOpen = true;
        }
    }
}
