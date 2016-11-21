using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class PrefabsObject : ScriptableObject
{
    public List<PrefabParams> allPrefabs;
}

[System.Serializable]
public class PrefabParams
{
    public string name;
    public GameObject prefab;
}
