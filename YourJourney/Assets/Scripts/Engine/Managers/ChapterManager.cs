using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
	public SpawnMarker startMarker;

	List<Chapter> chapterList;
	TileGroup previousGroup;

	public void Init( Scenario s )
	{
		chapterList = new List<Chapter>( s.chapterObserver );
		previousGroup = null;
		//Debug.Log( $"Chapter Manager: {chapterList.Count} Chapters Found" );
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
		int[] tiles = GlowEngine.RandomizeArray( c.randomTilePool.ToArray() );

		string s = "Prepare the following tiles:\r\n\r\n";
		if ( !c.isRandomTiles )
			foreach ( HexTile t in c.tileObserver )
				s += t.idNumber + ", ";
		else
			foreach ( int x in tiles )
				s += x + ", ";
		s = s.Substring( 0, s.Length - 2 );

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( s, ButtonIcon.Continue, () =>
		{
			TileGroup tg = FindObjectOfType<TileManager>().CreateGroupFromChapter( c, tiles );

			if ( previousGroup != null )
				tg.AttachTo( previousGroup );

			tg.AnimateTileUp( firstChapter );
			if ( firstChapter )
			{
				tg.Colorize();
				tg.isExplored = true;
			}

			FindObjectOfType<CamControl>().MoveTo( tg.groupCenter );
			previousGroup = tg;

			if ( tg.startPosition.x != -1000 )
			{
				GlowTimer.SetTimer( 1, () =>
				{
					startMarker.Spawn( tg.startPosition );
				} );
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( "Place your Heroes in the indicated position.", ButtonIcon.Continue );
			}
		} );
	}
}
