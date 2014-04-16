/*
 * State.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#include <iostream>
#include "State.h"
#include "HelperFunctions.h"

using std::ostream;
using std::map;
using std::string;
using std::make_pair;
using std::pair;

State::State(const string& initializer)
: _varVals{splitConjunction(initializer)}
{
}

State::State(const State& other)
: _varVals{other._varVals}
{
}

State::~State() {
}

//pair<bool,unsigned int> State::evaluate(const Condition& cond) const {
//	return cond.evaluate(this);
//}

pair<bool,bool> State::value(const string& var) const {
	if (var.find('[') || var.find(']')) {
		const string err{"trying to evaluate a global condition on a state"};
		throw err;
	}
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

// TODO:
// This code is copied from Condition operator<<
// Move the printout of the map to either a helper function or a class
// that wraps the joint map
ostream& operator<<(ostream& out, const State& st) {
	bool first{true};
	for (auto varPol : st._varVals) {
		if (!first) {
			out << "&";
		}
		if (!(varPol.second)) {
			out << "!";
		}
		out << varPol.first;
		first = false;
	}
	return out;
}
