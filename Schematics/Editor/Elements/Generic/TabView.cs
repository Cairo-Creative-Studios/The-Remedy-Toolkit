using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TabView : VisualElement
{
    private VisualElement _tabScrollBar;
    private readonly VisualElement _tabBar;
    private readonly VisualElement _tabContentContainer;
    private readonly Dictionary<string, Tab> _tabContentMap = new();
    private string _selectedTab;
    private string _tabBarName = string.Empty;
    private bool _hideLabels = false;

    public Tab this[string tabID]
    {
        get
        {
            return _tabContentMap[tabID];
        }
    }

    public TabView(string labelText = "", string className = "", bool topScrollBar = false, bool hideTabLabels = false)
    {
        _tabBarName = className.Replace(" ", "");
        _hideLabels = hideTabLabels;

        style.flexDirection = FlexDirection.Column;

        if(!topScrollBar)
        {
            _tabScrollBar = new ScrollView(ScrollViewMode.Horizontal)
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    height = 24
                }
            };
        }
        else
            _tabScrollBar = new FlippedHorizontalScrollView();

        _tabScrollBar.AddToClassList(className + "-tab-scrollbar");

        _tabBar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 0
            }
        };

        _tabScrollBar.Add(_tabBar);
        _tabBar.AddToClassList(className + "-tabbar");

        _tabContentContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1
            }
        };
        _tabContentContainer.AddToClassList(className + "-tab-content");

        Add(_tabScrollBar);
        Add(_tabContentContainer);

        if (className != string.Empty)
        {
            var label = new Label(labelText)
            {
                style = {
                    position = Position.Absolute,
                    right = 0,
                    bottom = 0
                }
            };

            _tabBar.parent.Add(label);

            label.AddToClassList(className + "-tab-label");
        }
    }

    public Tab AddTab(string tabId, Texture icon = null)
    {
        if (_tabContentMap.ContainsKey(tabId))
            throw new ArgumentException($"Tab '{tabId}' already exists.");
        
        var tab = new Tab(this, tabId, icon);
        _tabContentContainer.Add(tab.Content);

        return tab;
    }

    public Button GetTabButton(string tabID)
    {
        return this[tabID].Button;
    }

    public void RemoveTab(string tabId)
    {
        if (!_tabContentMap.ContainsKey(tabId)) return;

        var content = _tabContentMap[tabId];
        _tabContentContainer.Remove(content);
        _tabContentMap.Remove(tabId);

        var tabButton = _tabBar.Q<Button>(name: tabId);
        if (tabButton != null) _tabBar.Remove(tabButton);

        if (_selectedTab == tabId)
        {
            _selectedTab = null;
            if (_tabContentMap.Count > 0)
            {
                var nextTab = new List<string>(_tabContentMap.Keys)[0];
                SelectTab(nextTab);
            }
        }
    }

    public void SelectTab(string tabId)
    {
        if (!_tabContentMap.ContainsKey(tabId)) return;

        foreach (var kvp in _tabContentMap)
        {
            kvp.Value.SetVisible(false);
        }

        _tabContentMap[tabId].SetVisible(true);
        _selectedTab = tabId;

        // Optional: update tab button styles
        foreach (var child in _tabBar.Children())
        {
            child.RemoveFromClassList(_tabBarName + "-tab-selected");
        }

        var selectedButton = _tabBar.Children().ElementAt(new List<string>(_tabContentMap.Keys).IndexOf(tabId));
        selectedButton.AddToClassList(_tabBarName + "-tab-selected");
    }

    public class Tab : VisualElement
    {
        private bool _lazyLoad = false;
        private bool _alreadyLoaded = false;
        private Func<VisualElement> _factory;
        public VisualElement Content;
        public Button Button;

        public Tab(TabView tabView, string tabId, Texture icon = null)
        {
            Content = new VisualElement();

            // === Tab Content ===
            Content.style.display = DisplayStyle.None;
            tabView._tabContentContainer.Add(Content);
            tabView._tabContentMap[tabId] = this;

            // === Tab Button ===
            Button = new Button
            {
                text = "",
                tooltip = tabId,
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,

                    marginLeft = 0,
                    marginRight = 0,

                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    borderBottomWidth = 0,
                    bottom = 0
                }
            };

            if (icon != null)
            {
                var img = new Image { image = icon };
                img.style.width = 16;
                img.style.height = 16;
                //img.style.marginRight = 4;
                Button.Add(img);
            }
            
            if(!tabView._hideLabels || icon == null)
            {
                var label = new Label(tabId)
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold }
                };
                Button.Add(label);
            }

            Button.AddToClassList(tabView._tabBarName + "-tab");
            Button.RegisterCallback<MouseEnterEvent>(_ => Button.AddToClassList(tabView._tabBarName + "-tab-hover"));
            Button.RegisterCallback<MouseLeaveEvent>(_ => Button.RemoveFromClassList(tabView._tabBarName + "-tab-hover"));

            Button.clicked += () =>
            {
                tabView.SelectTab(tabId);

                if (_lazyLoad && !_alreadyLoaded)
                {
                    Content.Clear();
                    Content.Add(_factory());
                    _alreadyLoaded = true;
                }
            };

            tabView._tabBar.Add(Button);

            // Auto-select first tab
            if (tabView._tabContentMap.Count == 1)
                tabView.SelectTab(tabId);

            base.Add(Content);
        }

        public void UseFactory(Func<VisualElement> factory, bool reuseGenerated = true)
        {
            _factory = factory;
            _lazyLoad = reuseGenerated;
        }

        public void SetVisible(bool visible)
        {
            Content.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Set(VisualElement content)
        {
            Content.Clear();
            Content.Add(content);
        }

        public new void Add(VisualElement content)
        {
            Content.Add(content);
        }
    }
}
