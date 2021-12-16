using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectF
{
    public static class TransformExtends
    {
        public static IEnumerable<Transform> FindChildrenLike(this Transform self, string name)
        {
            return FindChildrenLike(self, name, true);
        }

        public static IEnumerable<Transform> FindChildrenLike(this Transform self, string name, bool includeInactive)
        {
            return GetChildren(self, includeInactive).Where(t => t.name.StartsWith(name));
        }

        public static IEnumerable<Transform> FindChildrenLikeRecursively(this Transform self, string name)
        {
            return FindChildrenLikeRecursively(self, name, true);
        }

        public static IEnumerable<Transform> FindChildrenLikeRecursively(this Transform self, string name, bool includeInactive)
        {
            return GetChildrenRecursively(self, includeInactive).Where(t => t.name.StartsWith(name));
        }

        public static IEnumerable<Transform> FindChildrenRecursively(this Transform self, string name)
        {
            return FindChildrenRecursively(self, name, true);
        }

        public static IEnumerable<Transform> FindChildrenRecursively(this Transform self, string name, bool includeInactive)
        {
            return GetChildrenRecursively(self, includeInactive).Where(t => t.name == name);
        }

        public static Transform GetChild(this Transform self, string name)
        {
            return GetChild(self, name, true);
        }

        public static Transform GetChild(this Transform self, string name, bool includeInactive)
        {
            return GetChildren(self, includeInactive).FirstOrDefault(t => t.name == name);
        }

        public static Transform GetChildRecursively(this Transform self, string name)
        {
            return GetChildRecursively(self, name, true);
        }

        public static Transform GetChildRecursively(this Transform self, string name, bool includeInactive)
        {
            return GetChildrenRecursively(self, includeInactive).FirstOrDefault(t => t.name == name);
        }

        public static IEnumerable<Transform> GetChildren(this Transform self)
        {
            return GetChildren(self, true);
        }

        public static IEnumerable<Transform> GetChildren(this Transform self, bool includeInactive)
        {
            for (int i = 0; i < self.childCount; ++i)
            {
                var child = self.GetChild(i);
                if (includeInactive || child.gameObject.activeSelf)
                    yield return child;
            }
        }

        public static IEnumerable<Transform> GetChildrenRecursively(this Transform self)
        {
            return GetChildrenRecursively(self, true);
        }

        public static IEnumerable<Transform> GetChildrenRecursively(this Transform self, bool includeInactive)
        {
            yield return self;

            foreach (var child in GetChildren(self, includeInactive))
            {
                foreach (var t in GetChildrenRecursively(child, includeInactive))
                    yield return t;
            }
        }
    }
}