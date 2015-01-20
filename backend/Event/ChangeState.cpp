/*
 * ChangeState.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include "ChangeState.h"

using std::unique_ptr;
using std::ostream;
using std::vector;
using std::string;

ChangeState::ChangeState(float d, float t, State* oldS, State* newS,/*Simulation* s,*/ Cell* c) 
: Event(d, t,/*s,*/c), _oldState{ oldS }, _newState{ newS } {
}

ChangeState::~ChangeState() {
}

//vector<Event*> ChangeState::execute() const
//{
//	Simulation *sim(simulation());
//	//TODO: implement this
//	// 1. get the best match for this program
//	// 2. create a new division that matches
//	// 3. what you've found
//	return vector<Event*>{};
//}
//
void ChangeState::output(ostream& out) const {
}

bool ChangeState::concerns(const string& name) const {
	return cell()->name()==name;
}

bool ChangeState::expressed(const string& cell, const string& var) const {
	if (cell != this->cell()->name()) {
		return false;
	}
	if (nullptr != _oldState.get()) {
		auto existsVal = _oldState->value(var);
		if (existsVal.first && existsVal.second) {
			return true;
		}
	}
	if (nullptr != _newState.get()) {
		auto existsVal = _newState->value(var);
		if (existsVal.first && existsVal.second) {
			return true;
		}
	}
	return false;
}

string ChangeState::toString() const {
	return "";
}

ostream& operator<<(ostream& out,const ChangeState& state) {
	state.output(out);
	return out;
}
