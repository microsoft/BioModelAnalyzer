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
#include <functional>
#include <memory>
#include "Simulation.h"
#include "Condition.h"
#include "Directive/Directive.h"
#include "Event/Event.h"


// typedef std::map<Mutation,Pair<Distribution,Pair<Cell,Cell>>> Plan;

class CellProgram {
public:
	CellProgram() = delete;
	CellProgram(const std::string& n, Simulation* s);
	virtual ~CellProgram();

	std::string name() const;
	Simulation* simulation() const;
	std::vector<Event*> firstEvent(float currentTime, State* state) const;
	// std::vector<Event*> nextEvent(float currentTime, Cell* cell, State* state) const;
	std::vector<std::string> otherPrograms() const;

	void addCondition(Condition* c, Directive* d);

	friend std::ostream& operator<<(std::ostream&, const CellProgram&);
private:
	std::string _name;
	Simulation* _sim;
	std::map<Condition*,Directive*,std::function<bool(Condition* a,Condition* b)>> _program;

	bool _conditionExists(Condition*) const;
	Directive* _bestMatch(const State* st) const;
};

#endif /* CELL_H_ */
