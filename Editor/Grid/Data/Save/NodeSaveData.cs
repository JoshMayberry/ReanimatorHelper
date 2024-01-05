using System;
using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.Data.Save
{
    [Serializable]
    public class NodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }

        [field: SerializeField] public Vector2 Position { get; set; }
    }
}