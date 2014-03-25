/*
 * Divide.cpp
 *
 *  Created on: 25 Mar 2014
 *      Author: np183
 */

#include "Divide.h"

using std::string;
using std::vector;

Divide::Divide(float mean, float sd, std::string d1, std::string d2)
: Directive(mean,sd), _daughter1(d1), _daughter2(d2)
{}

Divide::~Divide() {
}


vector<string> Divide::programs() const {
	return vector<string>{_daughter1,_daughter2};
}

vector<Event*> Divide::nextEvents(Event* current) const {
	// TODO: implement this
//	float currentTime{current->execTime()};
	return vector<Event*>{};
}
