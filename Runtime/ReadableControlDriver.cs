using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using Aarthificial.Reanimation.Nodes;
using Aarthificial.Reanimation.Common;
using System.Collections.Generic;

namespace jmayberry.ReanimatorHelper {
    public class ReadableControlDriver : ControlDriver {
        public string GetName() {
            return this.name;
        }
        public void SetName(string value) {
            this.name = value;
        }
        public bool GetAutoIncrement() {
            return this.autoIncrement;
        }
        public void SetAutoIncrement(bool value) {
            this.autoIncrement = value;
        }
        public bool GetPercentageBased() {
            return this.percentageBased;
        }
        public void SetPercentageBased(bool value) {
            this.percentageBased = value;
        }
    }
}
