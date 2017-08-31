using System.Linq;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public static class Extensions
    {
        /// <summary>
        /// Returns and ensures the component exist within the game object, i.e. gets the specified component if the component does not exist, adds one.
        /// </summary>
        public static T EnsureComponent<T>(this Component item) where T : Component { return item.gameObject.EnsureComponent<T>(); }

        /// <summary>
        /// Returns and ensures the component exist within the game object, i.e. gets the specified component if the component does not exist, adds one.
        /// </summary>
        public static T EnsureComponent<T>(this GameObject go) where T : Component { return go.GetComponent<T>() ?? go.AddComponent<T>(); }

        /// <summary>
        /// Extracts the quaternion rotation values form the specified matrix.
        /// </summary>
        public static Quaternion ExtractQuaternion(this Matrix4x4 m)
        {
            var q = new Quaternion();

            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));

            return q;
        }

        /// <summary>
        /// Appends the specified material to the renderer shared material array.
        /// </summary>
        public static void AppendMaterial(this GameObject go, Material material) { AppendMaterial(go.GetComponent<Renderer>(), material); }

        /// <summary>
        /// Removes the specified material from the renderer shared material array.
        /// </summary>
        public static void RemoveMaterial(this GameObject go, Material material) { RemoveMaterial(go.GetComponent<Renderer>(), material); }

        /// <summary>
        /// Appends the specified material to the renderer shared material array.
        /// </summary>
        public static void AppendMaterial(this Renderer renderer, Material material)
        {
            Debug.Assert(renderer != null, "Cannot append material to null renderer.");
            Debug.Assert(material != null, "Cannot append null material to renderer.");

            renderer.sharedMaterials = renderer.sharedMaterials.Concat(new[] { material }).ToArray();
        }

        /// <summary>
        /// Removes the specified material from the renderer shared material array.
        /// </summary>
        public static void RemoveMaterial(this Renderer renderer, Material material)
        {
            Debug.Assert(renderer != null, "Cannot remove material from null renderer.");
            Debug.Assert(material != null, "Cannot remove null material from renderer.");
            Debug.Assert(renderer.sharedMaterials.Contains(material), "Cannot remove material from renderer. Material is not assigned to renderer.");

            var materials = renderer.sharedMaterials.ToList();
            materials.Remove(material);
            renderer.sharedMaterials = materials.ToArray();
        }
    }
}
