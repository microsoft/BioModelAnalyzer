// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/*
 * StateTransition.h
 *
 *  Created on: 29 Apr 2014
 *      Author: np183
 */

#ifndef STATETRANSITION_H_
#define STATETRANSITION_H_

#include <map>
#include <vector>
#include "../State.h"
#include "Directive.h"

class StateTransition: public Directive {
public:
	StateTransition()=delete;
	StateTransition(CellProgram* c,float m, float s);
	StateTransition(CellProgram* c, float m, float s, State* st);

	virtual ~StateTransition();

	// void addChange(const std::string&, bool);

	virtual std::vector<std::string> programs() const;

	//virtual std::vector<Event*> nextEvents(float, Cell*, State*) const;
	virtual std::pair<Event*,std::vector<Happening*>> apply(Cell*,float duration, float time) const;

private:
	float _mean;
	float _sd;
	std::auto_ptr<State> _changes;
	// Why not change this to a State?????
	// std::map<std::string,bool> _changes;
};

#endif /* STATETRANSITION_H_ */
