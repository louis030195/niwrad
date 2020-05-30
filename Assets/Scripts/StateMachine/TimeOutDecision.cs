using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Decisions/TimeOut")]
    public class TimeOutDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return TimeOut(controller);
        }

        private bool TimeOut(StateController controller)
        {
            return controller.CheckIfCountDownElapsed(controller.parameters.timeOut);
        }
    }
}
