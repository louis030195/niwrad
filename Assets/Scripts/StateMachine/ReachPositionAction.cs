using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/ReachPosition")]
    public class ReachPositionAction : Action
    {
        public GameObject position;
        public override void Act(StateController controller)
        {
            ReachPosition(controller);
        }

        private void ReachPosition(StateController controller)
        {
            if (controller.movement.remainingDistance <= controller.movement.stoppingDistance && !controller.movement.pathPending)
            {
                // controller.movement.MoveTo(Position.AboveGround(position.transform.position, 1));
            }

        }
    }
}
