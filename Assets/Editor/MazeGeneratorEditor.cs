using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
	private SerializedProperty roomDim = null;

	private SerializedProperty floor = null;
	private SerializedProperty wall = null;
	private SerializedProperty ceiling = null;
	private SerializedProperty endPoint = null;

	private MazeGenerator mazeGenerator = null;

	private void OnEnable()
	{
		roomDim = serializedObject.FindProperty("roomDim");

		floor = serializedObject.FindProperty("floor");
		wall = serializedObject.FindProperty("wall");
		ceiling = serializedObject.FindProperty("ceiling");
		endPoint = serializedObject.FindProperty("endPoint");

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

		GUI.enabled = false;
		EditorGUILayout.EnumPopup("Maze generation state", mazeGenerator.state);
		GUI.enabled = true;

		if (GUILayout.Button("Step"))
			((MazeGenerator)target).Step();

		serializedObject.ApplyModifiedProperties();
	}
}
