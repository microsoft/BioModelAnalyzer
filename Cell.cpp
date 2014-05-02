/*
 * Cell.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include "Cell.h"

using std::string;
using std::pair;

Cell::Cell(const CellProgram* prog, const string& state) : _alive(true), _state(new State(state)), _program(prog) {
}

Cell::Cell(const CellProgram* prog, State* state) : _alive(true), _state(state), _program(prog) {
}

Cell::~Cell() {
	if (_state)
		delete _state;
}

const State* Cell::state() const {
	return _state;
}

pair<bool,unsigned int> Cell::evaluate(Condition* cond) const {
	return cond->evaluate(_state,_program->simulation());
}

bool Cell::update(const string& var,bool val) {
	return _state->update(var,val);
}

const string Cell::name() const {
	return _program->name();
}

const CellProgram* Cell::program() const {
	return _program;
}

bool Cell::expressed(const string& cond) const {
	return _state->value(cond).first && _state->value(cond).second;
}

bool Cell::alive() const {
	return _alive;
}

void Cell::kill() {
	_alive=false;
}
