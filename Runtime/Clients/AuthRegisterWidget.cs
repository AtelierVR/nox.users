using Nox.UI;

namespace Nox.Users.Runtime.Clients {
	public class AuthRegisterWidget : IPage {
		internal static string GetStaticKey()
			=> "register";

		public string GetKey()
			=> GetStaticKey();

		public object[] GetContext() {
			throw new System.NotImplementedException();
		}
		public IMenu    GetMenu() {
			throw new System.NotImplementedException();
		}
	}
}