using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralCreatureGenerator))]
public class ProceduralCreatureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralCreatureGenerator generator = (ProceduralCreatureGenerator)target;
        
        serializedObject.Update();
        
        SerializedProperty statsProp = serializedObject.FindProperty("stats");
        
        if (statsProp != null)
        {
            SerializedProperty seedProp = statsProp.FindPropertyRelative("seed");
            SerializedProperty randomizeSeedProp = statsProp.FindPropertyRelative("randomizeSeed");
            
            // EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(seedProp);
            EditorGUILayout.PropertyField(randomizeSeedProp);
            
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(1f, 0.9f, 0.7f);
            if (GUILayout.Button("Generate from Seed (Randomize Properties)", GUILayout.Height(30)))
            {
                Undo.RecordObject(generator, "Generate Creature from Seed");
                generator.GenerateCreature();
                EditorUtility.SetDirty(generator);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(10);
            
            SerializedProperty prop = statsProp.Copy();
            prop.NextVisible(true);
            
            while (prop.NextVisible(false))
            {
                if (prop.name == "seed" || prop.name == "randomizeSeed")
                    continue;
                    
                EditorGUILayout.PropertyField(prop, true);
            }
        }
        
        SerializedProperty creatureRootProp = serializedObject.FindProperty("creatureRoot");
        if (creatureRootProp != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(creatureRootProp);
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("Generate from Current Properties", GUILayout.Height(35)))
        {
            Undo.RecordObject(generator, "Generate Creature from Properties");
            generator.GenerateFromCurrentStats();
            EditorUtility.SetDirty(generator);
        }
        GUI.backgroundColor = Color.white;
    }
}