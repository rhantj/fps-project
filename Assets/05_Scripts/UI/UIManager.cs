using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIKey
{
    HUD,
    Menu,
    Setting,
    GameOver,
    FadeOut
}

public class UIManager : MonoBehaviour, IRegistryAdder
{
    [Header("UI Panel")]
    [SerializeField] UIPanel hudUI;
    [SerializeField] UIPanel menuUI;
    [SerializeField] UIPanel settingUI;

    HUDViewModel hudVM;
    MenuViewModel menuVM;
    public SettingViewModel settingVM { get; set; }

    List<UIPanel> rewindList = new();
    Dictionary<UIKey, UIPanel> panels = new();
    Dictionary<UIPanel, UIKey> panelKeys = new();

    void Awake()
    {
        AddRegistry();
        AddtoDictionary();
    }

    void Start()
    {
        var player = StaticRegistry.Find<PlayerController>();
        var gm = StaticRegistry.Find<GameManager>();
        var sm = StaticRegistry.Find<SoundManager>();

        hudVM = new HUDViewModel(player.playerCtx, player.weaponManager);
        menuVM = new MenuViewModel(this, gm);
        settingVM = new SettingViewModel(this, sm);

        Show(UIKey.HUD, true);
        ShowMenu(MenuMode.Start, true);
    }

    public void AddRegistry()
    {
        StaticRegistry.Add(this);
    }

    void AddtoDictionary()
    {
        panels.Clear();
        panelKeys.Clear();

        Add(UIKey.HUD, hudUI);
        Add(UIKey.Menu, menuUI);
        Add(UIKey.Setting, settingUI);
    }

    void Add(UIKey key, UIPanel panel)
    {
        if (!panel) return;
        panels[key] = panel;
        panelKeys[panel] = key;
    }

    public void Show(UIKey key, bool open)
    {
        if (!panels.TryGetValue(key, out var p) || !p) return;

        if (open)
        {
            OpenPanel(key, p);
        }
        else
        {
            CloseTo(p);
        }
    }

    public void ShowMenu(MenuMode mode, bool open = true)
    {
        menuVM?.SetMode(mode);
        Show(UIKey.Menu, open);
    }

    void BindVM(UIKey key, UIPanel panel)
    {
        switch (key)
        {
            case UIKey.HUD:
                if(panel is IBindable<HUDViewModel> hud)
                {
                    hud.Bind(hudVM);
                    hudVM.Activate();
                }
                break;

            case UIKey.Menu:
                if(panel is IBindable<MenuViewModel> menu)
                {
                    menu.Bind(menuVM);
                }
                break;

            case UIKey.Setting:
                if(panel is IBindable<SettingViewModel> setting)
                {
                    setting.Bind(settingVM);
                }
                break;
        }
    }

    void UnbindVM(UIKey key, UIPanel panel)
    {
        switch (key)
        {
            case UIKey.HUD:
                if (panel is IBindable<HUDViewModel> hud)
                {
                    hudVM?.Deactivate();
                    hud.Unbind();
                }
                break;

            case UIKey.Menu:
                if (panel is IBindable<MenuViewModel> menu)
                {
                    menu.Unbind();
                }
                break;

            case UIKey.Setting:
                if (panel is IBindable<SettingViewModel> set)
                {
                    set.Unbind();
                }
                break;
        }
    }

    public bool CloseUpperUI()
    {
        for (int i = rewindList.Count - 1; i >= 0; --i)
        {
            var top = rewindList[i];
            if (!top)
            {
                rewindList.RemoveAt(i);
                continue;
            }

            if (top == hudUI) return false;

            var key = panelKeys[top];
            UnbindVM(key, top);
            top.Close();
            rewindList.RemoveAt(i);
            return true;
        }

        return false;
    }

    void CloseTo(UIPanel target)
    {
        if (!target) return;

        int idx = rewindList.LastIndexOf(target);
        if (idx < 0) return;

        // target 위에 있는 것들부터 닫기
        for (int i = rewindList.Count - 1; i > idx; --i)
        {
            var p = rewindList[i];
            if (!p) { rewindList.RemoveAt(i); continue; }

            if (p == hudUI) continue; // HUD는 닫지 않음

            var key = panelKeys[p];
            UnbindVM(key, p);
            p.Close();
            rewindList.RemoveAt(i);
        }

        if (target != hudUI)
        {
            var key = panelKeys[target];
            UnbindVM(key, target);
            target.Close();
            rewindList.RemoveAt(idx);
        }
    }

    void OpenPanel(UIKey key, UIPanel panel)
    {
        if (panel.IsOpen)
        {
            if (!rewindList.Contains(panel))
                rewindList.Add(panel);
            return;
        }

        BindVM(key, panel);
        panel.Open();

        if (!rewindList.Contains(panel))
            rewindList.Add(panel);
    }
}