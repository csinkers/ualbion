using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemData)]
    public class ItemDataLoader : IAssetLoader<IList<ItemData>>
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config) =>
            Serdes(null, new AlbionReader(br, streamLength), key, config);

        public IList<ItemData> Serdes(IList<ItemData> items, ISerializer s, AssetKey key, AssetInfo config)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            items ??= new List<ItemData>();
            s.Begin();
            if (s.Mode == SerializerMode.Reading)
            {
                int i = 0;
                while (!s.IsComplete())
                {
                    var item = ItemData.Serdes(i, null, s);
                    items.Add(item);
                    i++;
                }
            }
            else
            {
                foreach (var item in items)
                    ItemData.Serdes((int) item.Id, item, s);
            }

            s.End();
            return items;
        }
    }
}
