using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Biome
    {
        public static HorizontalBiomeType GetHorizontalTerrainType(float value)
        {
            if (value <= 0.1)
            {
                return HorizontalBiomeType.Ocean;
            }
            else if (value < 0.3)
            {
                return HorizontalBiomeType.Forest;
            }
            else if (value < 0.4)
            {
                return HorizontalBiomeType.Plains;
            }
            else if (value >= 0.6)
            {
                return HorizontalBiomeType.Mountains;
            }
            // ... add other conditions here

            return HorizontalBiomeType.Plains; 
        }

        public float temperature;
        public float amplitude;
        public BiomeType type;
        public HorizontalBiomeType horizontalType;

        public static Dictionary<int, (float amp, float frequency)> biomeStats = new Dictionary<int, (float amp, float frequency)>()
        {
            {0, (1,1)},
            {1, (10,0.15f)},
            {2, (20,0.30f)},
            {3, (300, 1)}, // no frequency greater than 1
        };
        
        public enum BiomeType
        {
            
            Troposphere,
            Stratosphere,
            Exosphere,
            Space,
            Underground,
            Tunnels,
            Caves,
            Surface,
            GravityDistortionZone
            
            
        }




        public enum HorizontalBiomeType
        {
            Ocean = 0,
            Forest = 1,
            Plains = 2,
            Mountains = 3
            
        }

        public static BiomeType GetBiomeType(float[] heights) {
            float lowestHeight = heights[0];
            BiomeType biomeType;
            for (int X = 1; X < 64; X++)
            {
                float height = heights[X];
                if (height < lowestHeight)
                {
                    lowestHeight = height;
                }
            }
        
            if (lowestHeight > 200)
            {
                biomeType = BiomeType.Underground;
                
            }
            else if (lowestHeight < 160) {
                biomeType = BiomeType.Space;
            } else {
                biomeType = BiomeType.Surface;
            }
            return biomeType;
        }
        
        
        public Biome(HorizontalBiomeType horizontalType, float temp, float amp) 
        {
            this.horizontalType = horizontalType;
            temperature = temp;
            amplitude = amp; 
            
        }
    }
}
