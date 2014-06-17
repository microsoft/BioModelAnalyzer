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

State::State(const string& initializer)
: _vars{splitConjunction(initializer)}
{
}

State::State(const State& other)
: _vars{other._vars} // Check what does copy constructor do
{
	// TODO: Need to implement a deep copy of the variables!!!
}

State::~State() {
	// TODO: need to release the memory stored in the variables
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
	// TODO: add some type checking 
	if (update(var, val)) {
		return true;
	}
	_vars.insert(make_pair(var,new Variable(var,val)));
	return false;
}

bool State::set(const string& var, Type::Value& val) {
	// TODO: add some type checking
	if (update(var, val)) {
		return true;
	}
	// TODO: How do I get the type from a value???
	_vars.insert(make_pair(var, new Variable(var, val.type(), val)))
}

// TODO: add some type checking 
bool State::update(const string& var, bool val) {
	if (_varVals.find(var) == _varVals.end()) {
		return false;
	}

	_varVals[var]=val;
	return true;
}

// TODO: add some type checking 
bool State::update(const State* other) {
	if (other == nullptr)
		return false;

	bool ret{ true };
	for (auto varVal : other->_varVals) {
		if (!update(varVal.first, varVal.second)) {
			ret = false;
			set(varVal.first, varVal.second);
		}
	}
	return ret;
}

State* State::copyOverwrite(const State* other) const {
	State* ret{new State(*this)};
	if (nullptr==other) {
		return ret;
	}

	for (auto varVal : other->_varVals) {
		ret->set(varVal.first,varVal.second);
	}
	return ret;
}


string State::toString() const {
	stringstream temp{};
	temp << *this;
	return temp.str();
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
