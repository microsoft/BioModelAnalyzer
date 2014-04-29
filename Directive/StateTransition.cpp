/*
 * StateTransition.cpp
 *
 *  Created on: 29 Apr 2014
 *      Author: np183
 */

#include "StateTransition.h"

using std::string;
using std::vector;

StateTransition::StateTransition(float m, float s, CellProgram *c)
: Directive(m,s,c) {}

StateTransition::~StateTransition() {
}

void StateTransition::addChange(const std::string& var, bool val) {
	if (_changes.find(var)!=_changes.end()) {
		_changes[var]=val;
	}
	else {
		_changes.insert(make_pair(var,val));
	}
}

vector<string> StateTransition::programs() const {
	return vector<string>{_cProg->name()};
}

vector<Event*> nextEvents(float t, Cell* c, State* s) const {
	// TODO: Implement this
	return vector<Event*>{};
}
