using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Maneuver.LocalData.Cryptography;

namespace Maneuver.LocalData
{
    public sealed class LocalData
    {
        public const string DEFAULT_FILE_NAME = "PlayerData";
        public const string FILES_EXTENSION = ".save";
        public static readonly string FilePath = Application.persistentDataPath + "/";

        // Encryption
        private const string ENCRYPTION_KEY = "t1w6(Dx-:lsW_db^xN(%fCOw8qpK1=|,"; // must be 32 chars
        private const string ENCRYPTION_SYMBOL = "#";
        private static readonly ICryptographer _crypto = new AesCrypto();

        // Callbacks
        /// <summary>Give the file path</summary>
        public static event Action<string> onLoadError;

        public static event Action onLoadFinish;
        public static event Action onSaveFinish;

        private static bool _isInitialized;

        private static SaveData _data;

        public static SaveData Data
        {
            get
            {
                if (_isInitialized)
                    return _data;

                Initialize();

                return _data;
            }
        }

        #region Save / Load

        public static void Save(bool encrypt)
        {
            if (Data.IsDirty())
                SaveToFile(DEFAULT_FILE_NAME, encrypt);
        }

        public static void Save()
        {
            Save(Data.enableEncryption);
        }

        public static void Load(bool encrypt)
        {
            LoadFromFile(DEFAULT_FILE_NAME, encrypt);
        }

        public static void Load()
        {
            Load(Data.enableEncryption);
        }

        public static void DeleteFile()
        {
            string fileName = Data.enableEncryption ? DEFAULT_FILE_NAME + ENCRYPTION_SYMBOL : DEFAULT_FILE_NAME;
            string filePath = Path.Combine(FilePath, fileName, FILES_EXTENSION);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }


        private static void SaveToFile(string fileName, bool encrypt)
        {
            if (fileName.Contains(ENCRYPTION_SYMBOL))
            {
                fileName = fileName.Replace(ENCRYPTION_SYMBOL, "");
            }

            string fullName = fileName + FILES_EXTENSION;
            string fullCryptoName = fileName + ENCRYPTION_SYMBOL + FILES_EXTENSION;
            string filePath = FilePath + fullName;
            string cryptoFilePath = FilePath + fullCryptoName;

            string json = JsonUtility.ToJson(Data);

            if (encrypt)
            {
                if (File.Exists(filePath)) // Delete old file
                {
                    File.Delete(filePath);
                }

                byte[] encryptedData = _crypto.Encrypt(json, ENCRYPTION_KEY);
                File.WriteAllBytes(cryptoFilePath, encryptedData);
                onSaveFinish?.Invoke();
            }
            else
            {
                if (File.Exists(cryptoFilePath)) // Delete old encrypted file
                {
                    File.Delete(cryptoFilePath);
                }

                File.WriteAllText(filePath, json);
                onSaveFinish?.Invoke();
            }
        }

        private static void LoadFromFile(string fileName, bool encrypt)
        {
            if (fileName.Contains(ENCRYPTION_SYMBOL))
            {
                fileName = fileName.Replace(ENCRYPTION_SYMBOL, "");
            }

            string fullName = fileName + FILES_EXTENSION;
            string fullCryptoName = fileName + ENCRYPTION_SYMBOL + FILES_EXTENSION;
            string filePath = FilePath + fullName;
            string cryptoFilePath = FilePath + fullCryptoName;

            string json;
            bool normalFileExists = File.Exists(filePath);
            bool cryptoFileExists = File.Exists(cryptoFilePath);

            if (normalFileExists) // Not encrypted file exists
            {
                Data.enableEncryption = false;
                try
                {
                    json = File.ReadAllText(filePath);
                    JsonUtility.FromJsonOverwrite(json, Data);

                    if (encrypt) // Save loaded file as encrypted
                    {
                        Data.enableEncryption = true;
                        SaveToFile(fileName, encrypt);
                    }
                }
                catch // The file is damaged or encrypted
                {
                    json = Decrypt(filePath); // Try to decrypt
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, Data);
                        onLoadFinish?.Invoke();
                    }
                    else // File is damaged
                        onLoadError?.Invoke(filePath);
                }
            }

            if (cryptoFileExists) // File is encrypted
            {
                if (normalFileExists && !encrypt) // We don't need encrypted file - delete it
                {
                    File.Delete(cryptoFilePath);
                }
                else
                {
                    Data.enableEncryption = true;
                    json = Decrypt(cryptoFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, Data);
                        onLoadFinish?.Invoke();
                    }
                    else // File is marked as encrypted, but decryption has been failed
                    {
                        try
                        {
                            json = File.ReadAllText(cryptoFilePath); // Try to read it straightforward
                            JsonUtility.FromJsonOverwrite(json, Data);
                            onLoadFinish?.Invoke();
                        }
                        catch // File is damaged?
                        {
                            onLoadError?.Invoke(filePath);
                        }
                    }
                }
            }

            if (!normalFileExists && !cryptoFileExists) // No file found, create new empty one
            {
                DeleteAll();
                SaveToFile(fileName, encrypt);
            }
        }

        private static string Decrypt(string filePath)
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            return _crypto.Decrypt(encryptedData, ENCRYPTION_KEY);
        }

        #endregion

        #region Data Manipulation

        // String
        public static string GetString(string key, string defaultValue = default)
        {
            return Data.strings.GetValue(key, defaultValue);
        }

        public static string SetString(string key, string value = default)
        {
            return Data.strings.SetValue(key, value);
        }

        // Bool
        public static bool GetBool(string key, bool defaultValue = default)
        {
            return Data.bools.GetValue(key, defaultValue);
        }

        public static bool SetBool(string key, bool value)
        {
            return Data.bools.SetValue(key, value);
        }

        // Integer
        public static int GetInt(string key, int defaultValue = default)
        {
            return Data.ints.GetValue(key, defaultValue);
        }

        public static int SetInt(string key, int value)
        {
            return Data.ints.SetValue(key, value);
        }

        // Float
        public static float GetFloat(string key, float defaultValue = default)
        {
            return Data.floats.GetValue(key, defaultValue);
        }

        public static float SetFloat(string key, float value)
        {
            return Data.floats.SetValue(key, value);
        }

        // Vector2
        public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            return Data.vector2.GetValue(key, defaultValue);
        }

        public static Vector2 SetVector2(string key, Vector2 value)
        {
            return Data.vector2.SetValue(key, value);
        }

        // Vector3
        public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            return Data.vector3.GetValue(key, defaultValue);
        }

        public static Vector3 SetVector3(string key, Vector3 value)
        {
            return Data.vector3.SetValue(key, value);
        }

        /// <summary>Returns true if key exist in data types.</summary>
        public static bool HasKey(string key)
        {
            foreach (var data in Data.DataDict.Values)
                if (data.ContainsKey(key))
                    return true;
            return false;
        }

        /// <summary>Clear all keys and values from all data types. Use with caution.</summary>
        public static void DeleteAll()
        {
            foreach (var data in Data.DataDict.Values)
                data.DeleteAll();
        }

        /// <summary>Remove key and it's value from data types.</summary>
        public static bool DeleteKey(string key)
        {
            bool removed = false;
            foreach (var data in Data.DataDict.Values)
                if (data.RemoveKey(key))
                    removed = true;
            return removed;
        }

        #endregion

        #region Generic functions

        /// <summary>Returns true if key exist in given data.</summary>
        public static bool HasKey<T>(string key)
        {
            Type t = typeof(T);
            bool isSupported = false;
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                isSupported = true;
                if (data.ContainsKey(key))
                    return true;
            }

            if (!isSupported)
                Debug.LogError(TypeIsNotSupported("HasKey<T>", t));
            return false;
        }

        public static bool TryGetValue<T>(string key, out T value)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                bool containsKey = data.ContainsKey(key);
                value = containsKey ? (T)data.GetValue(key) : default;
                return containsKey;
            }

            value = default;
            return false;
        }

        /// <summary>Remove key and it's value from given data.</summary>
        public static bool DeleteKey<T>(string key)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                if (data.RemoveKey(key))
                {
                    data.RemoveKey(key);
                    return true;
                }
            }

            Debug.LogError(TypeIsNotSupported("RemoveKey<T>", t));
            return false;
        }

        /// <summary>Clear all keys and values from given data. Use with caution.</summary>
        public static void DeleteAll<T>()
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                data.DeleteAll();
                return;
            }

            Debug.LogError(TypeIsNotSupported("ClearAll", t));
        }

        /// <summary>Find key in all data types and change it.</summary>
        public static string ChangeKey(string oldKey, string newKey)
        {
            foreach (var data in Data.DataDict.Values)
                data.ChangeKey(oldKey, newKey);
            return newKey;
        }

        /// <summary>Find key in given data and change it.</summary>
        public static string ChangeKey<T>(string oldKey, string newKey)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                return data.ChangeKey(oldKey, newKey);
            }

            Debug.LogError(TypeIsNotSupported("ChangeKey", t));
            return oldKey;
        }

        /// <summary>Find first key with given value and change it to new one.
        /// <para>This operation is much slower that changing by previous key. Use only if performance is not a consideration.</para></summary>
        public static string ChangeKey<T>(T value, string newKey)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                string key = data.KeyByValue(value);
                return data.ChangeKey(key, newKey);
            }

            Debug.LogError(TypeIsNotSupported("ChangeKey<T>", t));
            return default;
        }

        /// <summary>Look up for one key with given value and removes it.<para />
        /// Returns true if key is found.<para />
        /// This operation is slow, don't use it constantly.</summary>
        public static bool RemoveKeyByValue<T>(T value)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                string key = data.KeyByValue(value);
                if (key != default)
                    data.RemoveKey(key);
                return key != default;
            }

            Debug.LogError(TypeIsNotSupported("RemoveKeyByValue", t));
            return false;
        }

        /// <summary>Look up for keys with given value and remove them.<para /> 
        /// Returns true if at least one key is found.<para />
        /// This operation is slow, don't use it constantly.</summary>
        public static bool RemoveKeysByValue<T>(T value)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                List<string> keys = data.KeysByValue(value);
                data.RemoveKeys(keys);
                return keys.Count > 0;
            }

            Debug.LogError(TypeIsNotSupported("RemoveKeysByValue", t));
            return false;
        }

        /// <summary>Find value in this data by key and set new value to it.</summary>
        public static T Set<T>(string key, T value)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                data.SetValue(key, value);
                return value;
            }

            Debug.LogError($"{ClassName}: Trying to Set not supported type ({t.Name})");
            return default;
        }

        /// <summary>Return value of key in this data.</summary>
        public static T Get<T>(string key, T defaultValue = default)
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                return (T)data.GetValue(key, defaultValue);
            }

            Debug.LogError($"{ClassName}: Trying to Get not supported type ({t.Name})");
            return default;
        }

        /// <summary>Returns the count of keys presented in this data.</summary>
        public static int KeysCount<T>()
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                return data.Count;
            }

            Debug.LogError(TypeIsNotSupported("KeysCount", t));
            return 0;
        }

        /// <summary>Returns every existing key in this data. Consider to use it only once.</summary>
        public static string[] AllKeys<T>()
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                return data.AllKeys(t);
            }

            Debug.LogError(TypeIsNotSupported("GetAllKeys", t));
            return null;
        }

        /// <summary>Returns every existing value of this type. It's a very slow operation, consider to use it only once.</summary>
        public static List<T> AllValues<T>()
        {
            Type t = typeof(T);
            if (Data.DataDict.TryGetValue(t, out IData data))
            {
                List<object> tempValues = data.AllValues(t);
                List<T> allValues = new List<T>(tempValues.Count);
                allValues.AddRange(tempValues.Select(v => (T)v));
                return allValues;
            }

            Debug.LogError(TypeIsNotSupported("GetAllValues", t));
            return null;
        }

        #endregion

        private static void Initialize()
        {
#if UNITY_EDITOR
            onSaveFinish += () => Debug.Log("Local Data || Save Finished");
            onLoadFinish += () => Debug.Log("Local Data || Load Finished");
            onLoadError += path => Debug.Log($"Local Data || Load Error :: \"{path}\"");
#endif

            _data = new SaveData();
            _isInitialized = true;
            LoadFromFile(DEFAULT_FILE_NAME, _data.enableEncryption);

            if (Application.isPlaying && _data.autoSave)
            {
                Application.focusChanged += SaveOnFocusChange;
                Application.wantsToQuit += SaveOnQuit;
            }
        }

        private static void SaveOnFocusChange(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save();
            }
        }

        private static bool SaveOnQuit()
        {
            Save();
            return true;
        }

        private static string TypeIsNotSupported(string methodName, Type t)
            => $"{ClassName} {methodName}: Type \"{t.Name}\" is not supported.";

        private static string ClassName => nameof(LocalData);
    }
}