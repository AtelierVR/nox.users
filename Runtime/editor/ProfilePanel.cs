#if UNITY_EDITOR
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.Editor.Panel;
using UnityEngine.UIElements;

namespace api.nox.user {
	public class ProfilePanel : IEditorModInitializer, Nox.Editor.Panel.IPanel {
		internal IEditorModCoreAPI  API;
		internal ProfileInstance    Instance;

		public void OnInitializeEditor(IEditorModCoreAPI api) { API = api; EditorUser.Profile = this; }
		public void OnDisposeEditor() { Instance?.OnDestroy(); API = null; EditorUser.Profile = null; }

		public string[] GetPath()  => new[] { "user", "profile" };
		public string   GetLabel() => "User/Profile";

		public IInstance[] GetInstances()
			=> Instance != null ? new IInstance[] { Instance } : System.Array.Empty<IInstance>();

		public IInstance Instantiate(IWindow window, Dictionary<string, object> data)
			=> Instance = new ProfileInstance(this, window);
	}

	public class ProfileInstance : IInstance {
		private readonly ProfilePanel  _panel;
		private readonly IWindow       _window;
		private          VisualElement _root;

		public ProfileInstance(ProfilePanel panel, IWindow window) {
			_panel  = panel;
			_window = window;
		}

		public Nox.Editor.Panel.IPanel GetPanel()  => _panel;
		public IWindow                 GetWindow() => _window;
		public string                  GetTitle()  => "Profile";
		public void                    OnDestroy() => _panel.Instance = null;

		public IToolOption[] GetOptions() => new IToolOption[] {
			new DefaultToolOption("Logout", OnLogout)
		};

		private async void OnLogout() {
			var success = await Main.Instance.Network.Logout();
			if (success)
				_window.SetActive(EditorUser.Auth);
		}

		public VisualElement GetContent() {
			if (_root != null) return _root;
			_root = EditorUser.CoreAPI.AssetAPI
				.GetAsset<VisualTreeAsset>("profile.uxml")
				.CloneTree();
			_root.style.flexGrow = 1;

			var user = Main.Instance.Network.CurrentUser;

			_root.Q<UnsignedIntegerField>("id").value = user.Id;
			_root.Q<TextField>("server").value        = user.Server;
			_root.Q<TextField>("display").value       = user.Display;
			_root.Q<TextField>("username").value      = user.Username;
			_root.Q<TextField>("email").value         = user.Email;

			var banner    = user.Banner;
			var thumbnail = user.Thumbnail;

			var withoutVisual = _root.Q<VisualElement>("without-banner");
			var withVisual    = _root.Q<VisualElement>("with-banner");
			if (string.IsNullOrEmpty(banner)) {
				withoutVisual.EnableInClassList("hidden", false);
				withVisual.EnableInClassList("hidden", true);
				withoutVisual.Q<Label>("display_name").text = user.Display;
				var thumbnailImage = withoutVisual.Q<Image>("thumbnail");
				if (!thumbnailImage.image) UpdateImage(thumbnailImage, thumbnail).Forget();
			} else {
				withoutVisual.EnableInClassList("hidden", true);
				withVisual.EnableInClassList("hidden", false);
				withVisual.Q<Label>("display_name").text = user.Display;
				var bannerImage    = withVisual.Q<Image>("banner");
				var thumbnailImage = withVisual.Q<Image>("thumbnail");
				if (!bannerImage.image) UpdateImage(bannerImage, banner).Forget();
				if (!thumbnailImage.image) UpdateImage(thumbnailImage, thumbnail).Forget();
			}

			var logoutButton = _root.Q<Button>("logout-button");
			if (logoutButton != null) logoutButton.RemoveFromHierarchy();

			return _root;
		}

		private async UniTask UpdateImage(Image image, string url) {
			if (string.IsNullOrEmpty(url)) { image.image = null; return; }
			image.image = await Main.NetworkAPI.FetchTexture(url);
		}
	}
}
#endif