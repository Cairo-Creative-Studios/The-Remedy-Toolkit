using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Remedy.Schematics.Utils
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Union
    {
        public enum ValueType : byte
        {
            Union,
            Bool, Int, Float,
            Vector2, Vector3, Vector4,
            Color, Color32,
            Quaternion,
            LayerMask,

            String, GameObject, Transform, Component, Material, Texture, AudioClip, Array, Object, Null, PhysicsMaterial,

            List, Scene, Dictionary
        }

        public static readonly Dictionary<ValueType, Type> TypeLookup = new()
        {
            { ValueType.Union, typeof(Union) },

            { ValueType.Bool, typeof(bool) },
            { ValueType.Int, typeof(int) },
            { ValueType.Float, typeof(float) },

            { ValueType.Vector2, typeof(Vector2) },
            { ValueType.Vector3, typeof(Vector3) },
            { ValueType.Vector4, typeof(Vector4) },

            { ValueType.Color, typeof(Color) },
            { ValueType.Color32, typeof(Color32) },
            { ValueType.Quaternion, typeof(Quaternion) },
            { ValueType.LayerMask, typeof(LayerMask) },

            { ValueType.String, typeof(string) },
            { ValueType.GameObject, typeof(GameObject) },
            { ValueType.Transform, typeof(Transform) },
            { ValueType.Component, typeof(Component) },
            { ValueType.Material, typeof(Material) },
            { ValueType.Texture, typeof(Texture) },
            { ValueType.AudioClip, typeof(AudioClip) },

            { ValueType.Array, typeof(Array) },
            { ValueType.Object, typeof(UnityEngine.Object) },
            { ValueType.Null, null },
            { ValueType.PhysicsMaterial, typeof(PhysicsMaterial) },

            { ValueType.List, typeof(System.Collections.IList) },
            { ValueType.Scene, typeof(Scene) },
            { ValueType.Dictionary, typeof(System.Collections.IDictionary) },
        };
        public static readonly Dictionary<Type, ValueType> ReverseTypeLookup;
        private static readonly List<(Type type, ValueType valueType)> _assignableTypeMap;

        [FieldOffset(0)] public ValueType Type;       // 1 byte
        [FieldOffset(1)] public byte LastUpdateTick;  // 1 byte
        [FieldOffset(8)] unsafe private fixed ulong _storage[2]; // 16 bytes

        private static int GlobalTick = 0;
        public static byte NextTick() => (byte)Interlocked.Increment(ref GlobalTick);

        private static int _sceneIdCounter = 1;
        private static readonly Dictionary<int, Scene> _sceneLookup = new();
        public static Union Null
        {
            get
            {
                return new Union
                {
                    Type = ValueType.Null
                };
            }
        }

        public static ref TTo UnsafeCastAs<TFrom, TTo>(ref TFrom source)
        {
            return ref UnsafeUtility.As<TFrom, TTo>(ref source);
        }
        public unsafe T GetValue<T>()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                return UnsafeUtility.As<object, T>(ref UnsafeUtility.As<ulong, object>(ref _storage[0]));

            return UnsafeUtility.As<ulong, T>(ref _storage[0]);
        }
        public unsafe void SetValue<T>(T value)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                UnsafeUtility.As<ulong, object>(ref _storage[0]) = value;
            else
                UnsafeUtility.As<ulong, T>(ref _storage[0]) = value;
        }


        // Constructors
        public Union(bool value) : this() { Type = ValueType.Bool; SetValue(new Vector4(value ? 1f : 0f, 0, 0, 0)); }
        public Union(int value) : this() { Type = ValueType.Int; SetValue(new Vector4(value, 0, 0, 0)); }
        public Union(float value) : this() { Type = ValueType.Float; SetValue(new Vector4(value, 0, 0, 0)); }
        public Union(Vector2 value) : this() { Type = ValueType.Vector2; SetValue(new Vector4(value.x, value.y, 0, 0)); }
        public Union(Vector3 value) : this() { Type = ValueType.Vector3; SetValue(new Vector4(value.x, value.y, value.z, 0)); }
        public Union(Vector4 value) : this() { Type = ValueType.Vector4; SetValue(value); }
        public Union(Color value) : this() { Type = ValueType.Color; SetValue(new Vector4(value.r, value.g, value.b, value.a)); }
        public Union(Color32 value) : this() { Type = ValueType.Color32; SetValue(new Vector4(value.r / 255f, value.g / 255f, value.b / 255f, value.a / 255f)); }
        public Union(Quaternion value) : this() { Type = ValueType.Quaternion; SetValue(new Vector4(value.x, value.y, value.z, value.w)); }
        public Union(LayerMask value) : this() { Type = ValueType.LayerMask; SetValue(new Vector4(value.value, 0, 0, 0)); }
        public Union(string value) : this() { Type = ValueType.String; SetValue(value); }
        public Union(GameObject value) : this() { Type = ValueType.GameObject; SetValue(value); }
        public Union(Component value) : this() { Type = ValueType.Component; SetValue(value); }
        public Union(PhysicsMaterial value) : this() { Type = ValueType.PhysicsMaterial; SetValue(value); }
        public Union(List<Union> value) : this() { Type = ValueType.List; SetValue(value); }
        public Union(Dictionary<string, Union> value) : this() { Type = ValueType.Dictionary; SetValue(value); }
        public Union(Array value) : this() { Type = ValueType.Array; SetValue(value); }
        public Union(Union value) : this() { Type = ValueType.Union; SetValue(value); }
        public Union(Type type) : this() { Type = ReverseTypeLookup[type]; }

        static Union()
        {
            ReverseTypeLookup = new Dictionary<Type, ValueType>();
            _assignableTypeMap = new List<(Type, ValueType)>();

            foreach (var kvp in TypeLookup)
            {
                if (kvp.Value == null)
                    continue;

                ReverseTypeLookup[kvp.Value] = kvp.Key;
                _assignableTypeMap.Add((kvp.Value, kvp.Key));
            }
        }

        public Union(Scene scene) : this()
        {
            Type = ValueType.Scene;

            int id = _sceneIdCounter++;
            _sceneLookup[id] = scene;

            SetValue(new Vector4(id, 0, 0, 0));
        }

        public static implicit operator Union(bool value) => new(value);
        public static implicit operator Union(int value) => new(value);
        public static implicit operator Union(float value) => new(value);
        public static implicit operator Union(Vector2 value) => new(value);
        public static implicit operator Union(Vector3 value) => new(value);
        public static implicit operator Union(Vector4 value) => new(value);
        public static implicit operator Union(Color value) => new(value);
        public static implicit operator Union(Color32 value) => new(value);
        public static implicit operator Union(Quaternion value) => new(value);
        public static implicit operator Union(string value) => new(value);
        public static implicit operator Union(LayerMask value) => new(value);
        public static implicit operator Union(GameObject value) => new(value);
        public static implicit operator Union(Component value) => new(value);
        public static implicit operator Union(PhysicsMaterial value) => new(value);
        public static implicit operator Union(List<Union> value) => new(value);
        public static implicit operator Union(Dictionary<string, Union> value) => new(value);
        public static implicit operator Union(Array value) => new(value);
        public static implicit operator Union(Scene scene) => new(scene);

        public static implicit operator bool(Union value) => value.GetValue<bool>();
        public static implicit operator int(Union value) => value.GetValue<int>();
        public static implicit operator float(Union value) => value.GetValue<float>();
        public static implicit operator Vector2(Union value) => value.GetValue<Vector2>();
        public static implicit operator Vector3(Union value) => value.GetValue<Vector3>();
        public static implicit operator Vector4(Union value) => value.GetValue<Vector4>();
        public static implicit operator Color(Union value) => value.GetValue<Color>();
        public static implicit operator Color32(Union value) => value.GetValue<Color32>();
        public static implicit operator Quaternion(Union value) => value.GetValue<Quaternion>();
        public static implicit operator string(Union value) => value.GetValue<string>();
        public static implicit operator GameObject(Union value) => value.GetValue<GameObject>();
        public static implicit operator Component(Union value) => value.GetValue<Component>();
        public static implicit operator PhysicsMaterial(Union value) => value.GetValue<PhysicsMaterial>();
        public static implicit operator Array(Union value) => value.GetValue<Array>();
        public static implicit operator List<Union>(Union value) => value.GetValue<List<Union>>();
        public static implicit operator Dictionary<string, Union>(Union value) => value.GetValue<Dictionary<string, Union>>();
        public static implicit operator Scene(Union value) => value.GetScene();

        public override string ToString()
        {
            var sv = GetValue<Vector4>();
            return Type switch
            {
                ValueType.Null => "null",
                ValueType.Bool => (sv.x != 0f).ToString(),
                ValueType.Int => ((int)sv.x).ToString(),
                ValueType.Float => sv.x.ToString(),
                ValueType.Vector2 => $"({sv.x}, {sv.y})",
                ValueType.Vector3 => $"({sv.x}, {sv.y}, {sv.z})",
                ValueType.Vector4 => $"({sv.x}, {sv.y}, {sv.z}, {sv.w})",
                ValueType.Color => $"RGBA({sv.x:F2}, {sv.y:F2}, {sv.z:F2}, {sv.w:F2})",
                ValueType.LayerMask => $"LayerMask({(int)sv.x})",
                _ => GetValue<object>()?.ToString() ?? Type.ToString()
            };
        }

        public Scene GetScene()
        {
            if (Type != ValueType.Scene) return default;

            int id = (int)GetValue<Vector4>().x;
            return _sceneLookup.TryGetValue(id, out var scene) ? scene : default;
        }
        public static Union operator +(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a + (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a + (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a + (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a + (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union((Vector2)a + (Vector2)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union((Vector3)a + (Vector3)b),
                (ValueType.Vector4, ValueType.Vector4) => new Union((Vector4)a + (Vector4)b),
                (ValueType.Color, ValueType.Color) => new Union((Color)a + (Color)b),
                (ValueType.Color32, ValueType.Color32) => new Union(AddColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Vector3) => new Union(AddColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(AddColorVector3((Color)b, (Vector3)a)),
                (ValueType.Color, ValueType.Vector4) => new Union(AddColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(AddColorVector4((Color)b, (Vector4)a)),
                (ValueType.String, _) => new Union((string)a + b.ToString()),
                (_, ValueType.String) => new Union(a.ToString() + (string)b),
                (ValueType.Array, ValueType.Array) => new Union(CombineArrays((Array)a, (Array)b)),
                (ValueType.List, ValueType.List) => new Union(CombineLists(a.GetValue<List<Union>>(), b.GetValue<List<Union>>())),
                (ValueType.Array, _) => new Union(AddToArray((Array)a, b.GetValue<object>())),
                (ValueType.List, _) => new Union(AddToList(a.GetValue<List<Union>>(), b)),
                _ => new(GetNull())
            };
        }

        private static string GetNull()
        {
            return null;
        }

        private static Color32 AddColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)Mathf.Min(255, a.r + b.r),
                (byte)Mathf.Min(255, a.g + b.g),
                (byte)Mathf.Min(255, a.b + b.b),
                (byte)Mathf.Min(255, a.a + b.a)
            );
        }

        private static Color AddColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r + vector.x),
                Mathf.Clamp01(color.g + vector.y),
                Mathf.Clamp01(color.b + vector.z),
                color.a
            );
        }

        private static Color AddColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r + vector.x),
                Mathf.Clamp01(color.g + vector.y),
                Mathf.Clamp01(color.b + vector.z),
                Mathf.Clamp01(color.a + vector.w)
            );
        }

        private static Array CombineArrays(Array a, Array b)
        {
            var elementType = a.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, a.Length + b.Length);
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        private static List<Union> CombineLists(List<Union> a, List<Union> b)
        {
            var result = new List<Union>();
            foreach (var item in a) result.Add(item);
            foreach (var item in b) result.Add(item);
            return result;
        }

        private static Array AddToArray(Array array, object item)
        {
            var elementType = array.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, array.Length + 1);
            Array.Copy(array, result, array.Length);
            result.SetValue(item, array.Length);
            return result;
        }

        private static List<Union> AddToList(List<Union> list, Union item)
        {
            var result = new List<Union>();
            foreach (var existingItem in list) result.Add(existingItem);
            result.Add(item);
            return result;
        }

        public static Union operator *(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a * (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a * (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a * (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a * (int)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union(Vector3.Scale((Vector3)a, ((Vector3)b))),
                (ValueType.Vector2, ValueType.Vector2) => new Union(Vector2.Scale((Vector2)a, ((Vector2)b))),
                (ValueType.Vector4, ValueType.Vector4) => new Union(Vector4.Scale((Vector4)a, ((Vector4)b))),
                (ValueType.Vector2, ValueType.Float) => new Union((Vector2)a * ((float)b)),
                (ValueType.Float, ValueType.Vector2) => new Union((Vector2)b * ((float)a)),
                (ValueType.Vector4, ValueType.Float) => new Union((Vector4)a * ((float)b)),
                (ValueType.Float, ValueType.Vector4) => new Union((Vector4)b * ((float)a)),
                (ValueType.Vector3, ValueType.Float) => new Union((Vector3)a * (float)b),
                (ValueType.Float, ValueType.Vector3) => new Union((float)a * (Vector3)b),
                (ValueType.Color, ValueType.Color) => new Union(MultiplyColor((Color)a, (Color)b)),
                (ValueType.Color32, ValueType.Color32) => new Union(MultiplyColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Float) => new Union((Color)a * (float)b),
                (ValueType.Float, ValueType.Color) => new Union((Color)b * (float)a),
                (ValueType.Color, ValueType.Vector3) => new Union(MultiplyColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(MultiplyColorVector3((Color)b, (Vector3)a)),
                (ValueType.Color, ValueType.Vector4) => new Union(MultiplyColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(MultiplyColorVector4((Color)b, (Vector4)a)),
                (ValueType.String, ValueType.Int) => new Union(string.Concat(Enumerable.Repeat((string)a, (int)b))),
                _ => new(GetNull())
            };
        }

        private static Color MultiplyColor(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }

        private static Color32 MultiplyColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)((a.r * b.r) / 255),
                (byte)((a.g * b.g) / 255),
                (byte)((a.b * b.b) / 255),
                (byte)((a.a * b.a) / 255)
            );
        }

        private static Color MultiplyColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r * vector.x),
                Mathf.Clamp01(color.g * vector.y),
                Mathf.Clamp01(color.b * vector.z),
                color.a
            );
        }

        private static Color MultiplyColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r * vector.x),
                Mathf.Clamp01(color.g * vector.y),
                Mathf.Clamp01(color.b * vector.z),
                Mathf.Clamp01(color.a * vector.w)
            );
        }

        public static Union operator -(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a - (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a - (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a - (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a - (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union((Vector2)a - (Vector2)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union((Vector3)a - (Vector3)b),
                (ValueType.Vector4, ValueType.Vector4) => new Union((Vector4)a - (Vector4)b),
                (ValueType.Color, ValueType.Color) => new Union((Color)a - (Color)b),
                (ValueType.Color32, ValueType.Color32) => new Union(SubtractColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Vector3) => new Union(SubtractColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(SubtractVector3Color((Vector3)a, (Color)b)),
                (ValueType.Color, ValueType.Vector4) => new Union(SubtractColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(SubtractVector4Color((Vector4)a, (Color)b)),
                (ValueType.String, _) => new Union(((string)a).Replace(b.ToString(), "")),
                (_, ValueType.String) => new Union(a.ToString().Replace((string)b, "")),
                (ValueType.Array, _) => new Union(RemoveFromArray((Array)a, b.GetValue<object>())),
                (ValueType.List, _) => new Union(RemoveFromList(a.GetValue<List<Union>>(), b.GetValue<object>())),
                _ => new(GetNull())
            };
        }

        private static Color32 SubtractColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)Mathf.Max(0, a.r - b.r),
                (byte)Mathf.Max(0, a.g - b.g),
                (byte)Mathf.Max(0, a.b - b.b),
                (byte)Mathf.Max(0, a.a - b.a)
            );
        }

        private static Color SubtractColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r - vector.x),
                Mathf.Clamp01(color.g - vector.y),
                Mathf.Clamp01(color.b - vector.z),
                color.a
            );
        }

        private static Vector3 SubtractVector3Color(Vector3 vector, Color color)
        {
            return new Vector3(
                vector.x - color.r,
                vector.y - color.g,
                vector.z - color.b
            );
        }

        private static Color SubtractColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r - vector.x),
                Mathf.Clamp01(color.g - vector.y),
                Mathf.Clamp01(color.b - vector.z),
                Mathf.Clamp01(color.a - vector.w)
            );
        }

        private static Vector4 SubtractVector4Color(Vector4 vector, Color color)
        {
            return new Vector4(
                vector.x - color.r,
                vector.y - color.g,
                vector.z - color.b,
                vector.w - color.a
            );
        }

        private static Array RemoveFromArray(Array array, object itemToRemove)
        {
            var list = new List<object>();
            bool removed = false;

            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                if (!removed && Equals(item, itemToRemove))
                {
                    removed = true;
                    continue;
                }
                list.Add(item);
            }

            var elementType = array.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                result.SetValue(list[i], i);
            }
            return result;
        }

        private static List<Union> RemoveFromList(List<Union> list, object itemToRemove)
        {
            var result = new List<Union>();
            bool removed = false;

            foreach (var item in list)
            {
                if (!removed && Equals(item, itemToRemove))
                {
                    removed = true;
                    continue;
                }
                result.Add(item);
            }
            return result;
        }

        public static Union operator /(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a / (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a / (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a / (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a / (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union(new Vector2(((Vector2)a).x / ((Vector2)b).x,
                                                                                    ((Vector2)a).y / ((Vector2)b).y)),
                (ValueType.Vector3, ValueType.Vector3) => new Union(new Vector3(((Vector3)a).x / ((Vector3)b).x,
                                                                                    ((Vector3)a).y / ((Vector3)b).y,
                                                                                    ((Vector3)a).z / ((Vector3)b).z)),
                (ValueType.Vector4, ValueType.Vector4) => new Union(new Vector4(((Vector4)a).x / ((Vector4)b).x,
                                                                                    ((Vector4)a).y / ((Vector4)b).y,
                                                                                    ((Vector4)a).z / ((Vector4)b).z,
                                                                                    ((Vector4)a).w / ((Vector4)b).w)),
                (ValueType.Vector2, ValueType.Float) => new Union((Vector2)a / (float)b),
                (ValueType.Float, ValueType.Vector2) => new Union(new Vector2((float)a / ((Vector2)b).x, (float)a / ((Vector2)b).y)),
                (ValueType.Vector3, ValueType.Float) => new Union((Vector3)a / (float)b),
                (ValueType.Float, ValueType.Vector3) => new Union(new Vector3((float)a / ((Vector3)b).x, (float)a / ((Vector3)b).y, (float)a / ((Vector3)b).z)),
                (ValueType.Vector4, ValueType.Float) => new Union((Vector4)a / (float)b),
                (ValueType.Float, ValueType.Vector4) => new Union(new Vector4((float)a / ((Vector4)b).x, (float)a / ((Vector4)b).y, (float)a / ((Vector4)b).z, (float)a / ((Vector4)b).w)),
                (ValueType.Color, ValueType.Color) => new Union(DivideColor((Color)a, (Color)b)),
                (ValueType.Color32, ValueType.Color32) => new Union(DivideColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Float) => new Union((Color)a / (float)b),
                (ValueType.Color, ValueType.Vector3) => new Union(DivideColorVector3((Color)a, (Vector3)b)),
                (ValueType.Color, ValueType.Vector4) => new Union(DivideColorVector4((Color)a, (Vector4)b)),
                _ => new(GetNull())
            };
        }

        private static Color DivideColor(Color a, Color b)
        {
            return new Color(
                b.r != 0 ? a.r / b.r : 0,
                b.g != 0 ? a.g / b.g : 0,
                b.b != 0 ? a.b / b.b : 0,
                b.a != 0 ? a.a / b.a : 0
            );
        }

        private static Color32 DivideColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)(b.r != 0 ? (a.r * 255) / b.r : 0),
                (byte)(b.g != 0 ? (a.g * 255) / b.g : 0),
                (byte)(b.b != 0 ? (a.b * 255) / b.b : 0),
                (byte)(b.a != 0 ? (a.a * 255) / b.a : 0)
            );
        }

        private static Color DivideColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                vector.x != 0 ? Mathf.Clamp01(color.r / vector.x) : 0,
                vector.y != 0 ? Mathf.Clamp01(color.g / vector.y) : 0,
                vector.z != 0 ? Mathf.Clamp01(color.b / vector.z) : 0,
                color.a
            );
        }

        private static Color DivideColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                vector.x != 0 ? Mathf.Clamp01(color.r / vector.x) : 0,
                vector.y != 0 ? Mathf.Clamp01(color.g / vector.y) : 0,
                vector.z != 0 ? Mathf.Clamp01(color.b / vector.z) : 0,
                vector.w != 0 ? Mathf.Clamp01(color.a / vector.w) : 0
            );
        }
    }
}