using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.Framework
{
    public static class ObjectExtensions
    {
        public static void DestroyChildren(this UnityEngine.Transform a)
        {
            while (a.childCount > 0)
            {
                GameObject.DestroyImmediate(a.GetChild(0).gameObject);
            }
        }

        public static Transform FindOrCreate(this UnityEngine.Transform a, string name)
        {
            return a.Find(name) ?? new GameObject(name).transform;
        }

        public static T GetCachedComponent<T>(this UnityEngine.Object a) where T : Component
        {
            return a is Component c ? SystemManager.GetCachedComponent<T>(c.gameObject) :
                   a is GameObject g ? SystemManager.GetCachedComponent<T>(g) :
                   null;
        }
        public static T GetCachedComponentInParents<T>(this UnityEngine.Object a) where T : Component
        {
            return a is Component c ? SystemManager.GetCachedComponentInParents<T>(c.gameObject) :
                   a is GameObject g ? SystemManager.GetCachedComponentInParents<T>(g) :
                   null;
        }

        public static T GetCachedComponent<T>(this Component a) where T : Component
        {
            return a is Component c ? SystemManager.GetCachedComponent<T>(c.gameObject) :
                   null;
        }
        public static T GetCachedComponentInParents<T>(this Component a) where T : Component
        {
            return a is Component c ? SystemManager.GetCachedComponentInParents<T>(c.gameObject) :
                   null;
        }

        /// <summary>
        /// Destroys all components that are required by the given MonoBehaviour
        /// Based on user PizzaPies Answer: https://answers.unity.com/questions/1445663/how-to-auto-remove-the-component-that-was-required.html
        /// </summary>
        /// <param name="monoInstanceCaller"></param>
        public static void DestroyWithRequiredComponents(this MonoBehaviour monoInstanceCaller)
        {
            MemberInfo memberInfo = monoInstanceCaller.GetType();
            RequireComponent[] requiredComponentsAtts = Attribute.GetCustomAttributes(memberInfo, typeof(RequireComponent), true) as RequireComponent[];
            var monoInstance = monoInstanceCaller.gameObject;
            List<Type> typesToDestroy = new List<Type>();

            foreach (RequireComponent rc in requiredComponentsAtts)
            {
                if (rc != null && monoInstanceCaller.GetComponent(rc.m_Type0) != null)
                {
                    typesToDestroy.Add(rc.m_Type0);
                }
            }

            UnityEngine.Object.DestroyImmediate(monoInstanceCaller);

            foreach (Type type in typesToDestroy)
            {
                UnityEngine.Object.DestroyImmediate(monoInstance.GetComponent(type));
            }
        }

        public static List<T> GetComponentsImplementing<T>(this GameObject gameObject) where T : class
        {
            List<T> components = new List<T>();
            foreach (MonoBehaviour component in gameObject.GetComponents<MonoBehaviour>())
            {
                if (component is T)
                {
                    components.Add(component as T);
                }
            }
            return components;
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }

        public static List<T> GetComponentsInImmediateChildren<T>(this Transform parent)
        {
            var results = new List<T>();

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                // Get component on the child itself
                var comp = child.GetComponent<T>();
                if (comp != null)
                    results.Add(comp);
            }

            return results;
        }

        public static class ExtendedResources
        {
            public static GameObject[] LoadAllByComponent<T>(string path) where T : Component
            {
                // Use the LINQ method syntax to filter the gameObjects by component type
                return Resources.LoadAll<GameObject>(path).Where(x => x.GetComponents<T>().Count() > 0).ToArray();
            }
        }

        public static object GetValueFromSerializedProperty(this SerializedProperty property, object root)
        {
            string[] path = property.propertyPath.Replace(".Array.data[", "[").Split('.');
            object obj = root;

            foreach (string part in path)
            {
                if (part.Contains("["))
                {
                    string fieldName = part.Substring(0, part.IndexOf("["));
                    int index = int.Parse(part.Substring(part.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetFieldValue(obj, fieldName);
                    if (obj is System.Collections.IEnumerable enumerable)
                    {
                        var enumerator = enumerable.GetEnumerator();
                        for (int i = 0; i <= index; i++)
                        {
                            if (!enumerator.MoveNext()) return null;
                        }
                        obj = enumerator.Current;
                    }
                }
                else
                {
                    obj = GetFieldValue(obj, part);
                }

                if (obj == null) break;
            }

            return obj;
        }

        private static object GetFieldValue(object obj, string fieldName)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return field?.GetValue(obj);
        }



#if UNITY_EDITOR
        /// <summary>
        /// Get's all the references to the given object in the Editor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static List<string> FindReferences(this UnityEngine.Object target)
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            List<string> referencingAssets = new List<string>();

            foreach (string path in allAssetPaths)
            {
                if (!path.StartsWith("Assets")) continue;

                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null || asset == target) continue;

                SerializedObject so = new SerializedObject(asset);
                SerializedProperty prop = so.GetIterator();
                bool foundReference = false;

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (prop.objectReferenceValue == target)
                        {
                            referencingAssets.Add(path);
                            foundReference = true;
                            break;
                        }
                    }
                }

                if (foundReference)
                    continue;
            }

            return referencingAssets;
        }

        /// <summary>
        /// Replaces all references to this object in the Project to another object.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="replacement"></param>
        public static void ReplaceAssetReferences(this UnityEngine.Object original, UnityEngine.Object replacement)
        {
            if (original == null || replacement == null)
            {
                Debug.LogError("Original and replacement assets must not be null.");
                return;
            }

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            int replaceCount = 0;

            foreach (string path in allAssetPaths)
            {
                if (!path.StartsWith("Assets")) continue;

                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null || asset == original) continue;

                SerializedObject so = new SerializedObject(asset);
                bool assetModified = false;

                SerializedProperty prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (prop.objectReferenceValue == original)
                        {
                            prop.objectReferenceValue = replacement;
                            assetModified = true;
                            replaceCount++;
                        }
                    }
                }

                if (assetModified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(asset);
                    Debug.Log($"Modified: {path}", asset);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Reference replacement complete. {replaceCount} references updated.");
        }
#endif
    }
}
