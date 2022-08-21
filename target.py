import copy
import random
import numpy as np

# insertion, deletion, change
MUTATION_WEIGHTS = [1, 1, 1]


def fill_actions_list(actions: list, num_choices: int, num_actions: int):
    while len(actions) < num_actions:
        actions.append(random.randint(0, num_choices - 1))


class Target:
    def __init__(self, num_choices: int, num_actions: int):
        self.num_actions = num_actions
        self.num_choices = num_choices
        self.actions = []
        fill_actions_list(self.actions, num_actions=num_actions, num_choices=num_choices)

    def get_actions(self):
        return self.actions[:self.num_actions]

    def mutate(self, mutation_chance: float):
        for e, a in enumerate(self.actions):
            if random.random() > mutation_chance:
                continue
            mut_type = random.choices(range(len(MUTATION_WEIGHTS)), weights=MUTATION_WEIGHTS)[0]
            # print(mut_type)

            new_action = random.randint(0, self.num_choices - 1)

            if mut_type == 0:  # insertion
                self.actions.insert(e, new_action)
            elif mut_type == 1:  # deletion
                self.actions.pop(e)
            elif mut_type == 2:  # change
                self.actions[e] = new_action
        fill_actions_list(self.actions, self.num_choices, self.num_actions)
