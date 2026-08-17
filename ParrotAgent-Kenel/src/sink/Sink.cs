namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Sink
    {
        /// <summary>
        /// 
        /// </summary>
        public SinkChannel Input { get; } = new SinkChannel();
        /// <summary>
        /// 
        /// </summary>
        public SinkChannel Output { get; } = new SinkChannel();
    }
}
