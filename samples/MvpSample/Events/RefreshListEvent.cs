using MvpSample.Data;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Events
{
    internal record RefreshListEvent(Note? SelectedNote = null) : IEvent
    {
    }
}