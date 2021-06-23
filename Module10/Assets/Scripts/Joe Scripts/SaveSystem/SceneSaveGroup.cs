using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneSaveGroup
{
    [SerializeField] private string         groupName;
    [SerializeField] private List<string>   sceneNames;
}
