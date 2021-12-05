using MvpSample.Data;

namespace MvpSample.Events
{
    internal record RefreshListEvent(Note? SelectedNote = null) : IEvent
    {
    }
}