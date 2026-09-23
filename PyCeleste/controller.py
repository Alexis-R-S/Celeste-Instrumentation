import keyboard

class Controller:
    """Classe de contrôle pour gérer les actions du joueur dans le jeu Celeste."""
    def __init__(self):
        pass

    def update(self, player_state):
        """Met à jour les actions du joueur en fonction de l'état actuel.
        Args:
            player_state: L'état actuel du joueur fourni par l'interface de communication. Voir SessionData pour les détails sur la structure de player_state.
        Returns:
            Une liste de 7 inputs (floats) représentant les actions à effectuer. Chaque float doit être dans la plage [0, 1]. Si la valeur est supérieure à 0.5, l'action correspondante sera effectuée.
            Voir SessionData.Inputs pour les détails sur la signification de chaque input.
        """

        print(player_state)  # Affiche l'état actuel du joueur pour le débogage

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