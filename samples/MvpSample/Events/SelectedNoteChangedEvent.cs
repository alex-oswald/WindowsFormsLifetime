using MvpSample.Data;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Events
{
    internal record SelectedNoteChangedEvent(Note SelectedNote): IEvent
    {
    }
}