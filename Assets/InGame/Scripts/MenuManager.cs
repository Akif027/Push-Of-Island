
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button PlayB;
    [SerializeField] Button RulesB;




    void Start()
    {
        PlayB.onClick.AddListener(() => LoadTheScene("Mode"));
        RulesB.onClick.AddListener(() => LoadTheScene("Rules"));
      

    }

    void LoadTheScene(string S)
    {
   
        CustomSceneManager.LoadSceneAsync(S);

    }


    void OnDisable()
    {
        PlayB.onClick.RemoveListener(() => LoadTheScene("Mode"));
        RulesB.onClick.RemoveListener(() => LoadTheScene("Rules"));
      

    }
}
