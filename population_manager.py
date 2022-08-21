import copy
import json
import random

from agent import Agent
import numpy as np

CARRY_OVER = 1

MIN_MUTATION_CHANCE = 1
MAX_MUTATION_CHANCE = 1


class Population:
    def __init__(self, population_size, write_results: bool = True):
        self.write_results = write_results
        self.agents = []
        self.generation_counter = 0

        mutation_spread = np.linspace(MIN_MUTATION_CHANCE, MAX_MUTATION_CHANCE, population_size)
        for i in range(population_size):
            a = Agent(mutation_chance=mutation_spread[i])
            self.agents.append(a)

        self.results = []
        self.dump_results()

    def add_target(self, num_actions, num_choices):
        for agent in self.get_agents():
            agent.add_target(num_actions=num_actions, num_choices=num_choices)

    def dump_results(self):
        with open("results.json", 'w+') as f:
            f.write(json.dumps({"results": self.results}, indent=4))

    def get_agents(self) -> list[Agent]:
        return self.agents

    def sort(self):
        self.agents.sort(key=lambda x: x.fitness, reverse=True)

    def evolve(self):
        self.sort()
        if len(self.agents) < CARRY_OVER:
            return
        for a in self.agents[CARRY_OVER:]:
            source_ind = random.randint(0, CARRY_OVER - 1)
            source = self.agents[source_ind]

            a.clone_from(source)

            a.mutate()

        if self.write_results:
            self.results.append({
                "generation": self.generation_counter,
                "fitness": self.agents[0].fitness,
                "actions": self.agents[0].get_actions(),
                "run_data": self.agents[0].get_run_data()
            })
            self.dump_results()

        self.generation_counter += 1

    def propogate(self):
        self.sort()
        for agent in self.agents[1:]:
            for e, target in enumerate(agent.targets):
                target.actions = copy.deepcopy(self.agents[0].targets[e].actions)
