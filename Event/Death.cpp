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
//	// TODO Auto-generated destructor stub
//	return vector<Event*> {};
//}
//
void Death::output(ostream& out) const {
	Event::output(out);
	out << " " << _cell << " X";
}

string Death::toString() const {
	// TODO implement me
	return "";
}

bool Death::concerns(const string& name) const {
	return (_cell==name);
}

ostream& operator<<(ostream& out,const Death& state) {
	state.output(out);
	return out;
}
