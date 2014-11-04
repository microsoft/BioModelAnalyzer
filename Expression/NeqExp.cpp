#include "NeqExp.h"

using std::string;
using std::pair;
using std::make_pair;

NeqExp::NeqExp(const string& var, const string& val)
	: EqExp(var, val)
{
}

NeqExp::~NeqExp()
{
}

BoolExp* NeqExp::copy() const {
	return new NeqExp(_var, _val);
}

pair<bool, unsigned int> NeqExp::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	if (EqExp::evaluate(st, sim, from, to).first) {
		return make_pair(false, 0);
	}
	return make_pair(true, 1);
}

std::string NeqExp::toString() const {
	return _var + "!=" + _val;
}
