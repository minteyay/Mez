using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
	private SerializedProperty _tileSize = null;
	private SerializedProperty _entranceLength = null;

	private SerializedProperty _uvPlane = null;
	private SerializedProperty _corridor = null;

	private SerializedProperty _regularShader = null;
	private SerializedProperty _seamlessShader = null;

	private SerializedProperty _stepThrough = null;
	private SerializedProperty _state = null;
	private MazeGenerator _mazeGenerator = null;

	private void OnEnable()
	{
		_tileSize = serializedObject.FindProperty("_tileSize");
		_entranceLength = serializedObject.FindProperty("_entranceLength");

		_uvPlane = serializedObject.FindProperty("_uvPlane");
		_corridor = serializedObject.FindProperty("_corridor");

		_regularShader = serializedObject.FindProperty("_regularShader");
		_seamlessShader = serializedObject.FindProperty("_seamlessShader");

		_stepThrough = serializedObject.FindProperty("_stepThrough");
		_state = serializedObject.FindProperty("_state");
		_mazeGenerator = (MazeGenerator)target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(_tileSize);
		EditorGUILayout.PropertyField(_entranceLength);
		EditorGUILayout.PropertyField(_uvPlane);
		EditorGUILayout.PropertyField(_corridor);
		EditorGUILayout.PropertyField(_regularShader);
		EditorGUILayout.PropertyField(_seamlessShader);

		GUILayout.Space(10.0f);

		EditorGUILayout.PropertyField(_stepThrough, new GUIContent("Manual step-through"));
		if (_stepThrough.boolValue)
		{
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_state, new GUIContent("Maze generation state"));
			GUI.enabled = true;

			MazeGenerator.State mazeGenState = (MazeGenerator.State)_state.intValue;

			if (mazeGenState == MazeGenerator.State.Idle)
				GUI.enabled = false;
			if (GUILayout.Button("Step"))
				((MazeGenerator)target).Step();
			GUI.enabled = true;

			if (_mazeGenerator.currentSprawlerRuleset != null)
			{
				EditorGUILayout.LabelField("Current sprawler ruleset", EditorStyles.boldLabel);
				EditorGUILayout.TextArea(_mazeGenerator.currentSprawlerRuleset.ToString());
			}

			if (_mazeGenerator.messageLog != null && _mazeGenerator.messageLog.Count > 0)
			{
				EditorGUILayout.LabelField("Sprawler events", EditorStyles.boldLabel);

				string messageLog = "";
				string[] sprawlerMessages = _mazeGenerator.messageLog.ToArray();
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
