using UnityEngine;
using UnityEngine.InputSystem;

namespace Ursaanimation.CubicFarmAnimals
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;
        public string walkForwardAnimation = "walk_forward";
        public string walkBackwardAnimation = "walk_backwards";
        public string runForwardAnimation = "run_forward";
        public string turn90LAnimation = "turn_90_L";
        public string turn90RAnimation = "turn_90_R";
        public string trotAnimation = "trot_forward";
        public string sittostandAnimation = "sit_to_stand";
        public string standtositAnimation = "stand_to_sit";
        public string idle = "idle"; 

        void Start()
        {
            animator = GetComponent<Animator>();
        }

    //    void Update()
    //    {
    //        var kb = Keyboard.current;
    //        if (kb == null) return;   // No keyboard connected

    //        if (kb.wKey.isPressed)
    //        {
    //            animator.Play(walkForwardAnimation);
    //        }
    //        else if (kb.sKey.wasPressedThisFrame)
    //        {
    //            animator.Play(walkBackwardAnimation);
    //        }
    //        else if (kb.digit1Key.wasPressedThisFrame)
    //        {
    //            animator.Play(runForwardAnimation);
    //        }
    //        else if (kb.aKey.wasPressedThisFrame)
    //        {
    //            animator.Play(turn90LAnimation);
    //        }
    //        else if (kb.dKey.wasPressedThisFrame)
    //        {
    //            animator.Play(turn90RAnimation);
    //        }
    //        else if (kb.digit2Key.wasPressedThisFrame)
    //        {
    //            animator.Play(trotAnimation);
    //        }
    //        else if (kb.digit4Key.wasPressedThisFrame)
    //        {
    //            animator.Play(sittostandAnimation);
    //        }
    //        else if (kb.digit3Key.wasPressedThisFrame)
    //        {
    //            animator.Play(standtositAnimation);
    //        }
    //        else
    //        {
    //            animator.Play(idle); 
    //        }
    //    }
    }
}
