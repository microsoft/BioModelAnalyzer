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

Death::Death(Simulation* s) : Event(0.0,0.0,s) {
	// TODO Auto-generated constructor stub

}

Death::~Death() {
	// TODO Auto-generated destructor stub
}

vector<Event*> Death::execute() const {
	// TODO Auto-generated destructor stub
	return vector<Event*> {};
}

//void Death::output(ostream& out) const {
//	Event::output(out);
//}
//

bool Death::concerns(const string&) const {
	return false;
}

ostream& operator<<(ostream& out,const Death& state) {
	state.output(out);
	return out;
}
