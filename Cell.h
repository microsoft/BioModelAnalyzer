/*
 * Cell.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */


class Cell;

#ifndef CELL_H_
#define CELL_H_

#include <string>
#include <map>
#include <iosfwd>
#include <random>
#include "Simulation.h"
#include "Event.h"

// typedef std::map<Mutation,Pair<Distribution,Pair<Cell,Cell>>> Plan;

class Cell {
public:
	Cell() = delete;
	Cell(const std::string& n, const float& m, const float& sd, const std::string& d1, const std::string& d2,Simulation* s);
	virtual ~Cell();

	std::vector<Event*> firstEvent(float currentTime) const;
	std::vector<Event*> nextEvent(float currentTime, Event* lastEvent) const;
	std::vector<std::string> otherPrograms() const;

	friend std::ostream& operator<<(std::ostream&, const Cell&);
private:
	std::string _name;
	float _meanTime;
	float _sd;
	std::string _daughter1;
	std::string _daughter2;
	Simulation* _sim;
// 	Plan _plan;

//	bool _pre;
//	bool _dead;
	static std::random_device _randomDev;
	static std::mt19937 _randomGen;


};

#endif /* CELL_H_ */
