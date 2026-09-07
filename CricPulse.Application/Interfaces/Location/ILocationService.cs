using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.Interfaces.Location
{
    public interface ILocationService
    {
        Task<string?> GetStateAsync(double latitude, double longitude);


    }
}
