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

    public PositionsWindow(int id, ICameraProvider cameraProvider)
    {
        _cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
        _name = $"Positions###Positions{id}";
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

        ImGui.Text($"Cursor Pix: {mousePosition} UI: {uiPos} Scale: {window.GuiScale} PixSize: {window.Size} Norm: {normPos}");
        ImGui.Text($"Camera World: {cameraPosition} Tile: {cameraTilePosition} Dir: {cameraDirection} Mag: {cameraMagnification}");
        ImGui.Text($"TileSize: {map?.TileSize} PlayerTilePos: {playerTilePos}");

        ImGui.End();

        if (!open)
            Remove();
    }
}