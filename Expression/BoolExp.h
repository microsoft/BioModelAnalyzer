#ifndef BOOLEXP_H_
#define BOOLEXP_H_

class BoolExp;

#include <string>

#include "../State.h"
#include "../Simulation.h"

class BoolExp
{
public:
	BoolExp();
	virtual ~BoolExp();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const=0;
	virtual std::string toString() const =0;
};


#endif // !BOOLEXP_H_
