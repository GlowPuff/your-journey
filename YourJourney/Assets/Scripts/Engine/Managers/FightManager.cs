using UnityEngine;

public class FightManager : MonoBehaviour
{
	InteractionManager im;
	MonsterManager mm;

	//elite enemies are able to perform counterattacks even	when they are exhausted
	//stunning exhausts a group - if the group is elite it also cannot counterattack this attack

	public void Provoke( Monster monster )
	{
		//provoking does NOT exhaust the group
		im = FindObjectOfType<InteractionManager>();
		mm = FindObjectOfType<MonsterManager>();

		im.GetNewDamagePanel().ShowCombatCounter( monster, () =>
		{
			mm.UnselectAll();
		} );
	}
}
