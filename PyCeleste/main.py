from celeste_connector import CelesteConnector
from CelestePythonInterface import SessionData
from controller import Controller

celeste_conn = CelesteConnector()
celeste_conn.connect()

controller = Controller()
celeste_conn.set_agent(controller.update)

while True:     # Control goes back here after each player death
    final_state = celeste_conn.run()
    
    print("\n=== Session terminée ===")
    print(f"Frames écoulées   : {final_state[SessionData.NUMBER_OF_LEVELS_FINISHED.value]}")
    print(f"Temps (s)         : {final_state[SessionData.SECONDS_ELAPSED.value]:.2f}")
    print(f"Distance objectif X: {final_state[SessionData.X_DISTANCE_TO_OBJECTIVE.value]:.1f}")
    print(f"Distance objectif Y: {final_state[SessionData.Y_DISTANCE_TO_OBJECTIVE.value]:.1f}")

    controller.has_displayed = False  # Reset the display flag for the next session