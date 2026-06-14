namespace AzubiLog.Models
{
    public class Kategorie
    {
        public int ID { get; set; }
        public int BenutzerId { get; set; }
        public User Benutzer { get; set; } = null!;
        public string Name { get; set; } = "";
        public string FarbeHex { get; set; } = "#000000";
        public int Reihenfolge { get; set; }
    }
}
