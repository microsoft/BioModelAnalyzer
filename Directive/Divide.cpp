/*
 * Divide.cpp
 *
 *  Created on: 25 Mar 2014
 *      Author: np183
 */

#include "Divide.h"
#include "../Event/Division.h"

using std::string;
using std::vector;

Divide::Divide(float mean, float sd, CellProgram* c, std::string d1, State* st1, std::string d2, State* st2)
: Directive(mean,sd,c), _daughter1(d1), _st1(st1), _daughter2(d2), _st2(st2)
{}

Divide::~Divide() {
	delete _st1;
	delete _st2;
}


vector<string> Divide::programs() const {
	return vector<string>{_daughter1,_daughter2};
}

vector<Event*> Divide::nextEvents(float currentTime, Cell* c, State* currentState) const {
	// TODO: implement this
	float duration{_randomTime()};
	State* st1Copy=(_st1==nullptr ? nullptr : new State(*_st1));
	State* st2Copy=(_st2==nullptr ? nullptr : new State(*_st2));
	Event* div=new Division(_cProg->name(),_daughter1,st1Copy,
			                _daughter2,st2Copy,
			                duration,currentTime+duration,
			                _cProg->simulation(),c);

	return vector<Event*>{div};
}
