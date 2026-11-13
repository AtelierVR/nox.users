using Cysharp.Threading.Tasks;
using Nox.CCK.Mods.Events;
using Nox.CCK.Utils;
using Nox.UI;
using Nox.Users.Runtime.Base;
using UnityEngine;

namespace Nox.Users.Runtime.Clients {
	public class UserPage : IPage {
		static internal string GetStaticKey()
			=> "users";

		public string GetKey()
			=> GetStaticKey();

		private int _mId;
		private object[] _context;
		private GameObject _content;
		private UserComponent _component;
		private Identifier _identifier;
		private IUser _user;
		private bool _isLoading;


		public void OnRefresh()
			=> Refresh().Forget();

		private static bool T<T>(object[] o, int index, out T value) {
			if (o.Length > index && o[index] is T t) {
				value = t;
				return true;
			}

			value = default;
			return false;
		}

		internal static IPage OnGotoAction(IMenu menu, object[] context) {
			if (!T(context, 0, out string type)) return null;
			switch (type) {
				case "id_server" when T(context, 1, out uint id0) && T(context, 2, out string ser0):
					return OnPageByIdentifier(menu, context, new Identifier("u", id0, null, ser0));
				case "identifier" when T(context, 1, out string id2):
					return OnPageByIdentifier(menu, context, Identifier.Parse(id2));
				case "identifier" when T(context, 1, out Identifier ui0):
					return OnPageByIdentifier(menu, context, ui0);
				case "user" when T(context, 1, out IUser usr3):
					return OnPageByUser(menu, context, usr3);
			}

			return null;
		}

		private static UserPage OnPageByIdentifier(IMenu menu, object[] context, Identifier identifier) {
			var page = new UserPage {
				_mId = menu.Id,
				_context = context,
				_identifier = identifier,
				_user = null
			};
			page.Refresh().Forget();
			return page;
		}

		private static UserPage OnPageByUser(IMenu menu, object[] context, IUser user) {
			return new UserPage {
				_mId = menu.Id,
				_context = context,
				_identifier = user.Identifier,
				_user = user
			};
		}


		private async UniTask Refresh() {
			if (_isLoading) return;
			_isLoading = true;
			// A refresh must not rebuild the interface: keep the currently displayed profile
			// and only update the info once the new data arrives. The loading placeholder is
			// shown only on the very first load (no user displayed yet).
			if (_component != null && _user == null)
				_component.UpdateLoading();
			_user = await Main.Instance.Fetch(_identifier);
			_isLoading = false;
			if (_component == null) return;
			if (_user != null) _component.UpdateContent(_user);
			else _component.UpdateError("User not found or loading failed.");
			UpdateLayout.UpdateImmediate(_content);
		}

		public object[] GetContext()
			=> _context;

		public IMenu GetMenu()
			=> Client.UiAPI.Get<IMenu>(_mId);

		/// <summary>
		/// Replaces the cached user without re-rendering it, used when only the relation with the
		/// current user changed (the component already refreshed its own buttons).
		/// </summary>
		internal void UpdateUserCache(IUser user) {
			if (user == null)
				return;
			_user = user;
		}

		public GameObject GetContent(RectTransform parent) {
			if (_content) return _content;
			(_content, _component) = UserComponent.Generate(this, parent);
			_component.UpdateLoading();
			Listen();
			return _content;
		}

		private EventSubscription _changed;
		private EventSubscription _relation;
		private EventSubscription _presence;

		private void Listen() {
			if (_changed != null) return;

			var events = Main.Instance?.CoreAPI?.EventAPI;
			if (events == null) return;

			_changed = events.Subscribe("user_changed", OnUserChanged);
			_relation = events.Subscribe("user_relation_changed", OnRelationChanged);
			_presence = events.Subscribe("user_presence", OnPresenceChanged);

			Main.Instance?.WatchPresence();
		}

		private void Unlisten() {
			var events = Main.Instance?.CoreAPI?.EventAPI;

			if (events != null) {
				if (_changed != null) events.Unsubscribe(_changed);
				if (_relation != null) events.Unsubscribe(_relation);
				if (_presence != null) events.Unsubscribe(_presence);
			}

			Main.Instance?.UnwatchPresence();

			_changed = null;
			_relation = null;
			_presence = null;
		}

		private void OnUserChanged(EventData context) {
			if (!context.TryGet(0, out IUser user) || user == null) return;
			if (!user.Identifier.Equals(_identifier)) return;

			_user = user;

			if (_component == null) return;

			_component.UpdateContent(_user);
			UpdateLayout.UpdateImmediate(_content);
		}

		private void OnRelationChanged(EventData context) {
			if (!context.TryGet(0, out Identifier identifier)) return;
			if (!identifier.Equals(_identifier)) return;

			OnRefresh();
		}

		private void OnPresenceChanged(EventData context) {
			if (!context.TryGet(0, out Identifier identifier)) return;
			if (!identifier.Equals(_identifier)) return;

			if (!context.TryGet(1, out UserStatus status)) return;

			context.TryGet(2, out string text);
			context.TryGet(3, out string[] locations);

			if (_user is User user) {
				user.ApplyPresence(status, text, locations);
				_component?.RefreshPresence();
			}
		}

		public void OnRemove() {
			Unlisten();
			_component = null;
			_content = null;
			_isLoading = false;
		}

		public void OnDisplay(IPage lastPage) {
			if (_user != null) _component.UpdateContent(_user);
			else if (_isLoading) _component.UpdateLoading();
			else _component.UpdateError("User not found or loading failed.");
		}
	}
}
