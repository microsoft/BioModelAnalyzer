#include "BoolVar.h"

using std::string;
using std::pair;
using std::make_pair;

BoolVar::BoolVar(const string& var)
	: _var(var)
{
}


BoolVar::~BoolVar()
{
}

BoolExp* BoolVar::copy() const {
	return new BoolVar(_var);
}

pair<bool,unsigned int> BoolVar::evaluate(const State* st, const Simulation* sim, float from, float to) const {
	if (_var.find("[") != std::string::npos) {
		if (sim->expressed(_var, from, to)) {
			return make_pair(true, 1);
		}
		return make_pair(false, 0);
	}

	if (!st->value(_var).first) {
		return make_pair(false, 0);
	}

	if (st->value(_var).second->toString() == "TRUE") {
		return make_pair(true, 1);
	}

	return make_pair(false, 0);
}

string BoolVar::toString() const {
	return _var;
}
