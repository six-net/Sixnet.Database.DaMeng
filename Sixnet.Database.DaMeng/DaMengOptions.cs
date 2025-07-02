namespace Sixnet.Database.DaMeng
{
    /// <summary>
    /// Defines DaMeng options
    /// </summary>
    public class DaMengOptions
    {
        /// <summary>
        /// Indicates whether wrap field and table name with quotes
        /// Default value is true
        /// </summary>
        public bool WrapWithQuotes { get; set; } = true;

        /// <summary>
        /// Indicates whether formatting guid
        /// </summary>
        public bool FormattingGuid { get; set; } = true;
    }
}
