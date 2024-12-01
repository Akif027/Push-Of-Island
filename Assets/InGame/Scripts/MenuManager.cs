
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button PlayB;
    [SerializeField] Button RulesB;

    [SerializeField] GameObject RulesPanel;

    void Start()
    {
        PlayB.onClick.AddListener(() => LoadTheScene("GamePlay"));
        // RulesB.onClick.AddListener(RulesInfo);


    }

    void LoadTheScene(string S)
    {

        CustomSceneManager.LoadSceneAsync(S);

    }
    void RulesInfo()
    {

        RulesPanel.SetActive(RulesPanel.gameObject.activeSelf ? true : false);

    }

    void OnDisable()
    {
        PlayB.onClick.RemoveListener(() => LoadTheScene("Mode"));
        RulesB.onClick.RemoveListener(() => LoadTheScene("Rules"));


    }
}
