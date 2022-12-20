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
using Dalamud.Logging;
using System;
using ImGuiScene;

namespace LogogramHelper
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Logogram Helper";
        private const string CommandName = "/logos";


        [PluginService]
        internal GameGui GameGui { get; init; }
        [PluginService]
        internal static DataManager DataManager { get; private set; } = null!;
        internal DalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("LogogramHelper");
        internal List<LogosAction> LogosActions;
        internal IDictionary<int, Logogram> Logograms;
        internal IDictionary<int, int> LogogramStock;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

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
            //Load Logograms/Mnemes, IDK what the actual name is.
            using var logogramReader = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logograms.json"));
            var logogramJson = logogramReader.ReadToEnd();
            var Logos = JsonConvert.DeserializeObject<List<Logogram>>(logogramJson);
            Logograms = Logos.ToDictionary(keySelector: l => l.Id, elementSelector: l => l);
            logogramReader.Close();

            LogogramStock = Configuration.logogramStock;
            if (LogogramStock.Count == 0) {
                LogogramStock = Logos.ToDictionary(keySelector: l => l.Id, elementSelector: l => 0);
                Configuration.logogramStock = LogogramStock;
                Configuration.Save();
            }

            //Load Logos Actions
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
