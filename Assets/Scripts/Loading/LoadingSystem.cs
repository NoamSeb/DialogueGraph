using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSystem : MonoBehaviour
{
    public static LoadSceneMode LastLoadMode { get; private set; }

    #region Settings

    [Header("Loading Settings")]
    [Dropdown("buildSettingsSceneList")]
    public string exampleScene;
    List<string> buildSettingsSceneList = new List<string>();
    private List<string> Scenes { get { return buildSettingsSceneList; } }
    [SerializeField] private LoadSceneMode _exampleLoadSceneMode = LoadSceneMode.Single;

    #endregion
    
    [Button]
    public void LoadSceneFromEditor()
    {
        LoadScene(exampleScene, _exampleLoadSceneMode);
    }
    
    [Space]
    [Header("Loading Attributes")]
    [SerializeField] private Animator _loadingScreen;
    
    private void OnValidate()
    {
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        
        for (int i = 0; i < totalScenes; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Scenes.Add(sceneName);
        }
        
    }
    

    private void Start()
    {
        if (LastLoadMode == LoadSceneMode.Single || LastLoadMode == null)
        {
            _loadingScreen.SetTrigger("IsEnteringSingle");
            Debug.Log("J'ai été Load en Single");
        }
        else
        {
            _loadingScreen.SetTrigger("IsEnteringAdditive");
            Debug.Log("J'ai été Load en Additive");
        }
    }

    private void LoadScene(string SceneToLoad, LoadSceneMode LoadSceneMode = LoadSceneMode.Single)
    {
        if (LoadSceneMode == LoadSceneMode.Single)
        {
            _loadingScreen.SetTrigger("IsLoading");
        }

        LastLoadMode = LoadSceneMode;
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode);
    }
}