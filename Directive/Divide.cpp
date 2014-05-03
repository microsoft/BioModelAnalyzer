/*
 * Divide.cpp
 *
 *  Created on: 25 Mar 2014
 *      Author: np183
 */

#include "Divide.h"
#include "../Event/Division.h"

using std::string;
using std::vector;
//using std::tuple;
//using std::make_tuple;

Divide::Divide(CellProgram* c,
			   string d1, State* st1, float mean1, float sd1,
			   string d2, State* st2, float mean2, float sd2)
: Directive(c),
  _daughter1(d1), _st1(st1), _mean1(mean1), _sd1(sd1),
  _daughter2(d2), _st2(st2), _mean2(mean2), _sd2(sd2)
{}

Divide::~Divide() {
	delete _st1;
	delete _st2;
}


vector<string> Divide::programs() const {
	return vector<string>{_daughter1,_daughter2};
}


//tuple<const string&, const State*, float, float>
//Divide::daughter1() {
//	return make_tuple(_daughter1,_st1,_mean1,_sd1);
//}
//
//tuple<const string&, const State*, float, float>
//Divide::daughter2() {
//	return make_tuple(_daughter2,_st2,_mean2,_sd2);
//}
//
//vector<Event*> Divide::nextEvents(float currentTime, Cell* c) const {
//	// TODO: implement this
//	float duration{_randomTime()};
//	State* st1Copy=(_st1==nullptr ? nullptr : new State(*_st1));
//	State* st2Copy=(_st2==nullptr ? nullptr : new State(*_st2));
//	Event* div=new Division(_cProg->name(),_daughter1,st1Copy,
//			                _daughter2,st2Copy,
//			                duration,currentTime+duration,
//			                _cProg->simulation(),c);
//
//	return vector<Event*>{div};
//}

std::pair<Event*,std::vector<Happening*>> Divide::apply(Cell* c,float duration, float time) const {
	State* st1Copy{_st1==nullptr ? nullptr : new State(*_st1)};
	State* st2Copy{_st2==nullptr ? nullptr : new State(*_st2)};
	Event* e{new Division(_cProg->name(),
						  _daughter1,st1Copy,
						  _daughter2,st2Copy,
						  duration,time,c)};

	Simulation* sim{c->program()->simulation()};

	c->kill();

	vector<Happening*> ret{};
	CellProgram* d1{sim->program(_daughter1)};
	if (d1!=nullptr) {
		const string st1Str{_st1==nullptr ? "" : _st1->toString()};
		vector<Happening*> first{d1->firstEvent(time,st1Str,_mean1,_sd1)};
		ret.insert(ret.end(),first.begin(),first.end());
	}
	CellProgram* d2{sim->program(_daughter2)};
	if (d2!=nullptr) {
		const string st2Str{_st2==nullptr ? "" : _st2->toString()};
		vector<Happening*> second{d2->firstEvent(time,st2Str,_mean2,_sd2)};
		ret.insert(ret.end(),second.begin(),second.end());
	}

	return make_pair(e,ret);
}
