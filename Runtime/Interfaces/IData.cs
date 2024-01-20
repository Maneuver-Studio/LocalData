using System;
using System.Collections.Generic;

namespace Maneuver.LocalData
{
    public interface IData
    {
        int Length { get; }
        int Count { get; }
        Type Type { get; }
        bool isDirty { get; }
        object SetValue(string key, object value);
        object GetValue(string key, object defaultValue = default);
        bool ContainsKey(string key);
        bool ContainsValue(object value);
        string ChangeKey(string oldKey, string newKey);
        string KeyByValue(object value);
        List<string> KeysByValue(object value);
        bool RemoveKey(string key);
        void RemoveKeys(List<string> keysList);
        string[] AllKeys(Type type);
        List<object> AllValues(Type type);
        void DeleteAll();
    }
}