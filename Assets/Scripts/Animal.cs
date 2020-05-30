using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
	[Range(20, 80)] public float initialLife = 40f;
	[Range(2, 20)] public float initialSpeed = 5f;

	private float m_Life;
	private float m_Speed;
	private NavMeshAgent m_Navigation;
    // Start is called before the first frame update
    void Start()
    {
	    m_Life = initialLife;
	    m_Speed = initialSpeed;

	    m_Navigation = GetComponent<NavMeshAgent>();
	    m_Navigation.speed = m_Speed;
	    m_Navigation.destination = new Vector3(50, 0, 50);
    }

    // Update is called once per frame
    void Update()
    {
	    // Does the ray intersect any objects excluding the player layer
	    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit, Mathf.Infinity))
	    {
		    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
		    if (hit.transform.CompareTag("vegetation")) Debug.Log("Did see vegetation");
	    }
	    else
	    {
		    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
		    // Debug.Log("Did not Hit");
	    }
    }
}
