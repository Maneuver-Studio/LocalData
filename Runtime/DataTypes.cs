using System;
using UnityEngine;

namespace Maneuver.LocalData
{
    // Need to create subclasses for generic types to serialize them
    [Serializable] public class DataString : Data<string> { }
	[Serializable] public class DataBool : Data<bool> { }
	[Serializable] public class DataInt : Data<int> { }
	[Serializable] public class DataFloat : Data<float> { }
	[Serializable] public class DataVector2 : Data<Vector2> { }
	[Serializable] public class DataVector3 : Data<Vector3> { }
}
