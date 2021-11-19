namespace MvpSample
{
    public record Note
    {
        public DateTime CreatedOn { get; } = DateTime.Now;

        public string? Notes { get; set; }

        public string Summary
        {
            get
            {
                if (Notes is null)
                {
                    return $"{CreatedOn:o}";
                }

                if (Notes.Length > 20)
                {
                    return $"{CreatedOn:o} {Notes?.Take(17)}...";
                }

                return $"{CreatedOn:o} {Notes}";
            }
        }
    }
}