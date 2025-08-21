namespace WellandPoolLeagueMud.Data.Models
{
    public class Changelog
    {
        public int ID { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Author { get; set; } = null!;
    }
}
