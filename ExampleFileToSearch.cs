using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Krig
{
    public class KrigGame
    {
        public event Action GameChanged = () => { };
        public event Action PlayersChanged = () => { };

        public int NumberOfPlayers 
        { 
            get
            {
                return _numberOfPlayers;
            }
            set 
            {
                if (value < 2) { throw new Exception("The number of players cannot be less than two."); }
                _numberOfPlayers = value;
                if(_numberOfPlayers != _prevNumberOfPlayers)
                {
                    _prevNumberOfPlayers = _numberOfPlayers;
                    Initialize();                    
                }
            }
        }

        public int RemainingCards
        {
            get {

                int totalCards = 0;

                foreach (CardDeck cardDeck in _cardDecks) 
                { 
                    totalCards += cardDeck.RemainingCards;
                }

                return totalCards;
            }
        }

        public enum State
        {
            Idle,
            InProgress,
            Finished
        }

        public State CurrentState { get; private set; }

        public string CurrentStateString
        {
            get {
                switch (CurrentState) {
                    case State.Idle:
                        return "Idle";                        
                    case State.InProgress:
                        return "In progress";
                    case State.Finished:
                        return "Game is finished. " + GetGameResult();                    
                    default:
                        return "Undefined state";                        
                }
            }
        }

        public List<Player> Players 
        {
            get
            {
                return _players;
            }
        }

        public Player GetPlayer(int playerId)
        {
            Player? playerFound = _players.Find((player) => player.PlayerId == playerId);
            if (playerFound is null)
            {
                throw new Exception("Invalid player id");
            }

            else return playerFound;            
        }

        private int _numberOfPlayers;
        private int _prevNumberOfPlayers;
        private int _numberOfCardDecks;
        private List<Player> _players;
        private List<CardDeck> _cardDecks;
        private const int DEFAULT_NUMBER_OF_PLAYERS = 2;


        public KrigGame()
        {
            _numberOfPlayers = DEFAULT_NUMBER_OF_PLAYERS;
            _prevNumberOfPlayers = DEFAULT_NUMBER_OF_PLAYERS;
            _players = new List<Player>();
            _cardDecks = new List<CardDeck>();
            
            Initialize();
        }

        private void Initialize()
        {
            GeneratePlayers();
            _cardDecks.Clear();
            CurrentState = State.Idle;
            
            GameChanged.Invoke();            
        }

        private void GeneratePlayers()
        {            
            _players.Clear();
            for (int i = 0; i < _numberOfPlayers; i++)
            {
                Player newPlayer = new Player();
                newPlayer.PlayerId = i + 1;
                _players.Add(newPlayer);
            }

            PlayersChanged.Invoke();
        }

        private void GenerateCards()
        {
            // Create card decks
            // Rule is 1 deck for every 2 players
            _cardDecks.Clear();
            _numberOfCardDecks = _numberOfPlayers / 2;
            for (int i = 0; i < _numberOfCardDecks; i++)
            {
                CardDeck newCardDeck = new CardDeck();
                _cardDecks.Add(newCardDeck);
            }
        }

        public void StartNewGame()
        {
            Initialize();
            GenerateCards();
            CurrentState = State.InProgress;

            GameChanged.Invoke();
        }

        public void Deal()
        {
            // Cant deal if game is not in progress
            if (CurrentState != State.InProgress) return;


            foreach (var player in _players)             
            {
                player.CurrentCard = GetNextCard();
            }

            UpdateScoreFromCurrentCards();

            if (RemainingCards == 0) {
                CurrentState = State.Finished;
            }

            GameChanged.Invoke();
        }

        private void UpdateScoreFromCurrentCards()
        {
            int bestCardValue = 0;
            foreach (var player in _players)
            {
                if(player.CurrentCard != null && player.CurrentCard.RankValue > bestCardValue)
                {
                    bestCardValue = player.CurrentCard.RankValue;
                }
            }

            List<Player> playersWithBestCards = new List<Player>();
            foreach (var player in _players)
            {
                if (player.CurrentCard != null && player.CurrentCard.RankValue == bestCardValue)
                {
                    playersWithBestCards.Add(player);
                }
            }

            // If one player has the highest card he should have 2 points.
            // If more players have the card with the highest value they should have 1 point.
            if(playersWithBestCards.Count == 1)
            {
                playersWithBestCards[0].Score += 2;
            }

            else if (playersWithBestCards.Count > 1)
            {
                foreach (var player in playersWithBestCards)
                {
                    player.Score++;
                }
            }
        }

        private Card? GetNextCard()
        {
            Card? returnCard = null;

            int deckIndex = _cardDecks.Count - 1;
            if (deckIndex >= 0)
            {
                CardDeck cardDeck = _cardDecks[deckIndex];
                returnCard = cardDeck.DealCard();

                // remove deck instance if no cards remain
                if (cardDeck.RemainingCards == 0)
                {
                    _cardDecks.RemoveAt(deckIndex);
                    _numberOfCardDecks = _cardDecks.Count;
                }
            }

            return returnCard;
        }

        private string GetGameResult()
        {
            if(CurrentState == State.Finished)
            {
                // Find highest score among players
                int highestScore = 0;
                foreach (Player player in _players)
                {
                    if(player.Score > highestScore)
                    {
                        highestScore = player.Score;
                    }
                }

                // Get the players with the highest score
                List<Player> winningPlayers = new List<Player>();
                foreach (Player player in _players)
                {
                    if(player.Score == highestScore)
                    {
                        winningPlayers.Add(player);
                    }
                }

                if (winningPlayers.Count == 1)
                {
                    return $"Player {winningPlayers[0].PlayerId} won the game!";
                }
                else if (winningPlayers.Count > 1)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (Player player in winningPlayers)
                    {
                        sb.Append(player.PlayerId.ToString());

                        if (player != winningPlayers[winningPlayers.Count - 1])
                        {
                            sb.Append(", ");
                        }
                    }

                    return $"It's a tie between players {sb}!";
                }
                else
                {
                    return "Error";
                }
            }
            else
            {
                return "Game is not finished!";
            }

        }
    }
}
