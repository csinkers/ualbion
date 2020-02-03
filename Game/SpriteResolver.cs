using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game
{
    public class SpriteResolver : Component, ISpriteResolver
    {
        ITexture _defaultTexture;

        static readonly IDictionary<Type, AssetType> AssetTypeLookup = new Dictionary<Type, AssetType>
        {
            {typeof(AutoMapId), AssetType.AutomapGraphics},
            {typeof(CombatBackgroundId), AssetType.CombatBackground},
            {typeof(CombatGraphicsId), AssetType.CombatGraphics},
            {typeof(CoreSpriteId), AssetType.CoreGraphics},
            {typeof(DungeonBackgroundId), AssetType.BackgroundGraphics},
            {typeof(DungeonFloorId), AssetType.Floor3D},
            {typeof(DungeonObjectId), AssetType.Object3D},
            {typeof(DungeonOverlayId), AssetType.Overlay3D},
            {typeof(DungeonWallId), AssetType.Wall3D},
            {typeof(FullBodyPictureId), AssetType.FullBodyPicture},
            {typeof(IconGraphicsId), AssetType.IconGraphics},
            {typeof(ItemSpriteId), AssetType.ItemGraphics},
            {typeof(LargeNpcId), AssetType.BigNpcGraphics},
            {typeof(LargePartyGraphicsId), AssetType.BigPartyGraphics},
            {typeof(MonsterGraphicsId), AssetType.MonsterGraphics},
            {typeof(PictureId), AssetType.Picture},
            {typeof(SmallNpcId), AssetType.SmallNpcGraphics},
            {typeof(SmallPartyGraphicsId), AssetType.SmallPartyGraphics},
            {typeof(SmallPortraitId), AssetType.SmallPortrait},
            {typeof(TacticId), AssetType.TacticalIcon},
            {typeof(SlabId), AssetType.Slab}
        };

        public override void Subscribed() { _defaultTexture = Resolve<IAssetManager>().LoadTexture(DungeonWallId.DefaultTexture); }

        public Vector2 GetSize(Type idType, int id, int subObject)
        {
            var assetType =  AssetTypeLookup[idType];
            ITexture texture = Resolve<IAssetManager>().LoadTexture(assetType, id);
            if(texture == null)
                return Vector2.One;
            texture.GetSubImageDetails(subObject, out var size, out _, out _, out _);
            return size;
        }

        public Tuple<SpriteKey, SpriteInstanceData> Resolve(Sprite sprite)
        {
            var assetType = AssetTypeLookup[sprite.IdType];
            var id = sprite.NumericId;
            ITexture texture = Resolve<IAssetManager>().LoadTexture(assetType, id);
            if (texture == null)
            {
                return Tuple.Create(new SpriteKey(_defaultTexture, (int)DrawLayer.Diagnostic, sprite.Flags),
                    SpriteInstanceData.Centred(sprite.Position,
                        new Vector2(_defaultTexture.Width, _defaultTexture.Height),
                        Vector2.Zero, Vector2.One, 0, 0));
            }

            texture.GetSubImageDetails(sprite.SubObject, out var size, out var texOffset, out var texSize, out var layer);

            var key = new SpriteKey(texture, sprite.RenderOrder, sprite.Flags);
            var instance = SpriteInstanceData.CopyFlags(
                sprite.Position,
                sprite.Size ?? size, texOffset, texSize, layer,
                sprite.Flags | (texture.Format == PixelFormat.R8_UNorm ? SpriteFlags.UsePalette : 0)
            );
            return Tuple.Create(key, instance);
        }
    }
}
