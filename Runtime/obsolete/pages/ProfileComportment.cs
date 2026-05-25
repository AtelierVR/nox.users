// using Cysharp.Threading.Tasks;
// using Nox.CCK.Language;
// using UnityEngine;
// using UnityEngine.UI;
// using Logger = Nox.CCK.Utils.Logger;
//
// namespace api.nox.user.pages
// {
//     [RequireComponent(typeof(RectTransform))]
//     public class ProfileComportment : MonoBehaviour
//     {
//         public GameObject withBanner;
//         public GameObject withoutBanner;
//         public RawImage bannerImage;
//
//         public RawImage thumbnailImage;
//         public GameObject thumbnailContainer;
//
//         public TextLanguage displayText;
//
//         public TextLanguage identifierText;
//         public Button identifierButton;
//
//         public GameObject statusContainer;
//         public Image statusImage;
//
//         public GameObject descriptionContainer;
//         public TextLanguage descriptionText;
//
//         public Button refreshButton;
//         public Button backButton;
//
//         private ProfilePage _page;
//
//         internal void Initiate(ProfilePage page)
//         {
//             if (_page != null) return;
//             _page = page;
//         }
//
//         private void Awake()
//         {
//             if (_page == null)
//             {
//                 Logger.LogError("ProfileComportment: ProfilePage is not initiated.");
//                 return;
//             }
//
//             refreshButton.onClick.AddListener(OnRefresh);
//             backButton.onClick.AddListener(OnBack);
//         }
//
//         private void OnRefresh()
//             => _page?.Refresh().Forget();
//
//         private void OnBack()
//             => _page?.GoBack();
//
//         private void OnDestroy()
//         {
//             refreshButton.onClick.RemoveListener(OnRefresh);
//             backButton.onClick.RemoveListener(OnBack);
//         }
//         
//         private async UniTask FetchBanner(string url)
//         {
//             var hasBanner = !string.IsNullOrEmpty(url);
//             if (!hasBanner)
//             {
//                 bannerImage.texture = null;
//                 withBanner.SetActive(false);
//                 withoutBanner.SetActive(true);
//                 return;
//             }
//
//             var hasTexture = bannerImage.texture;
//             if (hasTexture)
//             {
//                 withBanner.SetActive(true);
//                 withoutBanner.SetActive(false);
//                 return;
//             }
//
//             try
//             {
//                 var texture = await Main.NetworkAPI
//                     .CallAsyncMethod<Texture2D>("FetchTexture", url, null, null, null);
//                 if (!texture)
//                 {
//                     bannerImage.texture = null;
//                     withBanner.SetActive(false);
//                     withoutBanner.SetActive(true);
//                     return;
//                 }
//
//                 bannerImage.texture = texture;
//                 withBanner.SetActive(true);
//                 withoutBanner.SetActive(false);
//                 return;
//             }
//             catch
//             {
//                 // ignored
//             }
//
//             bannerImage.texture = null;
//             withBanner.SetActive(false);
//             withoutBanner.SetActive(true);
//         }
//
//
//         internal void UpdateData()
//         {
//             var user = _page.User;
//             var isFetching = _page.IsFetching;
//             refreshButton.interactable = !isFetching;
//
//             if (user == null && isFetching)
//             {
//                 displayText.UpdateText("user.loading");
//                 identifierText.UpdateText("user.loading");
//                 descriptionContainer.SetActive(false);
//                 withoutBanner.SetActive(true);
//                 withBanner.SetActive(false);
//                 thumbnailImage.texture = null;
//                 identifierButton.interactable = false;
//                 statusContainer.SetActive(false);
//                 return;
//             }
//
//             if (user == null)
//             {
//                 displayText.UpdateText("user.not_found");
//                 identifierText.UpdateText("user.not_found");
//                 descriptionContainer.SetActive(false);
//                 withoutBanner.SetActive(true);
//                 withBanner.SetActive(false);
//                 thumbnailImage.texture = null;
//                 identifierButton.interactable = false;
//                 statusContainer.SetActive(false);
//                 return;
//             }
//
//             var server = user.GetField<string>("server");
//             var display = user.GetField<string>("display");
//             var username = user.GetField<string>("username");
//
//             displayText.UpdateText("user.display", new[] { display, username });
//
//             var identifier = user.CallMethod("ToIdentifier", true);
//             var identifierStr = identifier.CallMethod<string>("ToMinimalString", server);
//             identifierText.UpdateText("user.identifier", new[] { identifierStr });
//             identifierButton.interactable = true;
//
//             FetchBanner(user.GetField<string>("banner")).Forget();
//
//             var strIcon = user.GetField<string>("thumbnail");
//             if (!string.IsNullOrEmpty(strIcon))
//                 Client.FetchTexture(thumbnailImage, thumbnailContainer, strIcon).Forget();
//             else thumbnailContainer.SetActive(false);
//
//             var description = user.GetField<string>("bio");
//             if (!string.IsNullOrEmpty(description))
//             {
//                 descriptionText.UpdateText(new[] { description });
//                 descriptionContainer.SetActive(true);
//             }
//             else descriptionContainer.SetActive(false);
//         }
//     }
// }