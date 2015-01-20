/*
 * Cell.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include "Cell.h"

using std::unique_ptr;
using std::string;
using std::pair;

Cell::Cell(const CellProgram* prog, const string& state) : _alive(true), _state(new State(state)), _program(prog) {
}

Cell::Cell(const CellProgram* prog, State* state) : _alive(true), _state(state), _program(prog) {
}

Cell::~Cell() {
	// _state is unique_ptr.
	// There is no need to release it
}

const State* Cell::state() const {
	return _state.get();
}

//pair<bool,unsigned int> Cell::evaluate(Condition* cond) const {
//	return cond->evaluate(_state,_program->simulation());
//}
//
bool Cell::update(const string& var,bool val) {
	return _state->update(var,val);
}

bool Cell::update(const State* s) {
	return _state->set(s);
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
