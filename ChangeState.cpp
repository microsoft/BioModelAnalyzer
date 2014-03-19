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

ChangeState::ChangeState(Simulation* s, Cell* c) : Event(0.0,0.0,s,c) {
	// TODO Auto-generated constructor stub

}

ChangeState::~ChangeState() {
	// TODO Auto-generated destructor stub
}

vector<Event*> ChangeState::execute() const
{
	return vector<Event*>{};
}


bool ChangeState::concerns(const string&) const {
	return false;
}

ostream& operator<<(ostream& out,const ChangeState& state) {
	state.output(out);
	return out;
}
