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
        /// Fetch the latest service alerts
        /// </summary>
        /// <returns>A List of service alerts</returns>
        Task<(List<ServiceAlert>, ulong)?> GetServiceAlerts(DateTimeOffset? lastSavedTimestamp);
    }
}
