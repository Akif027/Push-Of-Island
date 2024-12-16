using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject playUpperPanel;
    [SerializeField] private GameObject playLowerPanel;
    [SerializeField] private GameObject playAttackLowerPanel;
    [SerializeField] private GameObject pausePanel;
    //  [SerializeField] private GameObject hiringPanel;
    [SerializeField] private GameObject infoPanel;
    public Button OkButton;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    public void OpenPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("The panel you tried to open is not assigned.");
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("The panel you tried to close is not assigned.");
        }
    }

    // Helper methods for each specific panel
    public void OpenPlayUpperPanel() => OpenPanel(playUpperPanel);
    public void ClosePlayUpperPanel() => ClosePanel(playUpperPanel);

    public void OpenPlayLowerPanel() => OpenPanel(playLowerPanel);
    public void ClosePlayLowerPanel() => ClosePanel(playLowerPanel);

    public void OpenPlayAttackLowerPanel() => OpenPanel(playAttackLowerPanel);
    public void ClosePlayAttackLowerPanel() => ClosePanel(playAttackLowerPanel);

    public void OpenPausePanel() => OpenPanel(pausePanel);
    public void ClosePausePanel() => ClosePanel(pausePanel);

    // public void OpenHiringPanel() => OpenPanel(hiringPanel);
    //  public void CloseHiringPanel() => ClosePanel(hiringPanel);

    public void OpenInfoPanel() => OpenPanel(infoPanel);
    public void CloseInfoPanel() => ClosePanel(infoPanel);



    // Method to exit the application
    public void QuitApplication()
    {
        // Log to indicate the application is quitting (useful for debugging in the Editor)
        Debug.Log("Exiting application...");

        // Quit the application
        Application.Quit();

        // Optionally, stop playing in the editor if you're testing in Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
