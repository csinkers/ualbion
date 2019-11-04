using System;
using System.Collections.Generic;

namespace UAlbion.Game
{
    public class DynamicText : ITextSource
    {
        readonly Func<IEnumerable<TextBlock>> _generator;
        readonly Func<int> _getVersion;
        int _version = 1;

        public DynamicText(Func<IEnumerable<TextBlock>> generator)
        {
            _generator = generator;
            _getVersion = () => _version;
        }
        public DynamicText(Func<IEnumerable<TextBlock>> generator, Func<int> getVersion)
        {
            _generator = generator;
            _getVersion = getVersion;
        }

        public int Version => _getVersion();
        public void Invalidate() => _version++;
        public IEnumerable<TextBlock> Get() => _generator();
    }
}