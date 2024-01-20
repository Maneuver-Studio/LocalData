using System.IO;
using UnityEditor;
using UnityEngine;
using Maneuver.LocalData;

namespace Maneuver.LocalData.Editor
{
    public static class CleanHelperMenu
    {
        [MenuItem("Maneuver/Clean/Clean PlayerPrefs")]
        public static void CleanPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [MenuItem("Maneuver/Clean/Clean PersistentDataPath Files")]
        public static void CleanFiles()
        {
            if (Directory.Exists(Application.persistentDataPath))
            {
                Directory.Delete(Application.persistentDataPath, true);
            }
        }

        [MenuItem("Maneuver/Clean/Clean LocalData")]
        public static void CleanLocalData()
        {
            LocalData.DeleteAll();
            LocalData.Save();
        }

        [MenuItem("Maneuver/Clean/Clean All")]
        public static void CleanAll()
        {
            CleanPlayerPrefs();
            CleanFiles();
            Debug.Log("Cleaned All Up!");
        }
    }
}