﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace GameplayIngredients.Editor
{
    internal static class AdvancedHierarchyPreferences
    {
        [SettingsProvider]
        public static SettingsProvider GetAdvancedHierarchyPreferences()
        {
            var provider = new SettingsProvider("Preferences/Gameplay Ingredients/Advanced Hierarchy View", SettingsScope.User)
            {
                label = "Advanced Hierarchy Options",
                guiHandler = OnGUI
            };
            return provider;
        }

        private static Dictionary<Type, bool> s_CachedVisibility;
        private const string componentPrefix = "GameplayIngredients.HierarchyHints.";
        private const string staticPref = "GameplayIngredients.HierarchyHints.Static";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (s_CachedVisibility == null)
                s_CachedVisibility = new Dictionary<Type, bool>();

            foreach (var type in AdvancedHierarchyView.allTypes)
            {
                if (!s_CachedVisibility.ContainsKey(type))
                    s_CachedVisibility.Add(type, EditorPrefs.GetBool(componentPrefix + type.Name, true));
                else
                    s_CachedVisibility[type] = EditorPrefs.GetBool(componentPrefix + type.Name, true);
            }
        }

        public static bool showStatic => EditorPrefs.GetBool(staticPref, true);

        public static bool IsVisible(Type t) => 
            s_CachedVisibility.ContainsKey(t) && s_CachedVisibility[t];

        private static void OnGUI(string search)
        {
            EditorGUIUtility.labelWidth = 260;
            EditorGUI.indentLevel ++;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preferences", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            var s = EditorGUILayout.Toggle("Show Static", showStatic);
            if(EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(staticPref, s);
                EditorApplication.RepaintHierarchyWindow();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope())
            {

                GUILayout.Label("Visible Components", EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("All"))
                    ToggleAll(true);
                if (GUILayout.Button("None"))
                    ToggleAll(false);
                if (GUILayout.Button("Invert"))
                    ToggleInvert();
            }

            EditorGUI.indentLevel ++;
            foreach (var type in AdvancedHierarchyView.allTypes)
            {
                EditorGUI.BeginChangeCheck();
                var value = EditorGUILayout.Toggle(type.Name, s_CachedVisibility[type]);
                if(EditorGUI.EndChangeCheck())
                {
                    SetValue(type, value, true);
                }
            }
            EditorGUI.indentLevel -= 2;
        }

        private static void SetValue(Type type, bool value, bool repaint = false)
        {
            s_CachedVisibility[type] = value;
            EditorPrefs.SetBool(componentPrefix + type.Name, value);
            if(repaint)
                EditorApplication.RepaintHierarchyWindow();

        }

        private static void ToggleAll(bool value)
        {
            var allTypes = s_CachedVisibility.Keys.ToArray();
            foreach(var type in allTypes) SetValue(type, value);
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void ToggleInvert()
        {
            var allTypes = s_CachedVisibility.Keys.ToArray();
            foreach (var type in allTypes) SetValue(type, !s_CachedVisibility[type]);
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
