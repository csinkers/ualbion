using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Game.Text
{
    public class DynamicText : ITextSource
    {
        readonly Func<IEnumerable<TextBlock>> _generator;
        readonly Func<int, int> _getVersion;
#if DEBUG
        IList<TextBlock> _lastResult = new List<TextBlock>();
        public override string ToString()
        {
            var lastText = string.Join(" ", _lastResult.Select(x => x.Text));
            return $"DynTxt \"{lastText}\"";
        }
#endif
        int _version = 1;

        public DynamicText(Func<IEnumerable<TextBlock>> generator)
        {
            _generator = generator;
            _getVersion = x => _version;
        }
        public DynamicText(Func<IEnumerable<TextBlock>> generator, Func<int, int> getVersion)
        {
            _generator = generator;
            _getVersion = getVersion;
        }

        public int Version => _getVersion(_version);
        public void Invalidate() => _version++;
        public IEnumerable<TextBlock> Get()
        {
#if DEBUG
            _lastResult = _generator().ToList();
            return _lastResult;
#else
            return _generator();
#endif
        }
    }
}