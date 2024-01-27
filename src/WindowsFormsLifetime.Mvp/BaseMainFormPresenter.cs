namespace WindowsFormsLifetime.Mvp;

public abstract class BaseMainFormPresenter<TView>
    where TView : class
{
    protected BaseMainFormPresenter(TView view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    public TView View { get; }
}