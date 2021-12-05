namespace WindowsFormsLifetime.Mvp
{
    public abstract class BaseMainFormPresenter<TStartForm, TView>
        where TStartForm : Form, TView
        where TView : class
    {
        public BaseMainFormPresenter(ApplicationContext applicationContext)
        {
            View = applicationContext.MainForm as TView
                ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        public TView View { get; }
    }
}