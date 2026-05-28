using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorvallisBus.Core.WebClients
{
    public interface ITransitClient
    {
        (BusSystemData data, List<string> errors) LoadTransitData();
        Task<ConnexionzPlatformET?> GetEta(int platformTag);

        /// <summary>
        /// Fetch details for all buses
        /// </summary>
        /// <param name="lastSavedTimestamp">Last saved timestamp, or NULL to fetch all data</param>
        /// <returns>A List of bus details</returns>
        Task<(List<BusDetails>, ulong)?> GetBusDetails(DateTimeOffset? lastSavedTimestamp);

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
