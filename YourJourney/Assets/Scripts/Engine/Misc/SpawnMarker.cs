using UnityEngine;

public class SpawnMarker : MonoBehaviour
{
	public static float SPAWN_HEIGHT = 0.5f;

	public void Spawn( Vector3 pos )
	{
		gameObject.SetActive( true );
		transform.position = pos;
	}

	private void Update()
	{
		transform.localScale = ( GlowEngine.SineAnimation( .06f, .07f, 15 ) ).ToVector3();
	}
}
