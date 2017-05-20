using System.Collections.Generic;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Represents a deck of cards with a discard pile.
    /// </summary>
    public interface ICardDeck<TCard>
    {
        /// <summary>
        /// Reset the deck to its original state, restoring any cards that were consumed.
        /// </summary>
        void Reset();

        /// <summary>
        /// Shuffle the discard pile back into the draw pile.
        /// </summary>
        void Shuffle();

        /// <summary>
        /// Peek at the top of the deck.
        /// </summary>
        /// <param name="numCards">The number of cards to peek.</param>
        /// <returns>Cards at the top of the deck. If returning more than one card, the topmost card is at index 0.</returns>
        IReadOnlyList<TCard> Peek(int numCards = 1);

        /// <summary>
        /// Draw cards from the top of the deck.
        /// </summary>
        /// <param name="numCards">The number of cards to draw.</param>
        /// <returns>Cards at the top of the deck. If returning more than one card, the topmost card is at index 0.</returns>
        IReadOnlyList<TCard> Draw(int numCards = 1);

        /// <summary>
        /// Place cards onto the discard pile. These cards can/will be shuffled back in.
        /// </summary>
        /// <param name="discard">Cards to add to the discard pile.</param>
        void Discard(IEnumerable<TCard> discard);

        /// <summary>
        /// Place a card onto the discard pile. These cards can/will be shuffled back in.
        /// </summary>
        /// <param name="discard">Card to add to the discard pile.</param>
        void Discard(TCard discard);
    }
}