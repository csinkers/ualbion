namespace UAlbion.Api.Eventing;

public enum DiagEditStyle
{
    Label, // Default, read-only label
    NumericInput,
    NumericSlider, // Requires min+max
    ColorPicker, // for Vector3, Vector4
    Dropdown, // For enum
    Checkboxes, // For flags enum
}