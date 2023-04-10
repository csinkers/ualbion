using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Visual;
using UAlbion.Game.Input;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Diag;

public class PositionsWindow : Component, IImGuiWindow
{
    readonly ICameraProvider _cameraProvider;
    readonly string _name;

    public PositionsWindow(string name, ICameraProvider cameraProvider)
    {
        _name = name;
        _cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
    }

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);

        var window = Resolve<IGameWindow>();
        var mousePosition = Resolve<ICursorManager>().Position;
        var camera = _cameraProvider.Camera;
        if (camera == null)
        {
            ImGui.Text("No camera found");
            ImGui.End();
            return;
        }

        Vector3 cameraPosition = camera.Position;
        Vector3 cameraTilePosition = cameraPosition;

        var map = Resolve<IMapManager>().Current;
        if (map != null)
            cameraTilePosition /= map.TileSize;

        Vector3 cameraDirection = camera.LookDirection;
        float cameraMagnification = camera.Magnification;

        var normPos = window.PixelToNorm(mousePosition);
        var uiPos = window.NormToUi(normPos);
        uiPos.X = (int)uiPos.X;
        uiPos.Y = (int)uiPos.Y;

        var walkOrder = Resolve<IParty>()?.WalkOrder;
        Vector3? playerTilePos = walkOrder?[0].GetPosition();

        static string Vec(Vector3 v) => $"<{v.X:N2}, {v.Y:N2}, {v.Z:N2}>";

        ImGui.TextUnformatted($"Mouse Pos: {mousePosition} UI: {uiPos}");
        ImGui.TextUnformatted($"Scale: {window.GuiScale}x Game Window: {window.Size}");
        ImGui.TextUnformatted($"Mouse Norm: {normPos}");
        ImGui.TextUnformatted($"Player Tile: {playerTilePos}");
        ImGui.TextUnformatted($"Camera Tile: {Vec(cameraTilePosition)}");
        ImGui.TextUnformatted($"Camera World: {Vec(cameraPosition)}");
        ImGui.TextUnformatted($"Camera Dir: {Vec(cameraDirection)} Mag: {cameraMagnification}");
        ImGui.TextUnformatted($"TileSize: {map?.TileSize}");

        ImGui.End();

        if (!open)
            Remove();
    }
}