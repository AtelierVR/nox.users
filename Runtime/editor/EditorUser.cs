#if UNITY_EDITOR
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;

namespace api.nox.user {
	public class EditorUser : IEditorModInitializer {
		internal static IEditorModCoreAPI      CoreAPI;
		public static   AuthentificationPanel Auth;
		public static   ProfilePanel          Profile;

		public void OnInitializeEditor(IEditorModCoreAPI api) => CoreAPI = api;
		public void OnDisposeEditor()                         => CoreAPI = null;
	}
}
#endif