using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;

namespace CorvallisBus.Core.WebClients
{
    /// <summary>
    /// Interface for transit clients.
    /// </summary>
    public interface ITransitClient
    {
        /// <summary>
        /// Loads transit data from Connexionz and combines it with Google Transit.
        /// </summary>
        (BusSystemData data, List<string> errors) LoadTransitData();

        /// <summary>
        /// Get an ETA for a stop
        /// </summary>
        Task<ConnexionzPlatformET?> GetEta(int platformTag);

        /// <summary>
        /// Fetch the latest service alerts
        /// </summary>
        /// <param name="lastSavedTimestamp">Last saved timestamp, or NULL to fetch all data</param>
        /// <returns>A List of service alerts</returns>
        Task<(List<ServiceAlert>, ulong)?> GetServiceAlerts(DateTimeOffset? lastSavedTimestamp);

        /// <summary>
        /// Fetch the latest vehicle posisitons
        /// </summary>
        /// <param name="lastSavedTimestamp">Last saved timestamp, or NULL to fetch all data</param>
        /// <returns>
        /// A List of GTFS-specific vehicle positions.
        /// It is likely this data will need to be reformatted in a TransitManager
        /// </returns>
        Task<List<BusPosition>?> GetVehiclePositions(DateTimeOffset? lastSavedTimestamp);
    }
}