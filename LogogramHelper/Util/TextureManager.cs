using ImGuiScene;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogogramHelper.Util
{
    internal static class TextureManager
    {
        private static readonly ConcurrentDictionary<uint, TextureWrap> TextureStorage = new();
        public static TextureWrap GetTex(uint id)
        {
            if (TextureStorage.TryGetValue(id, out var tex) && tex?.ImGuiHandle != IntPtr.Zero)
                return tex;

            LoadIcon(id);
            tex = TextureStorage[786];

            if (tex?.ImGuiHandle == IntPtr.Zero)
                throw new NullReferenceException("Texture failed");

            return tex;
        }

        public static async void LoadIcon(uint iconID)
        {
            if (iconID <= 0)
                return;

            var iconTex = await Task.Run(() => Plugin.DataManager.GetImGuiTextureIcon(iconID));
            TextureStorage[iconID] = iconTex;
        }

        public static void Dispose() {
            TextureStorage.Values.ToList().ForEach(v => v?.Dispose());
            TextureStorage.Clear();
        }
    }
}
