/*
 * State.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include "State.h"
#include "HelperFunctions.h"
#include "Variable/BoolType.h"

using std::ostream;
using std::stringstream;
using std::map;
using std::string;
using std::make_pair;
using std::pair;

const string CELL_CYCLE_VAR{ "CellCycle" };

State::State(const string& initializer, const Simulation* sim)
: _vars(splitConjunction(initializer,sim))
{
}

State::State(const State& other)
: _vars() 
{
	for (auto strVar : other._vars) {
		_vars.insert(make_pair(strVar.first, new Variable(*(strVar.second))));
	}
}

State::State(State&& other)
	: _vars(std::move(other._vars))
{}


State::~State() {
	for (auto pair : _vars) {
		if (pair.second) {
			delete pair.second;
		}
	}
}

pair<bool,const Type::Value*> State::value(const string& var) const {
	if (var.find('[')!=std::string::npos || var.find(']')!=std::string::npos) {
		const string err{"trying to evaluate a global condition on a state"};
		throw err;
	}
	auto it=_vars.find(var);
	if (it == _vars.end()) {
		return make_pair(false,nullptr);
	}
	return make_pair(true,it->second->value());
}

bool State::set(const string& var,bool val) {
	if (update(var, val)) {
		return true;
	}
	_vars.insert(make_pair(var,new Variable(var,val)));
	return false;
}

bool State::set(const string& var, const Type::Value& val) {
	// TODO: Check that the Value matches 
	if (update(var, val)) {
		return true;
	}
	// TODO: How do I get the type from a value???
	_vars.insert(make_pair(var, new Variable(var/*, val.type()*/, val.copy())));
	return false;
}

// TODO: add some type checking 
bool State::set(const State* other) {
	if (other == nullptr)
		return false;

	bool ret{ true };
	for (auto var : other->_vars) {
		if (!update(var.first, *(var.second->value()))) {
			ret = false;
			set(var.first, *(var.second->value()));
		}
	}
	return ret;
}


bool State::update(const string& var, bool val) {
	auto varIt=_vars.find(var);
	if (varIt == _vars.end()) {
		return false;
	}

	varIt->second->set(val);
	return true;
}

bool State::update(const string& var, const Type::Value& val) {
	auto varIt = _vars.find(var);
	if (varIt == _vars.end()) {
		return false;
	}
	varIt->second->set(val);
	return true;
}

void State::addCellCycle(const Type::Value& cellCycle) {
	// TODO: Find the unique cellcycle type and find the 
	// value that corresponds to the cell cycle
	set(CELL_CYCLE_VAR, cellCycle);
}

State* State::copyOverwrite(const State* other) const {
	State* ret{new State(*this)};
	if (nullptr==other) {
		return ret;
	}

	for (auto varVal : other->_vars) {
		ret->set(varVal.first,*(varVal.second->value()));
	}
	return ret;
}


// If the variable is empty then just output all compnents of the state
// If the variable is not empty then either output only this variable (match)
// or match everything except the variable (!match)
//
// TODO:
// This code shares a lot with Condition operator<<
// Move the printout of the map to either a helper function or a class
// that wraps the joint map
string State::toString(const string variable, bool match) const {

	stringstream temp{};

	if (variable.size() == 0) {
		bool first{ true };
		for (auto var : _vars) {
			if (!first) {
				temp << "&";
			}
			temp << *(var.second);
			first = false;
		}
		return temp.str();
	}

	if (match) {
		for (auto var : _vars) {
			if (var.first == variable && match) {
				temp << *(var.second);
				return temp.str();
			}
		}
		return "";
	}

	bool first{ true };
	for (auto var : _vars) {
		if (var.first != variable) {
			if (!first) {
				temp << "&";
			}
			temp << *(var.second);
			first = false;
		}
	}
	return temp.str();
}

ostream& operator<<(ostream& out, const State& st) {
	return	out << st.toString();
}
