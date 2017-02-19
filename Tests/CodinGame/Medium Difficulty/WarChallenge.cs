// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/winamax-battle
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	class Card : IComparable<Card> {
		public enum SUIT {
			DIAMOND,
			HEART,
			CLUB,
			SPADE
		}
		public int		m_value;
		public SUIT		m_suit;

		public Card( string _cardName ) {
			switch ( _cardName[_cardName.Length-1] ) {
				case 'D': m_suit = SUIT.DIAMOND; break;
				case 'H': m_suit = SUIT.HEART; break;
				case 'C': m_suit = SUIT.CLUB; break;
				case 'S': m_suit = SUIT.SPADE; break;
				default: throw new Exception( "Unsupported suit character!" );
			}

			_cardName = _cardName.Substring( 0, _cardName.Length-1 );
			switch ( _cardName ) {
				case "2": m_value = 0; break;
				case "3": m_value = 1; break;
				case "4": m_value = 2; break;
				case "5": m_value = 3; break;
				case "6": m_value = 4; break;
				case "7": m_value = 5; break;
				case "8": m_value = 6; break;
				case "9": m_value = 7; break;
				case "10": m_value = 8; break;
				case "J": m_value = 9; break;
				case "Q": m_value = 10; break;
				case "K": m_value = 11; break;
				case "A": m_value = 12; break;
				default: throw new Exception( "Unsupported card type!" );
			}
		}

		public override string ToString() {
			return m_suit + "(" + m_value + ")";
		}

		#region IComparable<Card> Members

		public int CompareTo( Card other ) {
			if ( m_value > other.m_value )
				return 1;
			if ( m_value < other.m_value )
				return -1;
			return 0;	// Equal...
		}

		#endregion
	}

	class	Deck {
		List< Card >	m_cards = new List< Card >();

		public int		Count { get { return m_cards.Count; } }

		public void		PushTop( Card _card ) {
			m_cards.Add( _card );
		}
		public Card		PopTop() {
			Card	C = m_cards[m_cards.Count-1];
			m_cards.RemoveAt( m_cards.Count-1 );
			return C;
		}
		public void		PushBottom( Card _card ) {
			m_cards.Insert( 0, _card );
		}
		public Card		PopBottom() {
			Card	C = m_cards[0];
			m_cards.RemoveAt( 0 );
			return C;
		}
		public void		PushBottom( Deck _deck ) {
			while ( _deck.Count > 0 ) {
//				Card	C = _deck.PopTop();
				Card	C = _deck.PopBottom();
				PushBottom( C );
			}
		}
	}

    public static void Main(string[] args) {
		Deck	cardsP1 = new Deck();
		Deck	cardsP2 = new Deck();

		string	line = Console.ReadLine();
        int n = int.Parse(line); // the number of cards for player 1
string	dbgP1Cards = "Player 1 cards = ";
        for (int i = 0; i < n; i++) {
			line = Console.ReadLine();
			Card	C = new Card( line );
dbgP1Cards += " " + C;
//			cardsP1.PushTop( C );	// Okay, the problem is very badly stated: "Cards are placed face down on top of each deck." => I'm reading this as "stacked on TOP of each other"...
			cardsP1.PushBottom( C );
		}
Console.Error.WriteLine( dbgP1Cards );

        int m = int.Parse(Console.ReadLine()); // the number of cards for player 2
string	dbgP2Cards = "Player 2 cards = ";
        for (int i = 0; i < m; i++) {
			line = Console.ReadLine();
			Card	C = new Card( line );
dbgP2Cards += " " + C;
//			cardsP2.PushTop( C );
			cardsP2.PushBottom( C );
        }
Console.Error.WriteLine( dbgP2Cards );

		int		winsCountP1 = 0;
		int		winsCountP2 = 0;
		Deck	warDeckP1 = new Deck();
		Deck	warDeckP2 = new Deck();
		while ( cardsP1.Count > 0 && cardsP2.Count > 0 ) {

			Card	C1 = cardsP1.PopTop();
			Card	C2 = cardsP2.PopTop();
			warDeckP1.PushTop( C1 );
			warDeckP2.PushTop( C2 );
Console.Error.WriteLine( "Battle => " + C1 + " ◄=► " + C2 );

			int		battle = C1.CompareTo( C2 );
			if ( battle > 0 ) {
				// Player 1 wins
int	dbgSumCards = warDeckP1.Count + warDeckP2.Count;
				cardsP1.PushBottom( warDeckP1 );
				cardsP1.PushBottom( warDeckP2 );
				winsCountP1++;
Console.Error.WriteLine( "Player 1 wins " + dbgSumCards + " cards (total = " + cardsP1.Count + ") (other player = " + cardsP2.Count + ")" );
			} else if ( battle < 0 ) {
				// Player 2 wins
int	dbgSumCards = warDeckP1.Count + warDeckP2.Count;
				cardsP2.PushBottom( warDeckP1 );
				cardsP2.PushBottom( warDeckP2 );
				winsCountP2++;
Console.Error.WriteLine( "Player 2 wins " + dbgSumCards + " cards (total = " + cardsP2.Count + ") (other player = " + cardsP1.Count + ")" );
			} else if ( battle == 0 ) {
				// WAR!
Console.Error.WriteLine( "WAR!" );

				// Check both players have enough cards for the war...
				if ( cardsP1.Count < 3 ) {
Console.Error.WriteLine( "Player 1 doesn't have enough cards... (" + cardsP1.Count + ") Current Score = " + winsCountP1 + "/" + winsCountP2 );
					winsCountP2++;
winsCountP1 = winsCountP2 = 1;	// I don't understand this rule...
					break;
				} else if ( cardsP2.Count < 3 ) {
Console.Error.WriteLine( "Player 2 doesn't have enough cards... (" + cardsP2.Count + ") Current Score = " + winsCountP1 + "/" + winsCountP2 );
					winsCountP1++;
winsCountP1 = winsCountP2 = 1;	// I don't understand this rule...
					break;
				}

				// Place current cards and the next 3 cards in a special "war deck"
				for ( int i=0; i < 3; i++ ) {
					warDeckP1.PushTop( cardsP1.PopTop() );
					warDeckP2.PushTop( cardsP2.PopTop() );
// 					warDeckP1.PushBottom( cardsP1.PopTop() );
// 					warDeckP2.PushBottom( cardsP2.PopTop() );
				}
			}
		}

		if ( winsCountP1 == winsCountP2 )
			Console.WriteLine("PAT");
		else if ( winsCountP1 > winsCountP2 )
			Console.WriteLine( "1 " + winsCountP1 );
		else // if ( winsCountP2 > winsCountP1 )
			Console.WriteLine( "2 " + winsCountP2 );
    }
}
