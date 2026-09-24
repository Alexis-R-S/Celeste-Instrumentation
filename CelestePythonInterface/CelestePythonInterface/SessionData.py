from enum import Enum

class SessionData(Enum):

    # Every frame you'll receive from the instrumeneted game a list of 26 values, which are the following:
    # Distances to the nearest object, calculated using raycasts
    X_POSITION = 0
    Y_POSITION = 1
    X_VELOCITY = 2
    Y_VELOCITY = 3
    TILE_SIZE = 4
    ON_GROUND = 5
    CAN_DASH = 6
    CAN_SECOND_DASH = 7
    STAMINA = 8
    X_DISTANCE_TO_OBJECTIVE = 9
    Y_DISTANCE_TO_OBJECTIVE = 10

    # Metadata, should be used for normalisation purposes
    TOTAL_SECONDS_ELAPSED = 11
    SECONDS_ELAPSED = 12
    NUMBER_OF_LEVELS_FINISHED = 13

    X_OCCUPANCY_MAP_POSITION = 14
    Y_OCCUPANCY_MAP_POSITION = 15

    BOUNDARY_RAYCASTS = 16
    TRANSITION_RAYCASTS = 24
    SOLID_RAYCASTS = 32
    SPIKES_RAYCASTS = 40

    OCCUPANCY_MAP = 48


    

    

class Inputs(Enum):
    # Then you'll have have to send back a list of 7 values, which are the following:
    # They are floats in the range [0,1]. If the value is greater than 0.5, the corresponding action will be performed.
    RIGHT = 0
    LEFT = 1
    UP = 2
    DOWN = 3
    JUMP = 4
    DASH = 5
    GRAB = 6
