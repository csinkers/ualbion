using System;
using System.ComponentModel;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface ICommandListHolder : IDisposable, INotifyPropertyChanged
{
    string Name { get; }
    CommandList CommandList { get; }
}