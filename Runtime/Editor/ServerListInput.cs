#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nox.Servers;
using Nox.Users.Runtime.Clients;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Users.Runtime.Editor {
	public class ServerListInput : IDisposable {
		private AuthentificationInstance                       _panel;
		private VisualElement                                  _container;
		private TextField                                      _search;
		private ScrollView                                     _scroll;
		private Label                                          _empty;
		private readonly List<(string address, Button button)> _items = new();

		public ServerListInput(VisualElement root, AuthentificationInstance panel) {
			_panel     = panel;
			_container = root.Q<VisualElement>("servers-sidebar");
			_search    = root.Q<TextField>("server-search");
			_scroll    = root.Q<ScrollView>("server-scroll");
			_empty     = root.Q<Label>("servers-empty");
			_search.RegisterCallback<ChangeEvent<string>>(OnSearchChanged);
		}

		public void SetActive(bool active) {
			_container.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
			if (active) RefreshAsync().Forget();
		}

		public void Refresh() => RefreshAsync().Forget();

		private void OnSearchChanged(ChangeEvent<string> evt) => FilterItems(evt.newValue);

		private void FilterItems(string query) {
			foreach (var (address, button) in _items) {
				var show = string.IsNullOrEmpty(query)
					|| address.Contains(query, StringComparison.OrdinalIgnoreCase);
				button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		private async UniTask RefreshAsync() {
			_items.Clear();
			_scroll.Clear();
			_empty.style.display = DisplayStyle.None;

			var addresses = AuthServerComponent.GetAuthenticationServers();
			if (addresses.Length == 0) {
				_empty.style.display = DisplayStyle.Flex;
				return;
			}

			foreach (var address in addresses) {
				try {
					var server = await Main.ServerAPI.Fetch(address);
					if (server == null) continue;
					var btn = MakeItem(server);
					_scroll.Add(btn);
					_items.Add((server.GetAddress(), btn));
				} catch (Exception e) {
					Logger.LogError(new Exception($"Failed to fetch server {address}", e));
				}
			}

			if (_items.Count == 0)
				_empty.style.display = DisplayStyle.Flex;
		}

		private Button MakeItem(IServer server) {
			var btn = new Button(() => OnServerClicked(server));
			btn.AddToClassList("server-list-item");
			btn.style.flexDirection    = FlexDirection.Column;
			btn.style.alignItems       = Align.FlexStart;
			btn.style.paddingTop       = 6;
			btn.style.paddingBottom    = 6;
			btn.style.paddingLeft      = 8;
			btn.style.paddingRight     = 8;
			btn.style.marginLeft       = 0;
			btn.style.marginRight      = 0;
			btn.style.marginTop        = 0;
			btn.style.marginBottom     = 0;
			btn.style.width            = new Length(100, LengthUnit.Percent);
			btn.style.borderTopLeftRadius     = 0;
			btn.style.borderTopRightRadius    = 0;
			btn.style.borderBottomLeftRadius  = 0;
			btn.style.borderBottomRightRadius = 0;

			var title = new Label(server.GetTitle() ?? server.GetAddress());
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.overflow     = Overflow.Hidden;
			title.style.textOverflow = TextOverflow.Ellipsis;
			title.style.whiteSpace   = WhiteSpace.NoWrap;

			var address = new Label(server.GetAddress());
			address.style.fontSize   = 9;
			address.style.opacity    = 0.6f;
			address.style.overflow   = Overflow.Hidden;
			address.style.textOverflow = TextOverflow.Ellipsis;
			address.style.whiteSpace = WhiteSpace.NoWrap;

			btn.Add(title);
			btn.Add(address);
			return btn;
		}

		private void OnServerClicked(IServer server) {
			Logger.Log($"[ServerListInput] Server selected: {server.GetAddress()}");
			_panel.Login.SetServer(server);
			_panel.Login.SetActive(true);
		}

		public void Dispose() {
			_search?.UnregisterCallback<ChangeEvent<string>>(OnSearchChanged);
			_items.Clear();
			_panel     = null;
			_container = null;
			_search    = null;
			_scroll    = null;
			_empty     = null;
		}
	}
}
#endif
