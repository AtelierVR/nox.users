using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Utils;
using Nox.UI;
using Nox.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Nox.Users.Runtime.Clients {
	public class UserWidget : MonoBehaviour, IWidget {
		public static string GetDefaultKey()
			=> "current_user";

		public string GetKey()
			=> GetDefaultKey();


		internal static readonly HashSet<UserWidget> All = new();
		private void Awake()     => All.Add(this);
		private void OnDestroy() => All.Remove(this);

		internal int              _mid;
		private Image             _image;
		private AspectRatioFitter _ratio;
		private GameObject        _container;
		private GameObject        _content;
		private Image             _icon;
		private TextLanguage      _label;

		private void OnClick()
			=> Client.UiAPI?.SendGoto(
				_mid, UserPage.GetStaticKey(),
				"user", Main.Instance.Network.CurrentUser
			);

		private static Identifier GetIdentifier()
			=> Main.Instance.Network.CurrentUser?.Identifier ?? Identifier.Invalid;

		public Vector2Int GetSize()
			=> new(3, 2);

		public int GetPriority()
			=> 100;

		public static bool TryMake(IMenu menu, RectTransform parent, out (GameObject, IWidget) values) {
			if (!GetIdentifier().IsValid()) {
				values = (null, null);
				return false;
			}

			var prefab    = Client.GetAsset<GameObject>("ui:prefabs/grid_item.prefab");
			var instance  = Instantiate(prefab, parent);
			var component = instance.AddComponent<UserWidget>();
			component._mid = menu.Id;

			var button = Reference.GetComponent<Button>("button", instance);
			button.onClick.AddListener(component.OnClick);
			instance.name = $"[{component.GetKey()}_{instance.GetEntityId().GetHashCode()}]";
			values        = (instance, component);

			prefab               = Client.GetAsset<GameObject>("ui:prefabs/large_widget.prefab");
			component._content   = Instantiate(prefab, Reference.GetComponent<RectTransform>("content", instance));
			component._image     = Reference.GetComponent<Image>("image", component._content);
			component._ratio     = Reference.GetComponent<AspectRatioFitter>("image_ratio", component._content);
			component._container = Reference.GetReference("image_container", component._content);
			component._icon      = Reference.GetComponent<Image>("icon", component._content);
			component._label     = Reference.GetComponent<TextLanguage>("label", component._content);

			component.UpdateContent().Forget();

			return true;
		}

		private async UniTask UpdateContent() {
			var identifier = GetIdentifier();
			if (!identifier.IsValid()) {
				_container.SetActive(false);
				await UpdateIcon();
				_label.UpdateText("user.not_logged_in");
				return;
			}

			if (!_image.sprite)
				_container.SetActive(false);
			await UpdateIcon();

			if (Main.Instance.Network.CurrentUser is not IUser user || !user.Identifier.Equals((Identifier)identifier))
				user = await Main.Instance.Network.Fetch(identifier);

			if (user == null) {
				_container.SetActive(false);
				await UpdateIcon();
				_label.UpdateText("user.not_logged_in");
				return;
			}

			_label.UpdateText(
				"value",
				new[] {
					user.Display
					?? user.Username
					?? identifier.ToString()
				}
			);

			await UniTask.WhenAll(
				UpdateBanner(user),
				UpdateThumbnail(user)
			);
		}

		private async UniTask UpdateBanner(IUser user) {
			var url = user.Banner;

			if (string.IsNullOrEmpty(url)) {
				_container.SetActive(false);
				return;
			}

			var banner = await Main.NetworkAPI.FetchTexture(url);
			if (!banner || banner.height == 0) {
				_container.SetActive(false);
				return;
			}

			_image.sprite = Sprite.Create(
				banner,
				new Rect(0, 0, banner.width, banner.height),
				new Vector2(0.5f, 0.5f)
			);
			_ratio.aspectRatio = (float)banner.width / banner.height;
			_container.SetActive(true);
		}

		private async UniTask UpdateThumbnail(IUser user) {
			var url = user.Thumbnail;

			if (string.IsNullOrEmpty(url)) {
				await UpdateIcon();
				return;
			}

			var thumbnail = await Main.NetworkAPI.FetchTexture(url);
			if (!thumbnail || thumbnail.height == 0) {
				await UpdateIcon();
				return;
			}

			await UpdateIcon(
				Sprite.Create(
					thumbnail,
					new Rect(0, 0, thumbnail.width, thumbnail.height),
					new Vector2(0.5f, 0.5f)
				)
			);
		}


		private async UniTask UpdateIcon(Sprite icon = null) {
			icon         ??= await Client.GetAssetAsync<Sprite>("ui:icons/person.png");
			_icon.sprite =   icon;
		}
	}
}