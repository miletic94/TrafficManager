using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Splines
{
    public static class SplineEditorToolbarExtension
    {
        public static bool HasSelection()
        {
            return SplineSelection.HasActiveSplineSelection();
        }

        public struct SelectedSplineElementInfo
        {
            public UnityEngine.Object target;
            public int targetIndex;
            public int knotIndex;

            public SelectedSplineElementInfo(UnityEngine.Object Object, int Index, int knot)
            {
                target = Object;
                targetIndex = Index;
                knotIndex = knot;
            }

            public static List<SelectedSplineElementInfo> GetSelection()
            {
                List<SelectableSplineElement> elements = SplineSelection.selection;
                List<SelectedSplineElementInfo> infos = new List<SelectedSplineElementInfo>();

                foreach (SelectableSplineElement element in elements)
                {
                    infos.Add(new SelectedSplineElementInfo(element.target, element.targetIndex, element.knotIndex));
                }

                return infos;
            }
        }
    }
}
