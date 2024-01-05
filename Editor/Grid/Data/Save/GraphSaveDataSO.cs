using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.Data.Save
{
    public class GraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<NodeSaveData> Nodes { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Nodes = new List<NodeSaveData>();
        }
    }
}