/*
 * CellProgram.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */


class CellProgram;

#ifndef CELLPROGRAM_H_
#define CELLPROGRAM_H_

#include <string>
#include <map>
#include <iosfwd>
#include <random>
#include "Simulation.h"
#include "Condition.h"
#include "Event.h"


// What are the things that I would like to do:
// 1. I have to separate between an event that is actually happnening
//    in the simulation and an EVENTTEMPLATE that just gives an
//    instruction. A template is part of a program
//    An event is part of an execution.
// 2. There is no such thing as first event of a program
//    There is only the next event and the next event is dependent
//    on a state.
//    The state can be produced by the simulation / event / whatever.


// typedef std::map<Mutation,Pair<Distribution,Pair<Cell,Cell>>> Plan;

class CellProgram {
public:
	CellProgram() = delete;
	CellProgram(const std::string& n, Simulation* s);
	virtual ~CellProgram();

	std::vector<Event*> firstEvent(float currentTime) const;
	std::vector<Event*> nextEvent(float currentTime, Event* lastEvent) const;
	std::vector<std::string> otherPrograms() const;

	void addCondition(const std::string& condition, Event* e);

	friend std::ostream& operator<<(std::ostream&, const CellProgram&);
private:
	std::string _name;
	std::map<Condition,Event*> _program;
	Simulation* _sim;

	static std::random_device _randomDev;
	static std::mt19937 _randomGen;

	bool _conditionExists(const Condition&) const;
};

#endif /* CELL_H_ */
