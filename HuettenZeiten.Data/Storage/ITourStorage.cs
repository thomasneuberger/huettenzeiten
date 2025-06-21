using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Storage
{
    /// <summary>
    /// Interface for tour storage operations.
    /// </summary>
    public interface ITourStorage
    {
        /// <summary>
        /// Loads all tours from the storage.
        /// </summary>
        /// <returns>A read-only list of Tour objects.</returns>
        IReadOnlyList<Tour> LoadTours();

        /// <summary>
        /// Saves a tour to the storage.
        /// </summary>
        /// <param name="tour">The Tour object to save.</param>
        void SaveTour(Tour tour);
    }
}
