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
using LogogramHelper.Util;
using Dalamud.Plugin.Services;

namespace LogogramHelper
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Logogram Helper";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

        public WindowSystem WindowSystem = new("LogogramHelper");
        public MainWindow MainWindow { get; init; }
        public LogosWindow LogosWindow { get; init; }

        internal LogogramHook LogogramHook { get; }
        internal List<LogosAction> LogosActions;
        internal IDictionary<int, Logogram> Logograms;
        internal IDictionary<ulong, LogogramItem> LogogramItems;
        internal IDictionary<int, int> LogogramStock = new Dictionary<int, int>();

        public Plugin()
        {
            this.LogogramHook = new LogogramHook(this);

            LoadData();

            MainWindow = new MainWindow(this);
            LogosWindow = new LogosWindow(this);

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(LogosWindow);

            PluginInterface.UiBuilder.Draw += DrawUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            TextureManager.Dispose();
            LogogramHook.Dispose();
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
            var addonPtr = GameGui.GetAddonByName("EurekaMagiciteItemSynthesis", 1);
            if (addonPtr != IntPtr.Zero)
                MainWindow.IsOpen = true;
            else
            {
                if (MainWindow.IsOpen) MainWindow.IsOpen = false;
                if (LogosWindow.IsOpen) LogosWindow.IsOpen = false;
            }

        }

        private void LoadData()
        {
            TextureManager.LoadIcon(786);

            using var logogramReader = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logograms.json"));
            var logogramJson = logogramReader.ReadToEnd();
            var Logos = JsonConvert.DeserializeObject<List<Logogram>>(logogramJson);
            Logograms = Logos.ToDictionary(keySelector: l => l.Id, elementSelector: l => l);
            logogramReader.Close();

            using var itemReader = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "itemContents.json"));
            var itemJson = itemReader.ReadToEnd();
            var items = JsonConvert.DeserializeObject<List<LogogramItem>>(itemJson);
            LogogramItems = items.ToDictionary(keySelector: i => i.Id, elementSelector: i => i);
            itemReader.Close();

            using var r = new StreamReader(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logosActions.json"));
            var logosJson = r.ReadToEnd();
            LogosActions = JsonConvert.DeserializeObject<List<LogosAction>>(logosJson);
            r.Close();

        }

        public void DrawLogosDetailUI(LogosAction action)
        {
            LogosWindow.SetDetails(action);
            LogosWindow.IsOpen = true;
        }
    }
}
