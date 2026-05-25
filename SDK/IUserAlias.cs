namespace Nox.Users {
	/// <summary>
	/// Represents a user alias,
	/// which is a key-value pair that can be used to store additional information about a user.
	/// The most common are api, iid, uid and web.
	/// </summary>
	public interface IUserAlias {
		/// <summary>
		/// Gets the key of the alias,
		/// which is a string that identifies the type of alias.
		/// Is a snake-case string that indicates the source or nature of the alias.
		/// </summary>
		public string Key { get; }
		
		/// <summary>
		/// Gets the value of the alias,
		/// which is a string that contains the actual alias information.
		/// </summary>
		public string Value { get; }
	}
}