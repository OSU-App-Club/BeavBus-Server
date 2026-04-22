using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using CorvallisBus.Core.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.WebClients
{
    public interface ITransitClient
    {
        (BusSystemData data, List<string> errors) LoadTransitData();
        Task<ConnexionzPlatformET?> GetEta(int platformTag);

        /// <summary>
        /// Get Service Alerts from GTFS
        /// </summary>
        Task<List<GtfsServiceAlert>> GetServiceAlerts();

        Task<List<GtfsVehiclePosition>> GetVehiclePositions();
    }
}
