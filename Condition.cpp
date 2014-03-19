/*
 * Condition.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include "Condition.h"
#include "HelperFunctions.h"

using std::string;
using std::map;

Condition::Condition(const string& initializer)
: _conjunction{splitConjunction(initializer)}
{
}

Condition::~Condition() {
}

bool Condition::evaluate(const State& st) const {
	// TODO: implement this
	return false;
}


bool Condition::operator==(const Condition& other) const {
	// TODO: implement this
	return false;
}
