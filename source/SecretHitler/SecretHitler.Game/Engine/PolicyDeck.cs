using System;
using System.Collections.Generic;
using System.Linq;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Engine
{
    /// <summary>
    /// Represents a deck of cards with a discard pile.
    /// </summary>
    public class PolicyDeck : ICardDeck<PolicyType>
    {
        private readonly IList<PolicyType> _policyDeck;
        private readonly IList<PolicyType> _discardDeck;

        /// <summary>
        /// Constructs a policy deck
        /// </summary>
        /// <param name="draw">
        /// The draw pile.
        /// </param>
        /// <param name="discard">
        /// The discard pile.
        /// </param>
        /// <param name="reset">
        /// Whether or not the decks should be reset to the default.
        /// </param>
        public PolicyDeck(IList<PolicyType> draw, IList<PolicyType> discard, bool reset)
        {
            _policyDeck = draw ?? throw new ArgumentNullException(nameof(draw));
            _discardDeck = discard ?? throw new ArgumentNullException(nameof(discard));

            if (reset)
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
            var _fascistCards = Enumerable.Range(1, Constants.TotalFascistPolicies).Select(_ => PolicyType.Fascist);
            var _liberalCards = Enumerable.Range(1, Constants.TotalLiberalPolicies).Select(_ => PolicyType.Liberal);
            _policyDeck.Clear();
            _policyDeck.AddRange(_fascistCards.Concat(_liberalCards));
            _policyDeck.Shuffle();
            _discardDeck.Clear();
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
        public IReadOnlyList<PolicyType> Peek(int numCards = 1)
        {
            ShuffleIfNeeded(numCards);
            return _policyDeck.Take(numCards).ToArray();
        }

        /// <summary>
        /// Draw cards from the top of the deck.
        /// </summary>
        /// <param name="numCards">The number of cards to draw.</param>
        /// <returns>Cards at the top of the deck. If returning more than one card, the topmost card is at index 0.</returns>
        public IReadOnlyList<PolicyType> Draw(int numCards = 1)
        {
            ShuffleIfNeeded(numCards);
            var drawn = Peek(numCards);

            for (var i = 0; i < drawn.Count; i++)
                _policyDeck.RemoveAt(0);

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
