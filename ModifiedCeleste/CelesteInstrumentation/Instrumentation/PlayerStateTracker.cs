using Celeste;
using CelesteInstrumentation.Instrumentation;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Instrumentation
{
	public class PlayerStateTracker : Component
	{
		public override void Update()
		{
			Level level = base.Entity.SceneAs<Level>();

			if ( this.PlayerState.SecondsElapsed >= Instrumentation.SessionParameters.TimeoutSeconds)
			{
				Instrumentation.EndSession(this.Player);
				return;
			}

            this.PlayerState.XPosition = this.Player.Position.X;
			this.PlayerState.YPosition = this.Player.Position.Y;
            this.PlayerState.XVelocity = this.Player.Speed.X;
            this.PlayerState.YVelocity = this.Player.Speed.Y;
			this.PlayerState.TileSize = 8;
			this.PlayerState.OnGround = this.Player.OnGround(1);
            this.PlayerState.CanDash = (this.Player.Dashes != 0);
			this.PlayerState.CanSecondDash = (this.Player.Dashes >= 2);
            this.PlayerState.Stamina = this.Player.Stamina;

			this.PlayerState.XDistanceToObjective = Math.Abs(this.Player.Position.X - this.objective.X);
			this.PlayerState.YDistanceToObjective = Math.Abs(this.Player.Position.Y - this.objective.Y);

			for (int ray_i = 0; ray_i < PlayerState.RaycastsAmount; ray_i++)
			{
				Vector2 raycastDirection = this.getRaycastDirection(ray_i);
				int[] raycasts = this.Raycast(raycastDirection, level);
				for (int channel_i=0; channel_i < raycasts.Length; channel_i++)
				{
					this.PlayerState.Raycasts[ray_i * PlayerState.TerrainChannelsAmount + channel_i] = raycasts[channel_i];
                }
            }

			// TODO : local occupancy map
			Tuple<int, int> occupancyMapPosition = this.GetOccupancyMapPosition(this.Player.Position);
            this.PlayerState.XOCcupancyMapPosition = occupancyMapPosition.Item1*PlayerState.TileSize;
			this.PlayerState.YOCcupancyMapPosition = occupancyMapPosition.Item2*PlayerState.TileSize;

            this.BuildOccupancyMap(occupancyMapPosition, level);


            if (Instrumentation.inSession)
            {
				InputsManager.ScheduleKeys(InputsManager.ByteToKeys(Socket.SendAndReceiveData(this.PlayerState.Serialize())[0]));
			}
			
			base.Update();
			this.FramesElapsed += 1f;
			this.PlayerState.SecondsElapsed = this.FramesElapsed / 60f;
			this.PlayerState.TotalSecondsElapsed = this.FramesElapsed / 60f;
		}

		public override void DebugRender(Camera camera)
		{
			// Render raycasts
			for (int i = 0; i < this.PlayerState.RaycastsAmount; i++)
			{
				float shortestCast = this.PlayerState.GetRaycast(i, 0);

                for (int channel_i = 1; channel_i < PlayerState.TerrainChannelsAmount; channel_i++)
				{
					float cast = (float)this.PlayerState.GetRaycast(i, channel_i);
                    shortestCast = Math.Min(shortestCast, cast == 0.0f ? float.PositiveInfinity : cast);
				}
				Draw.Line(
					base.Entity.Position,
					base.Entity.Position + this.getRaycastDirection(i) * (float)shortestCast,
					Color.Yellow,
					1.0f
				);
			}
			string text = ((int)this.Player.Position.X).ToString() + ", " + ((int)this.Player.Position.Y).ToString();
			Draw.Text(Draw.DefaultFont, text, this.Player.Position, Color.White);

			int xTile = (int)Math.Floor(PlayerState.XPosition / 8);
            int yTile = (int) Math.Floor((PlayerState.YPosition-1) / 8);

			Draw.HollowRect(
				xTile*8,
				yTile*8,
				8f,
				8f,
				Color.Purple
			);

			Draw.HollowRect((float)PlayerState.XOCcupancyMapPosition, (float)PlayerState.YOCcupancyMapPosition, 8f, 8f, Color.Purple);
            Draw.HollowRect((float)(PlayerState.XOCcupancyMapPosition+ 8*30 ), (float)(PlayerState.YOCcupancyMapPosition+8*30), 8f, 8f, Color.Purple);
            Draw.HollowRect((float)(PlayerState.XOCcupancyMapPosition + 8 * 30), (float)(PlayerState.YOCcupancyMapPosition), 8f, 8f, Color.Purple);
            Draw.HollowRect((float)(PlayerState.XOCcupancyMapPosition), (float)(PlayerState.YOCcupancyMapPosition + 8 * 30), 8f, 8f, Color.Purple);
            base.DebugRender(camera);
		}

		public PlayerStateTracker(Player player) : base(true, true)
		{
			this.PlayerState = new PlayerState();
			this.objective = new Vector2(Instrumentation.SessionParameters.ObjectiveXCoordinate, Instrumentation.SessionParameters.ObjectiveYCoordinate);
			this.Player = player;
			this.FramesElapsed = 0f;
		}

		public int[] Raycast(Vector2 directionUnitVector, Level level)
		{
            Vector2 origin = base.Entity.Position - Vector2.UnitY;
            int[] result = new int[PlayerState.TerrainChannelsAmount];
			bool[] channelsFound = new bool[PlayerState.TerrainChannelsAmount];	// Default : all are set to false
            
			for (int distance = 1; ; distance++)
			{
                Vector2 position = origin + directionUnitVector * (float)distance;

                // Ray has left playable area
                if (!level.Bounds.Contains((int)position.X, (int)position.Y))
                {
                    bool canTransition = level.Session.MapData.CanTransitionTo(
                        level,
                        position + directionUnitVector * 5f
                    );

                    result[(int)(canTransition ? TerrainChannels.Transition : TerrainChannels.Boundary)] = distance;

                    break;
                }

                if (!channelsFound[(int)TerrainChannels.Solid] && base.Scene.CollideCheck<Solid>(position))
                {
					result[(int)TerrainChannels.Solid] = distance;
					channelsFound[(int)TerrainChannels.Solid] = true;
                }
                if (!channelsFound[(int)TerrainChannels.Spikes] && base.Scene.CollideCheck<Spikes>(position))
                {
                    result[(int)TerrainChannels.Spikes] = distance;
					channelsFound[(int)TerrainChannels.Spikes] = true;
                }
            }


			return result;
		}

		public PlayerState PlayerState;

		public Player Player;

		public float FramesElapsed;

		private Vector2 objective;

		private Vector2 getRaycastDirection(int ray_index)
		{
            return new Vector2(1f, 0f).Rotate(2 * (float)Math.PI * ray_index / PlayerState.RaycastsAmount);
        }

        private Tuple<int, int> GetOccupancyMapPosition(Vector2 playerPosition)
        {
            int xPlayerTile = (int)Math.Floor(PlayerState.XPosition / PlayerState.TileSize);
            int yPlayerTile = (int)Math.Floor((PlayerState.YPosition - 1) / PlayerState.TileSize);

            int xMapTile = xPlayerTile - (PlayerState.OccupancyMapSize - 1) / 2;
            int yMapTile = yPlayerTile - (PlayerState.OccupancyMapSize - 1) / 2;

            return new Tuple<int, int>(xMapTile, yMapTile);
        }


        private void BuildOccupancyMap(Tuple<int, int> occupancyMapPosition, Level level)
        {
            for (int xOcc=0; xOcc < PlayerState.OccupancyMapSize; xOcc++)
			{
				for (int yOcc=0; yOcc < PlayerState.OccupancyMapSize; yOcc++)
				{
					PlayerState.SetOccupancyMap(
						xOcc,
						yOcc,
						GetChannelAt(xOcc + occupancyMapPosition.Item1, yOcc + occupancyMapPosition.Item2, level)
					);
				}
			}
        }

		private TerrainChannels GetChannelAt(int xTile, int yTile, Level level)
		{
			Vector2 position = new Vector2(xTile * PlayerState.TileSize, yTile * PlayerState.TileSize);

			Rectangle collideRect = new Rectangle((int)(position.X - 3), (int)(position.Y-3), 6, 6);

            if (! level.Bounds.Contains((int)position.X, (int)position.Y))
			{
                // If can transition to another level
                if (level.Session.MapData.CanTransitionTo(level, position))
				{
					return TerrainChannels.Transition;
				}
                return TerrainChannels.Boundary;
            }
			
			if (Scene.CollideCheck<Solid>(collideRect))
			{
				return TerrainChannels.Solid;
			}
			if (Scene.CollideCheck<Spikes>(collideRect))
			{
				return TerrainChannels.Spikes;
			}
			return TerrainChannels.Air;
		}
    }
}
