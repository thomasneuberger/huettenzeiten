using System.ComponentModel.DataAnnotations;

namespace HuettenZeiten.Data.Prompts;

public enum MainActions
{
    [Display(Name = "Hütten bearbeiten")]
    ManageHuts,

    [Display(Name = "Auslastung anzeigen")]
    OutputUsages,

    [Display(Name = "Beenden")]
    Quit
}
