using System;
using System.Linq;
using Nox.Search;
using UnityEngine;
using Nox.CCK.Search;

namespace Nox.Users.Runtime.Search {
	public class SearchHandler : IHandler {
		public string GetId()
			=> Main.Instance.CoreAPI.ModMetadata.GetId();

		public string GetTitleKey()
			=> "user.search.title";

		public string[] GetTitleArguments()
			=> Array.Empty<string>();

		public string GetPlaceholderKey()
			=> "user.search.placeholder";

		public string[] GetPlaceholderArguments()
			=> Array.Empty<string>();

		public Texture2D GetIcon()
			=> Main.Instance.CoreAPI.AssetAPI
				.GetAsset<Texture2D>("icons/person.png");

		public string GetDescriptionKey()
			=> "user.search.description";

		public string[] GetDescriptionArguments()
			=> Array.Empty<string>();

		public IWorker[] GetWorkers()
			=> SearchHelper.ServersBy("user")
				.Select(s => new SearchWorker { Title = s.Title, ServerAddress = s.Address })
				.ToArray<IWorker>();
	}
}