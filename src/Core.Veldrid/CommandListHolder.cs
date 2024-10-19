using System;
using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Core.Veldrid;

public sealed class CommandListHolder : Component, ICommandListHolder
{
    readonly Action<PrepareDeviceObjectsEvent> _prepareDelegate;
    CommandList _cl;

    public CommandListHolder(string name)
    {
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
        _prepareDelegate = Prepare;
        Name = name;
    }

    public string Name { get; }
    public CommandList CommandList
    {
        get => _cl;
        private set
        {
            if (_cl == value)
                return;

            _cl = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandList)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected override void Subscribed() => Dispose(); // We expect it to already be null in this case, so we're just calling dispose to subscribe to the prepare event.
    protected override void Unsubscribed() => Dispose();
    void Prepare(PrepareDeviceObjectsEvent e)
    {
        if (CommandList != null)
            Dispose();

        CommandList = e.Device.ResourceFactory.CreateCommandList();
        CommandList.Name = Name;
        Off<PrepareDeviceObjectsEvent>();
    }

    public void Dispose()
    {
        _cl?.Dispose();
        CommandList = null;
        On(_prepareDelegate);
    }
}
