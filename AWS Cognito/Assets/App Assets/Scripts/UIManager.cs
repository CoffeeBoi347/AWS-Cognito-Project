using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    #region Singleton

    // <summary>: Singleton
    public static UIManager instance { get; set; }

    public static UIManager Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.LogError("UI Manager not initialized! Please assign it from the inspector.");
            }

            return instance;
        }
    }
    #endregion

    public Dictionary<UIPageTypes, CanvasGroup> pageTypeHolder = new();
    public UIPageTypes oldPageType;

    public Transform pageHolder;
    public float fadeDuration;

    #region Initialize Pages
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); 
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        UIPageType[] pages = pageHolder.GetComponentsInChildren<UIPageType>(true);

        foreach(var page in pages)
        {
            var cg = page.GetComponent<CanvasGroup>() ?? page.AddComponent<CanvasGroup>();

            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            if (!pageTypeHolder.ContainsKey(page.pageType))
            {
                pageTypeHolder[page.pageType] = cg;
            }

            if(page.pageType == UIPageTypes.Home)
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    #endregion

    #region Page Operations

    // <summary> : Open a new page by fading in and fading out the old screen.
    public async void OpenPage(UIPageTypes pageToOpen)
    {
        CanvasGroup oldScreen = pageTypeHolder.ContainsKey(oldPageType) && oldPageType != null ? pageTypeHolder[oldPageType] : null;
        CanvasGroup newScreen = pageTypeHolder[pageToOpen];

        if (oldScreen != null && oldScreen != newScreen)
        {
            await FadeOut(oldScreen);
        }

        await FadeIn(newScreen);
        oldPageType = pageToOpen;
    }

    private async Task FadeIn(CanvasGroup pg)
    {
        pg.interactable = true;
        pg.blocksRaycasts = true;

        await pg.DOFade(1, fadeDuration).AsyncWaitForCompletion();
    }

    private async Task FadeOut(CanvasGroup pg)
    {
        pg.interactable = false;
        pg.blocksRaycasts = false;

        await pg.DOFade(0, fadeDuration).AsyncWaitForCompletion();
        pg.alpha = 0;
    }

    #endregion
}