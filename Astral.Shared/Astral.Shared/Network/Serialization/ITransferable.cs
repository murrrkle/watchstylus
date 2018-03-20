
namespace Astral.Net.Serialization
{
    #region Interface 'ITransferable'
    /// <summary>
    ///     <para>Generic interface that allows creating serializable objects.</para>
    /// </summary>
    public interface ITransferable
    {
        /// <summary>
        /// Should implement how data is added to the stream.
        /// </summary>
        /// <param name="info">The stream descriptor containing the data.</param>
        void GetStreamData(NetworkStreamInfo info);
    }
    #endregion
}
