using System;
using System.Collections.Generic;

namespace UAlbion.Core
{
    public class GameObject
    {
        public bool IsActive { get; set; }
        public GameObject Parent { get; }
        public IList<GameObject> Children { get; } = new List<GameObject>();
        /*
        public void Update(float deltaSeconds)
        {
            if (!IsActive)
                return;

            foreach (var child in Children)
                child.Update(deltaSeconds);
        }*/

        public GameObject()
        {

        }

        public GameObject(GameObject parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Parent.Children.Add(this);
        }
    }
}
