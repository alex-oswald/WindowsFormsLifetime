using MvpSample.Data;

namespace MvpSample.Events
{
    internal record SelectedNoteChangedEvent(Note SelectedNote): IEvent
    {
    }
}