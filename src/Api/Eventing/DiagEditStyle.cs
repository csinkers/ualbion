namespace UAlbion.Api.Eventing;

public enum DiagEditStyle
{
    Label, // Default, read-only label
    NumericInput,
    NumericSlider, // Requires min+max
    Dropdown, // For enum
    Checkboxes, // For flags enum
    Text, // For strings (Min, Max refer to length)

    ColorPicker, // for Vector3, Vector4
    Position,
    Size,

    CharacterAttribute,
    IdPicker,
    InvSlot
}