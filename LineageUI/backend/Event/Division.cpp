// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/*
 * Division.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include "Division.h"

using std::ostream;
using std::stringstream;
using std::string;
using std::vector;
using std::pair;
using std::map;

Division::Division(const std::string& p, State* stp,
				   const std::string& d1, State* st1,
		           const std::string& d2, State* st2,
		           float d, float t,
		           /*Simulation* s,*/ Cell* c)
: Event(d,t,/*s,*/c), _parent(p), _stp(stp), _daughter1(d1), _st1(st1), _daughter2(d2), _st2(st2) {}

Division::~Division() {
	if (nullptr != _stp)
		delete _stp;
	if (nullptr != _st1)
		delete _st1;
	if (nullptr != _st2)
		delete _st2;
}


string Division::parent() const {
	return _parent;
}
string Division::daughter1() const {
	return _daughter1;
}
string Division::dauthger2() const {
	return _daughter2;
}

void Division::setParent(const string& p) {
	_parent=p;
}

void Division::setDaughter1(const string& d1) {
	_daughter1=d1;
}

void Division::setDaughter2(const string& d2) {
	_daughter2=d2;
}

//vector<Event*> Division::execute() const {
//	Simulation* sim(simulation());
//	CellProgram* d1(sim->program(_daughter1));
//	CellProgram* d2(sim->program(_daughter2));
//	vector<Event*> events{},events1{},events2{};
//	this->cell()->kill();
//	if (d1)
//		events1=d1->firstEvent(this->execTime(),_st1);
//	if (d2)
//		events2=d2->firstEvent(this->execTime(),_st2);
//	for (auto event : events1) {
//		events.push_back(event);
//	}
//	for (auto event : events2) {
//		events.push_back(event);
//	}
//	return events;
//}
//

void Division::output(ostream& out) const {
	Event::output(out);
	out << " " << _parent;
	if (_stp != nullptr) {
		out << "[" << *_stp << "]";
	}
	out << " -> (" << _daughter1;
	if (_st1!=nullptr) {
		out << "[" << *_st1 << "]";
	}
	out << "," << _daughter2;
	if (_st2!=nullptr) {
		out << "[" << *_st2 << "]";
	}
	out << ")";
	// out << "(" << _daughter1.size() << "," << _daughter2.size() << ")";
}


bool Division::concerns(const string& name) const {
	return (_parent==name || _daughter1==name || _daughter2==name);
}

bool Division::expressed(const string& cell, const string& var) const {
	if (_parent==cell) {
		if (_stp != nullptr) {
			pair<bool, const Type::Value*> existsVal{ _stp->value(var) };
			if (existsVal.first && existsVal.second->operator()()) {
				return true;
			}
		}
	}
	if (_daughter1==cell) {
		if (_st1 != nullptr) {
			pair<bool, const Type::Value*> existsVal{ _st1->value(var) };
			if (existsVal.first && existsVal.second->operator()()) {
				return true;
			}
		}
	}
	if (_daughter2==cell) {
		if (_st2 != nullptr) {
			pair<bool, const Type::Value*> existsVal{ _st2->value(var) };
			if (existsVal.first && existsVal.second->operator()()) {
				return true;
			}
		}
	}
	return false;
}

string Division::toString() const {
	string ret{};
	ret+=Event::toString();
	ret+=",";
	ret+=_parent;
	ret+=",";
	if (_stp) {
		ret += _stp->toString("CellCycle",true);
		ret += ",";
		ret += _stp->toString("CellCycle", false);
	}
	else {
		ret += ",";
	}
	ret += ",";
	ret+=_daughter1;
	ret+=",";
	if (_st1!=nullptr) {
		ret += _st1->toString("CellCycle",true);
		ret += ",";
		ret += _st1->toString("CellCycle",false);
	}
	else {
		ret += ","; 
	}
	ret+=",";
	ret+=_daughter2;
	ret+=",";
	if (_st2!=nullptr) {
		ret += _st2->toString("CellCycle", true);
		ret += ",";
		ret+=_st2->toString("CellCycle",false);
	}
	else {
		ret += ",";
	}
	return ret;
}

string Division::toJson(unsigned int id, const map<string, string>& str2Json) const {
	stringstream ret;
	ret << "{id:\\\"node" << id << "\\\", name:\\\"";
	ret << cell()->name();
	ret << "\\\", data:{}, children:[";
	bool addComma = false;
	{
		auto ptr = str2Json.find(_daughter1);
		if (ptr != str2Json.end()) {
			ret << ptr->second;
			addComma = true;
		}
	}
	{
		auto ptr = str2Json.find(_daughter2);
		if (ptr != str2Json.end()) {
			if (addComma) {
				ret << ", ";
			}
			ret << ptr->second;
		}
	}
	ret << "]}";
	return ret.str();
}

ostream& operator<<(ostream& out, const Division& d) {
	d.output(out);
	return out;
}


