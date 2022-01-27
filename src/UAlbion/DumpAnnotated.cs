using System.Collections.Generic;
using System.IO;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Game.Assets;

namespace UAlbion;

class DumpAnnotated : Component, IAssetDumper
{
    public void Dump(string baseDir, ISet<AssetType> types, AssetId[] dumpIds)
    {
        var applier = Resolve<IModApplier>();

        void Write(string name, string content)
        {
            var filename = Path.Combine(baseDir, "data", "exported", "annotated", name);
            var directory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filename, content);
        }

        foreach (var type in types)
        {
            foreach (var id in DumpUtil.All(type, dumpIds))
            {
                var notes = applier.LoadAssetAnnotated(id, Language.English);
                var name = ConfigUtil.AssetName(id);
                Write($"{type}\\{id.Id}_{name}.txt", notes);
            }
        }
    }
}