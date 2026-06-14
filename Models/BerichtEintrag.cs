namespace AzubiLog.Models
{
    public class BerichtEintrag
    {
        public int Id { get; set; }
        public int BenutzerId { get; set; }
        public User Benutzer { get; set; } = null!;
        public int? AusbilderId { get; set; }
        public Ausbilder? Ausbilder { get; set; }
        public int? KategorieId { get; set; }
        public Kategorie? Kategorie { get; set; }
        public int WochenberichtId { get; set; }
        public Wochenbericht Wochenbericht { get; set; } = null!;
        public DateTime Datum { get; set; } = DateTime.Now;
        public string Tagestyp { get; set; } = "Betrieb";
        public string? Auftragsnummer { get; set; }
        public string Titel { get; set; } = "";
        public string Beschreibung { get; set; } = "";
        public string Notiz { get; set; } = "";
        public string? Fach { get; set; }
        public DateTime Startzeit { get; set; }
        public DateTime Endzeit { get; set; }
        public decimal? Dauer { get; set; }
        public string Status { get; set; } = "";
        public DateTime ErstelltAm { get; set; } = DateTime.Now;
        public DateTime GeändertAm { get; set; } = DateTime.Now;
    }
}
