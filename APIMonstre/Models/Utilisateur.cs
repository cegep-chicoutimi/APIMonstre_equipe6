namespace APIMonstre.Models
{
    public class Utilisateur
    {
        public int IdUtilisateur {  get; set; }
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public string Pseudo {  get; set; } = string.Empty;
        public DateTime DateInscription { get; set; }
        public bool estConnecte { get; set; }

        public Utilisateur() { }

        public Utilisateur(int idUtilisateur, string email, string motDePasse, string pseudo, DateTime dateInscription, bool estConnecte)
        {
            IdUtilisateur = idUtilisateur;
            Email = email;
            MotDePasse = motDePasse;
            Pseudo = pseudo;
            DateInscription = dateInscription;
            this.estConnecte = estConnecte;
        }
    }
}
