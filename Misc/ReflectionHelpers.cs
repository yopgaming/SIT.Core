using BepInEx.Logging;
using Comfort.Common;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Misc
{
    public static class ReflectionHelpers
    {
        static ManualLogSource Logger;
        static ReflectionHelpers()
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("ReflectionHelpers");
        }

        public static void ConvertDictionaryToObject(object o, Dictionary<string, object> dict)
        {
            foreach (var key in dict)
            {
                var prop = GetPropertyFromType(o.GetType(), key.Key);
                if (prop != null)
                {
                    prop.SetValue(o, key.Value);
                }
                var field = GetFieldFromType(o.GetType(), key.Key);
                if (field != null)
                {
                    field.SetValue(o, key.Value);
                }
            }
        }

        public static T DoSafeConversion<T>(object o)
        {
            var json = o.SITToJson();
            return json.SITParseJson<T>();
        }

        public static object GetSingletonInstance(Type singletonInstanceType)
        {
            Type generic = typeof(Singleton<>);
            Type[] typeArgs = { singletonInstanceType };
            var genericType = generic.MakeGenericType(typeArgs);
            return GetPropertyFromType(genericType, "Instance").GetValue(null, null);
        }

        public static PropertyInfo GetPropertyFromType(Type t, string name)
        {
            var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo property = properties.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (property != null)
                return property;

            return null;
        }

        public static FieldInfo GetFieldFromType(Type t, string name)
        {
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return fields.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

        }

        public static FieldInfo GetFieldFromTypeByFieldType(Type objectType, Type fieldType)
        {
            var fields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return fields.FirstOrDefault(x => x.FieldType == fieldType);

        }

        public static PropertyInfo GetPropertyFromTypeByPropertyType(Type objectType, Type propertyType)
        {
            var fields = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return fields.FirstOrDefault(x => x.PropertyType == propertyType);

        }

        public static MethodInfo GetMethodForType(Type t, string methodName, bool debug = false)
        {
            if (t == null)
                return null;

            return ReflectionHelpers.GetAllMethodsForType(t, debug).LastOrDefault(x => x.Name.ToLower() == methodName.ToLower());
        }

        public static async Task<MethodInfo> GetMethodForTypeAsync(Type t, string methodName, bool debug = false)
        {
            return await Task.Run(() => GetMethodForType(t, methodName, debug));
        }


        public static IEnumerable<MethodInfo> GetAllMethodsForType(Type t, bool debug = false)
        {
            foreach (var m in t.GetMethods(
                BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
                | BindingFlags.CreateInstance
                ))
            {
                if (debug)
                    Logger.LogInfo(m.Name);

                yield return m;
            }

            if (t.BaseType != null)
            {
                foreach (var m in t.BaseType.GetMethods(
                BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
                ))
                {
                    if (debug)
                        Logger.LogInfo(m.Name);

                    yield return m;
                }
            }

        }

        public static IEnumerable<MethodInfo> GetAllMethodsForObject(object ob)
        {
            return ReflectionHelpers.GetAllMethodsForType(ob.GetType());
        }

        public static IEnumerable<PropertyInfo> GetAllPropertiesForObject(object o)
        {
            if (o == null)
                return new List<PropertyInfo>();

            var t = o.GetType();
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            props.AddRange(t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic));
            props.AddRange(t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            props.AddRange(t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy));
            props.AddRange(t.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            if (t.BaseType != null)
            {
                t = t.BaseType;
                props.AddRange(t.GetProperties(BindingFlags.Instance | BindingFlags.Public));
                props.AddRange(t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic));
                props.AddRange(t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
                props.AddRange(t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                props.AddRange(t.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            }
            return props.Distinct(x => x.Name).AsEnumerable();
        }

        public static IEnumerable<FieldInfo> GetAllFieldsForObject(object o)
        {
            var t = o.GetType();
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();
            fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
            fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            fields.AddRange(t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy));
            fields.AddRange(t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            if (t.BaseType != null)
            {
                t = t.BaseType;
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Public));
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
                fields.AddRange(t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                fields.AddRange(t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
            }
            return fields.Distinct(x => x.Name).AsEnumerable();
        }

        public static T GetFieldOrPropertyFromInstance<T>(object o, string name, bool safeConvert = true)
        {
            PropertyInfo property = GetAllPropertiesForObject(o).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (property != null)
            {
                if (safeConvert)
                    return DoSafeConversion<T>(property.GetValue(o));
                else
                    return (T)property.GetValue(o);
            }
            FieldInfo field = GetAllFieldsForObject(o).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (field != null)
            {
                if (safeConvert)
                    return DoSafeConversion<T>(field.GetValue(o));
                else
                    return (T)field.GetValue(o);
            }

            return default(T);
        }

        public static async Task<T> GetFieldOrPropertyFromInstanceAsync<T>(object o, string name, bool safeConvert = true)
        {
            return await Task.Run(() => GetFieldOrPropertyFromInstance<T>(o, name, safeConvert));
        }

        public static void SetFieldOrPropertyFromInstance(object o, string name, object v)
        {
            var field = GetAllFieldsForObject(o).FirstOrDefault(x => x.Name.ToLower() == (name.ToLower()));
            if (field != null)
                field.SetValue(o, v);

            var property = GetAllPropertiesForObject(o).FirstOrDefault(x => x.Name.ToLower() == (name.ToLower()));
            if (property != null)
                property.SetValue(o, v);
        }

        public static void SetFieldOrPropertyFromInstance<T>(object o, string name, T v)
        {
            var field = GetAllFieldsForObject(o).FirstOrDefault(x => x.Name.ToLower() == (name.ToLower()));
            if (field != null)
                field.SetValue(o, v);

            var property = GetAllPropertiesForObject(o).FirstOrDefault(x => x.Name.ToLower() == (name.ToLower()));
            if (property != null)
                property.SetValue(o, v);
        }
    }
}
