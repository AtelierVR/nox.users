// using System;
// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using Nox.CCK.Mods.Events;
// using Nox.CCK.Utils;
// using UnityEngine;
// using Logger = Nox.CCK.Utils.Logger;
// using Object = UnityEngine.Object;
// using Transform = UnityEngine.Transform;
//
// namespace api.nox.user.pages
// {
//     public class ProfilePage
//     {
//         internal static string GetKey() => "user_profile";
//         private static EventSubscription _listener;
//
//         public static void Listen()
//         {
//             Logger.LogDebug("ProfilePage.Listen");
//             _listener = Main.CoreAPI.EventAPI.Subscribe("goto_page", OnGotoEvent);
//         }
//
//         public static void StopListen()
//         {
//             Main.CoreAPI.EventAPI.Unsubscribe(_listener);
//         }
//
//         private static void OnGotoEvent(EventData context)
//         {
//             Logger.LogDebug($"ProfilePage.OnGotoEvent: {context.EventName}");
//             if (!context.TryGet(0, out int menuId)) return;
//             Logger.LogDebug($"ProfilePage.OnGotoEvent: {context.EventName}, {menuId}");
//             if (!context.TryGet(1, out string pageKey)) return;
//             Logger.LogDebug($"ProfilePage.OnGotoEvent: {context.EventName}, {menuId}, {pageKey}");
//             if (!context.TryGet(2, out string type)) return;
//             Logger.LogDebug($"ProfilePage.OnGotoEvent: {context.EventName}, {menuId}, {pageKey}, {type}");
//             if (pageKey != GetKey()) return;
//             switch (type)
//             {
//                 case "id-server" when context.TryGet(3, out string id0) && context.TryGet(4, out uint ser0):
//                     OnPageByIdentifier(menuId, Main.UserAPI.CallMethod("IdentifierById", id0, ser0));
//                     break;
//                 case "username-server" when context.TryGet(3, out string usn1) && context.TryGet(4, out string ser1):
//                     OnPageByIdentifier(menuId, Main.UserAPI.CallMethod("IdentifierByUsername", usn1, ser1));
//                     break;
//                 case "identifier" when context.TryGet(3, out string id2):
//                     OnPageByIdentifier(menuId, Main.UserAPI.CallMethod("IdentifierByString", id2));
//                     break;
//                 case "user" when context.TryGet(3, out INoxObject usr3):
//                     OnPageByUser(menuId, usr3);
//                     break;
//                 default:
//                     context.Callback("Invalid page type");
//                     break;
//             }
//         }
//
//         private static void OnPageByIdentifier(int menuId, INoxObject identifier)
//         {
//             var page = new ProfilePage
//             {
//                 MenuId = menuId,
//                 Identifier = identifier,
//                 User = Main.UserAPI.CallMethod("GetUserByIdentifier", identifier)
//             };
//             page.Display();
//             page.Refresh().Forget();
//         }
//
//         private static void OnPageByUser(int menuId, INoxObject user)
//         {
//             var page = new ProfilePage
//             {
//                 MenuId = menuId,
//                 Identifier = user.CallMethod("ToIdentifier", false),
//                 User = user
//             };
//             page.Display();
//         }
//
//         private void Display()
//             => Main.CoreAPI.EventAPI.Emit("display_page", MenuId, new Dictionary<string, object>
//             {
//                 {
//                     "key", GetKey()
//                 }, // id of the page
//                 {
//                     "content", new Func<Transform, GameObject>(OnContent)
//                 }, // called when the menu need the content of the page (first call)
//                 /*
//                  {
//                      "open", (string key, GameObject go) => OnOpen(key, go)
//                  }, // called once when the page is display for the first time
//                  {
//                      "restore", (string key, GameObject go) => OnRestore(key, go)
//                  }, // called when the menu go back from history and display the page again
//                  {
//                      "remove", (GameObject go) => OnRemove(go)
//                  }, // called when the menu remove the page from history (last call)
//                  {
//                      "display", (string key, GameObject go) => OnDisplay(key, go)
//                  }, // called when the page is displayed
//                  {
//                      "hide", (string key, GameObject go) => OnHide(key, go)
//                  } // called when another page is displayed
//                  */
//             });
//
//         internal int MenuId;
//         internal INoxObject Identifier;
//         internal INoxObject User;
//         internal bool IsFetching;
//         private ProfileComportment _comportment;
//
//         private GameObject OnContent(Transform transform)
//         {
//             var asset = Main.CoreAPI.AssetAPI.GetAsset<GameObject>("prefabs/profile/content.prefab");
//             asset.SetActive(false);
//             var content = Object.Instantiate(asset, transform);
//             _comportment = content.GetComponent<ProfileComportment>();
//             _comportment.Initiate(this);
//             _comportment.UpdateData();
//             Logger.LogDebug($"ProfilePage.OnGetContent: {asset.name} {content.name}");
//             content.name = $"{GetKey()}_{content.name}";
//             return content;
//         }
//
//         internal void GoBack()
//             => Main.CoreAPI.EventAPI.Emit("goto_action", MenuId, "back");
//
//         internal async UniTask Refresh()
//         {
//             if (IsFetching || User == null) return;
//             IsFetching = true;
//             _comportment.UpdateData();
//             await User.InvokeAsyncMethod("Refresh");
//             IsFetching = false;
//             _comportment.UpdateData();
//         }
//     }
// }