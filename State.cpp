/*
 * State.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#include "State.h"
#include "HelperFunctions.h"

using std::map;
using std::string;
using std::make_pair;
using std::pair;

State::State(const string& initializer)
: _varVals{splitConjunction(initializer)}
{
}

State::~State() {
}

unsigned int State::evaluate(const Condition& cond) const {
	// TODO: complete this
	return 0;
}

pair<bool,bool> State::value(const string& var) const {
	auto it=_varVals.find(var);
	if (it == _varVals.end()) {
		return make_pair(false,false);
	}
	return make_pair(true,it->second);
}

bool State::update(const string& var, bool val) {
	if (_varVals.find(var) == _varVals.end()) {
		return false;
	}

	_varVals[var]=val;
	return true;
}

