using System.Collections.Generic;
using System.Linq;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Entities
{
    /// <summary>
    /// Represents a deck of cards with a discard pile.
    /// </summary>
    public class PolicyDeck : ICardDeck<PolicyType>
    {
        public const int TotalLiberalPolicies = 6;
        public const int TotalFascistPolicies = 11;
        private List<PolicyType> _policyDeck;
        private List<PolicyType> _discardDeck;

        /// <summary>
        /// Constructs a policy deck.
        /// </summary>
        public PolicyDeck()
        {
            Reset();
        }

        /// <summary>
        /// The number of cards currently in the draw pile.
        /// </summary>
        public int DrawPileCount => _policyDeck.Count;

        /// <summary>
        /// The number of cards currently in the discard pile.
        /// </summary>
        public int DiscardPileCount => _discardDeck.Count;

        /// <summary>
        /// Reset the deck to its original state, restoring any cards that were consumed.
        /// </summary>
        public void Reset()
        {
            var _fascistCards = Enumerable.Range(1, TotalFascistPolicies).Select(_ => PolicyType.Fascist);
            var _liberalCards = Enumerable.Range(1, TotalLiberalPolicies).Select(_ => PolicyType.Liberal);
            _policyDeck = _fascistCards.Concat(_liberalCards).ToList();
            _policyDeck.Shuffle();
            _discardDeck = new List<PolicyType>();
        }

        /// <summary>
        /// Shuffle the discard pile back into the draw pile.
        /// </summary>
        public void Shuffle()
        {
            _policyDeck.AddRange(_discardDeck);
            _policyDeck.Shuffle();
        }

        /// <summary>
        /// Peek at the top of the deck.
        /// </summary>
        /// <param name="numCards">The number of cards to peek.</param>
        /// <returns>Cards at the top of the deck. If returning more than one card, the topmost card is at index 0.</returns>
        public IReadOnlyCollection<PolicyType> Peek(int numCards = 1)
        {
            ShuffleIfNeeded(numCards);
            return _policyDeck.Take(numCards).ToArray();
        }

        /// <summary>
        /// Draw cards from the top of the deck.
        /// </summary>
        /// <param name="numCards">The number of cards to draw.</param>
        /// <returns>Cards at the top of the deck. If returning more than one card, the topmost card is at index 0.</returns>
        public IReadOnlyCollection<PolicyType> Draw(int numCards = 1)
        {
            ShuffleIfNeeded(numCards);
            var drawn = Peek(numCards);
            _policyDeck.RemoveRange(0, drawn.Count);
            return drawn;
        }

        /// <summary>
        /// Place cards onto the discard pile. These cards can/will be shuffled back in.
        /// </summary>
        /// <param name="discard">Cards to add to the discard pile.</param>
        public void Discard(IEnumerable<PolicyType> discard)
        {
            _discardDeck.AddRange(discard);
        }

        /// <summary>
        /// Place a card onto the discard pile. These cards can/will be shuffled back in.
        /// </summary>
        /// <param name="discard">Card to add to the discard pile.</param>
        public void Discard(PolicyType discard)
        {
            _discardDeck.Add(discard);
        }

        private void ShuffleIfNeeded(int cardsToDraw)
        {
            if (_policyDeck.Count() < cardsToDraw)
                Shuffle();
        }
    }
}
