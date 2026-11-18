using UnityEngine;

namespace SmallScaleInc.CharacterCreatorFantasy
{
    [RequireComponent(typeof(Animator))]
    public class GearAnimatorSync : MonoBehaviour
    {
        [Tooltip("Drag your player (with TopDownCharacterController) here.")]
        public TopDownCharacterController playerController;
        public GenericTopDownController genericPlayerController;

        Animator _gearAnim;
        Animator _playerAnim;
        bool _subscribedToPlayer;
        bool _subscribedToGenericPlayer;

        void Awake()
        {
            _gearAnim = GetComponent<Animator>();
            if (playerController != null)
                BindToPlayer(playerController);
            else if (genericPlayerController != null)
                BindToGenericPlayer(genericPlayerController);
        }

        void OnDestroy()
        {
            if (playerController != null && _subscribedToPlayer)
                playerController.OnTriggerFired -= OnPlayerTrigger;

            if (genericPlayerController != null && _subscribedToGenericPlayer)
                genericPlayerController.OnAnimationTriggerSent -= OnPlayerTrigger;
        }

        public void BindToPlayer(TopDownCharacterController controller)
        {
            if (playerController != null && _subscribedToPlayer)
                playerController.OnTriggerFired -= OnPlayerTrigger;

            if (genericPlayerController != null && _subscribedToGenericPlayer)
                genericPlayerController.OnAnimationTriggerSent -= OnPlayerTrigger;

            playerController = controller;
            genericPlayerController = null;
            _playerAnim = playerController != null ? playerController.GetComponent<Animator>() : null;
            _subscribedToPlayer = false;
            _subscribedToGenericPlayer = false;

            if (playerController != null)
            {
                playerController.OnTriggerFired += OnPlayerTrigger;
                _subscribedToPlayer = true;
            }
        }

        public void BindToGenericPlayer(GenericTopDownController controller)
        {
            if (playerController != null && _subscribedToPlayer)
                playerController.OnTriggerFired -= OnPlayerTrigger;

            if (genericPlayerController != null && _subscribedToGenericPlayer)
                genericPlayerController.OnAnimationTriggerSent -= OnPlayerTrigger;

            playerController = null;
            genericPlayerController = controller;
            _playerAnim = genericPlayerController != null ? genericPlayerController.GetComponent<Animator>() : null;
            _subscribedToPlayer = false;
            _subscribedToGenericPlayer = false;

            if (genericPlayerController != null)
            {
                genericPlayerController.OnAnimationTriggerSent += OnPlayerTrigger;
                _subscribedToGenericPlayer = true;
            }
        }

        // void LateUpdate()
        // {
        //     if (_playerAnim == null) return;

        //     // copy floats & ints
        //     _gearAnim.SetFloat(  "Direction", _playerAnim.GetFloat("Direction"));
        //     _gearAnim.SetInteger("DirIndex",  _playerAnim.GetInteger("DirIndex"));

        //     // copy all bools
        //     foreach (var p in new[]{
        //         "IsRun","IsWalk","IsStrafeLeft","IsStrafeRight","IsRunBackwards",
        //         "UseIdle2","UseIdle3","UseIdle4",
        //         "IsCrouching","IsMounted",
        //         "Speed1x","Speed2x",
        //         // "IsCrouchMoving","IsRideMoving"
        //     })
        //         _gearAnim.SetBool(p, _playerAnim.GetBool(p));

        //     // copy playback speed
        //     _gearAnim.speed = _playerAnim.speed;
        // }

        public void SyncFromPlayer(Animator playerAnimator)
        {
            if (!CanSyncAnimator(_gearAnim) || !CanSyncAnimator(playerAnimator)) return;

            _gearAnim.SetFloat("Direction", playerAnimator.GetFloat("Direction"));
            _gearAnim.SetInteger("DirIndex", playerAnimator.GetInteger("DirIndex"));

            foreach (var p in new[]{
                "IsRun","IsWalk","IsStrafeLeft","IsStrafeRight","IsRunBackwards",
                "UseIdle2","UseIdle3","UseIdle4",
                "IsCrouching","IsMounted",
                "Speed1x","Speed2x"
            })
                _gearAnim.SetBool(p, playerAnimator.GetBool(p));

            _gearAnim.speed = playerAnimator.speed;
        }

        private bool CanSyncAnimator(Animator animator)
        {
            return animator != null &&
                   animator.isActiveAndEnabled &&
                   animator.runtimeAnimatorController != null;
        }


        // this is called whenever the player fires a trigger
        void OnPlayerTrigger(string triggerName)
        {
            if (!CanSyncAnimator(_gearAnim))
                return;

            _gearAnim.SetTrigger(triggerName);
        }
    }
}
