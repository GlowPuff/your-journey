using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
	public SpawnMarker startMarker;

	[HideInInspector]
	public List<Chapter> chapterList;

	TileGroup previousGroup;
	List<string> tokenTriggerQueue = new List<string>();
	List<int> darknessTiles = new List<int>();

	public void Init( Scenario s )
	{
		chapterList = new List<Chapter>( s.chapterObserver );
		previousGroup = null;
		darknessTiles.AddRange( new int[] { 204, 207, 208, 303, 306, 307 } );
		//Debug.Log( $"Chapter Manager: {chapterList.Count} Chapters Found" );
	}

	/// <summary>
	/// true if there are Darkness Tokens or tiles with Darkness on board
	/// </summary>
	public bool IsDarknessVisible()
	{
		//any darkness TOKENS in explored tiles?
		var tokensfound = from chapter in chapterList
											from tile in chapter.tileObserver
											where ( (HexTile)tile ).isExplored
											from token in ( (HexTile)tile ).tokenList
											where token.tokenType == TokenType.Darkness
											select token;

		//any darkness TILES that are explored?
		var tilesfound = from chapter in chapterList
										 from tile in chapter.tileObserver
										 where ( (HexTile)tile ).tileSide == "B"
										 where ( (HexTile)tile ).isExplored
										 where darknessTiles.Contains( ( (HexTile)tile ).idNumber )
										 select tile;

		int tk = tokensfound.Count();
		int tl = tilesfound.Count();
		if ( tokensfound.Count() > 0 || tilesfound.Count() > 0 )
			return true;

		return false;
	}

	/// <summary>
	/// Enqueues a trigger name to activate a token later, after the chapter has been activated, but only if a token exists Triggered By the name
	/// </summary>
	public void EnqueueTokenTrigger( string name )
	{
		var foo = from chapter in chapterList
							from tile in chapter.tileObserver
							from token in ( (HexTile)tile ).tokenList
							where token.triggeredByName == name
							select token;

		if ( foo.Count() > 0 && !tokenTriggerQueue.Contains( name ) )
		{
			//Debug.Log( "Chapter EnqueueTokenTrigger: " + name );
			tokenTriggerQueue.Add( name );
		}
		//else
		//	Debug.Log( "*NOT Chapter EnqueueTokenTrigger: " + name );
	}

	public bool TriggerChapterByTrigger( string triggername )
	{
		if ( chapterList.Any( x => x.triggeredBy == triggername ) )
		{
			Chapter c = chapterList.Where( x => x.triggeredBy == triggername ).First();
			Debug.Log( "Found Chapter: " + c.dataName );
			TryTriggerChapter( c.dataName, false );
			return true;
		}
		return false;
	}

	/// <summary>
	/// creates Chapter tilegroup, attaches it to previous group, animates it up, optionally pre-explores it (colorize + reveal tokens), prompts starting location if applicable
	/// </summary>
	public void TryTriggerChapter( string chname, bool firstChapter )
	{
		Debug.Log( "TryTriggerChapter::" + chname );// + "::firstChapter=" + firstChapter );
		if ( chapterList.Any( x => x.dataName == chname ) )
		{
			Chapter c = chapterList.Where( x => x.dataName == chname ).First();
			Debug.Log( "Found Chapter: " + c.dataName );
			//show flavor text
			if ( !c.noFlavorText )
			{
				var im = FindObjectOfType<InteractionManager>().GetNewTextPanel();
				im.ShowOkContinue( c.flavorBookData.pages[0], ButtonIcon.Continue, () =>
				 {
					 FinishChapterTrigger( c, firstChapter );
				 } );
			}
			else
				FinishChapterTrigger( c, firstChapter );
		}
	}

	void FinishChapterTrigger( Chapter c, bool firstChapter )
	{
		string s = "Prepare the following tiles:\r\n\r\n";
		foreach ( HexTile ht in c.tileObserver )
			s += ht.idNumber + ", ";
		s = s.Substring( 0, s.Length - 2 );

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( s, ButtonIcon.Continue, () =>
		{
			TileGroup tg = FindObjectOfType<TileManager>().CreateGroupFromChapter( c );

			if ( previousGroup != null )
				tg.AttachTo( previousGroup );

			tg.AnimateTileUp( c );

			if ( firstChapter && c.isPreExplored )
			{
				tg.Colorize();
				tg.isExplored = true;
			}

			FindObjectOfType<CamControl>().MoveTo( tg.groupCenter );
			previousGroup = tg;

			//check triggered token queue
			var foo = from tname in tokenTriggerQueue from tile in tg.tileList.Where( x => x.HasTriggeredToken( tname ) ) select new { tile, tname };
			//if ( foo.Count() > 0 )
			//Debug.Log( "FinishChapterTrigger::" + foo.Count() );
			foreach ( var item in foo )
			{
				item.tile.EnqueueTokenTrigger( item.tname );
				//tokenTriggerQueue.Remove( item.tname );
			}
			tokenTriggerQueue.Clear();

			if ( tg.startPosition.x != -1000 )
			{
				GlowTimer.SetTimer( 1, () =>
				{
					startMarker.Spawn( tg.startPosition );
					if ( firstChapter && !c.isPreExplored )
					{
						tg.Colorize( true );
					}
				} );
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( "Place your Heroes in the indicated position.", ButtonIcon.Continue );
			}
		} );
	}
}
