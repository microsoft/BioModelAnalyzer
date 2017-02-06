// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/*
 * Happening.h
 *
 *  Created on: May 2, 2014
 *      Author: np183
 */

class Happening;

#ifndef HAPPENING_H_
#define HAPPENING_H_

#include <vector>
#include <random>
#include "Cell.h"
#include "Simulation.h"
#include "Event/Event.h"

class Happening {
public:
	Happening() =delete;
	Happening(float t, float mean, float sd, Simulation* s, Cell* c);
	~Happening();

	Cell* cell() const;
	Simulation* simulation() const;
	float duration() const;
	float execTime() const;

	std::pair<Event*,std::vector<Happening*>> execute() const;

	bool operator<(const Happening& other) const;
private:
	float _duration;
	float _execTime;
	Simulation* _sim;
	Cell* _cell;

	static std::random_device _randomDev;
	static std::mt19937 _randomGen;

	float _randomTime(const float& mean, const float& sd) const;
	// float _randomTime() const;
};

#endif /* HAPPENING_H_ */
