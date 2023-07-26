import random

import pygame

from constants import *


class Obstacle(pygame.sprite.Sprite):
    def __init__(self):
        super().__init__()

        # Create obstacle surface
        self.image = pygame.image.load('../graphics/obstacles/skull.png').convert_alpha()

        # Get player rectangle
        self.rect = self.image.get_rect(bottomleft=(random.randint(1100, 1300), OBSTACLE_START_Y))

        # Define obstacle's velocity
        self.vel_x = -5
        self.vel_y = 0

    def update(self):
        self.move()
        self.destroy()

    def move(self):
        self.rect.x += self.vel_x

    def destroy(self):
        if self.rect.right < 0:
            self.kill()
