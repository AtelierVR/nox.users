#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nox.CCK.Network;
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
		private VisualElement                                  _manualConnect;
		private TextField                                      _manualInput;
		private Button                                         _manualButton;
		private readonly List<(string address, Button button)> _items = new();

		public ServerListInput(VisualElement root, AuthentificationInstance panel) {
			_panel         = panel;
			_container     = root.Q<VisualElement>("servers-sidebar");
			_search        = root.Q<TextField>("server-search");
			_scroll        = root.Q<ScrollView>("server-scroll");
			_empty         = root.Q<Label>("servers-empty");
			_manualConnect = root.Q<VisualElement>("manual-connect");
			_manualInput   = root.Q<TextField>("manual-server-input");
			_manualButton  = root.Q<Button>("manual-connect-btn");
			_search.RegisterCallback<ChangeEvent<string>>(OnSearchChanged);
			_manualButton.RegisterCallback<ClickEvent>(OnManualConnect);
			_manualInput.RegisterCallback<KeyUpEvent>(OnManualInputKeyUp);
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

		private void OnManualInputKeyUp(KeyUpEvent evt) {
			if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
				OnManualConnect();
		}

		private void OnManualConnect(ClickEvent evt = null) {
			OnManualConnect();
		}

		private async void OnManualConnect() {
			var input = _manualInput?.value?.Trim();
			if (string.IsNullOrEmpty(input)) return;

			_manualButton.SetEnabled(false);
			_manualInput.SetEnabled(false);

			try {
				var gateway = await NodeDiscover.GetGateway(input);
				// Use gateway as the server address for the login flow
				_panel.Login.SetServer(new ManualServer(input));
				_panel.Login.SetActive(true);
			} catch (Exception e) {
				Logger.LogError(new Exception($"Failed to connect to {input}", e));
				_manualButton.SetEnabled(true);
				_manualInput.SetEnabled(true);
			}
		}

		private async UniTask RefreshAsync() {
			_items.Clear();
			_scroll.Clear();
			_empty.style.display = DisplayStyle.None;

			var addresses = AuthServerComponent.GetAuthenticationServers().ToArray();
			if (addresses.Length == 0) {
				_empty.style.display = DisplayStyle.Flex;
			} else {
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

			// Always show the manual connect section
			_manualConnect.style.display = DisplayStyle.Flex;
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
			_manualButton?.UnregisterCallback<ClickEvent>(OnManualConnect);
			_manualInput?.UnregisterCallback<KeyUpEvent>(OnManualInputKeyUp);
			_items.Clear();
			_panel         = null;
			_container     = null;
			_search        = null;
			_scroll        = null;
			_empty         = null;
			_manualConnect = null;
			_manualInput   = null;
			_manualButton  = null;
		}
	}

	/// <summary>
	/// Minimal IServer implementation for manually-entered server addresses.
	/// </summary>
	internal class ManualServer : IServer {
		private readonly string _address;
		public ManualServer(string address) => _address = address;

		public string Id            => _address;
		public string Address       => _address;
		public IServerGateway       Gateway      => null;
		public IServerMetadata      Metadata     => null;
		public IServerVersions      Versions     => null;
		public IServerEndpoints     Endpoints    => null;
		public string[]             Features     => Array.Empty<string>();
		public string[]             Capabilities => Array.Empty<string>();
		public IServerSoftware      Software     => null;
		public DateTime             ReadyAt      => DateTime.UtcNow;
		public string               PublicKey    => null;
		public int                  Port         => 0;
		public string               Status       => "online";
		public string               Maintenance  => null;

		public string GetAddress() => _address;
		public string GetTitle()   => _address;
	}
}
#endif
