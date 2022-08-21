import copy
from target import Target
import random
import numpy as np


class Agent:
    def __init__(self, mutation_chance: float):
        self.targets: list[Target] = []

        self.fitness = 0
        self.run_data = {}
        self.mutation_chance = mutation_chance

    def add_target(self, num_actions, num_choices):
        target = Target(num_actions=num_actions, num_choices=num_choices)
        self.targets.append(target)

    def get_run_data(self):
        return self.run_data

    def get_actions(self):
        actions = []
        for target in self.targets:
            actions += target.get_actions()
            actions.append(-1)
        return actions

    def set_fitness(self, f):
        self.fitness = f

    def clone_from(self, other):
        self.targets = copy.deepcopy(other.targets)
        # for i in range(len(other.targets)):
        #     self.argets.appned(copy.deepcopy(other.targets[i]))

    def mutate(self):
        self.targets[-1].mutate(self.mutation_chance)

    def get_fitness(self):
        return self.fitness

    def cut_to(self, action_counter):
        self.targets[-1].num_actions = action_counter
        pass
