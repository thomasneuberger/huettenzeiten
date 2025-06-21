using System.ComponentModel.DataAnnotations;

namespace HuettenZeiten.Cli.Prompts;

public enum ManageHutsActions
{
    [Display(Name = "Hütte hinzufügen")]
    AddHut,

    [Display(Name = "Hütte entfernen")]
    RemoveHut,
    
    [Display(Name = "Fertig")]
    Finished
}
