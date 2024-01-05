using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.Editor {
    public class ReanimatorSearchWindow : ScriptableObject, ISearchWindowProvider {
        private ReanimatorGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(ReanimatorGraphView graphView) {
            this.graphView = graphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return new List<SearchTreeEntry>() {
                new SearchTreeGroupEntry(new GUIContent("Nodes")),
                new SearchTreeEntry(new GUIContent("Switch", indentationIcon)) { level = 1, userData = "Switch" },
                new SearchTreeEntry(new GUIContent("SimpleAnimation", indentationIcon)) { level = 1, userData = "SimpleAnimation" },
                new SearchTreeEntry(new GUIContent("MirroredAnimation", indentationIcon)) { level = 1, userData = "MirroredAnimation" },
            };
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData) {
                case "Switch": {
                        graphView.AddElement(graphView.CreateNode("Switch", localMousePosition));
                        return true;
                    }

                case "SimpleAnimation": {
                        graphView.AddElement(graphView.CreateNode("SimpleAnimation", localMousePosition));
                        return true;
                    }

                case "MirroredAnimation": {
                        graphView.AddElement(graphView.CreateNode("MirroredAnimation", localMousePosition));
                        return true;
                    }

                default: {
                        return false;
                    }
            }
        }
    }
}
