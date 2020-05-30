using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/StopAndTalk")]
    public class StopAndTalkAction : Action
    {
        public AudioClip[] clips;
        public override void Act(StateController controller)
        {
            StopAndTalk(controller);
        }

        private void StopAndTalk(StateController controller)
        {
            if (controller.audioSource)
            {
                controller.audioSource.clip = clips[Random.Range(0, clips.Length)];
                controller.audioSource.Play();

                // Stop him
                controller.movement.MoveTo(controller.transform.position);
            }
            else
            {
                Debug.Print($"You forgot to add me an audiosource");
            }
        }
    }
}
