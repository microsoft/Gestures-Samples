using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Microsoft.Gestures.UnitySdk;
using System.Linq;
using System;
using System.Xml;

namespace Microsoft.Gestures.Toolkit
{
    [CustomEditor(typeof(GestureTrigger))]
    public class GestureSendMessageEditor : Editor
    {
        private const string SegmentsTagName = "Gesture.Segments";
        private const string NameAttribute = "Name";
        private bool _xamlFolded = false;

        /// <summary>
        /// Returns an array of the gesture segments available in the specified gesture XAML.
        /// </summary>
        private string[] GetGestureSegments(string gestureXAML)
        {
            var gestureSegments = new string[0];

            var doc = new XmlDocument();
            doc.LoadXml(gestureXAML);
            var segmentsElement = doc.GetElementsByTagName(SegmentsTagName).OfType<XmlElement>().FirstOrDefault();
            if (segmentsElement != null)
            {
                var segmentElements = segmentsElement.ChildNodes.OfType<XmlElement>();
                var segmentsQuery = from element in segmentElements
                                    where element.HasAttribute(NameAttribute)
                                    select element.GetAttribute(NameAttribute);
                gestureSegments = segmentsQuery.ToArray();
            }

            return gestureSegments;
        }

        public override void OnInspectorGUI()
        {
            var trigger = target as GestureTrigger;

            var isCustomGesture = trigger.IsCustomGesture;
            var gestureXaml = (trigger.GestureXaml ?? string.Empty).Trim();
            var stockGesture = trigger.Gesture;

            // Gesture Definition
            var text = new[] { "Stock Gesture", "XAML Gesture" };
            isCustomGesture = (GUILayout.SelectionGrid(isCustomGesture ? 1 : 0, text, 2, EditorStyles.radioButton) == 1);
            var gestureSegments = new string[0];

            if (isCustomGesture)
            {
                EditorStyles.textField.wordWrap = true;
                _xamlFolded = EditorGUILayout.Foldout(_xamlFolded, "Gesture XAML:");
                if (_xamlFolded) gestureXaml = EditorGUILayout.TextArea(gestureXaml);

                try
                {
                     gestureSegments = GetGestureSegments(gestureXaml);
                }
                catch (Exception ex)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox("Gesture XAML is invalid!" + Environment.NewLine + ex.Message, MessageType.Warning, true);
                    GUI.color = Color.white;
                }
            }
            else
            {
                stockGesture = (StockGestures)EditorGUILayout.EnumPopup("Stock Gesture", stockGesture);
            }
            
            // Whole gesture trigger Invocation event
            var onTriggerProperty = serializedObject.FindProperty("OnTrigger");
            EditorGUILayout.PropertyField(onTriggerProperty);
            

            var removeIndicies = new List<int>();
            // Gesture's segments trigger Invocation events
            for (int i = 0; i < trigger.SegmentTriggers.Length; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                var segmentTrigger = trigger.SegmentTriggers[i];

                var segmentIndex = Array.IndexOf(gestureSegments, segmentTrigger.Segment);
                segmentIndex = EditorGUILayout.Popup("Segment #" + (i + 1) + ": ", segmentIndex, gestureSegments);
                segmentTrigger.Segment = segmentIndex < 0 ? string.Empty : gestureSegments[segmentIndex];
                if (GUILayout.Button("-", GUILayout.Width(16), GUILayout.Height(14))) removeIndicies.Add(i);
                EditorGUILayout.EndHorizontal();

                var listProperty = serializedObject.FindProperty("SegmentTriggers");
                var segmentTriggerProp = listProperty.GetArrayElementAtIndex(i);
                var eventProperty = segmentTriggerProp.FindPropertyRelative("OnTrigger");
                EditorGUILayout.PropertyField(eventProperty);
                EditorGUILayout.Space();
            }

            // Manage SegmentTriggers array add and remove operations.
            var segmentTriggers = new List<GestureTrigger.SegmentTrigger>(trigger.SegmentTriggers);

            for (int i = removeIndicies.Count - 1; i >= 0; i--) segmentTriggers.RemoveAt(removeIndicies[i]);
            if (GUILayout.Button("+ Add Gesture Segment Event")) segmentTriggers.Add(new GestureTrigger.SegmentTrigger());
            trigger.SegmentTriggers = segmentTriggers.ToArray();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RegisterCompleteObjectUndo(trigger, "Trigger Change");
                EditorUtility.SetDirty(trigger);
                trigger.IsCustomGesture = isCustomGesture;
                trigger.GestureXaml = gestureXaml;
                trigger.Gesture = stockGesture;
            }
        }
    } 
}