using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.Directory)]
    public class DirectoryContainerLoader : IContainerLoader
    {
        public ISerializer Open(string path, AssetInfo info)
        {
            throw new NotImplementedException();
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            var subIds = new List<int>();
            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                var file = Path.GetFileName(filePath);
                int index = file.IndexOf('_');
                var part = index == -1 ? file : file.Substring(0, index);
                if (!int.TryParse(part, out var asInt))
                    continue;

                subIds.Add(asInt);
            }

            subIds.Sort();
            return FormatUtil.SortedIntsToRanges(subIds);
        }
    }
}