#if UNITY_EDITOR
using System.Collections.Generic;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.Editor.Panel;
using UnityEngine.UIElements;

namespace Nox.Users.Runtime.Editor {
	public class AuthentificationPanel : IEditorModInitializer, Nox.Editor.Panel.IPanel {
		internal IEditorModCoreAPI          API;
		internal AuthentificationInstance   Instance;

		public void OnInitializeEditor(IEditorModCoreAPI api) { API = api; EditorUser.Auth = this; }
		public void OnDisposeEditor() { Instance?.OnDestroy(); API = null; EditorUser.Auth = null; }

		public string[] GetPath()  => new[] { "user", "auth" };
		public string   GetLabel() => "User/Authentification";
		public bool     IsVisible() => Main.Instance?.Network?.CurrentUser == null;

		public IInstance[] GetInstances()
			=> Instance != null ? new IInstance[] { Instance } : System.Array.Empty<IInstance>();

		public IInstance Instantiate(IWindow window, Dictionary<string, object> data)
			=> Instance = new AuthentificationInstance(this, window);
	}

	public class AuthentificationInstance : IInstance {
		private readonly AuthentificationPanel _panel;
		private readonly IWindow              _window;
		private          VisualElement        _root;
		internal         AddressInput         Address;
		internal         LoginInput           Login;
		internal         VerificationInput    Verification;

		public AuthentificationInstance(AuthentificationPanel panel, IWindow window) {
			_panel  = panel;
			_window = window;
		}

		public Nox.Editor.Panel.IPanel GetPanel()  => _panel;
		public IWindow                 GetWindow() => _window;
		public string                  GetTitle()  => "Authentification";
		public void                    OnDestroy() => _panel.Instance = null;

		public VisualElement GetContent() {
			if (_root != null) return _root;
			_root = EditorUser.CoreAPI.AssetAPI
				.GetAsset<VisualTreeAsset>("auth.uxml")
				.CloneTree();

			Address      = new AddressInput(_root, this);
			Login        = new LoginInput(_root, this);
			Verification = new VerificationInput(_root, this);

			Address.SetActive(true);
			Login.SetActive(false);
			Verification.SetActive(false);

			return _root;
		}
	}
}
#endif