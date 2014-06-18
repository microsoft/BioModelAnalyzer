#include "EqExp.h"

using std::string;
using std::pair;
using std::make_pair;

EqExp::EqExp(const string& var, const string& val) 
	: _var(var), _val(val)
{
}


EqExp::~EqExp()
{
}

pair<bool, unsigned int> EqExp::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	if (_var.find("[") != std::string::npos) {
		if (sim->expressed(_var, from, to)) {
			return make_pair(true, 1);
		}
		return make_pair(false, 0);
	}

	auto existVal = st->value(_var); 
	if (!existVal.first || existVal.second->toString() != _val) {
		return make_pair(false, 0);
	}
	return make_pair(true, 1);
}

std::string EqExp::toString() const {
	return _var + "=" + _val;
}

