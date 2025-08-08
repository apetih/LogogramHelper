using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using LogogramHelper.Windows;
using System.Collections.Generic;
using Newtonsoft.Json;
using LogogramHelper.Classes;
using System.Linq;
using System;
using Dalamud.Plugin.Services;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace LogogramHelper
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Logogram Helper";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

        public WindowSystem WindowSystem = new("LogogramHelper");
        public MainWindow MainWindow { get; init; }
        public LogosWindow LogosWindow { get; init; }

        internal List<LogosAction> LogosActions;
        internal IDictionary<int, Logogram> Logograms;
        internal IDictionary<ulong, LogogramItem> LogogramItems;
        internal IDictionary<int, int> LogogramStock = new Dictionary<int, int>();

        public Plugin()
        {

            LoadData();

            MainWindow = new MainWindow(this);
            LogosWindow = new LogosWindow(this);

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(LogosWindow);

            PluginInterface.UiBuilder.Draw += DrawUI;

            AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", ItemDetailOnUpdate);
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
                MainWindow.IsOpen = true;
            else
            {
                if (MainWindow.IsOpen) MainWindow.IsOpen = false;
                if (LogosWindow.IsOpen) LogosWindow.IsOpen = false;
            }

        }

        private void LoadData()
        {

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

        private unsafe void ItemDetailOnUpdate(AddonEvent type, AddonArgs args)
        {
            var id = GameGui.HoveredItem;
            if (LogogramItems.ContainsKey(id))
            {
                var contentsId = LogogramItems[id].Contents;
                var contents = new List<string>();
                contentsId.ForEach(content =>
                {
                    contents.Add(Logograms[content].Name);
                });

                var arrayData = Framework.Instance()->GetUIModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;
                var stringArrayData = arrayData.StringArrays[26];
                var seStr = GetTooltipString(stringArrayData, 13);
                if (seStr == null) return;

                var insert = $"\n\nPotential logograms contained: {string.Join(", ", contents.ToArray())}";
                if (!seStr.TextValue.Contains(insert)) seStr.Payloads.Insert(1, new TextPayload(insert));

                stringArrayData->SetValue(13, seStr.Encode(), false, true, true);
            }
        }

        private static unsafe SeString? GetTooltipString(StringArrayData* stringArrayData, int field)
        {
            var stringAddress = new IntPtr(stringArrayData->StringArray[field]);
            return stringAddress != IntPtr.Zero ? MemoryHelper.ReadSeStringNullTerminated(stringAddress) : null;
        }
    }
}
