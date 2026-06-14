namespace AzubiLog.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Vorname { get; set; } = "";
        public string Nachname { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswortHash { get; set; } = "";
        public string Schule { get; set; } = "";
        public string Klasse { get; set; } = "";
        public string Ausbildungsberuf { get; set; } = "";
        public bool Aktiv { get; set; } = true;
        public string Rolle { get; set; } = "Azubi";

        public List<Wochenbericht> Wochenberichte { get; set; } = new();
        public List<BerichtEintrag> BerichtEintrag { get; set; } = new();
        public List<Kategorie> Kategorien { get; set; } = new();
    }
}
