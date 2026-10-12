using System;
using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Users.Runtime.Clients {
	public class UserComponent : MonoBehaviour {
		public UserPage Page;

		// profile
		public TextLanguage  title;
		public TextLanguage  identifier;
		public Image         thumbnail;
		public GameObject    withThumbnail;
		public GameObject    withoutThumbnail;
		private NetworkImage _thumbnailNetworkImage;

		// banner + presence (separate from the thumbnail)
		public Image             banner;
		public AspectRatioFitter bannerAspect;
		public GameObject        withBanner;
		public GameObject        withoutBanner;
		public Image             presence;
		private NetworkImage     _bannerNetworkImage;

		// full container header
		public Image         labelIcon;
		public TextLanguage  label;
		public RectTransform content;

		// actions
		public GameObject    actionsContainer;
		public RectTransform actions;

		public Button       favoriteButton;
		public Image        favoriteIcon;
		public TextLanguage favoriteLabel;

		public Button       followButton;
		public Image        followIcon;
		public TextLanguage followLabel;

		public Button       inviteSendButton;
		public Image        inviteSendIcon;
		public TextLanguage inviteSendLabel;

		public Button       inviteRequestButton;
		public Image        inviteRequestIcon;
		public TextLanguage inviteRequestLabel;

		// bio
		public GameObject   bioContainer;
		public TextLanguage bioText;

		private IUser _user;
		private bool _isFavorite;
		private bool  _isFavoriteHover;
		private bool  _isFollowHover;

		public void UpdateContent(IUser user) {
			if (user == null) return;
			_user = user;

			var name = !string.IsNullOrEmpty(user.Display) ? user.Display : user.Username;

			title.UpdateText("user.display", new[] { name });
			identifier.UpdateText(
				"user.identifier", new[] {
					user.Identifier.ToString(),
					user.Id.ToString(),
					user.Username,
					user.Server
				}
			);
			label.UpdateText("user.about.title", new[] { name ?? user.Identifier.ToString() });

			if (!string.IsNullOrEmpty(user.Bio)) {
				bioText.SetMarkdown(user.Bio);
				bioContainer.SetActive(true);
			} else bioContainer.SetActive(false);

			UpdateThumbnail(user);
			UpdateBanner(user);
			UpdatePresence(user);
			UpdateActions(user);
		}

		private void UpdateThumbnail(IUser user) {
			if (string.IsNullOrEmpty(user?.Thumbnail)) {
				thumbnail.sprite = null;
				withThumbnail.SetActive(false);
				withoutThumbnail.SetActive(false);
				return;
			}

			_thumbnailNetworkImage = thumbnail.GetOrAddComponent<NetworkImage>();
			_thumbnailNetworkImage.Url = user.Thumbnail;
			withThumbnail.SetActive(true);
			withoutThumbnail.SetActive(false);
		}

		/// <summary>Updates the banner (independent from the thumbnail).</summary>
		private void UpdateBanner(IUser user) {
			_bannerNetworkImage = banner.GetOrAddComponent<NetworkImage>();

			var url = user.Banner;
			if (string.IsNullOrEmpty(url)) {
				// No banner at all: fall back to the placeholder instead of hiding both slots.
				banner.sprite = null;
				_bannerNetworkImage.Url = null;
				withBanner.SetActive(false);
				withoutBanner.SetActive(true);
				return;
			}

			// Set the URL BEFORE activating the container: NetworkImage only starts loading
			// while active, and activating with an empty URL fires an "empty URL" error that
			// would immediately hide the banner again (it then only loaded on the next refresh).
			_bannerNetworkImage.Url = url;
			withoutBanner.SetActive(false);
			withBanner.SetActive(true);
		}

		/// <summary>Shows the presence dot on the thumbnail, coloured from the user status.</summary>
		private void UpdatePresence(IUser user) {
			var status = user.Presence;
			if (status == null) {
				presence.gameObject.SetActive(false);
				return;
			}

			presence.color = status.Status.ToColor();
			presence.gameObject.SetActive(true);
		}

		#region Actions

		/// <summary>No actions on your own profile, or when not authenticated.</summary>
		private static bool HasActions(IUser user)
			=> Main.Instance?.Current != null && !Main.Instance.Current.Identifier.Equals(user.Identifier);

		private void UpdateActions(IUser user) {
			// Nothing can be done on your own profile (or when not authenticated): the whole actions
			// box is hidden, and it is shown again as soon as a button is available.
			if (!HasActions(user)) {
				ShowActions(false);
				SetActionsInteractable(false);
				return;
			}

			ShowActions(true);
			SetActionsInteractable(true);
			UpdateFavoriteState().Forget();
			UpdateFollowState();
		}

		/// <summary>
		/// Shows/hides the action buttons. The actions box itself is hidden too when there is no
		/// button to display. The favorites are only offered for the users the current user follows.
		/// </summary>
		private void ShowActions(bool visible) {
			followButton.gameObject.SetActive(visible);
			inviteSendButton.gameObject.SetActive(visible);
			inviteRequestButton.gameObject.SetActive(visible);
			favoriteButton.gameObject.SetActive(visible && IsFollowing);
			actionsContainer.SetActive(visible);
		}

		/// <summary>
		/// Relation of the current user towards <see cref="_user"/>. The search endpoint (and therefore
		/// the friends list) does not carry it, so it is null until the user has been fetched.
		/// </summary>
		private IUserRelation Relations
			=> _user?.Relations;

		/// <summary>True when the current user follows the displayed user.</summary>
		private bool IsFollowing
			=> (Relations?.Out ?? UserRelationType.NONE) == UserRelationType.FOLLOW;

		/// <summary>Enables/disables every action button without hiding the container.</summary>
		private void SetActionsInteractable(bool value) {
			favoriteButton.interactable      = value;
			followButton.interactable        = value;
			inviteSendButton.interactable    = value;
			inviteRequestButton.interactable = value;
		}

		#region Favorite

		private async UniTask UpdateFavoriteState() {
			if (_user == null) return;
			var favorites = await Main.Instance.GetFavorites();
			_isFavorite = favorites?.Values != null && Array.Exists(favorites.Values, v => v.Equals(_user.Identifier));
			HoverFavorite(_isFavoriteHover);
		}

		private void HoverFavorite(bool isHover) {
			if (_user == null || !HasActions(_user)) return;
			_isFavoriteHover    = isHover;
			favoriteIcon.sprite = Client.GetAsset<Sprite>($"ui:icons/{(isHover ? _isFavorite ? "bookmark_remove" : "bookmark_add" : _isFavorite ? "bookmark_star" : "bookmark")}.png");
			favoriteLabel.UpdateText(
				isHover
					? _isFavorite ? "user.favorite.remove" : "user.favorite.add"
					: _isFavorite ? "user.favorite.star"   : "user.favorite.none"
			);
		}

		private async UniTask OnFavoriteClickedAsync() {
			if (_user == null || !favoriteButton.interactable) return;

			_isFavorite                 = !_isFavorite;
			favoriteButton.interactable = false;
			HoverFavorite(_isFavoriteHover);

			var favorites = _isFavorite
				? await Main.Instance.AddFavorite(_user.Identifier)
				: await Main.Instance.RemoveFavorite(_user.Identifier);

			_isFavorite = favorites?.Values != null && Array.Exists(favorites.Values, v => v.Equals(_user.Identifier));
			HoverFavorite(_isFavoriteHover);
			favoriteButton.interactable = true;
		}

		#endregion

		#region Follow / Friend request

		/// <summary>
		/// Renders the follow button from the relation between the current user and the target:
		/// a pending request received from the target is accepted (and refused while hovered),
		/// otherwise the button follows or unfollows.
		/// Users coming from a search result or from the friends list do not carry their relation,
		/// so it is fetched once and the button stays disabled until it is known.
		/// </summary>
		private void UpdateFollowState() {
			_isFollowHover = false;
			SetFollowInteractable(Relations != null);
			HoverFollow(false);

			if (Relations == null && _user != null)
				FetchRelationAsync(_user.Identifier).Forget();
		}

		/// <summary>
		/// Resolves the relation of the current user towards <paramref name="identifier"/>: the search
		/// endpoint does not return it, so the whole user is fetched and replaces the displayed one.
		/// </summary>
		private async UniTaskVoid FetchRelationAsync(Identifier identifier) {
			var fetched = await Main.Instance.Fetch(identifier);
			// The displayed profile may have changed while the request was in flight.
			if (fetched == null || _user == null || !_user.Identifier.Equals(identifier))
				return;

			SetUser(fetched);
			SetFollowInteractable(Relations != null);
			HoverFollow(_isFollowHover);
		}

		/// <summary>Replaces the displayed user without rebuilding the interface.</summary>
		private void SetUser(IUser user) {
			_user = user;
			Page?.UpdateUserCache(user);
		}

		private void SetFollowInteractable(bool value)
			=> followButton.interactable = value;

		private void HoverFollow(bool isHover) {
			if (_user == null || !HasActions(_user)) return;
			_isFollowHover = isHover;

			// A pending request received from that user takes priority over everything else: the
			// button accepts it, and refuses it while hovered (the hover reveals the other action).
			if ((Relations?.In ?? UserRelationType.NONE) == UserRelationType.REQUEST) {
				followIcon.sprite = Client.GetAsset<Sprite>("ui:icons/success.png");
				followLabel.UpdateText(isHover ? "user.follow.refuse" : "user.follow.accept");
			} else switch (Relations?.Out ?? UserRelationType.NONE) {
				case UserRelationType.FOLLOW:
					followIcon.sprite = Client.GetAsset<Sprite>($"ui:icons/{(isHover ? "cancel" : "friend")}.png");
					followLabel.UpdateText(isHover ? "user.follow.unfollow" : "user.follow.following");
					break;
				case UserRelationType.REQUEST:
					followIcon.sprite = Client.GetAsset<Sprite>($"ui:icons/{(isHover ? "cancel" : "person_add")}.png");
					followLabel.UpdateText(isHover ? "user.follow.cancel" : "user.follow.requested");
					break;
				default:
					followIcon.sprite = Client.GetAsset<Sprite>("ui:icons/person_add.png");
					followLabel.UpdateText("user.follow.follow");
					break;
			}

			// The favorites are only offered for the users the current user follows.
			favoriteButton.gameObject.SetActive(IsFollowing);
		}

		private async UniTask OnFollowClickedAsync() {
			if (_user == null || !followButton.interactable) return;

			var outgoing = Relations?.Out ?? UserRelationType.NONE;
			// A request received from that user is accepted, or refused when the button is hovered.
			var isRequest = (Relations?.In ?? UserRelationType.NONE) == UserRelationType.REQUEST;
			var accept    = !_isFollowHover;

			followButton.interactable = false;
			try {
				var ok = isRequest
					? await Main.Instance.RespondToRequest(_user.Identifier, accept)
					: outgoing == UserRelationType.NONE
						? await Main.Instance.Follow(_user.Identifier)
						: await Main.Instance.Unfollow(_user.Identifier);

				if (ok) {
					// The relation changed server-side: refresh it so the button follows, without
					// rebuilding the page (the images would be reloaded).
					var refreshed = await Main.Instance.Fetch(_user.Identifier);
					if (refreshed != null)
						SetUser(refreshed);
				}

				// The hover has been consumed: render the new state without the refusal hint.
				_isFollowHover = false;
				HoverFollow(false);
			} finally {
				followButton.interactable = Relations != null;
			}
		}

		#endregion

		#region Invitations

		// The server does not implement invitations yet: the buttons are shown but inactive.
		private void OnInviteSendClicked()
			=> Logger.LogWarning("Sending an invitation is not implemented yet.", tag: nameof(UserComponent));

		private void OnInviteRequestClicked()
			=> Logger.LogWarning("Requesting an invitation is not implemented yet.", tag: nameof(UserComponent));

		#endregion

		#endregion

		public void UpdateError(string error) {
			title.UpdateText("user.error");
			identifier.UpdateText("user.error");
			label.UpdateText("user.error");
			thumbnail.sprite = null;
			withThumbnail.SetActive(false);
			withoutThumbnail.SetActive(true);
			bioContainer.SetActive(false);
			banner.sprite = null;
			banner.GetOrAddComponent<NetworkImage>().Url = null;
			withBanner.SetActive(false);
			withoutBanner.SetActive(true);
			presence.gameObject.SetActive(false);
			ShowActions(false);
			SetActionsInteractable(false);
		}

		public void UpdateLoading() {
			title.UpdateText("user.loading");
			identifier.UpdateText("user.loading");
			label.UpdateText("user.loading");
			thumbnail.sprite = null;
			withThumbnail.SetActive(false);
			withoutThumbnail.SetActive(true);
			bioContainer.SetActive(false);
			banner.sprite = null;
			banner.GetOrAddComponent<NetworkImage>().Url = null;
			withBanner.SetActive(false);
			withoutBanner.SetActive(true);
			presence.gameObject.SetActive(false);
			ShowActions(false);
			SetActionsInteractable(false);
		}

		public static (GameObject, UserComponent) Generate(UserPage userPage, RectTransform parent) {
			var content              = Client.GetAsset<GameObject>("ui:prefabs/split.prefab").Instantiate(parent);
			var iconAsset            = Client.GetAsset<GameObject>("ui:prefabs/header_icon.prefab");
			var labelAsset           = Client.GetAsset<GameObject>("ui:prefabs/header_label.prefab");
			var withTitleAsset       = Client.GetAsset<GameObject>("ui:prefabs/with_title.prefab");
			var listAsset            = Client.GetAsset<GameObject>("ui:prefabs/list.prefab");
			var scrollAsset          = Client.GetAsset<GameObject>("ui:prefabs/scroll.prefab");
			var boxAsset             = Client.GetAsset<GameObject>("ui:prefabs/box.prefab");
			var actionButtonAsset    = Client.GetAsset<GameObject>("ui:prefabs/action_button.prefab");
			var actionContainerAsset = Client.GetAsset<GameObject>("ui:prefabs/action_container.prefab");

			var component = content.AddComponent<UserComponent>();
			component.Page = userPage;
			content.name   = $"[{userPage.GetKey()}_{content.GetId()}]";

			var splitContent   = Reference.GetComponent<RectTransform>("content", content);
			var containerAsset = Client.GetAsset<GameObject>("ui:prefabs/container.prefab");

			// generate profile
			var container = containerAsset.Instantiate(splitContent);
			var profile = Client.GetAsset<GameObject>("ui:prefabs/profile.prefab")
				.Instantiate(Reference.GetComponent<RectTransform>("content", container));

			component.identifier       = Reference.GetComponent<TextLanguage>("identifier", profile);
			component.title            = Reference.GetComponent<TextLanguage>("title", profile);
			component.thumbnail        = Reference.GetComponent<Image>("thumbnail", profile);
			component.withThumbnail    = Reference.GetReference("with_thumbnail", profile);
			component.withoutThumbnail = Reference.GetReference("without_thumbnail", profile);
			component.banner           = Reference.GetComponent<Image>("banner", profile);
			component.bannerAspect     = Reference.GetComponent<AspectRatioFitter>("banner_aspect", profile);
			component.withBanner       = Reference.GetReference("with_banner", profile);
			component.withoutBanner    = Reference.GetReference("without_banner", profile);
			component.presence         = Reference.GetComponent<Image>("presence", profile);

			// Keep the banner aspect ratio in sync with the loaded texture.
			var bannerNetworkImage = component.banner.GetOrAddComponent<NetworkImage>();
			bannerNetworkImage.OnSuccess.AddListener(texture => {
				if (texture.height > 0) {
					component.bannerAspect.aspectRatio = (float)texture.width / texture.height;
					component.withBanner.SetActive(true);
					component.withoutBanner.SetActive(false);
				}
			});
			bannerNetworkImage.OnError.AddListener(_ => {
				// An empty URL means "no banner": keep the loading skeleton as-is.
				if (string.IsNullOrEmpty(bannerNetworkImage.Url)) return;
				component.withBanner.SetActive(false);
				component.withoutBanner.SetActive(true);
			});

			// generate dashboard
			container = Client.GetAsset<GameObject>("ui:prefabs/container_full.prefab").Instantiate(splitContent);
			var withTitle = withTitleAsset.Instantiate(Reference.GetComponent<RectTransform>("content", container));

			var header = Reference.GetReference("header", withTitle);
			var icon   = iconAsset.Instantiate(Reference.GetComponent<RectTransform>("before", header));
			var label  = labelAsset.Instantiate(Reference.GetComponent<RectTransform>("content", header));

			component.labelIcon        = Reference.GetComponent<Image>("image", icon);
			component.label            = Reference.GetComponent<TextLanguage>("text", label);
			component.labelIcon.sprite = Client.GetAsset<Sprite>("ui:icons/person.png");

			var contentDash = Reference.GetComponent<RectTransform>("content", withTitle);
			// setup scroll + list
			var scroll = scrollAsset.Instantiate(contentDash);
			var list   = listAsset.Instantiate(Reference.GetComponent<RectTransform>("content", scroll));
			component.content = Reference.GetComponent<RectTransform>("content", list);

			// add box actions
			component.actionsContainer = boxAsset.Instantiate(component.content);
			Reference.GetComponent<TextLanguage>("text", component.actionsContainer).UpdateText("user.about.actions");
			component.actions = Reference.GetComponent<RectTransform>("content", actionContainerAsset.Instantiate(Reference.GetComponent<RectTransform>("content", component.actionsContainer)));

			#region Invite Buttons (not implemented yet)

			var inviteSend             = actionButtonAsset.Instantiate(component.actions);
			var inviteSendEventTrigger = Reference.GetComponent<EventTrigger>("button", inviteSend);
			component.inviteSendButton = Reference.GetComponent<Button>("button", inviteSend);
			component.inviteSendIcon   = Reference.GetComponent<Image>("image", inviteSend);
			component.inviteSendLabel  = Reference.GetComponent<TextLanguage>("text", inviteSend);
			component.inviteSendLabel.UpdateText("user.invite.send");
			component.inviteSendIcon.sprite = Client.GetAsset<Sprite>("ui:icons/upload.png");
			SetupEvents(
				inviteSendEventTrigger,
				() => component.OnInviteSendClicked(),
				() => { },
				() => { }
			);

			var inviteRequest             = actionButtonAsset.Instantiate(component.actions);
			var inviteRequestEventTrigger = Reference.GetComponent<EventTrigger>("button", inviteRequest);
			component.inviteRequestButton = Reference.GetComponent<Button>("button", inviteRequest);
			component.inviteRequestIcon   = Reference.GetComponent<Image>("image", inviteRequest);
			component.inviteRequestLabel  = Reference.GetComponent<TextLanguage>("text", inviteRequest);
			component.inviteRequestLabel.UpdateText("user.invite.request");
			component.inviteRequestIcon.sprite = Client.GetAsset<Sprite>("ui:icons/download.png");
			SetupEvents(
				inviteRequestEventTrigger,
				() => component.OnInviteRequestClicked(),
				() => { },
				() => { }
			);

			#endregion

			#region Follow / Friend request Button

			var follow             = actionButtonAsset.Instantiate(component.actions);
			var followEventTrigger = Reference.GetComponent<EventTrigger>("button", follow);
			component.followButton = Reference.GetComponent<Button>("button", follow);
			component.followIcon   = Reference.GetComponent<Image>("image", follow);
			component.followLabel  = Reference.GetComponent<TextLanguage>("text", follow);
			component.followLabel.UpdateText("user.follow.follow");
			component.followIcon.sprite = Client.GetAsset<Sprite>("ui:icons/person_add.png");
			SetupEvents(
				followEventTrigger,
				() => component.OnFollowClickedAsync().Forget(),
				() => component.HoverFollow(true),
				() => component.HoverFollow(false)
			);

			#endregion

			#region Favorite Button (only offered for the followed users)

			var favorite             = actionButtonAsset.Instantiate(component.actions);
			var favoriteEventTrigger = Reference.GetComponent<EventTrigger>("button", favorite);
			component.favoriteButton = Reference.GetComponent<Button>("button", favorite);
			component.favoriteIcon   = Reference.GetComponent<Image>("image", favorite);
			component.favoriteLabel  = Reference.GetComponent<TextLanguage>("text", favorite);
			component.favoriteLabel.UpdateText("user.favorite.none");
			component.favoriteIcon.sprite = Client.GetAsset<Sprite>("ui:icons/bookmark.png");
			SetupEvents(
				favoriteEventTrigger,
				() => component.OnFavoriteClickedAsync().Forget(),
				() => component.HoverFavorite(true),
				() => component.HoverFavorite(false)
			);

			#endregion

			// add box description
			component.bioContainer = boxAsset.Instantiate(component.content);
			Reference.GetComponent<TextLanguage>("text", component.bioContainer).UpdateText("user.about.bio");
			component.bioText = Reference.GetComponent<TextLanguage>(
				"text",
				Client.GetAsset<GameObject>("ui:prefabs/text.prefab")
					.Instantiate(Reference.GetComponent<RectTransform>("content", component.bioContainer))
			);

			return (content, component);
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private static void SetupEvents(EventTrigger eventTrigger, Action click, Action enter, Action exit) {
			if (!eventTrigger)
				return;
			var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
			entry.callback.AddListener(_ => click());
			eventTrigger.triggers.Add(entry);
			entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
			entry.callback.AddListener(_ => enter());
			eventTrigger.triggers.Add(entry);
			entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
			entry.callback.AddListener(_ => exit());
			eventTrigger.triggers.Add(entry);
		}
	}
}