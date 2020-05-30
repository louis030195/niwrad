using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Patrol")]
    public class PatrolAction : Action
    {
        [Tooltip("Distance to be from a waypoint to switch to another")] public int precision = 5;
        [Tooltip("Distance to be from a waypoint to switch to another")] public GameObject wayPointList;
        private int nextWayPoint;
        private bool direction;
        public override void Act(StateController controller)
        {
            Patrol(controller);
        }

        private void Patrol(StateController controller)
        {
            Debug.Assert(wayPointList.transform.childCount > 0, $"wayPointList.transform.childCount <= 0");


            controller.movement.MoveTo(wayPointList.transform.GetChild(nextWayPoint).position);

            if (Vector3.Distance(controller.transform.position, wayPointList.transform.GetChild(nextWayPoint).position) < precision)
            {
                nextWayPoint = Random.Range(0, wayPointList.transform.childCount);
                /*
                Debug.Log($"wayPointList.transform.childCount {wayPointList.transform.childCount} - nextWayPoint {nextWayPoint}");
                if (nextWayPoint == wayPointList.transform.childCount || nextWayPoint == 0) direction = !direction;
                nextWayPoint = direction ? nextWayPoint + 1 : nextWayPoint - 1;*/
            }
        }
    }
}
