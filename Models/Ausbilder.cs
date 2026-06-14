namespace AzubiLog.Models
{
    public class Ausbilder
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Abteilung { get; set; } = "";

        public List<BerichtEintrag> BerichtEintrag { get; set; } = new();
    }
}
