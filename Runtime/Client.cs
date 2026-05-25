using System;
using System.Collections.Generic;
using System.Linq;
using api.nox.user.client;
using api.nox.user.widget;
using Cysharp.Threading.Tasks;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Events;
using Nox.CCK.Mods.Initializers;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using Nox.UI;
using Nox.UI.Widgets;
using UnityEngine;

namespace api.nox.user {
	public class Client : IClientModInitializer {
		internal static IUiAPI UiAPI
			=> Main.Instance.CoreAPI.ModAPI
				.GetMod("ui")
				.GetInstance<IUiAPI>();

		public static T GetAsset<T>(ResourceIdentifier path) where T : UnityEngine.Object
			=> Instance.CoreAPI.AssetAPI.GetAsset<T>(path);

		public static UniTask<T> GetAssetAsync<T>(ResourceIdentifier path) where T : UnityEngine.Object
			=> Main.Instance.CoreAPI.AssetAPI.GetAssetAsync<T>(path);

		private EventSubscription[] _events = Array.Empty<EventSubscription>();

		internal static Client           Instance;
		internal        IClientModCoreAPI CoreAPI;

		public void OnInitializeClient(IClientModCoreAPI api) {
			Instance = this;
			CoreAPI  = api;
			_events = new[] {
				CoreAPI.EventAPI.Subscribe("menu_goto", OnGoto),
				CoreAPI.EventAPI.Subscribe("widget_request", OnWidgetRequest),
				CoreAPI.EventAPI.Subscribe("user_update", OnUserUpdate)
			};
		}

		private void OnGoto(EventData context) {
			if (!context.TryGet(0, out int mid)) return;
			if (!context.TryGet(1, out string key)) return;
			var menu = UiAPI?.Get<IMenu>(mid);
			if (menu == null) return;
			IPage page = null;
			if (UserPage.GetStaticKey() == key)
				page = UserPage.OnGotoAction(menu, context.Data[2..]);
			if (AuthServerPage.GetStaticKey() == key)
				page = AuthServerPage.OnGotoAction(menu, context.Data[2..]);
			if (page == null) return;
			Main.Instance.CoreAPI.EventAPI.Emit("menu_display", menu.Id, page);
		}

		private void OnUserUpdate(EventData context) {
			var isLoggedIn = context.Data.Length > 0 && context.Data[0] != null;
			if (isLoggedIn) {
				foreach (var w in AuthWidget.All.ToArray()) {
					var parent = w.transform.parent as RectTransform;
					var menu   = UiAPI?.Get<IMenu>(w._mid);
					CoreAPI.EventAPI.Emit("widget_removed", w.GetKey());
					if (parent && menu != null && UserWidget.TryMake(menu, parent, out var widget) && widget.Item2 != null)
						CoreAPI.EventAPI.Emit("widget_added", widget.Item2);
				}
			} else {
				foreach (var w in UserWidget.All.ToArray()) {
					var parent = w.transform.parent as RectTransform;
					var menu   = UiAPI?.Get<IMenu>(w._mid);
					CoreAPI.EventAPI.Emit("widget_removed", w.GetKey());
					if (parent && menu != null && AuthWidget.TryMake(menu, parent, out var widget) && widget.Item2 != null)
						CoreAPI.EventAPI.Emit("widget_added", widget.Item2);
				}
			}
		}

		private void OnWidgetRequest(EventData context) {
			if (!context.TryGet(0, out int mid)) return;
			if (!context.TryGet(1, out RectTransform tr)) return;
			var menu = UiAPI?.Get<IMenu>(mid);
			if (menu == null) return;
			List<(GameObject, IWidget)> widgets = new();
			if (UserWidget.TryMake(menu, tr, out var widget))
				widgets.Add(widget);
			if (AuthWidget.TryMake(menu, tr, out var authWidget))
				widgets.Add(authWidget);
			foreach (var value in widgets)
				context.Callback(value.Item2, value.Item1);
		}

		public void OnDisposeClient() {
			foreach (var w in AuthWidget.All.ToArray())
				CoreAPI.EventAPI.Emit("widget_removed", w.GetKey());
			foreach (var w in UserWidget.All.ToArray())
				CoreAPI.EventAPI.Emit("widget_removed", w.GetKey());
			foreach (var e in _events)
				CoreAPI.EventAPI.Unsubscribe(e);
			_events  = Array.Empty<EventSubscription>();
			CoreAPI  = null;
			Instance = null;
		}
	}
}