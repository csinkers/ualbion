using System;
using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public class SamplerHolder : Component, ISamplerHolder
    {
        SamplerAddressMode _addressModeU;
        SamplerAddressMode _addressModeV;
        SamplerAddressMode _addressModeW;
        SamplerFilter _filter;
        ComparisonKind? _comparisonKind;
        uint _maximumAnisotropy;
        uint _minimumLod;
        uint _maximumLod;
        int _lodBias;
        SamplerBorderColor _borderColor;
        string _name;

        public event PropertyChangedEventHandler PropertyChanged;
        public Sampler Sampler { get; private set; }

        public SamplerHolder()
        {
            On<DeviceCreatedEvent>(_ => Dirty());
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        void Dirty() => On<PrepareFrameResourcesEvent>(e => Update(e.Device));

        void Update(GraphicsDevice gd)
        {
            if (Sampler != null)
                Dispose();

            Sampler = gd.ResourceFactory.CreateSampler(new SamplerDescription(
                _addressModeU,
                _addressModeV,
                _addressModeW,
                _filter,
                _comparisonKind,
                _maximumAnisotropy,
                _minimumLod,
                _maximumLod,
                _lodBias,
                _borderColor));

            GC.ReRegisterForFinalize(this);
            if (!string.IsNullOrEmpty(_name))
                Sampler.Name = _name;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sampler)));
            Off<PrepareFrameResourcesEvent>();
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;
                if (Sampler != null)
                    Sampler.Name = value;
            }
        }

        public SamplerAddressMode AddressModeU
        {
            get => _addressModeU;
            set { _addressModeU = value; Dirty(); }
        }

        public SamplerAddressMode AddressModeV
        {
            get => _addressModeV;
            set { _addressModeV = value; Dirty(); }
        }

        public SamplerAddressMode AddressModeW
        {
            get => _addressModeW;
            set { _addressModeW = value; Dirty(); }
        }
        public SamplerFilter Filter
        {
            get => _filter;
            set { _filter = value; Dirty(); }
        }

        public ComparisonKind? ComparisonKind
        {
            get => _comparisonKind;
            set { _comparisonKind = value; Dirty(); }
        }

        public uint MaximumAnisotropy
        {
            get => _maximumAnisotropy;
            set { _maximumAnisotropy = value; Dirty(); }
        }

        public uint MinimumLod
        {
            get => _minimumLod;
            set { _minimumLod = value; Dirty(); }
        }

        public uint MaximumLod
        {
            get => _maximumLod;
            set { _maximumLod = value; Dirty(); }
        }

        public int LodBias
        {
            get => _lodBias;
            set { _lodBias = value; Dirty(); }
        }

        public SamplerBorderColor BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Dirty(); }
        }


        protected virtual void Dispose(bool disposing)
        {
            Sampler?.Dispose();
            Sampler = null;
        }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}
