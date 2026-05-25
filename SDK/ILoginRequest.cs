namespace Nox.Users {
	public interface ILoginRequest {
		public string        GetPassword();
		
		public ILoginRequest SetPassword(string password);
		
		public string        GetIdentifier();

		public ILoginRequest SetIdentifier(string identifier);
		
		public string        GetFactorCode();
		
		public ILoginRequest SetFactorCode(string factorCode);
		
		public string GetPublicKey();
		
		public ILoginRequest SetPublicKey(string publicKey);
	}
}