namespace AzubiLog.Models
{
    public class Wochenbericht
    {
        public int Id { get; set; }
        public int BenutzerId { get; set; }
        public User Benutzer { get; set; } = null!;
        public int Kalenderwoche { get; set; }
        public double Gesamtstunden { get; set; }
        public int Jahr { get; set; }
        public string Status { get; set; } = "";
        public string Kommentar { get; set; } = "";
        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public List<BerichtEintrag> BerichtEintrag { get; set; } = new();
    }
}
