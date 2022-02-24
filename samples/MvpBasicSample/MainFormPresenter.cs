using WindowsFormsLifetime.Mvp;

namespace MvpBasicSample
{
    internal class MainFormPresenter : BaseMainFormPresenter<IMainView>
    {
        public MainFormPresenter(IMainView view)
            : base(view)
        {
            View.OnIncrementClicked += OnIncrementClicked;
            View.Count = 0;
        }

        private void OnIncrementClicked(object? sender, EventArgs e)
        {
            View.Count++;
        }
    }
}