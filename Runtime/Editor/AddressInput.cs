#if UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using Nox.Servers;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Users.Runtime.Editor {
	public class AddressInput : IDisposable {
		public IServer Server;

		private AuthentificationInstance _panel;
		private VisualElement         _container;
		private Label                 _error;
		private TextField             _input;
		private Button                _submit;

		public AddressInput(VisualElement root, AuthentificationInstance panel) {
			_panel     = panel;
			_container = root.Q<VisualElement>("address");
			_error     = root.Q<Label>("address_error");
			_input     = root.Q<TextField>("address_input");
			_submit    = root.Q<Button>("address_submit");
			_submit.RegisterCallback<ClickEvent>(OnSubmit);
			_input.RegisterCallback<KeyUpEvent>(OnKeyUp);
			_input.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		}

		public void SetActive(bool active) {
			if (active) {
				_container.style.display = DisplayStyle.Flex;
				_input.Focus();
			} else _container.style.display = DisplayStyle.None;
		}

		private void OnKeyUp(KeyUpEvent evt) {
			if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;
			OnSubmit().Forget();
			evt.StopPropagation();
		}

		private void OnNavigationSubmit(NavigationSubmitEvent evt) {
			OnSubmit().Forget();
			evt.StopPropagation();
		}

		private void OnSubmit(ClickEvent evt) {
			OnSubmit().Forget();
			evt.StopPropagation();
		}

		public void Dispose() {
			_submit.UnregisterCallback<ClickEvent>(OnSubmit);
			_input.UnregisterCallback<KeyUpEvent>(OnKeyUp);
			_input.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_panel     = null;
			_container = null;
			_error     = null;
			_input     = null;
			_submit    = null;
		}

		private void SetEnabled(bool enabled, string err = null) {
			_input.SetEnabled(enabled);
			_submit.SetEnabled(enabled);
			if (string.IsNullOrEmpty(err)) {
				_error.text          = string.Empty;
				_error.style.display = DisplayStyle.None;
			} else {
				_error.text          = err;
				_error.style.display = DisplayStyle.Flex;
				Logger.LogWarning(err);
			}
		}

		private async UniTask OnSubmit() {
			if (!_submit.enabledSelf) return;

			SetEnabled(false);

			var address = _input.text.Trim();
			if (string.IsNullOrEmpty(address)) {
				SetEnabled(true, "Address cannot be empty");
				_input.Focus();
				return;
			}

			var server = await Main.ServerAPI.Fetch(address);
			if (server == null) {
				SetEnabled(true, "Server not found");
				_input.Focus();
				return;
			}

			Logger.Log($"Server found: {server.GetTitle()} ({server.GetAddress()})");
			_panel.Login.SetServer(server);
			_panel.Login.SetActive(true);
			SetActive(false);
			SetEnabled(true);
		}
	}
}
#endif