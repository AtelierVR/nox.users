using api.nox.user.client;
using Cysharp.Threading.Tasks;
using Nox.Network;
using Nox.Search;
using UnityEngine;

namespace api.nox.user.search {
	public class SearchData : IResultData {
		public User Reference;

		public int Id
			=> Reference.Identifier.ToString().GetHashCode();

		public string[] TitleArguments
			=> new[] { Reference.Display ?? Reference.Username };

		public UniTask<Texture2D> Image
			=> Client.Instance.CoreAPI.ModAPI.GetMod("network")
					?.GetInstance<INetworkAPI>()
					?.FetchTexture(Reference.Thumbnail)
				?? UniTask.FromResult<Texture2D>(null);

		public void OnClick(int menuId)
			=> Client.UiAPI?.SendGoto(menuId, UserPage.GetStaticKey(), "user", Reference);
	}
}