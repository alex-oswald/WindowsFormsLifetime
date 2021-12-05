namespace WindowsFormsLifetime.Mvp
{
    public abstract class BaseMainFormPresenter<TView>
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