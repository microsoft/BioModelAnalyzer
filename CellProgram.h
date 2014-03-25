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
#include "Simulation.h"
#include "Condition.h"
#include "Directive/Directive.h"
#include "Event/Event.h"


// What are the things that I would like to do:
// 1. I have to separate between an event that is actually happnening
//    in the simulation and an EVENTTEMPLATE that just gives an
//    instruction. A template is part of a program
//    An event is part of an execution.
//    I think that I need a Directive class
//    it will be very similar to event
//    but it's main action would be to take a state
//    and return an appropriate event.
//    for now it can be a string.
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

	void addCondition(const std::string& condition, Directive* d);

	friend std::ostream& operator<<(std::ostream&, const CellProgram&);
private:
	std::string _name;
	std::map<Condition,Directive*> _program;
	Simulation* _sim;

	bool _conditionExists(const Condition&) const;
};

#endif /* CELL_H_ */
