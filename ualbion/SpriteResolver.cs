using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Game;
using UAlbion.Game.AssetIds;
using Veldrid;

namespace UAlbion
{
    public class SpriteResolver : ISpriteResolver
    {
        readonly Assets _assets;
        static readonly IDictionary<Type, AssetType> AssetTypeLookup = new Dictionary<Type, AssetType>
        {
            { typeof(AutoMapId)           , AssetType.AutomapGraphics },
            { typeof(DungeonBackgroundId) , AssetType.BackgroundGraphics },
            { typeof(LargeNpcId)          , AssetType.BigNpcGraphics },
            { typeof(LargePartyGraphicsId), AssetType.BigPartyGraphics },
            { typeof(CombatBackgroundId)  , AssetType.CombatBackground },
            { typeof(CombatGraphicsId)    , AssetType.CombatGraphics },
            { typeof(DungeonFloorId)      , AssetType.Floor3D },
            { typeof(FullBodyPictureId)   , AssetType.FullBodyPicture },
            { typeof(IconDataId)          , AssetType.IconData },
            { typeof(IconGraphicsId)      , AssetType.IconGraphics },
            { typeof(ItemId)              , AssetType.ItemGraphics },
            { typeof(MonsterGraphicsId)   , AssetType.MonsterGraphics },
            { typeof(DungeonObjectId)     , AssetType.Object3D },
            { typeof(DungeonOverlayId)    , AssetType.Overlay3D },
            { typeof(SmallNpcId)          , AssetType.SmallNpcGraphics },
            { typeof(SmallPartyGraphicsId), AssetType.SmallPartyGraphics },
            { typeof(SmallPortraitId)     , AssetType.SmallPortrait },
            { typeof(TacticId)            , AssetType.TacticalIcon },
            { typeof(DungeonWallId)       , AssetType.Wall3D},
            { typeof(PictureId)           , AssetType.Picture }
        };

        public SpriteResolver(Assets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public Tuple<SpriteRenderer.SpriteKey, SpriteRenderer.InstanceData> Resolve(SpriteDefinition spriteDefinition)
        {
            var assetType = AssetTypeLookup[spriteDefinition.IdType];
            var id = spriteDefinition.NumericId;
            ITexture texture = _assets.LoadTexture(assetType, id);

            texture.GetSubImageDetails(spriteDefinition.SubObject, out var offset, out var size, out var layer);

            var key = new SpriteRenderer.SpriteKey(texture, spriteDefinition.RenderOrder);
            var instance = new SpriteRenderer.InstanceData(
                spriteDefinition.Position,
                new Vector2(texture.Width, texture.Height),
                offset,
                size,
                layer,
                spriteDefinition.Flags | (texture.Format == PixelFormat.R8_UNorm ? SpriteFlags.UsePalette : 0)
            );
            return Tuple.Create(key, instance);
        }
    }
}