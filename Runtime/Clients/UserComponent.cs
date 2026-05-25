using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Utils;
using Nox.Users;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nox.Users.Runtime.Clients {
	public class UserComponent : MonoBehaviour {
		public GameObject        withBanner;
		public GameObject        withoutBanner;
		public Image             banner;
		public Image             thumbnail;
		public TextLanguage      display;
		public TextLanguage      identifier;
		public UserPage          Page;
		public AspectRatioFitter fitter;
		public GameObject        bioContainer;
		public TextLanguage      bioText;

		public void UpdateContent(IUser user) {
			if (user == null) return;

			display.UpdateText("user.display", new[] { user.Display });
			identifier.UpdateText(
				"user.identifier", new[] {
					user.Identifier.ToString(),
					user.Id.ToString(),
					user.Username,
					user.Server
				}
			);

			if (!string.IsNullOrEmpty(user.Bio)) {
				bioText.SetMarkdown(user.Bio);
				bioContainer.SetActive(true);
			} else bioContainer.SetActive(false);

			UpdateThumbnail(user).Forget();
			UpdateBanner(user).Forget();
		}

		private CancellationTokenSource _thumbnailTokenSource;
		private CancellationTokenSource _bannerTokenSource;

		private async UniTask UpdateThumbnail(IUser user) {
			if (_thumbnailTokenSource != null) {
				_thumbnailTokenSource?.Cancel();
				_thumbnailTokenSource?.Dispose();
			}

			_thumbnailTokenSource = new CancellationTokenSource();
			if (user?.Thumbnail != null) {
				var texture = await Main.NetworkAPI
					.FetchTexture(user.Thumbnail)
					.AttachExternalCancellation(_thumbnailTokenSource.Token);
				thumbnail.sprite = texture
					? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero)
					: null;
			} else thumbnail.sprite = null;

			_thumbnailTokenSource = null;
		}

		private async UniTask UpdateBanner(IUser user) {
			if (_bannerTokenSource != null) {
				_bannerTokenSource?.Cancel();
				_bannerTokenSource?.Dispose();
			}

			_bannerTokenSource = new CancellationTokenSource();
			if (user?.Banner != null) {
				var texture = await Main.NetworkAPI
					.FetchTexture(user.Banner)
					.AttachExternalCancellation(_bannerTokenSource.Token);
				if (texture && texture.height > 0) {
					banner.sprite      = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
					fitter.aspectRatio = (float)texture.width / texture.height;
					if (fitter.aspectRatio <= 1.3333333333333333f) // 4:3 minimum
						fitter.aspectRatio = 1.3333333333333333f;
					if (fitter.aspectRatio >= 2.75f) // 11:4 maximum
						fitter.aspectRatio = 2.75f;
					withBanner.SetActive(true);
					withoutBanner.SetActive(false);
				} else {
					banner.sprite = null;
					withBanner.SetActive(false);
					withoutBanner.SetActive(true);
				}
			} else {
				banner.sprite = null;
				withBanner.SetActive(false);
				withoutBanner.SetActive(true);
			}

			_bannerTokenSource = null;
		}

		public void UpdateError(string error) {
			display.UpdateText("user.error");
			identifier.UpdateText("user.error");
			thumbnail.sprite = null;
			banner.sprite    = null;
			withBanner.SetActive(false);
			withoutBanner.SetActive(true);
			bioContainer.SetActive(false);
		}

		public void UpdateLoading() {
			display.UpdateText("user.loading");
			identifier.UpdateText("user.loading");
			thumbnail.sprite = null;
			banner.sprite    = null;
			withBanner.SetActive(false);
			withoutBanner.SetActive(true);
			bioContainer.SetActive(false);
		}

		public static (GameObject, UserComponent) Generate(UserPage userPage, RectTransform parent) {
			var content = Instantiate(Client.GetAsset<GameObject>("ui:prefabs/split.prefab"), parent);

			var component = content.AddComponent<UserComponent>();
			component.Page = userPage;
			content.name   = $"[{userPage.GetKey()}_{content.GetEntityId().GetHashCode()}]";

			var splitContent   = Reference.GetComponent<RectTransform>("content", content);
			var containerAsset = Client.GetAsset<GameObject>("ui:prefabs/container.prefab");

			// generate profile
			var container = Instantiate(containerAsset, splitContent);
			var profile = Instantiate(
				Client.GetAsset<GameObject>("prefabs/profile.prefab"),
				Reference.GetComponent<RectTransform>("content", container)
			);
			component.identifier    = Reference.GetComponent<TextLanguage>("identifier", profile);
			component.display       = Reference.GetComponent<TextLanguage>("display", profile);
			component.thumbnail     = Reference.GetComponent<Image>("thumbnail", profile);
			component.banner        = Reference.GetComponent<Image>("banner", profile);
			component.withBanner    = Reference.GetReference("with_banner", profile);
			component.withoutBanner = Reference.GetReference("without_banner", profile);
			component.fitter        = Reference.GetComponent<AspectRatioFitter>("banner_aspect", profile);

			// generate dashboard
			container = Instantiate(Client.GetAsset<GameObject>("ui:prefabs/container_full.prefab"), splitContent);

			var withTitleAsset = Client.GetAsset<GameObject>("ui:prefabs/with_title.prefab");
			var scrollAsset    = Client.GetAsset<GameObject>("ui:prefabs/scroll.prefab");
			var listAsset      = Client.GetAsset<GameObject>("ui:prefabs/list.prefab");
			var boxAsset       = Client.GetAsset<GameObject>("ui:prefabs/box.prefab");

			var withTitle = Instantiate(
				withTitleAsset,
				Reference.GetComponent<RectTransform>("content", container)
			);

			var contentDash = Reference.GetComponent<RectTransform>("content", withTitle);
			// setup scroll + list
			var scroll      = Instantiate(scrollAsset, contentDash);
			var list        = Instantiate(listAsset, Reference.GetComponent<RectTransform>("content", scroll));
			var listContent = Reference.GetComponent<RectTransform>("content", list);

			// add box description
			component.bioContainer = Instantiate(boxAsset, listContent);
			Reference.GetComponent<TextLanguage>("text", component.bioContainer).UpdateText("user.about.bio");
			component.bioText = Reference.GetComponent<TextLanguage>(
				"text", Instantiate(
					Client.GetAsset<GameObject>("ui:prefabs/text.prefab"),
					Reference.GetComponent<RectTransform>("content", component.bioContainer)
				)
			);

			return (content, component);
		}
	}
}