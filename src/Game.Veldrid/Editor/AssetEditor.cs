using System;
using System.Globalization;
using System.Linq;
using System.Text;
using ImGuiNET;
using UAlbion.Core;

namespace UAlbion.Game.Veldrid.Editor
{
    public abstract class AssetEditor : Component
    {
        protected AssetEditor(object asset)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        }

        public abstract void Render();
        protected object Asset { get; }

        protected void IntSlider(string propertyName, int value, int min, int max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName, ref newValue, min, max);
            if (newValue != value)
            {
                var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                Raise(new EditorSetPropertyEvent(id, propertyName, value, newValue));
            }
        }

        protected void EnumCheckboxes<T>(string propertyName, T value) where T : struct, Enum
        {
            ImGui.Text(propertyName);
            ImGui.BeginGroup();
            int intValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            var powerOfTwoValues =
                Enum.GetValues(typeof(T))
                    .OfType<T>()
                    .Select(x => (Convert.ToInt32(x, CultureInfo.InvariantCulture), x))
                    .Where(x => x.Item1 != 0 && (x.Item1 & (x.Item1 - 1)) == 0)
                    .OrderBy(x => x.Item1);

            foreach (var (i, flag) in powerOfTwoValues)
            {
                bool isChecked = (intValue & i) != 0;
                ImGui.Checkbox(flag.ToString(), ref isChecked);

                if (isChecked != ((intValue & i) != 0))
                {
                    var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                    int newIntValue = isChecked ? intValue | i : intValue & ~i;
                    var newValue = (T)Enum.ToObject(typeof(T), newIntValue);
                    Raise(new EditorSetPropertyEvent(id, propertyName, value, newValue));
                }
            }
            ImGui.EndGroup();
        }

        protected void EnumRadioButtons<T>(string propertyName, T value) where T : struct, Enum
        {
            ImGui.Text(propertyName);
            ImGui.BeginGroup();
            var enumValues = Enum.GetValues(typeof(T)).OfType<T>();

            foreach (var enumValue in enumValues)
            {
                bool isSelected = Equals(value, enumValue);
                if (ImGui.RadioButton(enumValue.ToString(), isSelected))
                {
                    if (isSelected)
                        continue;

                    var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                    Raise(new EditorSetPropertyEvent(id, propertyName, value, enumValue));
                }
            }
            ImGui.EndGroup();
        }

        protected void Text(string propertyName, string value, int maxLength)
        {
            // TODO: Handle pre-existing overlength data.
            Span<byte> buffer = stackalloc byte[4 * maxLength];
            Encoding.UTF8.GetBytes(value ?? "", buffer);

            unsafe
            {
                fixed (byte* bufferPtr = &buffer[0])
                {
                    if (ImGui.InputText(propertyName, (IntPtr)bufferPtr, (uint)(4 * maxLength)))
                    {
                        var newValue = Encoding.UTF8.GetString(buffer);
                        if (newValue.Length > maxLength)
                            newValue = newValue.Substring(0, maxLength);

                        var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                        Raise(new EditorSetPropertyEvent(id, propertyName, value, newValue));
                    }
                }
            }
        }
    }
}