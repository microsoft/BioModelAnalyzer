/*
 * Cell.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include "Cell.h"

using std::string;
using std::pair;

Cell::Cell(const CellProgram* prog, const string& state) : _state(new State(state)), _program(prog) {
}

Cell::Cell(const CellProgram* prog, State* state) : _state(state), _program(prog) {
}

Cell::~Cell() {
	delete _state;
}

pair<bool,unsigned int> Cell::evaluate(Condition* cond) const {
	return cond->evaluate(_state);
}

bool Cell::update(const string& var,bool val) {
	return _state->update(var,val);
}
