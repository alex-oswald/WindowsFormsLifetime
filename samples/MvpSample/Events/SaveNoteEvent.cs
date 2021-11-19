using WindowsFormsLifetime.Mvp;

namespace MvpSample.Events
{
    internal class SaveNoteEvent : IEvent
    {
        public SaveNoteEvent(Note note)
        {
            Note = note;
        }

        public Note Note { get; }
    }
}