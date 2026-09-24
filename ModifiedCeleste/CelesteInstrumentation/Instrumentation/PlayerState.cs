using CelesteInstrumentation.Instrumentation;
using System;
using System.IO;
using System.Linq;

namespace Instrumentation
{
	public class PlayerState
	{
		public readonly int OccupancyMapSize = 31;
        public readonly int TerrainChannelsAmount = ((TerrainChannels[])Enum.GetValues(typeof(TerrainChannels))).Distinct().Count();
        public readonly int RaycastsAmount = 8;

		public PlayerState()
		{
            this.Raycasts = new int[RaycastsAmount * TerrainChannelsAmount];
			this.OccupancyMap = new int[OccupancyMapSize * OccupancyMapSize];
		}

		public byte[] Serialize()
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
                    binaryWriter.Write(this.XPosition);
                    binaryWriter.Write(this.YPosition);
                    binaryWriter.Write(this.XVelocity);
                    binaryWriter.Write(this.YVelocity);
                    binaryWriter.Write((float)this.TileSize);
                    binaryWriter.Write(this.OnGround ? 1f : 0f);
                    binaryWriter.Write(this.CanDash ? 1f : 0f);
                    binaryWriter.Write(this.CanSecondDash ? 1f : 0f);
                    binaryWriter.Write(this.Stamina);
                    binaryWriter.Write(this.XDistanceToObjective);
                    binaryWriter.Write(this.YDistanceToObjective);
                    binaryWriter.Write(this.TotalSecondsElapsed);
                    binaryWriter.Write(this.SecondsElapsed);
                    binaryWriter.Write((float)this.FinishedLevelsNumber);
					binaryWriter.Write(this.XOCcupancyMapPosition);
					binaryWriter.Write(this.YOCcupancyMapPosition);
					foreach (float ray in Raycasts)
					{
						binaryWriter.Write((float) ray);
					}
					foreach (float mapTile in OccupancyMap)
					{
                        binaryWriter.Write((float) mapTile);
                    }
					result = memoryStream.ToArray();
				}
			}
			return result;
		}

		// Game data (All distances are in pixels)
		public float XPosition;
		public float YPosition;
        public float XVelocity;
        public float YVelocity;
		public int TileSize;
		public bool OnGround;
		public bool CanDash;
		public bool CanSecondDash;
		public float Stamina;

		// Objective
		public float XDistanceToObjective;
		public float YDistanceToObjective;

		// Metadata
		public float TotalSecondsElapsed;	// Total session training time
		public float SecondsElapsed;		// Since last death/reset
		public int FinishedLevelsNumber;    // When end on level exit is false, counts number of levels finished

		// Flattened raycasts
		// [dir0_chan0, dir0_chan1, dir0_chan2, dir1_chan0, dir1_chan1, dir1_chan2, ...]
		public int[] Raycasts;

        public int GetRaycast(int rayIndex, int channelIndex)
		{
			checkRaycastRange(rayIndex, channelIndex);
			return Raycasts[rayIndex * TerrainChannelsAmount + channelIndex];
		}

		public void SetRaycast(int rayIndex, int channelIndex, int value)
		{
			checkRaycastRange(rayIndex, channelIndex);
            Raycasts[rayIndex * TerrainChannelsAmount + channelIndex] = value;
        }

		private void checkRaycastRange(int rayIndex, int channelIndex)
		{
            if (rayIndex * TerrainChannelsAmount + channelIndex >= TerrainChannelsAmount * RaycastsAmount)
            {
                throw new IndexOutOfRangeException($"Raycast index {rayIndex} with channel {channelIndex} is out of range.");
            }
        }

        // Occupancy map
        public float XOCcupancyMapPosition;		// Position of occupancy map on level
		public float YOCcupancyMapPosition;
        // Flattened local egocentric occupancy map
		// [x0y0, x1y0, x2y0, ...
		// x0y1, x1y1, x2y1, ...
		// x0y2, x1y2, x2y2, ... ]
        public int[] OccupancyMap;

		public int GetOccupancyMap(int xPosition, int yPosition)
		{
            checkOccupancyMapRange(xPosition, yPosition);
            return OccupancyMap[yPosition*OccupancyMapSize + xPosition];
		}

        public void SetOccupancyMap(int xPosition, int yPosition, TerrainChannels channel)
        {
            checkOccupancyMapRange(xPosition, yPosition);
            OccupancyMap[yPosition * OccupancyMapSize + xPosition] = (int) channel;
        }

		private void checkOccupancyMapRange(int xPosition, int yPosition)
		{
			if (yPosition * OccupancyMapSize + xPosition >= OccupancyMapSize*OccupancyMapSize)
			{
                throw new IndexOutOfRangeException($"Position ({xPosition}, {yPosition}) is out of range.");
            }
		}
    }
}
