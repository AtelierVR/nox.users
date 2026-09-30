using Cysharp.Threading.Tasks;
using Nox.Search;
using Nox.Users.Runtime.Base;
using Nox.Users.Runtime.Clients;

namespace Nox.Users.Runtime.Search {
	public class SearchData : IResultData {
		public User Reference;

		public int Id
			=> Reference.Identifier.ToString().GetHashCode();

		public string[] TitleArguments
			=> new[] { Reference.Display ?? Reference.Username };

		public UniTask<ImageSource> Image
			=> UniTask.FromResult(ImageSource.FromUrl(Reference.Thumbnail));

		public void OnClick(int menuId)
			=> Client.UiAPI?.SendGoto(menuId, UserPage.GetStaticKey(), "user", Reference);
	}
}