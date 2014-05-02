/*
 * StateTransition.cpp
 *
 *  Created on: 29 Apr 2014
 *      Author: np183
 */

#include "StateTransition.h"

using std::string;
using std::vector;

StateTransition::StateTransition(CellProgram *c, float m, float s)
: Directive(c), _mean(m), _sd(s) {}

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

std::pair<Event*,std::vector<Happening*>> StateTransition::apply(Cell* c) const {
	// TODO: Implement this
	Event* e{nullptr};
	return make_pair(e,vector<Happening*>{});
}
