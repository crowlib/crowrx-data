namespace CrowRx.Data
{
	/// <summary>
	/// internal use only
	/// </summary>
	public interface ITarget
	{
	}

	/// <summary>
	/// <see cref="TSource"/>에 의해 정보가 갱신되는 class의 기본 인터페이스.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public interface ITarget<TSource> : ITarget
		where TSource : ISource
	{
		/// <summary>
		/// 전달받은 <see cref="TSource"/> object를 사용하여 정보를 갱신한다.
		/// 구현부는 정보 갱신 위주로 최대한 단순하고 짧게 구현할 것.
		/// </summary>
		/// <param name="sourceData">정보 갱신을 위한 <see cref="TSource"/> object</param>
		/// <returns>정보 갱신 후 observer들에게 broadcast가 필요하면 true, 필요하지 않으면 false</returns>
		bool UpdateBy(in TSource sourceData);
	}
}