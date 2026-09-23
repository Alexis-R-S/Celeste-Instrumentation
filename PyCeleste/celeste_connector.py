from CelestePythonInterface import SocketInterface, SessionParameters, SocketServer

class CelesteConnector:
    def __init__(self, level="1", area_key=1, area_mode=0, timeout_seconds=999999999999, objective_x=282, objective_y=-24):
        """Initialise le connecteur Celeste avec les paramètres de session spécifiés.
        Args:
            level (str): Le niveau à charger.
            area_key (int): La clé de la zone à charger.
            area_mode (int): Le mode de la zone à charger.
            timeout_seconds (int): Le temps maximum de la session en secondes.
            objective_x (int): La coordonnée X de l'objectif.
            objective_y (int): La coordonnée Y de l'objectif.
        """
        self.params = SessionParameters()
        self.params.Level = level
        self.params.AreaKey = area_key
        self.params.AreaMode = area_mode
        self.params.TimeoutSeconds = timeout_seconds
        self.params.ObjectiveXCoordinate = objective_x
        self.params.ObjectiveYCoordinate = objective_y

        self.interface = None

    def connect(self):
        """Établit la connexion avec le jeu Celeste et initialise l'interface de communication."""

        print("En attente de connexion avec Celeste...")
        server = SocketServer()
        client_socket, addr = server.await_connection()
        print(f"Connecté ! ({addr})")

        def no_agent(player_state):     # Agent function to do nothing (default function)
            return [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]

        self.interface = SocketInterface(client_socket, no_agent, self.params)

    def set_agent(self, agent_function):
        """Définit la fonction de l'agent qui sera appelée à chaque frame.
        Args:
            agent_function (function): La fonction de l'agent qui prend en entrée l'état du joueur et retourne les actions à effectuer.
        """
        self.interface.agent = agent_function

    def run(self):
        """Exécute la session de jeu avec l'agent défini.
        S'arrête lorsque le joueur meurt ou que le temps imparti est écoulé.
        Returns:
            final_state: L'état final de la session.
        """
        final_state = self.interface.run()

        return final_state
