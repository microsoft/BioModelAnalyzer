// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/*
 * Condition.cpp
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#include <iostream>
#include "Condition.h"
#include "HelperFunctions.h"
#include "Expression/AndExp.h"
#include "Expression/EqExp.h"

using std::string;
using std::map;
using std::pair;
using std::make_pair;
using std::ostream;

Condition::Condition(const string& initializer)
	: _def(false), _conjunction(nullptr)
{
	if (removeSpace(initializer)=="DEFAULT") {
		_def=true;
		return;
	}
	_conjunction=parseBoolExp(initializer);
}

Condition::Condition(const Condition& other)
	: _def(other._def), _conjunction(other._conjunction->copy())
{
}

Condition::Condition(Condition&& other)
	: _def(other._def), _conjunction(other._conjunction)
{
	other._conjunction = nullptr;
}

Condition::~Condition() {
	if (_conjunction) {
		delete _conjunction;
	}
}

bool Condition::isDef() const {
	return _def;
}

void Condition::addCellCycle(const string& cellCycle) {
	if (_def) {
		_def = false;
	}
	EqExp* e = new EqExp("CellCycle", cellCycle);
	if (_conjunction) {
		_conjunction = new AndExp(e, _conjunction);
	}
	else {
		_conjunction = e;
	}
}

std::pair<bool,unsigned int> Condition::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	if (_def) {
		return make_pair(true,0);
	}
	if (!st) {
		return make_pair(false,0); // What do you do with a state that is null?
	}
	return 	_conjunction->evaluate(st, sim, from, to);
}

bool Condition::operator==(const Condition& other) const {
	if (_def || other._def) {
		return _def==other._def;
	}

	if ((_conjunction == nullptr && other._conjunction != nullptr) ||
		(_conjunction != nullptr && other._conjunction == nullptr)) {
		return false;
	}

	if (_conjunction == nullptr) { // No need to check other
		return true;
	}

	return _conjunction->toString() == other._conjunction->toString();
}

bool Condition::operator<(const Condition& other) const {
	if (_def || other._def) {
		return !other._def;
	}
	if (_conjunction == nullptr || other._conjunction == nullptr) {
		return other._conjunction != nullptr;
	}
	return _conjunction->toString() < other._conjunction->toString();
}

ostream& operator<<(ostream& out, const Condition& c) {
	if (c._def) {
		out << "DEFAULT";
		return out;
	}

	if (c._conjunction == nullptr) {
		out << "ERROR!!!!!";
		return out;
	}

	out << c._conjunction->toString();
	return out;
}

