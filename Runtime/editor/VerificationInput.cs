#if UNITY_EDITOR
using System;
using System.Threading;
using api.nox.user.network;
using Cysharp.Threading.Tasks;
using Nox.Servers;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = Nox.CCK.Utils.Logger;

namespace api.nox.user {
	public class VerificationInput : IDisposable {
		private AuthentificationInstance _panel;
		private VisualElement           _container;
		private Label                   _error;
		private TextField               _input;
		private Button                  _submit;
		private Button                  _back;
		private DropdownField           _method;
		private Label                   _methodLabel;
		private Label                   _methodDescription;
		private CancellationTokenSource _cts;

		public VerificationInput(VisualElement root, AuthentificationInstance panel) {
			_panel             = panel;
			_container         = root.Q<VisualElement>("verification");
			_error             = root.Q<Label>("verification_error");
			_input             = root.Q<TextField>("verification_input");
			_submit            = root.Q<Button>("verification_submit");
			_back              = root.Q<Button>("verification_back");
			_method            = root.Q<DropdownField>("verification_methods");
			_methodLabel       = root.Q<Label>("verification_label");
			_methodDescription = root.Q<Label>("verification_description");

			_submit.RegisterCallback<ClickEvent>(OnSubmit);
			_input.RegisterCallback<KeyUpEvent>(OnKeyUp);
			_input.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_back.RegisterCallback<ClickEvent>(OnBack);
			_method.RegisterCallback<ChangeEvent<string>>(OnMethodChanged);
		}

		public void SetActive(bool active) {
			if (active) {
				_container.style.display = DisplayStyle.Flex;
				_input.Focus();
			} else _container.style.display = DisplayStyle.None;
		}

		private void OnMethodChanged(ChangeEvent<string> evt) {
			if (_verification == null || _verification.Methods.Length == 0) return;
			var method = Array.Find(_verification.Methods, m => m.type == evt.newValue);
			if (method == null) return;
			_input.Focus();
			UpdateMethodDetails(method);
		}

		private void UpdateMethodDetails(VerificationMethod method) {
			_methodLabel.text       = method.GetTitle();
			_methodDescription.text = method.GetDescription();
		}

		private void OnKeyUp(KeyUpEvent evt) {
			if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;
			OnSubmit();
			evt.StopPropagation();
		}

		private void OnNavigationSubmit(NavigationSubmitEvent evt) {
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
			_input.UnregisterCallback<KeyUpEvent>(OnKeyUp);
			_input.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
			_back.UnregisterCallback<ClickEvent>(OnBack);
			_method.UnregisterCallback<ChangeEvent<string>>(OnMethodChanged);

			if (_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts               = null;
			_panel             = null;
			_container         = null;
			_error             = null;
			_input             = null;
			_submit            = null;
			_back              = null;
			_method            = null;
			_methodLabel       = null;
			_methodDescription = null;
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

		private void OnBack(ClickEvent evt) {
			if (_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts = null;

			_panel.Login.SetActive(true);
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

			if (_request == null) {
				SetEnabled(true, "No login request available.");
				_panel.Login.SetActive(true);
				SetActive(false);
				return;
			}

			var code = _input.text.Trim();
			if (string.IsNullOrEmpty(code)) {
				SetEnabled(true, "Verification code cannot be empty.");
				_input.Focus();
				return;
			}

			_request.FactorCode = code;
			var login = await Main.Instance.Network.Login(
				_request,
				_server.GetAddress()
			);

			if (login == null) {
				SetEnabled(true, "Login failed.");
				return;
			}

			if (login.IsVerificationRequired()) {
				SetEnabled(true, "Verification code is incorrect.");
				SetVerificationRequired(login.Verification);
				_input.Focus();
				return;
			}

			if (login.IsError()) {
				SetEnabled(true, login.Error);
				_input.Focus();
				return;
			}

			_panel.GetWindow().SetActive(EditorUser.Profile);
		}

		private IServer              _server;
		private LoginRequest         _request;
		private VerificationRequired _verification;

		public void SetServer(IServer server)
			=> _server = server;

		public void SetRequest(LoginRequest request)
			=> _request = request;

		public void SetVerificationRequired(VerificationRequired verification) {
			_verification ??= new VerificationRequired {
				Methods  = Array.Empty<VerificationMethod>(),
				Required = true
			};

			_verification = verification;
			_method.choices.Clear();

			foreach (var method in _verification.Methods)
				_method.choices.Add(method.type);

			if (_verification.Methods.Length <= 0)
				return;

			var current = Array.Find(
					_verification.Methods,
					m => m.type == _method.value
				)
				?? _verification.Methods[0];

			_method.value = current.type;
			UpdateMethodDetails(current);
		}
	}
}
#endif