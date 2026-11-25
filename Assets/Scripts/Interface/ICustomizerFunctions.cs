using UnityEngine;

namespace SAE.GAD176.BaldingGate.CharacterCustomiser
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
