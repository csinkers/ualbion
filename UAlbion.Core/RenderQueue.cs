using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace UAlbion.Core
{
    public class RenderQueue : IEnumerable<Renderable>
    {
        const int DefaultCapacity = 250;

        readonly List<RenderItemIndex> _indices = new List<RenderItemIndex>(DefaultCapacity);
        readonly List<Renderable> _renderables = new List<Renderable>(DefaultCapacity);

        public int Count => _renderables.Count;

        public void Clear()
        {
            _indices.Clear();
            _renderables.Clear();
        }

        public void AddRange(List<Renderable> renderables, Vector3 viewPosition)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                Renderable renderable = renderables[i];
                if (renderable != null)
                {
                    Add(renderable, viewPosition);
                }
            }
        }

        public void AddRange(IReadOnlyList<Renderable> renderables, Vector3 viewPosition)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                Renderable renderable = renderables[i];
                if (renderable != null)
                {
                    Add(renderable, viewPosition);
                }
            }
        }

        public void AddRange(IEnumerable<Renderable> renderables, Vector3 viewPosition)
        {
            foreach (Renderable item in renderables)
            {
                if (item != null)
                {
                    Add(item, viewPosition);
                }
            }
        }

        public void Add(Renderable item, Vector3 viewPosition)
        {
            int index = _renderables.Count;
            _indices.Add(new RenderItemIndex(item.GetRenderOrderKey(viewPosition), index));
            _renderables.Add(item);
            Debug.Assert(_renderables.IndexOf(item) == index);
        }

        public void Sort()
        {
            _indices.Sort();
        }

        public void Sort(Comparer<RenderOrderKey> keyComparer)
        {
            _indices.Sort((first, second) => keyComparer.Compare(first.Key, second.Key));
        }

        public void Sort(Comparer<RenderItemIndex> comparer)
        {
            _indices.Sort(comparer);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_indices, _renderables);
        }

        IEnumerator<Renderable> IEnumerable<Renderable>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Renderable>
        {
            readonly List<RenderItemIndex> _indices;
            readonly List<Renderable> _renderables;
            int _nextItemIndex;
            Renderable _currentItem;

            public Enumerator(List<RenderItemIndex> indices, List<Renderable> renderables)
            {
                _indices = indices;
                _renderables = renderables;
                _nextItemIndex = 0;
                _currentItem = null;
            }

            public Renderable Current => _currentItem;
            object IEnumerator.Current => _currentItem;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_nextItemIndex >= _indices.Count)
                {
                    _currentItem = null;
                    return false;
                }
                else
                {
                    var currentIndex = _indices[_nextItemIndex];
                    _currentItem = _renderables[currentIndex.ItemIndex];
                    _nextItemIndex += 1;
                    return true;
                }
            }

            public void Reset()
            {
                _nextItemIndex = 0;
                _currentItem = null;
            }
        }
    }
}
