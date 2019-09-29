using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
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
            {typeof(DungeonBackgroundId), AssetType.BackgroundGraphics},
            {typeof(LargeNpcId), AssetType.BigNpcGraphics},
            {typeof(LargePartyGraphicsId), AssetType.BigPartyGraphics},
            {typeof(CombatBackgroundId), AssetType.CombatBackground},
            {typeof(CombatGraphicsId), AssetType.CombatGraphics},
            {typeof(DungeonFloorId), AssetType.Floor3D},
            {typeof(FullBodyPictureId), AssetType.FullBodyPicture},
            {typeof(IconDataId), AssetType.IconData},
            {typeof(IconGraphicsId), AssetType.IconGraphics},
            {typeof(ItemId), AssetType.ItemGraphics},
            {typeof(MonsterGraphicsId), AssetType.MonsterGraphics},
            {typeof(DungeonObjectId), AssetType.Object3D},
            {typeof(DungeonOverlayId), AssetType.Overlay3D},
            {typeof(SmallNpcId), AssetType.SmallNpcGraphics},
            {typeof(SmallPartyGraphicsId), AssetType.SmallPartyGraphics},
            {typeof(SmallPortraitId), AssetType.SmallPortrait},
            {typeof(TacticId), AssetType.TacticalIcon},
            {typeof(DungeonWallId), AssetType.Wall3D},
            {typeof(PictureId), AssetType.Picture},
            {typeof(CoreSpriteId), AssetType.CoreGraphics}
        };

        static readonly Handler[] Handlers = {
            new Handler<SpriteResolver, SubscribedEvent>((x, _) =>
                x._defaultTexture = x.Exchange.Resolve<IAssetManager>().LoadTexture(DungeonWallId.DefaultTexture))
        };
        public SpriteResolver() : base(Handlers) { }

        public Tuple<SpriteKey, SpriteInstanceData> Resolve(SpriteDefinition spriteDefinition)
        {
            var assetType = AssetTypeLookup[spriteDefinition.IdType];
            var id = spriteDefinition.NumericId;
            ITexture texture = Exchange.Resolve<IAssetManager>().LoadTexture(assetType, id);
            if (texture == null)
            {
                return Tuple.Create(new SpriteKey(_defaultTexture, (int)DrawLayer.Diagnostic, spriteDefinition.DepthTested),
                    new SpriteInstanceData(spriteDefinition.Position,
                        new Vector2(_defaultTexture.Width, _defaultTexture.Height),
                        Vector2.Zero, Vector2.One, 0, 0));
            }

            texture.GetSubImageDetails(spriteDefinition.SubObject, out var size, out var texOffset, out var texSize, out var layer);

            var key = new SpriteKey(texture, spriteDefinition.RenderOrder, spriteDefinition.DepthTested);
            var instance = new SpriteInstanceData(
                spriteDefinition.Position,
                spriteDefinition.Size ?? size, texOffset, texSize, layer,
                spriteDefinition.Flags | (texture.Format == PixelFormat.R8_UNorm ? SpriteFlags.UsePalette : 0)
            );
            return Tuple.Create(key, instance);
        }
    }
}