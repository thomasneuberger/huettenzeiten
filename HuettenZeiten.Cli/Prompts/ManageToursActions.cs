using System.ComponentModel.DataAnnotations;

namespace HuettenZeiten.Cli.Prompts;

public enum ManageToursActions
{
    [Display(Name = "Neue Tour erstellen")]
    AddTour,

    [Display(Name = "Tour umbenennen")]
    RenameTour,

    [Display(Name = "Tour löschen")]
    RemoveTour,

    [Display(Name = "Tour verwalten")]
    ManageHuts,
    
    [Display(Name = "Fertig")]
    Finished
}
