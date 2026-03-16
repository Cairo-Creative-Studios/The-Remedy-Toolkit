using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SchematicAssets
{
    public static class SchematicAssetManager
    {
        private const string BaseFolder = "Assets/_Schematics";

        #region Public API – Create
        /// <summary>
        /// Creates a ScriptableObject asset scoped to a specific obj and property path.
        /// Returns the existing asset if it already exists with the same type.
        /// Useful for storing per-obj, per-field data in prefab assets.
        /// </summary>
        public static ScriptableObject Create(UnityEngine.Object obj, string propertyPath, string fieldName, string fileName)
        => CreateInternal<ScriptableObject>(obj, propertyPath, fieldName, fileName, typeof(ScriptableObject));

        /// <summary>
        /// Creates a typed ScriptableObject asset scoped to a specific obj and property path.
        /// Returns the existing asset if it already exists with the same type.
        /// </summary>
        public static T Create<T>(UnityEngine.Object obj, string propertyPath, string fieldName, string fileName)
        where T : ScriptableObject
        => CreateInternal<T>(obj, propertyPath, fieldName, fileName, typeof(T));

        /// <summary>
        /// Creates a ScriptableObject asset of the specified type, scoped to a specific obj and property path.
        /// Returns the existing asset if it already exists with the same type.
        /// </summary>
        public static ScriptableObject Create(UnityEngine.Object obj, string propertyPath, string fieldName, string fileName, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsSubclassOf(typeof(ScriptableObject)))
                throw new ArgumentException($"Type {type.Name} must inherit from ScriptableObject", nameof(type));

            return CreateInternal<ScriptableObject>(obj, propertyPath, fieldName, fileName, type);
        }

        /// <summary>
        /// Creates the given Memory Object as an Asset 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyPath"></param>
        /// <param name="memoryObject"></param>
        /// <returns></returns>
        public static ScriptableObject CreateFromMemory(UnityEngine.Object obj, string propertyPath, string fieldName, ScriptableObject memoryObject) 
            => CreateFromMemoryInternal(obj, propertyPath, fieldName, memoryObject);
        #endregion

        /*        public static void RenameFirst(UnityEngine.Object obj, string propertyPath, string newName)
                {
                    var assets = LoadAll(obj, propertyPath);

                    if(assets != null && assets.Length > 0)
                    {
                        var path = GetFolderForProperty(obj, propertyPath);
                        AssetDatabase.RenameAsset(GetAssetPath(obj, propertyPath, assets[0].name), newName);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }*/

        public static void Rename(UnityEngine.Object obj, string propertyPath, string fieldName, string oldName, string newName)
        {
            var assets = LoadAll(obj, propertyPath);

            if (assets != null && assets.Length > 0)
            {
                var path = GetFolderForProperty(obj, propertyPath);
                AssetDatabase.RenameAsset(GetAssetPath(obj, propertyPath, oldName, fieldName), newName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        #region Public API – Load
        /// <summary>
        /// Loads a ScriptableObject asset for a specific obj and property path.
        /// Returns null if the asset does not exist.
        /// Useful for retrieving per-field prefab data.
        /// </summary>
        public static ScriptableObject Load(UnityEngine.Object obj, string propertyPath, string fileName)
        => LoadInternal<ScriptableObject>(obj, propertyPath, fileName, typeof(ScriptableObject));

        /// <summary>
        /// Loads a typed ScriptableObject asset for a specific obj and property path.
        /// Returns null if the asset does not exist.
        /// </summary>
        public static T Load<T>(UnityEngine.Object obj, string propertyPath, string fileName)
        where T : ScriptableObject
        => LoadInternal<T>(obj, propertyPath, fileName, typeof(T));
/*
        /// <summary>
        /// Loads a ScriptableObject asset scoped to a obj without a property path.
        /// Returns null if the asset does not exist.
        /// </summary>
        public static ScriptableObject Load(UnityEngine.Object obj, string fileName)
        => LoadInternal<ScriptableObject>(obj, "", fileName, typeof(ScriptableObject));

        /// <summary>
        /// Loads a typed ScriptableObject asset scoped to a obj without a property path.
        /// </summary>
        public static T Load<T>(UnityEngine.Object obj, string fileName)
        where T : ScriptableObject
        => LoadInternal<T>(obj, "", fileName, typeof(T));
*/
        #endregion

        #region Public API – LoadAll
        /// <summary>
        /// Loads all ScriptableObject assets for a given obj and optional property path.
        /// Useful for iterating over all field-specific assets on a obj.
        /// </summary>
        public static ScriptableObject[] LoadAll(UnityEngine.Object obj, string propertyPath = "", string fieldName = "")
        => LoadAllInternal<ScriptableObject>(obj, propertyPath, fieldName);

        public static T[] LoadAll<T>(UnityEngine.Object obj, string propertyPath = "", string fieldName = "") where T : ScriptableObject
        => LoadAllInternal<T>(obj, propertyPath, fieldName);
        #endregion

        #region Public API - Delete
        /// <summary>
        /// Deletes a ScriptableObject asset scoped to a component and property path.
        /// </summary>
        public static void Delete(Component component, string propertyPath, string fileName)
        => DeleteInternal(component, propertyPath, fileName, null);

        /// <summary>
        /// Deletes a typed ScriptableObject asset scoped to a component and property path.
        /// </summary>
        public static void Delete<T>(Component component, string propertyPath, string fileName)
        where T : ScriptableObject
        => DeleteInternal(component, propertyPath, fileName, typeof(T));

        /// <summary>
        /// Deletes a ScriptableObject asset scoped to a component without a property path.
        /// </summary>
        public static void Delete(Component component, string fileName)
        => DeleteInternal(component, "", fileName, null);

        /// <summary>
        /// Deletes a typed ScriptableObject asset scoped to a component without a property path.
        /// </summary>
        public static void Delete<T>(Component component, string fileName)
        where T : ScriptableObject
        => DeleteInternal(component, "", fileName, typeof(T));
        #endregion

        #region Public API – DeleteAll
        /// <summary>
        /// Deletes all ScriptableObject assets for a given component and optional property path.
        /// </summary>
        public static void DeleteAll(UnityEngine.Object obj, string propertyPath = "", string fieldName = "")
        => DeleteAllInternal(obj, propertyPath, null, fieldName);

        /// <summary>
        /// Deletes all typed ScriptableObject assets for a given component and optional property path.
        /// </summary>
        public static void DeleteAll<T>(UnityEngine.Object obj, string propertyPath = "", string fieldName = "") where T : ScriptableObject
        => DeleteAllInternal(obj, propertyPath, typeof(T), fieldName);


        /// <summary>
        /// Deletes the Schematic Asset folder for the Object
        /// </summary>
        /// <param name="globalObjectID"></param>
        public static void DeleteObjectFolder(UnityEngine.Object obj)
        {
            var folder = GetObjectFolder(obj);
            if (!AssetDatabase.IsValidFolder(folder))
                return;

            AssetDatabase.DeleteAsset(folder);
            ValidateFolders();
        }

        /// <summary>
        /// Deletes the Schematic Asset folder for the Object with the given GlobalObjectID
        /// </summary>
        /// <param name="globalObjectID"></param>
        public static void DeleteObjectFolder(string globalObjectID)
        {
            string folder = Path.Combine(BaseFolder, globalObjectID).Replace("\\", "/");

            if (!AssetDatabase.IsValidFolder(folder))
                return;

            AssetDatabase.DeleteAsset(folder);
            ValidateFolders();
        }

        #endregion

        public static bool AssetExists(UnityEngine.Object obj, string propertyPath, string fieldName, string fileName)
        {
            var path = GetAssetPath(obj, propertyPath, fileName, fieldName);
            return AssetDatabase.AssetPathExists(path);
        }

        /// <summary>
        /// Returns true if a folder for the given Object already exists in the Schematics Asset Database.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ObjectFolderExists(UnityEngine.Object obj)
        {
            var objectFolder = GetObjectFolderPath(obj);
            if (AssetDatabase.IsValidFolder(objectFolder))
                return true;
            return false;
        }


        /// <summary>
        /// Recursively creates all missing folders in a Unity project path.
        /// </summary>
        internal static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath)) return;

            folderPath = folderPath.Replace("\\", "/");

            if (!folderPath.StartsWith("Assets"))
            {
                Debug.LogError("Folder path must start with 'Assets': " + folderPath);
                return;
            }

            string[] parts = folderPath.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                currentPath = nextPath;
            }
        }
        // ------------------------------------------------------------------------------------
        // INTERNAL IMPLEMENTATION
        // ------------------------------------------------------------------------------------
        private static void ValidateFolders()
        {
            if (!Directory.Exists(BaseFolder))
            {
                Debug.LogWarning($"Base folder does not exist: {BaseFolder}");
                return;
            }

            string[] directories = Directory.GetDirectories(BaseFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (var dir in directories)
            {
                string folderName = Path.GetFileName(dir);

                if (GlobalObjectId.TryParse(folderName, out GlobalObjectId globalId))
                {
                    UnityEngine.Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);

                    if (obj == null)
                    {
                        DeleteObjectFolder(folderName);
                    }
                }
            }
        }

        private static T CreateInternal<T>(UnityEngine.Object obj, string propertyPath, string fieldName, string fileName, Type type) where T : ScriptableObject
        {
            var assetPath = GetAssetPath(obj, propertyPath, fileName, fieldName);

            EnsureFolderExists(Path.GetDirectoryName(assetPath));

            var existing = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as ScriptableObject;
            if (existing != null)
            {
                if (existing.GetType() == type)
                    return existing as T;
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }

            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, assetPath);

            ValidateFolders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset as T;
        }

        private static ScriptableObject CreateFromMemoryInternal(UnityEngine.Object obj, string propertyPath, string fieldName, UnityEngine.Object memoryObject)
        {

            var type = memoryObject.GetType();

            var assetPath = GetAssetPath(obj, propertyPath, memoryObject.name, fieldName);
            EnsureFolderExists(Path.GetDirectoryName(assetPath));

            var existing = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as ScriptableObject;
            if (existing != null)
            {
                if (existing.GetType() == type)
                    return existing;
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }

            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, assetPath);

            ValidateFolders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }

        private static T LoadInternal<T>(UnityEngine.Object obj, string propertyPath, string fileName, Type type, string fieldName = "") where T : ScriptableObject
        {
            ValidateFolders();
            var assetPath = GetAssetPath(obj, propertyPath, fileName, fieldName);
            return AssetDatabase.LoadAssetAtPath(assetPath, type) as T;
        }

        private static T[] LoadAllInternal<T>(UnityEngine.Object obj, string propertyPath, string fieldName) where T : ScriptableObject
        {
            var folder = GetFolderForProperty(obj, propertyPath);

            if (!string.IsNullOrEmpty(fieldName))
                folder = Path.Combine(folder, fieldName).Replace("\\", "/");
            
            if (!AssetDatabase.IsValidFolder(folder))
                return Array.Empty<T>();

            var results = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    results.Add(asset);
            }

            ValidateFolders();

            return results.ToArray();
        }

        private static void DeleteInternal(UnityEngine.Object obj, string propertyPath, string fileName, Type type, string fieldName = "")
        {
            var assetPath = GetAssetPath(obj, propertyPath, fileName, fieldName);
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                ValidateFolders();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void DeleteAllInternal(UnityEngine.Object obj, string propertyPath, Type type, string fieldName)
        {
            var folder = GetFolderForProperty(obj, propertyPath);
            if (string.IsNullOrEmpty(fieldName))
                folder = Path.Combine(folder, fieldName);
            
            if (!AssetDatabase.IsValidFolder(folder))
                return;

            foreach (var guid in AssetDatabase.FindAssets("", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (type != null)
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
                    if (asset == null || asset.GetType() != type)
                        continue;
                }

                AssetDatabase.DeleteAsset(path);
            }

            ValidateFolders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ------------------------------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------------------------------

        private static string GetAssetPath(UnityEngine.Object obj, string propertyPath, string fileName, string fieldName)
        {
            string folder = GetFolderForProperty(obj, propertyPath);

            //string safeType = Sanitize(type?.Name ?? "Object");
            string safeFileName = Sanitize(fileName ?? "");
            string safePropPath = NormalizePropertyPath(propertyPath).Replace('/', '_');
            /*
                        string assetFileName = string.IsNullOrEmpty(safePropPath)
                            ? $"{safeType}__{safeFileName}.asset"
                            : $"{safeType}__{safeFileName}__{safePropPath}.asset";
            */

            if (!string.IsNullOrEmpty(fieldName))
                folder = Path.Combine(folder, fieldName);

            return Path.Combine(folder, safeFileName + ".asset").Replace("\\", "/");
        }

        private static string GetFolderForProperty(UnityEngine.Object obj, string propertyPath)
        {
            string componentFolder = GetObjectFolder(obj);

            string safePropPath = NormalizePropertyPath(propertyPath);
            string folder = string.IsNullOrEmpty(safePropPath)
                ? componentFolder
                : Path.Combine(componentFolder, safePropPath).Replace("\\", "/");

            EnsureFolderExists(folder);
            return folder;
        }

        private static string GetObjectFolder(UnityEngine.Object obj)
        {
            var path = GetObjectFolderPath(obj);
            EnsureFolderExists(path);
            return path;

            throw new InvalidOperationException("SchematicAssetManager is intended for prefab assets only.");
        }

        private static string GetObjectFolderPath(UnityEngine.Object obj)
        {
            string transformId;

            if (obj is GameObject go) // is prefab root
            {
                var objAssetPath = AssetDatabase.GetAssetPath(obj);
                transformId = AssetDatabase.GUIDFromAssetPath(objAssetPath).ToString();
            }
            else
                transformId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();


            string path = Path.Combine(BaseFolder, transformId).Replace("\\", "/");
            return path;
        }

        private static bool IsPrefabAsset(UnityEngine.Object obj)
        {
            if (obj is GameObject go)
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);

                return root == go;
            }
            else
            {
                return false;
            }
        }

        private static string NormalizePropertyPath(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) return "";

            string normalized = propertyPath
                .Replace(".Array.data[", "[")
                .Replace('.', '/');

            return SanitizePath(normalized);
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "_";
            foreach (char c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');
            return input;
        }

        private static string SanitizePath(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            foreach (char c in Path.GetInvalidPathChars())
                input = input.Replace(c, '_');
            return input;
        }
    }
}
