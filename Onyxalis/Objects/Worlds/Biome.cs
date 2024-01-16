using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Biome
    {
        public static BiomeType GetTerrainType(float value)
        {
            if (value <= 0.1)
            {
                return BiomeType.Ocean;
            }
            else if (value < 0.3)
            {
                return BiomeType.Forest;
            }
            else if (value < 0.4)
            {
                return BiomeType.Plains;
            }
            else if (value >= 0.6)
            {
                return BiomeType.Mountains;
            }
            // ... add other conditions here

            return BiomeType.Plains; 
        }

        public float temperature;
        public float amplitude;
        public BiomeType type;
        public static Dictionary<int, (float amp, float frequency)> biomeStats = new Dictionary<int, (float amp, float frequency)>()
        {
            {0, (1,1)},
            {1, (20,0.15f)},
            {2, (30,0.30f)},
            {3, (300, 1)}, // no frequency greater than 1
        };
        

        public enum BiomeType
        {
            Ocean = 0,
            Forest = 1,
            Plains = 2,
            Mountains = 3
            
        }


        public Biome(BiomeType type, float temp, float amp) 
        {
            this.type = type;
            temperature = temp;
            amplitude = amp; 
            
        }
    }
}
