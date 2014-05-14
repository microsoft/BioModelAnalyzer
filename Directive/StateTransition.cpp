/*
 * StateTransition.cpp
 *
 *  Created on: 29 Apr 2014
 *      Author: np183
 */

#include "StateTransition.h"
#include "../Event/ChangeState.h"

using std::string;
using std::vector;
using std::make_pair;

StateTransition::StateTransition(CellProgram *c, float m, float s)
: Directive(c), _mean(m), _sd(s) {}

StateTransition::StateTransition(CellProgram *c, float m, float s, State* st) 
	: Directive(c), _mean(m), _sd(s), _changes(st)

StateTransition::~StateTransition() {
}

//void StateTransition::addChange(const std::string& var, bool val) {
//	if (_changes.find(var)!=_changes.end()) {
//		_changes[var]=val;
//	}
//	else {
//		_changes.insert(make_pair(var,val));
//	}
//}
//

vector<string> StateTransition::programs() const {
	return vector<string>{_cProg->name()};
}

std::pair<Event*,std::vector<Happening*>> StateTransition::apply(Cell* c,float duration, float time) const {
	Event* e{ nullptr };
	if (c == nullptr) {
		return make_pair(e,vector<Happening*>{});
	}

	State* oldState{ c->state() == nullptr ? nullptr : new State(*(c->state())) };
	c->update(_changes);
	//for (auto varVal : _changes) {
	//	c->update(varVal.first, varVal.second);
	//}
	State* newState{ c->state() == nullptr ? nullptr : new State(*(c->state())) };
	e = new ChangeState(duration, time, oldState, newState, c);
	const Directive* d{ c->program()->bestDirective(c->state(),time-duration,time) };
	return make_pair(e, vector < Happening* > {new Happening(time, _mean, _sd, c->program()->simulation(), c)});
}
