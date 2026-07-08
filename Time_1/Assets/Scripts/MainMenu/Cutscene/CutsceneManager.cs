using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    private static CutsceneManager _instance;
    public static CutsceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CutsceneManager (auto)");
                _instance = go.AddComponent<CutsceneManager>();
            }
            return _instance;
        }
    }
    [SerializeField] private string cenaMenu = "MainMenu";

    private readonly HashSet<string> vistas = new();

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            _instance = null;
        }
    }
    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (!string.IsNullOrEmpty(cenaMenu) && cena.name == cenaMenu)
            vistas.Clear();
    }

    public bool JaViu(string id) => !string.IsNullOrEmpty(id) && vistas.Contains(id);
    public void MarcarVisto(string id) { if (!string.IsNullOrEmpty(id)) vistas.Add(id); }
    public void Resetar() => vistas.Clear();
}