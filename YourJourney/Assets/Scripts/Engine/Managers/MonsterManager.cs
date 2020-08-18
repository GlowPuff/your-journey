using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
	public CombatPanel combatPanel;
	public GameObject monsterButtonPrefab;
	public Transform bar, buttonAttach;
	public Button leftB, rightB;

	//Monster selectedMonster;
	int scrollOffset = -517;
	bool scrollReady = true;

	[HideInInspector]
	public List<Monster> monsterList = new List<Monster>();

	/// <summary>
	/// Add from saved game
	/// </summary>
	public void AddMonsterGroup( Monster m, IInteraction interaction = null )
	{
		bar.DOLocalMoveY( 50, .75f ).SetEase( Ease.InOutCubic );

		m.interaction = interaction;
		GameObject go = Instantiate( monsterButtonPrefab, buttonAttach );
		go.transform.localPosition = new Vector3( ( 175 * monsterList.Count ), 25, 0 );
		go.GetComponent<MonsterButton>().monster = m;
		go.GetComponent<MonsterButton>().Show( m.isElite );

		monsterList.Add( m );
	}

	private void Update()
	{
		leftB.interactable = GetFirstButtonPos() < 130;
		rightB.interactable = GetLastButtonPos() > 402;
	}

	public void OnScrollLeft()
	{
		leftB.interactable = GetLastButtonPos() != -517;

		if ( scrollOffset == -517 )
			return;
		scrollOffset += 175;

		if ( scrollReady )
		{
			Debug.Log( "left" );
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad );
		}
	}

	public void OnScrollRight()
	{
		rightB.interactable = GetLastButtonPos() > 402;

		if ( GetLastButtonPos() <= 402 )
			return;
		scrollOffset -= 175;

		if ( scrollReady )
		{
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad );
		}
	}

	float GetFirstButtonPos()
	{
		List<MonsterButton> buttons = new List<MonsterButton>();

		foreach ( Transform child in buttonAttach )
			return child.transform.position.x;

		return -517;
	}

	float GetLastButtonPos()
	{
		List<MonsterButton> buttons = new List<MonsterButton>();

		foreach ( Transform child in buttonAttach )
			buttons.Add( child.GetComponent<MonsterButton>() );

		if ( buttons.Count == 0 )
			return -517;

		return buttons[buttons.Count - 1].transform.position.x;
	}

	/// <summary>
	/// Add from interacting with a threat
	/// </summary>
	public void AddNewMonsterGroup( Monster[] array, IInteraction interaction )
	{
		bar.DOLocalMoveY( 50, .75f ).SetEase( Ease.InOutCubic );

		foreach ( Monster m in array )
		{
			m.interaction = interaction;
			m.currentHealth = new int[3] { m.health, m.health, m.health };
			GameObject go = Instantiate( monsterButtonPrefab, transform );
			go.transform.localPosition = new Vector3( ( 175 * monsterList.Count ), 25, 0 );
			go.GetComponent<MonsterButton>().monster = m;
			go.GetComponent<MonsterButton>().Show( m.isElite );

			monsterList.Add( m );
		}
	}

	//public void RemoveMonster( int i )
	//{
	//	if ( i >= monsterList.Count )
	//		return;
	//	RemoveMonster( monsterList[i] );
	//}

	/// <summary>
	/// removes monster group AND fires its Trigger After Event
	/// </summary>
	public void RemoveMonster( Monster m )
	{
		monsterList.Remove( m );

		//remove the prefab button from the screen and rearrange buttons
		int c = 0;
		foreach ( Transform child in buttonAttach )
		{
			if ( child.GetComponent<MonsterButton>().monster == m )
			{
				child.GetComponent<MonsterButton>().markRemove = true;
				child.GetComponent<MonsterButton>().cg.DOFade( 0, .5f );
				child.DOLocalMoveY( 0, .5f ).OnComplete( () =>
				{
					child.GetComponent<MonsterButton>().Remove();
				} );
			}
			else
				child.GetComponent<MonsterButton>().Regroup( c++ * 175 );
		}

		if ( monsterList.Count == 0 )
			bar.DOLocalMoveY( -45, .5f ).SetEase( Ease.InOutCubic );

		//fire After Event trigger for monster group
		if ( m.interaction != null )
			FindObjectOfType<TriggerManager>().FireTrigger( m.interaction.triggerAfterName );
	}

	public bool ShowCombatPanel( Monster m )
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				child.GetComponent<MonsterButton>().ToggleSelect( false );
			}
		}
		//selectedMonster = m;
		return combatPanel.Show( m );
	}

	public void UnselectAll()
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				child.GetComponent<MonsterButton>().ToggleSelect( false );
			}
		}
	}

	public void SelectMonster( Monster m, bool select )
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
					child.GetComponent<MonsterButton>().ToggleSelect( select );
			}
		}
	}

	public void ExhaustMonster( Monster m, bool exhaust )
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
				{
					child.GetComponent<MonsterButton>().ToggleExhausted( exhaust );
					return;
				}
			}
		}
		Debug.Log( "ExhaustMonster::no monster found with this GUID" );
	}

	public void ReadyAll()
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			child.GetComponent<MonsterButton>().ToggleExhausted( false );
		}
	}
}
