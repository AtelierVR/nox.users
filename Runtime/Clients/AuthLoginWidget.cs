using Nox.UI;
using UnityEngine;

namespace Nox.Users.Runtime.Clients {
	public class AuthLoginWidget : IPage {
		internal static string GetStaticKey()
			=> "login";

		public string GetKey()
			=> GetStaticKey();

		public object[] GetContext() {
			throw new System.NotImplementedException();
		}
		public IMenu    GetMenu() {
			throw new System.NotImplementedException();
		}

		private int        _mId;
		private object[]   _context;
		private GameObject _content;


		internal static IPage OnGotoAction(IMenu menu, object[] context) {
			var page = new AuthLoginWidget {
				_mId     = menu.Id,
				_context = context
			};
			return page;
		}
	}
}