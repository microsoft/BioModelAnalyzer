/*
 * Cell.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include "Cell.h"

using std::string;
using std::pair;

Cell::Cell(const string& condition) : _state(condition) {
}

Cell::~Cell() {
}

pair<bool,unsigned int> Cell::evaluate(const Condition& cond) const {
	return cond.evaluate(_state);
}

bool Cell::update(const string& var,bool val) {
	return _state.update(var,val);
}
