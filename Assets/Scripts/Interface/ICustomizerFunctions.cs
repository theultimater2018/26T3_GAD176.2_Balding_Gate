using UnityEngine;

namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public interface ICustomizerFunctions
    {
        void ToggleOptionForward();
        void ToggleOptionBackward();
        void FindAllUIRefs();
        void FindAllObjectRefs();
        void ConfirmSelection();
        void GoBack();
    }

}
