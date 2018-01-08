using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _pauseMenu = null;
    [SerializeField] private CanvasGroup _editorUI = null;

    public void SetPauseMenuEnabled(bool enabled)
    {
        _pauseMenu.alpha = (enabled) ? 1.0f : 0.0f;
        _pauseMenu.interactable = enabled;
        _pauseMenu.blocksRaycasts = enabled;
    }

    public void SetEditorGUIEnabled(bool enabled)
    {
        _editorUI.alpha = (enabled) ? 1.0f : 0.0f;
        _editorUI.interactable = enabled;
        _editorUI.blocksRaycasts = enabled;
    }
}
