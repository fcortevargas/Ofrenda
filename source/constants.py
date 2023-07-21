# Set window's width and height
WINDOW_WIDTH = 1080  # px
WINDOW_HEIGHT = 720  # px

# Set game's frame rate
FRAMERATE = 60  # Hz

# Define ground/window height ratio
GROUND_RATIO = 0.20

# Define ground dimensions
GROUND_WIDTH = WINDOW_WIDTH
GROUND_HEIGHT = WINDOW_HEIGHT * GROUND_RATIO

# Define ground origin position
GROUND_POS_X = 0
GROUND_POS_Y = WINDOW_HEIGHT * (1 - GROUND_RATIO)

# Define sky/window height ratio
SKY_RATIO = 1 - GROUND_RATIO

# Define sky dimensions
SKY_WIDTH = WINDOW_WIDTH
SKY_HEIGHT = WINDOW_HEIGHT * SKY_RATIO

# Define sky origin position
SKY_POS_X = 0
SKY_POS_Y = 0

# Define player's starting position
PLAYER_START_X = 30
PLAYER_START_Y = GROUND_POS_Y

# Define obstacle's starting y-position
OBSTACLE_START_Y = GROUND_POS_Y


