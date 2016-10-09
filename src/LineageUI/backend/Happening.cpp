/*
 * Happening.cpp
 *
 *  Created on: May 2, 2014
 *      Author: np183
 */

#include <string>
#include "Happening.h"

using std::vector;
using std::pair;
using std::make_pair;
using std::string;

std::random_device Happening::_randomDev{};
std::mt19937 Happening::_randomGen{Happening::_randomDev()};

Happening::Happening(float time, float mean, float sd, Simulation* s, Cell* c)
: _duration(_randomTime(mean,sd)), _execTime(time+_duration), _sim(s), _cell(c)
{
}

Happening::~Happening() {
}

Cell* Happening::cell() const
{
	return _cell;
}

Simulation* Happening::simulation() const
{
	return _sim;
}

float Happening::duration() const
{
	return _duration;
}

float Happening::execTime() const
{
	return _execTime;
}


bool Happening::operator<(const Happening& other) const {
	return (_execTime < other._execTime);
}

pair<Event*, vector<Happening*>> Happening::execute() const {
	Event* e{nullptr};
	const CellProgram* prog{_cell->program()};
	if (prog==nullptr) {
		return make_pair(e,vector<Happening*>{});
	}
	const Directive* d={prog->bestDirective(_cell->state(),_execTime-_duration,_execTime)};
	if (d==nullptr) {
		return make_pair(e,vector<Happening*>{});
	}
	return d->apply(_cell,_duration,_execTime);
}

float Happening::_randomTime(const float& mean, const float& sd) const {
	if (mean <= 0.0 || sd <= 0.0) {
		string err{ "Something that is to happen to " };
		err += _cell->name();
		err += " cannnot determine the time for next action. Some action for this cell type does not have a time.";
		throw err;
	}
	std::normal_distribution<> d(mean,sd);
	return static_cast<float>(d(_randomGen));
}
