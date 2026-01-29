using UnityEngine;
using UnityEngine.UIElements;

public class UISFXListener : MonoBehaviour
{
    private void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) return;

        VisualElement root = doc.rootVisualElement;

        // Écoute TOUS les clics UI
        root.RegisterCallback<ClickEvent>(OnAnyUIClick, TrickleDown.TrickleDown);
    }

    private void OnAnyUIClick(ClickEvent evt)
    {
        // Filtrer uniquement les boutons
        if (evt.target is Button)
        {
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayUIClick();
        }
    }
}
