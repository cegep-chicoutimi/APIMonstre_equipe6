using APIMonstre.Data.Context;
using APIMonstre.Models;
using System.Security.Cryptography.X509Certificates;

namespace APIMonstre.Models.Dto
{
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public string Pseudo { get; set; }
    }
    public class InstanceMonstreDto
    {
        public int MonstreId { get; set; }
        public string Nom { get; set; }
        public string SpriteUrl { get; set; }
        public int Niveau { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }

        // Position sur la carte
        public int X { get; set; }
        public int Y { get; set; }

        // Statistiques calculées du monstre
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Attaque { get; set; }
        public int Defense { get; set; }

        // Informations supplémentaires
        public int ExperienceDonnee { get; set; }
        public bool EstVivant { get; set; }
    }

    public class TuileAvecInfosDto
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public TypeTuile TypeTuile { get; set; }
        public string ImageURL { get; set; }

        // Monstre présent sur la tuile (null si aucun)
        public InstanceMonstreDto Monstre { get; set; }

        // Indique si le joueur peut se déplacer sur cette tuile
        public bool EstAccessible { get; set; }

        public static InstanceMonstreDto ConvertirInstanceMonstreVersDto(InstanceMonstre instanceMonstre, MonstreContext context)
        {
            instanceMonstre.Monstre = context.Monster.FirstOrDefault(m => m.IdMonster == instanceMonstre.MonstreId);

            return new InstanceMonstreDto
            {
                MonstreId = instanceMonstre.MonstreId,
                Nom = instanceMonstre.Monstre.Name,
                SpriteUrl = instanceMonstre.Monstre.SpriteURL,
                Niveau = instanceMonstre.Niveau,
                Type1 = instanceMonstre.Monstre.Type1,
                Type2 = instanceMonstre.Monstre.Type2,
                X = instanceMonstre.PositionX,
                Y = instanceMonstre.PositionY,
                PointsVieActuels = instanceMonstre.PointsVieActuels,
                PointsVieMax = instanceMonstre.PointsVieMax,
                Attaque = instanceMonstre.CalculerAttaque(),
                Defense = instanceMonstre.CalculerDefense(),
                ExperienceDonnee = (instanceMonstre.Monstre.ExperienceBase + instanceMonstre.Niveau) * 10,
                EstVivant = instanceMonstre.PointsVieActuels > 0
            };
        }

        public static TuileAvecInfosDto ConvertirTuileVersDto(Tuile tuile, MonstreContext context)
        {
            InstanceMonstre? monstre = context.InstanceMonstre.FirstOrDefault(im => im.PositionX == tuile.PositionX && im.PositionY == tuile.PositionY);

            return new TuileAvecInfosDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                ImageURL = tuile.ImageURL,
                TypeTuile = (TypeTuile)tuile.Type,
                EstAccessible = tuile.EstTraversable,
                Monstre = monstre != null ? ConvertirInstanceMonstreVersDto(monstre, context) : null
            };
        }

    }
    public class PersonnageInfosCombatDto
    {
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool Victoire { get; set; } 
        public bool Defaite { get; set; }
        public PersonnageLevelUpDto? LevelUp { get; set; } = null;
        public ChasseQuetesDto? ChasseQuetes { get; set; }
        public LevelUpQuetesDto? LevelUpQuetes { get; set; }
        public RandonneQuetesDto? RandonneQuetes { get; set; }

        public PersonnageInfosCombatDto() { }

        public PersonnageInfosCombatDto(Personnage personnage, bool victoire, bool defaite, PersonnageLevelUpDto? levelUp)
        {
            Experience = personnage.Experience;
            PointsVie = personnage.PointsVie;
            PointsVieMax = personnage.PointsVieMax;
            PositionX = personnage.PositionX;
            PositionY = personnage.PositionY;
            Victoire = victoire;
            Defaite = defaite;
            LevelUp = levelUp;
        }
    }
    public class PersonnageLevelUpDto
    {
        public int Niveau { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int SeuilsExperienceProchainNiveau { get; set; }

        public PersonnageLevelUpDto() { }
    }

    public class LoginResponseDto
    {
        public int IdUtilisateur { get; set; }
        public string Email { get; set; }
        public string Pseudo { get; set; }
        public bool EstConnecte { get; set; }
        public PersonnageDto Personnage { get; set; }

        public LoginResponseDto() { }
        public LoginResponseDto(int idUtilisateur, string email, string pseudo, PersonnageDto personnage, bool estConnecte)
        {
            IdUtilisateur = idUtilisateur;
            Email = email;
            Pseudo = pseudo;
            Personnage = personnage;
            EstConnecte = estConnecte;
        }
    }
    public class PersonnageDto
    {
        public int IdPersonnage { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int DernierVillageX { get; set; }
        public int DernierVillageY { get; set; }
        public ChasseQuetesDto? ChasseQuetes { get; set; }
        public LevelUpQuetesDto? LevelUpQuetes { get; set; }
        public RandonneQuetesDto? RandonneQuetes { get; set; }

        public PersonnageDto() { }
        public PersonnageDto(Personnage personnage, ChasseQuetes? chasse, RandonneQuetes? randonneQuetes, LevelUpQuetes? levelUpQuetes)
        {
            IdPersonnage = personnage.IdPersonnage;
            Nom = personnage.Nom;
            Niveau = personnage.Niveau;
            Experience = personnage.Experience;
            PointsVie = personnage.PointsVie;
            PointsVieMax = personnage.PointsVieMax;
            Force = personnage.Force;
            Defense = personnage.Defense;
            PositionX = personnage.PositionX;
            PositionY = personnage.PositionY;
            DernierVillageX = personnage.DernierVillageX;
            DernierVillageY = personnage.DernierVillageY;
            ChasseQuetes = new ChasseQuetesDto(chasse);
            LevelUpQuetes = new LevelUpQuetesDto(levelUpQuetes);
            RandonneQuetes = new RandonneQuetesDto(randonneQuetes);
        }
    }

    public class ExplorerDto
    {
        public int[][] Coords { get; set; }
        public string Email { get; set; }
        public ExplorerDto() { }
        public ExplorerDto(int[][] coords, string email)
        {
            Coords = coords;
            Email = email;
        }
    }
}

public class ChasseQuetesDto
{
    public int NbTue { get; set; } = 0;
    public string Nom { get; set; }
    public int ObjectifTue { get; set; }
    public string Description { get; set; }
    public bool EstComplete { get; set; } = false;
    public int XpRecompense { get; set; }
    public string? MessageFin { get; set; }

    public ChasseQuetesDto(ChasseQuetes quetes) { 
        NbTue = quetes.NbTue;
        ObjectifTue = quetes.ObjectifTue;
        Description = quetes.Description;
        EstComplete = quetes.EstComplete;
        Nom = quetes.Nom;
        XpRecompense = quetes.XpRecompense;
    }
}

public class LevelUpQuetesDto
{
    public string Nom { get; set; }
    public int NiveauDepart { get; set; }
    public int NiveauObjectif { get; set; }
    public string Description { get; set; }
    public bool EstComplete { get; set; } = false;
    public int XpRecompense { get; set; }
    public string? MessageFin { get; set; }

    public LevelUpQuetesDto(LevelUpQuetes quetes)
    {
        NiveauDepart = quetes.NiveauDepart;
        NiveauObjectif = quetes.NiveauObjectif;
        Description = quetes.Description;
        EstComplete = quetes.EstComplete;
        Nom = quetes.Nom;
        XpRecompense = quetes.XpRecompense;
    }
}

public class RandonneQuetesDto
{
    public string Nom { get; set; }
    public int TuileX { get; set; }
    public int TuileY { get; set; }
    public string Description { get; set; }
    public bool EstComplete { get; set; } = false;
    public int XpRecompense { get; set; }
    public string? MessageFin { get; set; }
    
    public RandonneQuetesDto(RandonneQuetes quetes)
    {
        TuileX = quetes.TuileX;
        TuileY = quetes.TuileY;
        Description = quetes.Description;
        EstComplete = quetes.EstComplete;
        Nom = quetes.Nom;
        XpRecompense = quetes.XpRecompense;
    }
}

public class QuetesListDto 
{
    public ChasseQuetesDto? ChasseQuetes { get; set; } = null;
    public LevelUpQuetesDto? LevelUpQuetes { get; set; } = null;
    public RandonneQuetesDto? RandonneQuetes { get; set; } = null;
    public DateTime ServerTimeUtc { get; internal set; }
    public DateTime NextRefreshUtc { get; internal set; }

    public QuetesListDto() { }
}

public class PokedexRequestDto
{
    public int IdPersonnage { get; set; }
    public string? TypeMonstre { get; set; }
    public bool? Hunted { get; set; }
    public string? recherche { get; set; }
    public int Page { get; set; } = 1;     // Numéro de page (1 = première page)
    public int PageSize { get; set; } = 25; // Taille de page par défaut

    public PokedexRequestDto() { }
}

public class HuntedMonsterDto
{
    public string Name { get; set; }
    public string SpriteUrl { get; set; }
    public bool Hunted {get; set; }

    public HuntedMonsterDto() { }
}

public class PokedexResponseDto
{
    public int TotalMonstre { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<HuntedMonsterDto> HuntedMonsters { get; set; }

    public PokedexResponseDto() { }
}

public class HintDto
{
    public int IdPersonnage { get; set; }
    public string TypeMonstre { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public HintDto() { }

    public HintDto(TypePositionHint tph)
    {
        IdPersonnage = tph.IdPersonnage;
        TypeMonstre = tph.Type;
        PositionX = tph.PositionX;
        PositionY = tph.PositionY;
    }
}
public class HintRequestDto
{
    public int IdPersonnage { get; set; }
    public string TypeMonstre { get; set; }
    public string Email { get; set; }
    public HintRequestDto() { }
}

public class HintResponseDto
{
    public HintDto CurrentHint { get; set; }
    public int PiecesOr { get; set; }
    public List<HintDto> HistoryHints { get; set; }
}

public class LigneClassementDto
{
    public int Rang { get; set; }
    public string Pseudo { get; set; }
    public int Valeur { get; set; }
}

public class PersonnageRequestDto
{
    public int IdPersonnage { get; set; }
    public string Email { get; set; }
}

public class ProfilResponseDto
{
    public string Pseudo { get; set; }
    public int Niveau { get; set; }
    public int Experience { get; set; }
    public int PointsVie { get; set; }
    public int PointsVieMax { get; set; }
    public int Force { get; set; }
    public int Defense { get; set; }
}