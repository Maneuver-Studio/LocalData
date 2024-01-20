using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maneuver.LocalData
{
    [Serializable]
    public class SaveData
    {
        public bool enableEncryption = true;
        public bool autoSave = true;

        private readonly Dictionary<Type, IData> _dataDict = new Dictionary<Type, IData>();
        public Dictionary<Type, IData> DataDict => _dataDict;

        public DataString strings = new DataString();
        public DataBool bools = new DataBool();
        public DataInt ints = new DataInt();
        public DataFloat floats = new DataFloat();
        public DataVector2 vector2 = new DataVector2();
        public DataVector3 vector3 = new DataVector3();

        public SaveData()
        {
            AddData(strings);
            AddData(bools);
            AddData(ints);
            AddData(floats);
            AddData(vector2);
            AddData(vector3);
        }

        public bool IsDirty()
        {
            return _dataDict.Values.Any(data => data.isDirty);
        }

        private void AddData(IData data)
        {
            _dataDict.Add(data.Type, data);
        }
    }
}