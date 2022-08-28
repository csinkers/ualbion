using System;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui;

public interface IDialogManager
{
    T AddDialog<T>(Func<int, T> constructor) where T : ModalDialog;
}