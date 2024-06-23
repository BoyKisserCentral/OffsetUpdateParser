namespace PG3DOffsetUpdater
{
    public class Offset
    {
        public Offset(string name, int oldOffset, string text, int newOffset, bool textFound, bool newFound, int minimum, int newLine)
        {
            this.Name = name;
            this.OldOffset = oldOffset;
            this.Text = text;
            this.NewOffset = newOffset;
            this.TextFound = textFound;
            this.NewFound = newFound;
            this.Minimum = minimum;
            this.NewLine = newLine;
        }

        public string Name { get; set; }
        public int OldOffset { get; set; }
        public string Text { get; set; }
        public int NewOffset { get; set; }
        public bool TextFound { get; set; }
        public bool NewFound { get; set; }
        public int Minimum { get; set; }
        public int NewLine { get; set; }
    }
}