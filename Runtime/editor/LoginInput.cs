#if UNITY_EDITOR
using System;
using System.Threading;
using api.nox.user.network;
using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;
using Nox.Servers;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = Nox.CCK.Utils.Logger;

namespace api.nox.user {
	public class LoginInput : IDisposable {
		private AuthentificationInstance _panel;
		private VisualElement           _container;
		private Label                   _error;
		private TextField               _inputIdentifier;
		private TextField               _inputPassword;
		private Button                  _submit;
		private Button                  _back;
		private CancellationTokenSource _cts;

		public LoginInput(VisualElement root, AuthentificationInstance panel) {
			_panel           = panel;
			_container       = root.Q<VisualElement>("login");
			_error           = root.Q<Label>("login_error");
			_inputIdentifier = root.Q<TextField>("login_identifier_input");
			_inputPassword   = root.Q<TextField>("login_password_input");
			_submit          = root.Q<Button>("login_submit");
			_back            = root.Q<Button>("login_back");

			_submit.RegisterCallback<ClickEvent>(OnSubmit);
			_inputIdentifier.RegisterCallback<KeyUpEvent>(OnKeyUp);
			_inputIdentifier.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_inputPassword.RegisterCallback<KeyUpEvent>(OnKeyUp);
			_inputPassword.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_back.RegisterCallback<ClickEvent>(OnBack);
		}

		public void SetActive(bool active) {
			if (active) {
				_container.style.display = DisplayStyle.Flex;
				_inputIdentifier.Focus();
			} else _container.style.display = DisplayStyle.None;
		}

		private void OnKeyUp(KeyUpEvent evt) {
			if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;
			if (evt.target == _inputIdentifier) {
				_inputPassword.Focus();
			} else if (evt.target == _inputPassword)
				OnSubmit();

			evt.StopPropagation();
		}

		private void OnNavigationSubmit(NavigationSubmitEvent evt) {
			if (evt.target == _inputIdentifier) {
				_inputPassword.Focus();
			} else if (evt.target == _inputPassword)
				OnSubmit();

			evt.StopPropagation();
		}

		private void OnSubmit(ClickEvent evt) {
			OnSubmit();
			evt.StopPropagation();
		}

		private void OnSubmit() {
			if (_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts = new CancellationTokenSource();
			OnSubmitAsync().AttachExternalCancellation(_cts.Token).Forget();
		}

		public void Dispose() {
			_submit.UnregisterCallback<ClickEvent>(OnSubmit);
			_inputIdentifier.UnregisterCallback<KeyUpEvent>(OnKeyUp);
			_inputIdentifier.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_inputPassword.UnregisterCallback<KeyUpEvent>(OnKeyUp);
			_inputPassword.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_back.UnregisterCallback<ClickEvent>(OnBack);

			if (_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts             = null;
			_panel           = null;
			_container       = null;
			_error           = null;
			_inputIdentifier = null;
			_inputPassword   = null;
			_submit          = null;
			_back            = null;
		}

		private void SetEnabled(bool enabled, string err = null) {
			_inputIdentifier.SetEnabled(enabled);
			_inputPassword.SetEnabled(enabled);
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

		private void OnBack(ClickEvent evt) {
			if (_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts = null;

			_panel.Address.SetActive(true);
			SetActive(false);
			SetEnabled(true);
		}

		private async UniTask OnSubmitAsync() {
			if (!_submit.enabledSelf) return;

			SetEnabled(false);

			if (_server == null) {
				SetEnabled(true, "No server selected.");
				_panel.Address.SetActive(true);
				SetActive(false);
				return;
			}

			var identifier = _inputIdentifier.text.Trim();
			var password   = _inputPassword.text;

			if (string.IsNullOrEmpty(identifier)) {
				SetEnabled(true, "Identifier cannot be empty.");
				_inputIdentifier.Focus();
				return;
			}

			if (string.IsNullOrEmpty(password)) {
				SetEnabled(true, "Password cannot be empty.");
				_inputPassword.Focus();
				return;
			}

			var request = new LoginRequest {
				Identifier = identifier,
				Password   = password, 
				PublicKey = Crypto.CompressPublicKey(Crypto.GetKeys())
			};

			var login = await Main.Instance.Network.Login(
				request,
				_server.GetAddress()
			);

			if (login == null) {
				SetEnabled(true, "Login failed.");
				return;
			}

			if (login.IsVerificationRequired()) {
				_panel.Verification.SetServer(_server);
				_panel.Verification.SetRequest(request);
				_panel.Verification.SetVerificationRequired(login.Verification);
				_panel.Verification.SetActive(true);
				SetActive(false);
				SetEnabled(true);
				return;
			}

			if (login.IsError()) {
				SetEnabled(true, login.Error);
				_inputPassword.Focus();
				return;
			}

			SetEnabled(true);

			_panel.GetWindow().SetActive(EditorUser.Profile);
		}

		private IServer _server;

		public void SetServer(IServer server)
			=> _server = server;
	}
}
#endif