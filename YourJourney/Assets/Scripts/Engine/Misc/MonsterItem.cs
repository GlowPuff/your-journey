using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MonsterItem : MonoBehaviour
{
	public CanvasGroup cg;
	public RectTransform healthMeter, shieldMeter, sorcMeter;
	public Text healthText, shieldText, sorcText;
	public GameObject skullnbones;

	[HideInInspector]
	public Monster monster;
	[HideInInspector]
	public bool isDead = false;

	int tempCurrentHealth, currentShield, currentSorc, currentMaxHealth, currentDead, idx, sunderModify, healthModify;
	float healthWidth = 324, shieldWidth = 324, sorcWidth = 324;
	bool startedDead;

	void Update()
	{
		healthMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : healthWidth );
		shieldMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : shieldWidth );
		sorcMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : sorcWidth );
		skullnbones.SetActive( isDead );
	}

	void UpdateUI()
	{
		healthText.text = tempCurrentHealth + "/" + monster.health;
		float width = 324 * tempCurrentHealth / monster.health;
		DOTween.To( () => healthWidth, x => healthWidth = x, width, .2f );

		shieldText.text = currentShield.ToString();
		width = 324 * currentShield / monster.health;
		DOTween.To( () => shieldWidth, x => shieldWidth = x, width, .2f );

		sorcText.text = currentSorc.ToString();
		width = 324 * currentSorc / monster.health;
		DOTween.To( () => sorcWidth, x => sorcWidth = x, width, .2f );
	}

	public int Damage( int damage, CombatModify modifier )
	{
		if ( startedDead )
		{
			isDead = true;
			return damage;
		}

		sunderModify += modifier.Sunder ? 1 : 0;
		healthModify = modifier.Lethal ? monster.health / 2 : 0;
		int damageUsed = 0;
		tempCurrentHealth = currentMaxHealth;
		currentShield = Mathf.Max( 0, monster.shieldValue - sunderModify );
		if ( modifier.Pierce )
			currentShield = 0;
		currentSorc = monster.sorceryValue;
		if ( modifier.Smite )
			currentSorc = 0;
		currentDead = ( currentMaxHealth - healthModify ) + currentShield + currentSorc;
		isDead = false;

		for ( int i = damage; i > 0; i-- )
		{
			if ( currentShield > 0 )
			{
				currentShield--;
				damageUsed++;
				continue;
			}
			if ( currentSorc > 0 )
			{
				currentSorc--;
				damageUsed++;
				continue;
			}
			if ( damageUsed < currentDead )
			{
				tempCurrentHealth--;
				damageUsed++;
			}
			if ( damageUsed == currentDead )
			{
				isDead = true;
				break;
			}
		}

		UpdateUI();

		//Debug.Log( "LEFTOVER:" + Mathf.Max( 0, damage - damageUsed ) );

		if ( modifier.Cleave )
			return damage;
		else
			return Mathf.Max( 0, damage - damageUsed );
	}

	public bool Apply( bool stun )
	{
		Debug.Log( "STUN? " + stun );
		if ( stun )
		{
			monster.isStunned = true;
			FindObjectOfType<MonsterManager>().ExhaustMonster( monster, true );
		}

		monster.currentHealth[idx] = tempCurrentHealth;
		monster.sunderValue = sunderModify;
		return !isDead;
	}

	public void Show( Monster m, int index )
	{
		idx = index;
		monster = m;
		currentMaxHealth = monster.currentHealth[index];
		startedDead = currentMaxHealth == 0;
		sunderModify = monster.sunderValue;

		Damage( 0, new CombatModify() );

		cg.DOFade( 1, .5f );
	}

	public void Hide()
	{
		cg.alpha = 0;
	}
}
