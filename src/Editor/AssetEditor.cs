using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using ImGuiNET;
using UAlbion.Core;

namespace UAlbion.Editor
{
    public abstract class AssetEditor : Component
    {
        static int _nextId;
        readonly int _id;
        protected AssetEditor(object asset)
        {
            _id = Interlocked.Increment(ref _nextId);
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        }

        public abstract void Render();
        protected object Asset { get; }

        protected void Int8Slider(string propertyName, sbyte value, sbyte min, sbyte max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName + _id, ref newValue, min, max);
            if (newValue == value) return;
            var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            Raise(new EditorSetPropertyEvent(id, propertyName, value, (sbyte)newValue));
        }

        protected void UInt8Slider(string propertyName, byte value, byte min, byte max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName + _id, ref newValue, min, max);
            if (newValue == value) return;
            var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            Raise(new EditorSetPropertyEvent(id, propertyName, value, (byte)newValue));
        }

        protected void Int16Slider(string propertyName, short value, short min, short max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName + _id, ref newValue, min, max);
            if (newValue == value) return;
            var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            Raise(new EditorSetPropertyEvent(id, propertyName, value, (short)newValue));
        }

        protected void UInt16Slider(string propertyName, ushort value, ushort min, ushort max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName + _id, ref newValue, min, max);
            if (newValue == value) return;
            var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            Raise(new EditorSetPropertyEvent(id, propertyName, value, (ushort)newValue));
        }

        protected void Int32Slider(string propertyName, int value, int min, int max)
        {
            int newValue = value;
            ImGui.SliderInt(propertyName + _id, ref newValue, min, max);
            if (newValue == value) return;
            var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            Raise(new EditorSetPropertyEvent(id, propertyName, value, newValue));
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
                ImGui.Checkbox($"{propertyName}_{flag}_{_id}", ref isChecked);

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
                if (ImGui.RadioButton($"{propertyName}_{enumValue}_{_id}", isSelected))
                {
                    if (isSelected)
                        continue;

                    var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                    Raise(new EditorSetPropertyEvent(id, propertyName, value, enumValue));
                }
            }
            ImGui.EndGroup();
        }

        // TODO: Single checkbox taking curValue, trueValue and falseValue.
/*
        protected void EnumDropdown<T>(string propertyName, T value) where T : struct, Enum
        {
            // TODO: Will need index->enum/name lookup. Compare w/ code in ItemSlotEditor
            var enumValues = Enum.GetValues(typeof(T)).OfType<T>();

            foreach (var enumValue in enumValues)
            {
                bool isSelected = Equals(value, enumValue);
            }

            int index = Convert.ToInt32(value);
            if (ImGui.Combo(propertyName, enumValue.ToString(), isSelected))
            {
                if (isSelected)
                    continue;

                var id = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
                Raise(new EditorSetPropertyEvent(id, propertyName, value, enumValue));
            }
        }*/

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
