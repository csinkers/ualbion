using System;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Diag;

#pragma warning disable CA1812 // Class is instantiated via reflection
class SignedEnumReflector<T> : EnumReflector, IReflector where T : struct, Enum
{
    readonly Func<T, long> _toNum;
    readonly Func<long, T> _fromNum;
    readonly EnumValue[] _values;
    readonly string[] _names;

    record EnumValue(long Numeric, string Name, bool IsPowerOfTwo);

    public SignedEnumReflector(Func<T, long> toNum, Func<long, T> fromNum) : base(typeof(T))
    {
        _toNum = toNum ?? throw new ArgumentNullException(nameof(toNum));
        _fromNum = fromNum ?? throw new ArgumentNullException(nameof(fromNum));

        var values = Enum.GetValues<T>();
        _values = new EnumValue[values.Length];
        _names = new string[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            long numeric = _toNum(values[i]);
            bool isPowerOfTwo = numeric != 0 && (numeric & (numeric - 1)) == 0;
            _values[i] = new EnumValue(numeric, values[i].ToString(), isPowerOfTwo);
            _names[i] = values[i].ToString();
        }
    }

    public void Reflect(in ReflectorState state)
    {
        var meta = state.Meta;
        var style = meta?.Options?.Style;
        switch (style)
        {
            case DiagEditStyle.Dropdown: RenderDropdown(state); break;
            case DiagEditStyle.Checkboxes: RenderCheckboxes(state); break;
            default: RenderLabel(state); break;
        }
    }

    void RenderCheckboxes(in ReflectorState state)
    {
        // Note: Don't include the value in the node label or changing the value will
        // collapse the tree.
        var label = ReflectorUtil.NameText(state);
        bool treeOpen = ImGui.TreeNode(label);
        ImGui.SameLine();
        ImGui.TextUnformatted($"{state.Target} ({TypeName})");

        if (!treeOpen)
            return;

        long numeric = _toNum((T)state.Target);
        long oldNumeric = numeric;
        foreach (var value in _values)
        {
            if (!value.IsPowerOfTwo) continue;
            bool isSet = (numeric & value.Numeric) != 0;
            if (ImGui.Checkbox(value.Name, ref isSet))
            {
                if (isSet) numeric |= value.Numeric;
                else numeric &= ~value.Numeric;
            }
        }

        if (numeric != oldNumeric)
        {
            var newValue = _fromNum(numeric);
            state.Meta.Setter(state, newValue);
        }

        ImGui.TreePop();
    }

    void RenderDropdown(in ReflectorState state)
    {
        long fullNumeric =_toNum((T)state.Target);
        if (fullNumeric > int.MaxValue)
        {
            RenderLabel(state);
            return;
        }

        int numeric = (int)fullNumeric;
        var label = ReflectorUtil.NameText(state);

        ImGui.Indent();
        ImGui.TextUnformatted(label); // Property names need to go on the left
        ImGui.SameLine();

        // ## suppresses printing the label
        if (ImGui.Combo("##" + label, ref numeric, _names, _names.Length))
        {
            var newValue = _fromNum(numeric);
            state.Meta.Setter(state, newValue);
        }

        ImGui.Unindent();
    }
}