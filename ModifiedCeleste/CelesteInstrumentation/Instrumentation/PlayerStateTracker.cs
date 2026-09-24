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
	}
}
