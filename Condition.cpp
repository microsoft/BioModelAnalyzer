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
: _conjunction{splitConjunction(initializer)}
{
}

Condition::~Condition() {
}

std::pair<bool,unsigned int> Condition::evaluate(const State& st) const {
	unsigned int ret{0};
	for (auto mapElem : _conjunction) {
		pair<bool,bool> stVal=st.value(mapElem.first);
		if (!stVal.first) {
			if (mapElem.second) {
				return make_pair(false,0);
			}
			else {
				++ret;
			}
		}
		else {
			if (stVal.second != mapElem.second) {
				return make_pair(false,0);
			}
			else {
				++ret;
			}
		}
	}
	return make_pair(true,ret);
}


bool Condition::operator==(const Condition& other) const {
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
	bool first{true};
	for (auto strBool : c._conjunction) {
		if (!first) {
			out << "&";
		}
		if (!(strBool.second)) {
			out << "!";
		}
		out << strBool.first;
	}
	return out;
}

