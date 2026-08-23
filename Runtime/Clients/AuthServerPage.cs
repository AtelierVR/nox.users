using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Network;
using Nox.CCK.Search;
using Nox.CCK.Utils;
using Nox.Servers;
using Nox.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Nox.Users.Runtime.Clients {
	public class AuthServerPage : IPage {
		internal static string GetStaticKey()
			=> "auth";

		public string GetKey()
			=> GetStaticKey();

		private int                 _mId;
		private object[]            _context;
		private GameObject          _content;
		private AuthServerComponent _component;

		internal static IPage OnGotoAction(IMenu menu, object[] context) {
			var page = new AuthServerPage {
				_mId     = menu.Id,
				_context = context
			};
			return page;
		}

		public object[] GetContext()
			=> _context;

		public IMenu GetMenu()
			=> Client.UiAPI.Get<IMenu>(_mId);

		public GameObject GetContent(RectTransform parent) {
			if (_content) return _content;
			(_content, _component) = AuthServerComponent.Generate(this, parent);
			return _content;
		}
	}

	public class AuthServerComponent : MonoBehaviour {
		public AuthServerPage           Page;
		public RectTransform            serverList;
		public GameObject               serverInfobox;
		public GameObject               serverListContainer;
		private CancellationTokenSource _serverTokenSource;

		internal async UniTask UpdateServers() {
			if (_serverTokenSource != null) {
				_serverTokenSource?.Cancel();
				_serverTokenSource?.Dispose();
			}

			_serverTokenSource = new CancellationTokenSource();
			var tasks          = new List<UniTask<IServer>>();

			var isEmpty = true;
			var isFirst = true;
			var action = new Action<IServer>(
				server => {
					Nox.CCK.Utils.Logger.LogDebug($"Found auth server: {server.GetTitle()} ({server.GetAddress()})");
					if (isFirst)
						foreach (UnityEngine.Transform child in serverList.transform)
							Destroy(child.gameObject);
					isFirst = false;

					var (go, comp) = ServerItemComponent.Generate(this, serverList);
					comp.UpdateContent(server);

					if (server != null) {
						isEmpty = false;
						serverInfobox.SetActive(false);
						serverListContainer.SetActive(true);
						UpdateLayout.UpdateImmediate(serverList);
					}
				}
			);

			foreach (var serverAddress in GetAuthenticationServers()) {
				if (_serverTokenSource.IsCancellationRequested) {
					_serverTokenSource = null;
					return;
				}

				tasks.Add(FetchServer(serverAddress, _serverTokenSource.Token, action));
			}

			await UniTask.WhenAll(tasks);
			if (isEmpty) {
				serverInfobox.SetActive(true);
				serverListContainer.SetActive(false);
			} else UpdateLayout.UpdateImmediate(serverList);

			_serverTokenSource = null;
		}

		private async UniTask<IServer> FetchServer(string address, CancellationToken token, Action<IServer> callback = null) {
			if (token.IsCancellationRequested)
				return null;

			var serverAPI = Main.ServerAPI;
			if (serverAPI == null) {
				Nox.CCK.Utils.Logger.LogError("ServerAPI is not available", this, tag: "AuthServerComponent");
				return null;
			}

			try {
				var server = await serverAPI.Fetch(address)
					.AttachExternalCancellation(token);
				if (token.IsCancellationRequested)
					return null;
				callback?.Invoke(server);
				return server;
			} catch (Exception e) {
				Nox.CCK.Utils.Logger.LogError(new Exception($"Failed to fetch server at {address}", e), this, tag: "AuthServerComponent");
				return null;
			}
		}

		public static IEnumerable<string> GetAuthenticationServers()
			=> SearchHelper
				.ServersBy("user")
				.Select(s => s.Address);

		private void OnRefreshServersClicked()
			=> UpdateServers().Forget();

		private void OnDestroy() {
			_serverTokenSource?.Cancel();
			_serverTokenSource?.Dispose();
		}

		public static (GameObject, AuthServerComponent) Generate(AuthServerPage page, RectTransform parent) {
			var content              = Instantiate(Client.GetAsset<GameObject>("ui:prefabs/split.prefab"), parent);
			var iconAsset            = Client.GetAsset<GameObject>("ui:prefabs/header_icon.prefab");
			var labelAsset           = Client.GetAsset<GameObject>("ui:prefabs/header_label.prefab");
			var withTitleAsset       = Client.GetAsset<GameObject>("ui:prefabs/with_title.prefab");
			var listAsset            = Client.GetAsset<GameObject>("ui:prefabs/list.prefab");
			var scrollAsset          = Client.GetAsset<GameObject>("ui:prefabs/scroll.prefab");
			var containerAsset       = Client.GetAsset<GameObject>("ui:prefabs/container.prefab");
			var containerFullAsset   = Client.GetAsset<GameObject>("ui:prefabs/container_full.prefab");
			var headerButtonAsset    = Client.GetAsset<GameObject>("ui:prefabs/header_button.prefab");

			var component = content.AddComponent<AuthServerComponent>();
			component.Page = page;
			content.name   = $"[{page.GetKey()}_{content.GetEntityId().GetHashCode()}]";

			var splitContent = Reference.GetComponent<RectTransform>("content", content);

			// Premier container (full, vide pour le moment)
			var containerFull = Instantiate(containerFullAsset, splitContent);
			// var contentFull   = Reference.GetComponent<RectTransform>("content", containerFull);
			// TODO: Ajouter du contenu ici plus tard

			// Second container (non-full, avec la liste des serveurs)
			var container = Instantiate(containerAsset, splitContent);
			var withTitle = Instantiate(
				withTitleAsset,
				Reference.GetComponent<RectTransform>("content", container)
			);

			// Header avec icône et label "Serveurs"
			var header = Reference.GetReference("header", withTitle);
			var icon   = Instantiate(iconAsset, Reference.GetComponent<RectTransform>("before", header));
			var label  = Instantiate(labelAsset, Reference.GetComponent<RectTransform>("content", header));

			Reference.GetComponent<Image>("image", icon).sprite = Client.GetAsset<Sprite>("ui:icons/host.png");
			Reference.GetComponent<TextLanguage>("text", label).UpdateText("auth.servers.title");

			// Bouton refresh dans le header
			var after         = Reference.GetComponent<RectTransform>("after", header);
			var refreshButton = Instantiate(headerButtonAsset, after);
			Reference.GetComponent<Button>("button", refreshButton)
				.onClick.AddListener(component.OnRefreshServersClicked);
			Reference.GetComponent<Image>("image", refreshButton).sprite = Client.GetAsset<Sprite>("ui:icons/refresh.png");

			// Contenu avec scroll et liste
			var contentIn = Reference.GetComponent<RectTransform>("content", withTitle);
			component.serverInfobox = Instantiate(Client.GetAsset<GameObject>("ui:prefabs/infobox.prefab"), contentIn);
			Reference.GetComponent<TextLanguage>("text", component.serverInfobox).UpdateText("auth.no_servers");
			
			component.serverListContainer = Instantiate(scrollAsset, contentIn);
			var list                      = Instantiate(listAsset, Reference.GetComponent<RectTransform>("content", component.serverListContainer));
			component.serverList          = Reference.GetComponent<RectTransform>("content", list);

			// Initialiser l'affichage
			component.serverInfobox.SetActive(true);
			component.serverListContainer.SetActive(false);

			// Charger les serveurs au démarrage
			component.UpdateServers().Forget();

			return (content, component);
		}
	}

	public class ServerItemComponent : MonoBehaviour {
		public static (GameObject, ServerItemComponent) Generate(AuthServerComponent reference, RectTransform parent) {
			var (go, comp) = GenerateBase(parent);
			comp.reference = reference;
			return (go, comp);
		}

		public static (GameObject, ServerItemComponent) Generate(AuthLoginComponent reference, RectTransform parent) {
			var (go, comp) = GenerateBase(parent);
			comp.loginReference = reference;
			return (go, comp);
		}

		private static (GameObject, ServerItemComponent) GenerateBase(RectTransform parent) {
			var serverItem = Instantiate(Client.GetAsset<GameObject>("servers:prefabs/server_item.prefab"), parent);
			var component  = serverItem.AddComponent<ServerItemComponent>();
			component.label     = Reference.GetComponent<TextLanguage>("label", serverItem);
			component.text      = Reference.GetComponent<TextLanguage>("text", serverItem);
			component.icon      = Reference.GetComponent<Image>("icon", serverItem);
			component.button    = Reference.GetComponent<Button>("button", serverItem);
			component.button.onClick.AddListener(component.OnClick);
			component.iconContainer = Reference.GetComponent<RectTransform>("icon_container", serverItem);
			return (serverItem, component);
		}

		public  AuthServerComponent       reference;
		public  AuthLoginComponent        loginReference;
		public  TextLanguage              label;
		public  TextLanguage              text;
		public  Button                    button;
		public  Image                     icon;
		public  RectTransform             iconContainer;
		private NetworkImage              _iconNetworkImage;
		private IServer                   _server;

		public void UpdateContent(IServer server) {
			_server = server;
			label.UpdateText("value", new[] { server.GetTitle() ?? server.GetAddress() });
			text.UpdateText("value", new[] { server.GetAddress(), server.GetDescription() ?? "" });
			
			iconContainer.gameObject.SetActive(false);
			UpdateIcon(server);
		}

		private void OnClick() {
			Nox.CCK.Utils.Logger.LogDebug($"Server {_server.GetAddress()} clicked");
			loginReference?.SelectServer(_server);
		}

		private void UpdateIcon(IServer server) {
			var url = server?.GetIconUrl();
			if (string.IsNullOrEmpty(url)) {
				icon.sprite = null;
				iconContainer.gameObject.SetActive(false);
				return;
			}

			_iconNetworkImage = icon.GetOrAddComponent<NetworkImage>();
			_iconNetworkImage.Url = url;
			iconContainer.gameObject.SetActive(true);
		}
	}
}