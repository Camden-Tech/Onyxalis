using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Weather
    {
        public enum WeatherConditions 
        { 
            Regular,
            Rain,
            Stormy,
            Hail,
            Tornado
        }

        public int globalTemperature;
        public int windSpeed;
        public Boolean windDirection;
        public WeatherConditions weather;
        public int weatherSeverity;


    }
}
