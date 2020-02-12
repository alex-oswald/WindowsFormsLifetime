using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    public class WindowsFormsLifetimeOptions
    {
        /// <summary>
        /// Indicates the <see cref="HighDpiMode"/>.
        /// The default is <see cref="HighDpiMode.SystemAware"/>.
        /// </summary>
        public HighDpiMode HighDpiMode { get; set; } = HighDpiMode.SystemAware;

        /// <summary>
        /// Indicates if visual styles are enabled.
        /// The default is true.
        /// </summary>
        public bool EnableVisualStyles { get; set; } = true;

        /// <summary>
        /// Indicates if compatible text rendering is enabled.
        /// The default is false.
        /// </summary>
        public bool CompatibleTextRenderingDefault { get; set; }

        /// <summary>
        /// Indicates if host lifetime status messages should be supressed such as on startup.
        /// The default is false.
        /// </summary>
        public bool SuppressStatusMessages { get; set; }

        /// <summary>
        /// Enables listening for Ctrl+C to additionally initiate shutdown.
        /// The default is false.
        /// </summary>
        public bool EnableConsoleShutdown { get; set; }
    }
}