using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System;

public class CombatPanel : MonoBehaviour
{
	public RectTransform healthMeter;
	public Sprite selectedButton, unselectedButton;
	public GameObject large, armored, bloodthirsty, eliteBG;
	public Text monsterName, damageText;
	public CanvasGroup canvasGroup;
	public MonsterItem[] monsterItems;
	public Button applyButton;
	public Button[] modifierButtons;

	CombatModify modifier;
	Monster monster = null;
	Guid monsterGUID;
	bool pierceSelected, lethalSelected, stunSelected, smiteSelected, sunderSelected, cleaveSelected;
	int damage;

	//pierce=ignore armor
	//smite=ignore sorcery
	//sunder=permanently reduce armor by 1 (before hits applied)
	//cleave=each enemy in group suffers full hits
	//lethal=if this attack reduces enemy health to at least half, kill it
	//stun=this attack exhausts the group.  if it's elite, also cannot counterattack

	void Start()
	{
		gameObject.SetActive( false );

		pierceSelected = lethalSelected = stunSelected = smiteSelected = sunderSelected = cleaveSelected = false;
	}

	public bool Show( Monster monster )
	{
		//remove any spawn markers, since player will have seen them by now
		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
		}

		if ( monsterGUID == monster.GUID )
		{
			Hide();
			return false;
		}
		else// if ( this.monster == null )
		{
			monsterGUID = monster.GUID;
			gameObject.SetActive( true );
			canvasGroup.alpha = 0;
			canvasGroup.DOFade( 1, .5f );
		}

		FindObjectOfType<TileManager>().ToggleInput( true );

		pierceSelected = lethalSelected = stunSelected = smiteSelected = sunderSelected = cleaveSelected = false;

		foreach ( var btn in modifierButtons )
			btn.GetComponent<Image>().sprite = unselectedButton;

		//populate data
		eliteBG.SetActive( monster.isElite );
		applyButton.interactable = true;
		damage = 0;
		damageText.text = damage.ToString();
		this.monster = monster;
		foreach ( var m in monsterItems )
			m.Hide();
		for ( int i = 0; i < monster.count; i++ )
			monsterItems[i].Show( monster, i );
		monsterName.text = monster.dataName;
		large.SetActive( monster.isLarge );
		armored.SetActive( monster.isArmored );
		bloodthirsty.SetActive( monster.isBloodThirsty );

		return true;
	}

	public void Hide()
	{
		//monster = null;
		monsterGUID = Guid.NewGuid();
		foreach ( var m in monsterItems )
			m.Hide();
		canvasGroup.DOFade( 0, .5f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			gameObject.SetActive( false );
		} );
	}

	public void OnSmite( Button btn )
	{
		smiteSelected = !smiteSelected;
		btn.GetComponent<Image>().sprite = smiteSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnSunder( Button btn )
	{
		sunderSelected = !sunderSelected;
		btn.GetComponent<Image>().sprite = sunderSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnCleave( Button btn )
	{
		cleaveSelected = !cleaveSelected;
		btn.GetComponent<Image>().sprite = cleaveSelected ? selectedButton : unselectedButton;
	}
	public void OnPierce( Button btn )
	{
		pierceSelected = !pierceSelected;
		btn.GetComponent<Image>().sprite = pierceSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnLethal( Button btn )
	{
		lethalSelected = !lethalSelected;
		btn.GetComponent<Image>().sprite = lethalSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnStun( Button btn )
	{
		stunSelected = !stunSelected;
		btn.GetComponent<Image>().sprite = stunSelected ? selectedButton : unselectedButton;
	}

	void DoDamage()
	{
		modifier = new CombatModify() { Pierce = pierceSelected, Smite = smiteSelected, Sunder = sunderSelected, Cleave = cleaveSelected, Lethal = lethalSelected, Stun = stunSelected };

		int r = monsterItems[0].Damage( damage, modifier );
		if ( monster.count > 1 )
		{
			r = monsterItems[1].Damage( r, modifier );
			if ( monster.count > 2 )
				monsterItems[2].Damage( r, modifier );
		}
	}

	public void OnAdd()
	{
		damage++;
		damageText.text = damage.ToString();

		DoDamage();
	}

	public void OnMinus()
	{
		if ( damage == 0 )
			return;

		damage = Mathf.Max( 0, damage - 1 );
		damageText.text = damage.ToString();

		DoDamage();
	}

	public void OnApply()
	{
		applyButton.interactable = false;
		int deadCount = monsterItems.Where( x => x.isDead ).Count() - monster.deathTally;

		monster.deadCount = deadCount;//shadow phase reads this number
		monster.deathTally += deadCount;

		for ( int i = 0; i < monster.count; i++ )
			monsterItems[i].Apply( stunSelected );

		Monster theMonster = this.monster;

		Hide();

		if ( deadCount > 0 )
		{
			//if we're here from the shadow phase AND ALL monsters died, bug out and let shadow phase routine handle reward
			if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase && monster.deathTally == theMonster.count )
				FindObjectOfType<MonsterManager>().RemoveMonster( theMonster );
			else
			{
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( $"Remove {deadCount} {theMonster.dataName} from the board.\r\n\r\nYou or a nearby Hero gain {deadCount} Inspiration.", ButtonIcon.Continue, () =>
				{
					if ( monster.deathTally < theMonster.count )
						QueryCounterAttack( theMonster );
					else
						FindObjectOfType<MonsterManager>().RemoveMonster( theMonster );
				} );
			}
		}
		else
			QueryCounterAttack( theMonster );
	}

	void QueryCounterAttack( Monster monster )
	{
		if ( ( !monster.isElite && monster.isExhausted ) || ( monster.isElite && monster.isStunned ) )
		{
			Debug.Log( "Monster group is exhausted, skipping counterattack" );
			FindObjectOfType<MonsterManager>().UnselectAll();
			return;
		}

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo( "Can the enemy attack?", res =>
		{
			if ( res.btn1 )
			{
				FindObjectOfType<InteractionManager>().GetNewStatPanel().ShowCombatCounter( monster, r =>
				 {
					 FindObjectOfType<MonsterManager>().UnselectAll();
				 } );
			}
			else
			{
				//show monster move textbox
				//Move X: Attack NAME (or closest Hero)
				//buttons: Attack/No Target
				FindObjectOfType<InteractionManager>()
				.GetNewTextPanel()
				.ShowCustom( "Move " + monster.movementValue + ": Attack " + Bootstrap.GetRandomHero() + " or closest Hero", "Attack", "No Target", r =>
					 {
						 if ( r.btn1 )
						 {
							 FindObjectOfType<InteractionManager>().GetNewStatPanel().ShowCombatCounter( monster, x =>
							 {
								 FindObjectOfType<MonsterManager>().UnselectAll();
							 } );
						 }
						 else
						 {
							 FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( "Move " + monster.maxMovementValue + " towards closest Hero", ButtonIcon.Continue, () =>
								{
									FindObjectOfType<MonsterManager>().UnselectAll();
								} );
						 }
					 } );
			}
		} );
	}
}
