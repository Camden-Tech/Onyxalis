using Lucene.Net.Support;
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
            if (value < 0.5)
            {
                return HorizontalBiomeType.Forest;
            }
            else if (value > 0.5f)
            {
                return HorizontalBiomeType.Plains;
            }
            // ... add other conditions here

            return HorizontalBiomeType.Plains; 
        }

        public float temperature;
        public float amplitude;
        public BiomeType type;
        public HorizontalBiomeType horizontalType;


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
            Forest = 2,
            Plains = 3
            
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
        
            if (lowestHeight > 300)
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
