using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemData)]
    public class ItemDataLoader : IAssetLoader<IList<ItemData>>
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config) =>
            Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);

        public IList<ItemData> Serdes(IList<ItemData> items, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            items ??= new List<ItemData>();
            if (s.Mode == SerializerMode.Reading)
            {
                int i = 0;
                while (!s.IsComplete())
                {
                    var item = ItemData.Serdes(i, null, mapping, s);
                    items.Add(item);
                    i++;
                }
            }
            else
            {
                foreach (var item in items)
                    ItemData.Serdes((int) item.Id, item, mapping, s);
            }

            return items;
        }
    }
}
