#include "AndExp.h"

using std::pair;
using std::make_pair;

AndExp::AndExp(BoolExp* left, BoolExp* right)
	: _sub1(left), _sub2(right)
{
}


AndExp::~AndExp()
{
	if (_sub1) {
		delete _sub1;
	}
	if (_sub2) {
		delete _sub2;
	}
}

BoolExp* AndExp::copy() const {
	return new AndExp(_sub1->copy(), _sub2->copy());
}

 pair<bool, unsigned int> AndExp::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	 if (_sub1->evaluate(st, sim, from, to).first && _sub2->evaluate(st, sim, from, to).second) {
		 return make_pair(true, _sub1->evaluate(st, sim, from, to).second + _sub2->evaluate(st, sim, from, to).second);
	 }
	 return make_pair(false, 0);
}

std::string AndExp::toString() const {
	return _sub1->toString() + "&" + _sub2->toString();
}
