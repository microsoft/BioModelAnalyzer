// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/*
 * Death.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include "Death.h"

using std::vector;
using std::ostream;
using std::string;

Death::Death(const string& name, /*Simulation* s,*/ Cell* c) : Event(0.0,0.0,/*s,*/c), _cell(name) {
}

Death::~Death() {
}

//vector<Event*> Death::execute() const {
//	return vector<Event*> {};
//}
//

void Death::output(ostream& out) const {
	Event::output(out);
	out << " " << cell()->name() << " X";
}

string Death::toString() const {
	// TODO implement me
	string ret{};
	ret += Event::toString();
	ret += ",";
	ret += _cell;
	return ret;
}

bool Death::concerns(const string& name) const {
	return (_cell==name);
}

bool Death::expressed(const string& cell, const string& var) const {
	return false;
}

ostream& operator<<(ostream& out,const Death& state) {
	state.output(out);
	return out;
}
