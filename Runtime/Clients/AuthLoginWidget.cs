using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Utils;
using Nox.Servers;
using Nox.UI;
using Nox.Users.Runtime.Networks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Users.Runtime.Clients {
	public class AuthLoginWidget : IPage {
		internal static string GetStaticKey() => "login";

		public string GetKey() => GetStaticKey();

		private int                  _mId;
		private object[]             _context;
		private GameObject           _content;
		private AuthLoginComponent   _component;

		public object[] GetContext() => _context;

		public IMenu GetMenu() => Client.UiAPI.Get<IMenu>(_mId);

		public GameObject GetContent(RectTransform parent) {
			if (_content) return _content;
			(_content, _component) = AuthLoginComponent.Generate(this, parent);
			return _content;
		}

		internal static IPage OnGotoAction(IMenu menu, object[] context) {
			var page = new AuthLoginWidget {
				_mId     = menu.Id,
				_context = context
			};
			return page;
		}
	}

	public class AuthLoginComponent : MonoBehaviour {
		public AuthLoginWidget Page;

		// Server list (left sidebar)
		public TMP_InputField            searchField;
		public RectTransform             serverList;
		public GameObject                serverInfobox;
		public GameObject                serverListContainer;

		// Address fetch (left sidebar)
		public TMP_InputField addressField;
		public Button         fetchButton;
		public GameObject     fetchErrorBox;
		public TextLanguage   fetchError;

		// Login form (right side)
		public GameObject    loginContainer;
		public GameObject    noServerContainer;
		public TMP_InputField identifierField;
		public TMP_InputField passwordField;
		public Button         loginButton;
		public TextLanguage   loginError;

		private CancellationTokenSource              _serverTokenSource;
		private CancellationTokenSource              _fetchTokenSource;
		private CancellationTokenSource              _loginTokenSource;
		private IServer                              _selectedServer;
		private readonly List<(IServer, ServerItemComponent)> _items = new();

		// ── Server list ──────────────────────────────────────────────

		private void FilterServers(string query) {
			foreach (var (server, item) in _items) {
				var visible = string.IsNullOrEmpty(query)
					|| server.GetTitle()?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
					|| server.GetAddress().Contains(query, StringComparison.OrdinalIgnoreCase);
				item.gameObject.SetActive(visible);
			}
		}

		internal async UniTask UpdateServers() {
			if (_serverTokenSource != null) {
				_serverTokenSource.Cancel();
				_serverTokenSource.Dispose();
			}

			_serverTokenSource = new CancellationTokenSource();
			_items.Clear();

			foreach (Transform child in serverList)
				Destroy(child.gameObject);

			serverInfobox.SetActive(true);
			serverListContainer.SetActive(false);

			var isEmpty = true;
			foreach (var address in AuthServerComponent.GetAuthenticationServers()) {
				if (_serverTokenSource.IsCancellationRequested) break;
				try {
					var server = await Main.ServerAPI
						.Fetch(address)
						.AttachExternalCancellation(_serverTokenSource.Token);
					await UniTask.SwitchToMainThread();
					if (server == null || _serverTokenSource.IsCancellationRequested) continue;

					var (_, comp) = ServerItemComponent.Generate(this, serverList);
					comp.UpdateContent(server);
					_items.Add((server, comp));

					if (isEmpty) {
						isEmpty = false;
						serverInfobox.SetActive(false);
						serverListContainer.SetActive(true);
					}

					UpdateLayout.UpdateImmediate(serverList);
				} catch (Exception e) {
					Logger.LogError(new Exception($"Failed to fetch server {address}", e));
				}
			}

			if (isEmpty) {
				serverInfobox.SetActive(true);
				serverListContainer.SetActive(false);
			} else {
				UpdateLayout.UpdateImmediate(serverList);
			}

			_serverTokenSource = null;
		}

		// ── Server selection ─────────────────────────────────────────

		internal void SelectServer(IServer server) {
			_selectedServer = server;
			loginContainer.SetActive(true);
			noServerContainer.SetActive(false);
			identifierField.text = string.Empty;
			passwordField.text   = string.Empty;
			SetLoginEnabled(true);
			identifierField.ActivateInputField();
		}

		// ── Login form ───────────────────────────────────────────────

		private void SetLoginEnabled(bool enabled, string error = null) {
			identifierField.interactable = enabled;
			passwordField.interactable   = enabled;
			loginButton.interactable     = enabled;

			if (string.IsNullOrEmpty(error)) {
				loginError.gameObject.SetActive(false);
			} else {
				loginError.UpdateText("value", new[] { error });
				loginError.gameObject.SetActive(true);
			}
		}

		internal void OnLoginClicked() {
			if (_loginTokenSource != null) {
				_loginTokenSource.Cancel();
				_loginTokenSource.Dispose();
			}

			_loginTokenSource = new CancellationTokenSource();
			LoginAsync().AttachExternalCancellation(_loginTokenSource.Token).Forget();
		}

		private async UniTask LoginAsync() {
			if (!loginButton.interactable) return;

			if (_selectedServer == null) {
				SetLoginEnabled(true, "No server selected.");
				return;
			}

			var identifier = identifierField.text.Trim();
			var password   = passwordField.text;

			if (string.IsNullOrEmpty(identifier)) {
				SetLoginEnabled(true, "Identifier cannot be empty.");
				identifierField.ActivateInputField();
				return;
			}

			if (string.IsNullOrEmpty(password)) {
				SetLoginEnabled(true, "Password cannot be empty.");
				passwordField.ActivateInputField();
				return;
			}

			SetLoginEnabled(false);

			var request = new LoginRequest {
				Identifier = identifier,
				Password   = password,
				PublicKey  = Crypto.CompressPublicKey(Crypto.GetKeys())
			};

			var result = await Main.Instance.Network.Login(request, _selectedServer.GetAddress());

			if (result == null) {
				SetLoginEnabled(true, "Login failed. Please try again.");
				return;
			}

			if (result.IsError()) {
				SetLoginEnabled(true, result.Error);
				passwordField.ActivateInputField();
				return;
			}

			if (result.IsVerificationRequired()) {
				SetLoginEnabled(true, "Verification required — not yet supported in client.");
				return;
			}

			SetLoginEnabled(true);
			Client.UiAPI?.SendAction(Page.GetMenu().Id, "back");
		}

		private void OnRefreshClicked() => UpdateServers().Forget();

		internal void OnSearchSubmitClicked() {
			if (_fetchTokenSource != null) {
				_fetchTokenSource.Cancel();
				_fetchTokenSource.Dispose();
			}
			_fetchTokenSource = new CancellationTokenSource();
			FetchServerByAddressAsync(searchField.text.Trim())
				.AttachExternalCancellation(_fetchTokenSource.Token).Forget();
		}

		internal void OnFetchServerByAddressClicked() {
			if (_fetchTokenSource != null) {
				_fetchTokenSource.Cancel();
				_fetchTokenSource.Dispose();
			}
			_fetchTokenSource = new CancellationTokenSource();
			FetchServerByAddressAsync(addressField.text.Trim())
				.AttachExternalCancellation(_fetchTokenSource.Token).Forget();
		}

		private void SetFetchEnabled(bool enabled, string error = null) {
			addressField.interactable = enabled;
			fetchButton.interactable  = enabled;
			if (string.IsNullOrEmpty(error)) {
				fetchErrorBox.SetActive(false);
			} else {
				fetchError.UpdateText("value", new[] { error });
				fetchErrorBox.SetActive(true);
			}
		}

		private async UniTask FetchServerByAddressAsync(string address) {
			if (string.IsNullOrEmpty(address)) return;

			// Already in list — just select it
			foreach (var (server, _) in _items) {
				if (string.Equals(server.GetAddress(), address, StringComparison.OrdinalIgnoreCase)) {
					SetFetchEnabled(true);
					SelectServer(server);
					return;
				}
			}

			SetFetchEnabled(false);

			try {
				var server = await Main.ServerAPI
					.Fetch(address)
					.AttachExternalCancellation(_fetchTokenSource.Token);
				await UniTask.SwitchToMainThread();

				if (server == null) {
					SetFetchEnabled(true, $"Server '{address}' not found.");
					return;
				}

				var (_, comp) = ServerItemComponent.Generate(this, serverList);
				comp.UpdateContent(server);
				_items.Add((server, comp));

				serverInfobox.SetActive(false);
				serverListContainer.SetActive(true);
				UpdateLayout.UpdateImmediate(serverList);

				SetFetchEnabled(true);
				SelectServer(server);
			} catch (OperationCanceledException) {
				SetFetchEnabled(true);
			} catch (Exception e) {
				Logger.LogError(new Exception($"Failed to fetch server {address}", e));
				SetFetchEnabled(true, "Failed to reach server.");
			}

			_fetchTokenSource = null;
		}

		private void OnDestroy() {
			_serverTokenSource?.Cancel();
			_serverTokenSource?.Dispose();
			_fetchTokenSource?.Cancel();
			_fetchTokenSource?.Dispose();
			_loginTokenSource?.Cancel();
			_loginTokenSource?.Dispose();
		}

		// ── Generate ─────────────────────────────────────────────────

		public static (GameObject, AuthLoginComponent) Generate(AuthLoginWidget page, RectTransform parent) {
			var content   = Instantiate(Client.GetAsset<GameObject>("ui:prefabs/split.prefab"), parent);
			content.name  = $"[{page.GetKey()}_{content.GetEntityId().GetHashCode()}]";
			var component = content.AddComponent<AuthLoginComponent>();
			component.Page = page;

			var splitContent       = Reference.GetComponent<RectTransform>("content", content);
			var containerAsset     = Client.GetAsset<GameObject>("ui:prefabs/container.prefab");
			var containerFullAsset = Client.GetAsset<GameObject>("ui:prefabs/container_full.prefab");
			var withSearchAsset    = Client.GetAsset<GameObject>("ui:prefabs/with_search.prefab");
			var withTitleAsset     = Client.GetAsset<GameObject>("ui:prefabs/with_title.prefab");
			var scrollAsset        = Client.GetAsset<GameObject>("ui:prefabs/scroll.prefab");
			var listAsset          = Client.GetAsset<GameObject>("ui:prefabs/list.prefab");
			var infoboxAsset       = Client.GetAsset<GameObject>("ui:prefabs/infobox.prefab");
			var iconAsset          = Client.GetAsset<GameObject>("ui:prefabs/header_icon.prefab");
			var labelAsset         = Client.GetAsset<GameObject>("ui:prefabs/header_label.prefab");
			var headerButtonAsset  = Client.GetAsset<GameObject>("ui:prefabs/header_button.prefab");
			var textAsset          = Client.GetAsset<GameObject>("ui:prefabs/text.prefab");

			// ── LEFT: server list sidebar ─────────────────────────────
			var leftContainer = Instantiate(containerAsset, splitContent);
			var withSearch    = Instantiate(
				withSearchAsset,
				Reference.GetComponent<RectTransform>("content", leftContainer)
			);
			var searchHeader = Reference.GetReference("header", withSearch);

			// Search field — filter + submit fetches by address
			component.searchField = Reference.GetComponent<TMP_InputField>("input", searchHeader);
			component.searchField.onValueChanged.AddListener(component.FilterServers);
			component.searchField.placeholder.GetComponent<TMP_Text>().text = "Search servers...";
			Reference.GetComponent<Button>("submit", searchHeader)
				.onClick.AddListener(component.OnSearchSubmitClicked);

			var refreshButton = Instantiate(
				headerButtonAsset,
				Reference.GetComponent<RectTransform>("after", searchHeader)
			);
			Reference.GetComponent<Button>("button", refreshButton)
				.onClick.AddListener(component.OnRefreshClicked);
			Reference.GetComponent<Image>("image", refreshButton).sprite =
				Client.GetAsset<Sprite>("ui:icons/refresh.png");

			var leftContent = Reference.GetComponent<RectTransform>("content", withSearch);

			component.serverInfobox = Instantiate(infoboxAsset, leftContent);
			Reference.GetComponent<TextLanguage>("text", component.serverInfobox)
				.UpdateText("auth.no_servers");

			component.serverListContainer = Instantiate(scrollAsset, leftContent);
			var serverListGo = Instantiate(
				listAsset,
				Reference.GetComponent<RectTransform>("content", component.serverListContainer)
			);
			component.serverList = Reference.GetComponent<RectTransform>("content", serverListGo);

			component.serverInfobox.SetActive(true);
			component.serverListContainer.SetActive(false);

			// ── RIGHT: login form ─────────────────────────────────────
			var rightContainer  = Instantiate(containerFullAsset, splitContent);
			var withTitle       = Instantiate(
				withTitleAsset,
				Reference.GetComponent<RectTransform>("content", rightContainer)
			);

			var loginHeader = Reference.GetReference("header", withTitle);
			var loginIcon   = Instantiate(iconAsset, Reference.GetComponent<RectTransform>("before", loginHeader));
			var loginLabel  = Instantiate(labelAsset, Reference.GetComponent<RectTransform>("content", loginHeader));
			Reference.GetComponent<Image>("image", loginIcon).sprite =
				Client.GetAsset<Sprite>("ui:icons/user.png");
			Reference.GetComponent<TextLanguage>("text", loginLabel).UpdateText("auth.login.title");

			var rightContent    = Reference.GetComponent<RectTransform>("content", withTitle);
			var centerAsset     = Client.GetAsset<GameObject>("ui:prefabs/center.prefab");
			var boxAsset        = Client.GetAsset<GameObject>("ui:prefabs/box.prefab");
			var btnIconAsset    = Client.GetAsset<GameObject>("ui:prefabs/btn_icon.prefab");
			var inputFieldAsset = Client.GetAsset<GameObject>("ui:prefabs/input_field.prefab");

			// ── Center 1: Select server (address input) ───────────────
			component.noServerContainer = Instantiate(centerAsset, rightContent);
			var addrBox        = Instantiate(boxAsset, Reference.GetComponent<RectTransform>("content", component.noServerContainer));
			Reference.GetComponent<TextLanguage>("text", addrBox)
				.UpdateText("auth.login.server_address");
			var addrList       = Instantiate(listAsset, Reference.GetComponent<RectTransform>("content", addrBox));
			var addrListContent = Reference.GetComponent<RectTransform>("content", addrList);

			// Fetch error text (hidden)
			component.fetchErrorBox = Instantiate(textAsset, addrListContent);
			component.fetchError    = Reference.GetComponent<TextLanguage>("text", component.fetchErrorBox);
			component.fetchErrorBox.SetActive(false);

			// Address input field
			var addrInput      = Instantiate(inputFieldAsset, addrListContent);
			addrInput.AddComponent<LayoutElement>().preferredHeight = 48f;
			Reference.GetReference("image_container", addrInput)?.SetActive(false);
			component.addressField = Reference.GetComponent<TMP_InputField>("input", addrInput);
			component.addressField.placeholder.GetComponent<TMP_Text>().text = "https://example.com";

			// "Connect" button
			var connectBtn = Instantiate(btnIconAsset, addrListContent);
			Reference.GetReference("image_container", connectBtn)?.SetActive(false);
			Reference.GetComponent<TextLanguage>("text", connectBtn)?.UpdateText("auth.login.connect");
			component.fetchButton = Reference.GetComponent<Button>("button", connectBtn);
			component.fetchButton.onClick.AddListener(component.OnFetchServerByAddressClicked);

			component.noServerContainer.SetActive(true);

			// ── Center 2: Login credentials ───────────────────────────
			component.loginContainer = Instantiate(centerAsset, rightContent);
			var loginBox        = Instantiate(boxAsset, Reference.GetComponent<RectTransform>("content", component.loginContainer));
			Reference.GetComponent<TextLanguage>("text", loginBox).UpdateText("auth.login.title");
			var loginList       = Instantiate(listAsset, Reference.GetComponent<RectTransform>("content", loginBox));
			var loginListContent = Reference.GetComponent<RectTransform>("content", loginList);

			// Login error text (hidden)
			var loginErrorGo = Instantiate(textAsset, loginListContent);
			component.loginError = Reference.GetComponent<TextLanguage>("text", loginErrorGo);
			loginErrorGo.SetActive(false);

			// Identifier input
			var identInput = Instantiate(inputFieldAsset, loginListContent);
			identInput.AddComponent<LayoutElement>().preferredHeight = 40f;
			Reference.GetReference("image_container", identInput)?.SetActive(false);
			component.identifierField = Reference.GetComponent<TMP_InputField>("input", identInput);
			component.identifierField.placeholder.GetComponent<TMP_Text>().text = "Identifier";

			// Password input
			var passwordInput = Instantiate(inputFieldAsset, loginListContent);
			passwordInput.AddComponent<LayoutElement>().preferredHeight = 40f;
			Reference.GetReference("image_container", passwordInput)?.SetActive(false);
			component.passwordField = Reference.GetComponent<TMP_InputField>("input", passwordInput);
			component.passwordField.contentType = TMP_InputField.ContentType.Password;
			component.passwordField.placeholder.GetComponent<TMP_Text>().text = "Password";
			component.passwordField.ForceLabelUpdate();

			// "Log in" button
			var signInBtn = Instantiate(btnIconAsset, loginListContent);
			Reference.GetReference("image_container", signInBtn)?.SetActive(false);
			Reference.GetComponent<TextLanguage>("text", signInBtn)?.UpdateText("auth.login.sign_in");
			component.loginButton = Reference.GetComponent<Button>("button", signInBtn);
			component.loginButton.onClick.AddListener(component.OnLoginClicked);

			component.loginContainer.SetActive(false);

			// Start loading servers
			component.UpdateServers().Forget();

			return (content, component);
		}
	}
}