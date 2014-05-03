/*
 * Condition.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include <iostream>
#include "Condition.h"
#include "HelperFunctions.h"

using std::string;
using std::map;
using std::pair;
using std::make_pair;
using std::ostream;

Condition::Condition(const string& initializer)
: _def(false)
{
	if (removeSpace(initializer)=="DEFAULT") {
		_def=true;
		return;
	}
	_conjunction=splitConjunction(initializer);
}

Condition::~Condition() {
}

bool Condition::isDef() const {
	return _def;
}

std::pair<bool,unsigned int> Condition::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	if (_def) {
		return make_pair(true,0);
	}
	if (!st) {
		return make_pair(false,0); // What do you do with a state that is null?
	}
	unsigned int ret{0};
	for (auto varPol : _conjunction) {
		if (_generalCondition(varPol.first)) {
			if (nullptr!=sim && sim->expressed(varPol.first,from,to)) {
				++ret;
			}
			else {
				return make_pair(false,0);
			}
		}
		else {
			pair<bool,bool> satVal{st->value(varPol.first)};
			if (!satVal.first) {
				if (varPol.second) {
					return make_pair(false,0);
				}
				else {
					++ret;
				}
			}
			else {
				if (satVal.second != varPol.second) {
					return make_pair(false,0);
				}
				else {
					++ret;
				}
			}
		}
	}
	return make_pair(true,ret);
}

bool Condition::operator==(const Condition& other) const {
	if (_def || other._def) {
		return _def==other._def;
	}

	auto otherIt = other._conjunction.begin();
	for (auto myIt = _conjunction.begin() ; myIt != _conjunction.end() ; ++myIt) {
		// Different lengths
		if (otherIt == other._conjunction.end()) {
			return false;
		}
		// There is a difference in the name of condition
		// or its polarity
		if (myIt->first != otherIt->first ||
			myIt->second != otherIt->second) {
			return false;
		}
		++otherIt;
	}
	// Different lengths
	if (otherIt!=other._conjunction.end()) {
		return false;
	}
	return true;
}

bool Condition::operator<(const Condition& other) const {
	if (_def || other._def) {
		return !other._def;
	}

	auto otherIt = other._conjunction.begin();
	for (auto myIt = _conjunction.begin(); myIt != _conjunction.end() ; ++myIt, ++otherIt) {
		// Other reached end first
		if (otherIt == other._conjunction.end()) {
			return true;
		}
		int compare{(myIt->first).compare(otherIt->first)};
		// other is more
		if (compare>0) {
			return true;
		}
		// other is less
		else if (compare<0) {
			return false;
		}
		// They have the same string
		else {
			if (myIt->second < otherIt->second) {
				return true;
			}
			else if (myIt->second > otherIt->second) {
				return false;
			}
		}
	}
	// Both reached the end of their conditions
	// and no difference was found -> they are equivalent
	if (otherIt == other._conjunction.end()) {
		return false;
	}
	// No difference was found but the other has
	// more conditions
	return true;
}

ostream& operator<<(ostream& out, const Condition& c) {
	if (c._def) {
		out << "DEFAULT";
		return out;
	}

	bool first{true};
	for (auto condVal : c._conjunction) {
		if (!first) {
			out << "&";
		}
		if (!(condVal.second)) {
			out << "!";
		}
		out << condVal.first;
		first = false;
	}
	return out;
}

bool Condition::_generalCondition(const string& name) const {
	return name.find('[')!=std::string::npos;
}
