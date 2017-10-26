using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu = null;
    [SerializeField] private GameObject _editorUI = null;

    public void SetPauseMenuEnabled(bool enabled)
    {
        _pauseMenu.SetActive(enabled);
    }

    public void SetEditorGUIEnabled(bool enabled)
    {
        _editorUI.SetActive(enabled);
    }
}
