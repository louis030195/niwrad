using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Decisions/InRange")]
    public class InRangeDecision : Decision
    {

        public override bool Decide(StateController controller)
        {
            return CheckIfInRange(controller);;
        }

        private bool CheckIfInRange(StateController controller)
        {
            // the origin of the ray need to start a bit behind
            // var castOrigin = controller.eyes.position - controller.eyes.forward.normalized * controller.parameters.lookSphereCastRadius;

            // Debug.DrawRay(castOrigin,
            //    controller.eyes.forward.normalized * controller.parameters.attackRange, controller.currentState.sceneGizmoColor);
            // We just need to check if we are close enough to attack our target
            var position = controller.transform.position;
            return controller.target != null && Vector3.Distance(position, controller.target.GetComponent<Collider>().ClosestPointOnBounds(position)) <
                   controller.parameters.attackRange;
            // We didn't use SphereCast here because SphereCast only detect moving thing ...
            // So there was a glitch is you stop moving the mob doesn't see you anymore :D
            // https://answers.unity.com/questions/677772/spherecast-wont-work.html
            // return Physics.OverlapSphere(castOrigin, controller.parameters.attackRange)
            //    .Any(hit => HitTarget(controller, hit));
        }
    }
}
