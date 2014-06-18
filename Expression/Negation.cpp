#include "Negation.h"

using std::string;
using std::make_pair;
using std::pair;

Negation::Negation(BoolExp *sub) 
	: _sub(sub)
{
}


Negation::~Negation()
{
	if (_sub) {
		delete _sub;
	}
}


pair<bool, unsigned int> Negation::evaluate(const State* st, const Simulation* sim, float from, float to) const  {
	if (_sub->evaluate(st, sim, from, to).first) {
		return make_pair(false, 0);
	}
	return make_pair(true, 1);
}

string Negation::toString() const {
	string temp{ "!" };
	return temp + _sub->toString();
}
