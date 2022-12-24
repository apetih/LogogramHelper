using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LogogramHelper
{
    public class LogogramHook : IDisposable
    {
        private readonly Plugin Plugin;

        private unsafe delegate void* AddonOnUpdate(AtkUnitBase* atkUnitBase, NumberArrayData* nums, StringArrayData* strings);
        [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 42 20", DetourName = nameof(ItemDetailOnUpdateDetour))]
        private Hook<AddonOnUpdate>? ItemDetailOnUpdateHook { get; init; }
        private readonly IntPtr mem = Marshal.AllocHGlobal(4096);

        public LogogramHook(Plugin plugin)
        {
            this.Plugin = plugin;
            SignatureHelper.Initialise(this);
            ItemDetailOnUpdateHook?.Enable();
        }

        private static unsafe SeString? GetTooltipString(StringArrayData* stringArrayData, int field)
        {
            var stringAddress = new IntPtr(stringArrayData->StringArray[field]);
            return stringAddress != IntPtr.Zero ? MemoryHelper.ReadSeStringNullTerminated(stringAddress) : null;
        }

        private unsafe void SetTooltipString(StringArrayData* stringArrayData, int field, SeString seString)
        {
            var bytes = seString.Encode();
            Marshal.Copy(bytes, 0, mem, bytes.Length);
            Marshal.WriteByte(mem, bytes.Length, 0);
            stringArrayData->StringArray[field] = (byte*)mem;
        }

        private unsafe void* ItemDetailOnUpdateDetour(AtkUnitBase* atkUnitBase, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
        {
            var id = Plugin.GameGui.HoveredItem;
            if(Plugin.LogogramItems.ContainsKey(id))
            {
                var contentsId = Plugin.LogogramItems[id].Contents;
                var contents = new List<string>();
                contentsId.ForEach(content => {
                    contents.Add(Plugin.Logograms[content].Name);
                });
                var seStr = GetTooltipString(stringArrayData, 13);
                if (seStr != null)
                {
                    var insert = $"\n\nPotential mnemes contained: {string.Join(", ", contents.ToArray())}";
                    if (!seStr.TextValue.Contains(insert)) seStr.Payloads.Insert(1, new TextPayload(insert));

                    SetTooltipString(stringArrayData, 13, seStr);
                }
            }
            return ItemDetailOnUpdateHook!.Original(atkUnitBase, numberArrayData, stringArrayData);
        }
        public void Dispose()
        {
            ItemDetailOnUpdateHook?.Dispose();
            Marshal.FreeHGlobal(mem);
        }
    }

}
