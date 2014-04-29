/*
 * ChangeState.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include "ChangeState.h"

using std::ostream;
using std::vector;
using std::string;

ChangeState::ChangeState(float d, float t, Simulation* s, Cell* c) : Event(d,t,s,c) {
}

ChangeState::~ChangeState() {
}

vector<Event*> ChangeState::execute() const
{
	Simulation *sim(simulation());
	//TODO: implement this
	// 1. get the best match for this program
	// 2. create a new division that matches
	// 3. what you've found
	return vector<Event*>{};
}

void ChangeState::output(ostream& out) const {
}

bool ChangeState::concerns(const string& name) const {
	return cell()->name()==name;
}

string ChangeState::toString() const {
	return "";
}

ostream& operator<<(ostream& out,const ChangeState& state) {
	state.output(out);
	return out;
}
