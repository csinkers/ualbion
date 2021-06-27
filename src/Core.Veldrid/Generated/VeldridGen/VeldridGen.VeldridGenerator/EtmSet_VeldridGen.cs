using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial class EtmSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("Properties", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1),
            new ResourceLayoutElementDescription("DayFloors", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("DayWalls", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("NightFloors", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("NightWalls", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("TextureSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16));

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.Visual.DungeonTileMapProperties> Properties
        {
            get => _properties;
            set
            {
                if (_properties == value)
                    return;
                _properties = value;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.Texture2DArrayHolder DayFloors
        {
            get => _dayFloors;
            set
            {
                if (_dayFloors == value) return;

                if (_dayFloors != null)
                    _dayFloors.PropertyChanged -= PropertyDirty;

                _dayFloors = value;

                if (_dayFloors != null)
                    _dayFloors.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.Texture2DArrayHolder DayWalls
        {
            get => _dayWalls;
            set
            {
                if (_dayWalls == value) return;

                if (_dayWalls != null)
                    _dayWalls.PropertyChanged -= PropertyDirty;

                _dayWalls = value;

                if (_dayWalls != null)
                    _dayWalls.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.Texture2DArrayHolder NightFloors
        {
            get => _nightFloors;
            set
            {
                if (_nightFloors == value) return;

                if (_nightFloors != null)
                    _nightFloors.PropertyChanged -= PropertyDirty;

                _nightFloors = value;

                if (_nightFloors != null)
                    _nightFloors.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.Texture2DArrayHolder NightWalls
        {
            get => _nightWalls;
            set
            {
                if (_nightWalls == value) return;

                if (_nightWalls != null)
                    _nightWalls.PropertyChanged -= PropertyDirty;

                _nightWalls = value;

                if (_nightWalls != null)
                    _nightWalls.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.SamplerHolder TextureSampler
        {
            get => _textureSampler;
            set
            {
                if (_textureSampler == value) 
                    return;

                if (_textureSampler != null)
                    _textureSampler.PropertyChanged -= PropertyDirty;

                _textureSampler = value;

                if (_textureSampler != null)
                    _textureSampler.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _properties.DeviceBuffer,
                _dayFloors.DeviceTexture,
                _dayWalls.DeviceTexture,
                _nightFloors.DeviceTexture,
                _nightWalls.DeviceTexture,
                _textureSampler.Sampler));
    }
}
