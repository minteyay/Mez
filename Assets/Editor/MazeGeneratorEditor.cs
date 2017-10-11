﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
	private SerializedProperty roomDim = null;

	private SerializedProperty floor = null;
	private SerializedProperty wall = null;
	private SerializedProperty ceiling = null;
	private SerializedProperty endPoint = null;

	private SerializedProperty stepThrough = null;
	private SerializedProperty state = null;
	private MazeGenerator mazeGenerator = null;

	private void OnEnable()
	{
		roomDim = serializedObject.FindProperty("roomDim");

		floor = serializedObject.FindProperty("floor");
		wall = serializedObject.FindProperty("wall");
		ceiling = serializedObject.FindProperty("ceiling");
		endPoint = serializedObject.FindProperty("endPoint");

		stepThrough = serializedObject.FindProperty("stepThrough");
		state = serializedObject.FindProperty("_state");
		mazeGenerator = (MazeGenerator)target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(roomDim);
		EditorGUILayout.PropertyField(floor);
		EditorGUILayout.PropertyField(wall);
		EditorGUILayout.PropertyField(ceiling);
		EditorGUILayout.PropertyField(endPoint);

		GUILayout.Space(10.0f);

		EditorGUILayout.PropertyField(stepThrough, new GUIContent("Manual step-through"));
		if (stepThrough.boolValue)
		{
			GUI.enabled = false;
			EditorGUILayout.PropertyField(state, new GUIContent("Maze generation state"));
			GUI.enabled = true;

			MazeGenerator.GenerationState mazeGenState = (MazeGenerator.GenerationState)state.intValue;

			if (mazeGenState == MazeGenerator.GenerationState.Idle)
				GUI.enabled = false;
			if (GUILayout.Button("Step"))
				((MazeGenerator)target).Step();
			GUI.enabled = true;

			if (mazeGenerator.currentSprawlerRuleset != null)
			{
				EditorGUILayout.LabelField("Current sprawler ruleset", EditorStyles.boldLabel);
				EditorGUILayout.TextArea(mazeGenerator.currentSprawlerRuleset.ToString());
			}

			if (mazeGenerator.messageLog != null && mazeGenerator.messageLog.Count > 0)
			{
				EditorGUILayout.LabelField("Sprawler events", EditorStyles.boldLabel);

				string messageLog = "";
				string[] sprawlerMessages = mazeGenerator.messageLog.ToArray();
				for (int i = 0; i < sprawlerMessages.Length; i++)
				{
					messageLog += sprawlerMessages[i];
					if (i < (sprawlerMessages.Length - 1))
						messageLog += '\n';
				}
				EditorGUILayout.TextArea(messageLog.ToString());
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}