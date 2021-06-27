﻿using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Scenes;
using UAlbion.Game.Text;

namespace UAlbion.Game.Entities
{
    public class LargeNpc : Component
    {
        readonly MapNpc _npc;
        readonly MapSprite _sprite;
        int _frameCount;

        public override string ToString() => $"LNpc {_npc.Id} {_sprite.Id}";

        public LargeNpc(MapNpc npc)
        {
            On<ShowMapMenuEvent>(OnRightClick);
            On<SlowClockEvent>(e =>
            {
                _frameCount += e.Delta;
                _sprite.Frame = _frameCount;
            });

            _npc = npc ?? throw new ArgumentNullException(nameof(npc));
            _sprite = AttachChild(new MapSprite(_npc.SpriteOrGroup, DrawLayer.Underlay - 1, 0, SpriteFlags.BottomAligned));
            _sprite.Selected += (sender, e) => { e.RegisterHit(this); e.Handled = true; };
        }

        protected override void Subscribed()
        {
            _sprite.TilePosition = new Vector3(
                _npc.Waypoints[0].X,
                _npc.Waypoints[0].Y,
                DepthUtil.IndoorCharacterDepth(_npc.Waypoints[0].Y)
            );
        }

        void OnRightClick(ShowMapMenuEvent e)
        {
            if (_npc.Node == null)
                return;

            var window = Resolve<IWindowManager>();
            var camera = Resolve<ICamera>();
            var tf = Resolve<ITextFormatter>();

            var normPosition = camera.ProjectWorldToNorm(_sprite.Position);
            var uiPosition = window.NormToUi(normPosition.X, normPosition.Y);

            // TODO: NPC type check.
            IText S(TextId textId) => tf.NoWrap().Center().Format(textId);
            var heading = S(Base.SystemText.MapPopup_Person);
            var options = new List<ContextMenuOption>
            {
                new(
                    S(Base.SystemText.MapPopup_TalkTo),
                    new TriggerChainEvent(_npc.ChainSource, _npc.Chain, _npc.Node, new EventSource(_npc.Id, TextId.None, TriggerTypes.TalkTo)),
                    ContextMenuGroup.Actions),

                new(
                    S(Base.SystemText.MapPopup_MainMenu),
                    new PushSceneEvent(SceneId.MainMenu),
                    ContextMenuGroup.System
                )
            };

            Raise(new ContextMenuEvent(uiPosition, heading, options));
            e.Propagating = false;
        }
    }
}

