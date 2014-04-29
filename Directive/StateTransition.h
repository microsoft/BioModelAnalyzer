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
	StateTransition(float m, float s, CellProgram* c);

	virtual ~StateTransition();

	void addChange(const std::string&, bool);

	virtual std::vector<std::string> programs() const;

	virtual std::vector<Event*> nextEvents(float, Cell*, State*) const;

private:
	std::map<std::string,bool> _changes;
};

#endif /* STATETRANSITION_H_ */
