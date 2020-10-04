using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
	public CombatPanel combatPanel;
	public GameObject monsterButtonPrefab;
	public Transform bar, buttonAttach;
	public Button leftB, rightB;
	public Canvas canvas;
	public Sprite[] banners, eliteBanners;

	Queue<Sprite> bannerQueue, eliteBannerQueue;
	//Monster selectedMonster;
	int scrollOffset = -517;
	bool scrollReady = true;
	[HideInInspector]
	public RectTransform attachRect, sbRect;
	public GameObject sizebar;
	[HideInInspector]
	public float scalar;

	[HideInInspector]
	public List<Monster> monsterList = new List<Monster>();

	private void Start()
	{
		attachRect = buttonAttach.GetComponent<RectTransform>();
		scalar = canvas.scaleFactor;
		bannerQueue = new Queue<Sprite>( banners );
		eliteBannerQueue = new Queue<Sprite>( banners );
	}

	/// <summary>
	/// Add from saved game
	/// </summary>
	public void AddMonsterGroup( Monster m, IInteraction interaction = null )
	{
		//check if monster can be spawned in this Difficulty mode
		if ( !m.IsValid() )
			return;

		bar.DOLocalMoveY( 50, .75f ).SetEase( Ease.InOutCubic );

		//modify monster for difficulty
		m.AdjustDifficulty();
		m.AdjustPlayerCountDifficulty();

		m.interaction = interaction;
		m.currentHealth = new int[3] { m.health, m.health, m.health };
		GameObject go = Instantiate( monsterButtonPrefab, buttonAttach );

		go.transform.localPosition = new Vector3( ( 175 * monsterList.Count ), 25, 0 );
		go.GetComponent<MonsterButton>().monster = m;
		go.GetComponent<MonsterButton>().AddToBar( m.isElite, this );

		monsterList.Add( m );

		//add banner
		var mc = from monster in monsterList
						 where monster.monsterType == m.monsterType && monster.isElite == m.isElite
						 select monster;
		if ( mc.Count() > 1 )
		{
			if ( !m.isElite && bannerQueue.Count > 0 )
				go.GetComponent<MonsterButton>().SetBanner( bannerQueue.Dequeue() );
			else if ( m.isElite && eliteBannerQueue.Count > 0 )
				go.GetComponent<MonsterButton>().SetBanner( eliteBannerQueue.Dequeue() );
		}

		scrollReady = false;
		foreach ( Transform child in buttonAttach )
		{
			if ( child.transform.position.x > sbRect.position.x + ( 1000f * scalar ) )//off the edge
				scrollOffset -= 175;
		}
		buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
	}

	private void Update()
	{
		scalar = canvas.scaleFactor;
		attachRect = buttonAttach.GetComponent<RectTransform>();
		sbRect = sizebar.GetComponent<RectTransform>();
		float maxX = sbRect.position.x + ( 1000f * scalar );

		leftB.interactable = GetFirstButtonPos() < sbRect.position.x;//130;
		rightB.interactable = GetLastButtonPos() > maxX;//402;
	}

	public void OnScrollLeft()
	{
		//leftB.interactable = GetLastButtonPos() != -517;

		if ( scrollOffset == -517 )
			return;

		if ( scrollReady )
		{
			scrollReady = false;
			scrollOffset += 175;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}
	}

	public void OnScrollRight()
	{
		//rightB.interactable = GetLastButtonPos() > 402;

		if ( GetLastButtonPos() <= buttonAttach.position.x + ( 1000f * scalar ) ) //402 )
			return;

		if ( scrollReady )
		{
			scrollReady = false;
			scrollOffset -= 175;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}
	}

	float GetFirstButtonPos()
	{
		foreach ( Transform child in buttonAttach )
			return child.transform.position.x;

		return 10000;//-517;
	}

	float GetLastButtonPos()
	{
		List<MonsterButton> buttons = new List<MonsterButton>();

		foreach ( Transform child in buttonAttach )
			buttons.Add( child.GetComponent<MonsterButton>() );

		if ( buttons.Count == 0 )
			return 0;//-517;

		return buttons[buttons.Count - 1].transform.position.x;
	}

	/// <summary>
	/// removes monster group, shows reward, and fires its Trigger After Event
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
					Sprite b = child.GetComponent<MonsterButton>().Remove();
					if ( b != null && !m.isElite )
						bannerQueue.Enqueue( b );
					else if ( b != null )
						eliteBannerQueue.Enqueue( b );
				} );
			}
			else
				child.GetComponent<MonsterButton>().Regroup( c++ * 175 );
		}

		if ( monsterList.Count == 0 )
			bar.DOLocalMoveY( -45, .5f ).SetEase( Ease.InOutCubic );
		else
		{
			scrollReady = false;
			scrollOffset = -517;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}

		//show reward on group defeated
		//then fire Defeated trigger only if ALL groups in the Event are defeated
		if ( m.interaction != null )
		{
			ThreatInteraction ti = (ThreatInteraction)m.interaction;
			var foo = ( from mnstr in ti.monsterCollection
										//where mnstr.IsValid()//no
									from mbtn in monsterList
									where mnstr.GUID == mbtn.GUID
									select mnstr );

			FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( $"Remove the {m.dataName}(s) from the board.\r\n\r\nYou or a nearby Hero gain 1 Inspiration.", ButtonIcon.Continue, () =>
			{
				if ( foo.Count() == 0 )
				{
					string trigger = ti.triggerDefeatedName;
					FindObjectOfType<TriggerManager>().FireTrigger( trigger );
					FindObjectOfType<LorePanel>().AddLore( ti.loreReward );
				}
			} );
		}
	}

	public bool ShowCombatPanel( Monster m )
	{
		foreach ( Transform child in buttonAttach )
		{
			child.GetComponent<MonsterButton>().ToggleSelect( false );
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
			if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
			{
				child.GetComponent<MonsterButton>().ToggleExhausted( exhaust );
				return;
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
