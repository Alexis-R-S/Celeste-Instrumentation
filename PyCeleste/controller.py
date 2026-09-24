import keyboard
from CelestePythonInterface import SessionData

class Controller:
    """Classe de contrôle pour gérer les actions du joueur dans le jeu Celeste."""
    def __init__(self):
        self.has_displayed = False

    def to_symbol(self, terrain_channel):
        if terrain_channel == 0:    # Air
            return " "
        elif terrain_channel == 1:  # Boundary
            return "."
        elif terrain_channel == 2:  # Transition
            return "@"
        elif terrain_channel == 3:  # Solid
            return "O"
        elif terrain_channel == 4:  # Spikes
            return "^"

    def update(self, player_state):
        """Met à jour les actions du joueur en fonction de l'état actuel.
        Args:
            player_state: L'état actuel du joueur fourni par l'interface de communication. Voir SessionData pour les détails sur la structure de player_state.
        Returns:
            Une liste de 7 inputs (floats) représentant les actions à effectuer. Chaque float doit être dans la plage [0, 1]. Si la valeur est supérieure à 0.5, l'action correspondante sera effectuée.
            Voir SessionData.Inputs pour les détails sur la signification de chaque input.
        """

        if not self.has_displayed and player_state[SessionData.SECONDS_ELAPSED.value] > 5:
            print(f"""
X_position: {player_state[SessionData.X_POSITION.value]}
Y_position: {player_state[SessionData.Y_POSITION.value]}
X_velocity: {player_state[SessionData.X_VELOCITY.value]}
Y_velocity: {player_state[SessionData.Y_VELOCITY.value]}
Tile size: {player_state[SessionData.TILE_SIZE.value]}
On ground: {player_state[SessionData.ON_GROUND.value]}
Can dash: {player_state[SessionData.CAN_DASH.value]}
Can second dash: {player_state[SessionData.CAN_SECOND_DASH.value]}
Stamina: {player_state[SessionData.STAMINA.value]}
X_distance_to_objective: {player_state[SessionData.X_DISTANCE_TO_OBJECTIVE.value]}
Y_distance_to_objective: {player_state[SessionData.Y_DISTANCE_TO_OBJECTIVE.value]}
Total seconds elapsed: {player_state[SessionData.TOTAL_SECONDS_ELAPSED.value]}
Seconds elapsed: {player_state[SessionData.SECONDS_ELAPSED.value]}
Levels finished: {player_state[SessionData.NUMBER_OF_LEVELS_FINISHED.value]}
X_Occupancy map position: {player_state[SessionData.X_OCCUPANCY_MAP_POSITION.value]}
Y_Occupancy map position: {player_state[SessionData.Y_OCCUPANCY_MAP_POSITION.value]}

Boundary raycasts: {player_state[SessionData.BOUNDARY_RAYCASTS.value:SessionData.BOUNDARY_RAYCASTS.value+8]}
Transition raycasts: {player_state[SessionData.TRANSITION_RAYCASTS.value:SessionData.TRANSITION_RAYCASTS.value+8]}
Solid raycasts: {player_state[SessionData.SOLID_RAYCASTS.value:SessionData.SOLID_RAYCASTS.value+8]}
Spikes raycasts: {player_state[SessionData.SPIKES_RAYCASTS.value:SessionData.SPIKES_RAYCASTS.value+8]}
""")
            print("Occupancy map:")
            for i in range(31):
                print("".join(map(self.to_symbol, player_state[SessionData.OCCUPANCY_MAP.value+i*31:SessionData.OCCUPANCY_MAP.value+(i+1)*31])))
            
            self.has_displayed = True

        # Implémentez votre logique de contrôle ici
        return [
            1.0 if keyboard.is_pressed("right") else 0.0,
            1.0 if keyboard.is_pressed("left") else 0.0,
            1.0 if keyboard.is_pressed("up") else 0.0,
            1.0 if keyboard.is_pressed("down") else 0.0,
            1.0 if keyboard.is_pressed("space") else 0.0,
            1.0 if keyboard.is_pressed("shift") else 0.0,
            1.0 if keyboard.is_pressed("e") else 0.0
        ]