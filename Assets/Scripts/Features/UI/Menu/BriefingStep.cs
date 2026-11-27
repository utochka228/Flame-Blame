namespace FlameBlame.UI.Menu
{
    public readonly struct BriefingStep
    {
        public string Title { get; }
        public string Body { get; }

        public BriefingStep(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }
}
