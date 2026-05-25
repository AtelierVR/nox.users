using Nox.CCK.Utils;
namespace Nox.Users {
	public interface ISearchRequest {
		public ISearchRequest SetQuery(string     query);
		public ISearchRequest SetIds(Identifier[] ids);
		public ISearchRequest SetOffset(uint      offset);
		public ISearchRequest SetLimit(uint       limit);

		public string GetQuery();
		public Identifier[] GetIds();
		public uint GetOffset();
		public uint GetLimit();
	}
}