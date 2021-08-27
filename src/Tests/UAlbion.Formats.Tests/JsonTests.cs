using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class JsonTests
    {
        [Fact]
        public void CustomDictionaryTest()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.Clear()
                .RegisterAssetType(typeof(Base.Npc), AssetType.Npc)
                .RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember)
                ;

            var sheets = new Dictionary<CharacterId, CharacterSheet>
            {
                { Base.Npc.Argim, new CharacterSheet(Base.Npc.Argim) },
                { Base.PartyMember.Tom, new CharacterSheet(Base.PartyMember.Tom) }
            };

            var json = JsonUtil.Serialize(sheets);
            var reloaded = JsonUtil.Deserialize<Dictionary<CharacterId, CharacterSheet>>(json);

            Assert.Collection(reloaded.OrderBy(x => x.Key.ToString()),
                kvp =>
                {
                    Assert.Equal(Base.Npc.Argim, kvp.Key);
                },
                kvp =>
                {
                    Assert.Equal(Base.PartyMember.Tom, kvp.Key);
                }
            );
        }
    }
}